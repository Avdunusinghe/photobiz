using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;

namespace Photobiz.Application.Features.Public.GetTenantDiagnostics
{
    public class GetTenantDiagnosticsQueryHandler : IRequestHandler<GetTenantDiagnosticsQuery, TenantDiagnosticsDto>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetTenantDiagnosticsQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TenantDiagnosticsDto> Handle(GetTenantDiagnosticsQuery request, CancellationToken cancellationToken)
        {
            var userCount = await _dbContext.Users.CountAsync(cancellationToken);

            var sampleUsernames = await _dbContext.Users
                .OrderBy(user => user.Username)
                .Take(5)
                .Select(user => user.Username)
                .ToListAsync(cancellationToken);

            return new TenantDiagnosticsDto(userCount, sampleUsernames);
        }
    }
}
