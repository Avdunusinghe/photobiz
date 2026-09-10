using Mapster;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Users.Common
{
    /// <summary>
    /// Mapster registration for <see cref="User"/> projections. Picked up by
    /// <c>TypeAdapterConfig.GlobalSettings.Scan(...)</c> during application start-up.
    /// </summary>
    public class UserMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<User, UserDto>()
                .Map(
                    dest => dest.Roles,
                    src => src.UserRoles
                        .Select(userRole => userRole.Role.Name)
                        .OrderBy(name => name));
        }
    }
}
