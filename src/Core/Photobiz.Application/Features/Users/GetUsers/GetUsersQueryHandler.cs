using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;

namespace Photobiz.Application.Features.Users.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public GetUsersQueryHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

            // The admin user list is the one place deactivated (soft-deleted) users must still be
            // visible and manageable, so it bypasses the global IsActive query filter and instead
            // exposes IsActive as an explicit, filterable column.
            var query = _dbContext.Users.IgnoreQueryFilters().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchText = request.SearchText.Trim().ToLower();
                query = query.Where(user =>
                    user.Username.ToLower().Contains(searchText) ||
                    user.FirstName.ToLower().Contains(searchText) ||
                    user.LastName.ToLower().Contains(searchText) ||
                    user.Email.ToLower().Contains(searchText) ||
                    (user.MobileNumber != null && user.MobileNumber.ToLower().Contains(searchText)));
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var role = request.Role.Trim();
                query = query.Where(user => user.UserRoles.Any(userRole => userRole.Role.Name == role));
            }

            if (request.IsActive is bool isActive)
            {
                query = query.Where(user => user.IsActive == isActive);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return PagedResult<UserDto>.Empty(pageNumber, pageSize);
            }

            var items = await query
                .OrderBy(user => user.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectToType<UserDto>(_mappingConfig)
                .ToListAsync(cancellationToken);

            return new PagedResult<UserDto>(items, totalCount, pageNumber, pageSize);
        }
    }
}
