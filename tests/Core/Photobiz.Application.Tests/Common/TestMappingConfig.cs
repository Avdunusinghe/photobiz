using Mapster;
using Photobiz.Application;

namespace Photobiz.Application.Tests.Common
{
    /// <summary>
    /// Builds a <see cref="TypeAdapterConfig"/> with every <c>IRegister</c> in the
    /// application assembly applied, mirroring the runtime Mapster registration.
    /// </summary>
    public static class TestMappingConfig
    {
        public static TypeAdapterConfig Create()
        {
            var config = new TypeAdapterConfig();
            config.Scan(typeof(ApplicationAssemblyMarker).Assembly);
            return config;
        }
    }
}
