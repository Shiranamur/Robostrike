// MapEndpoints.cs
using Microsoft.AspNetCore.Builder;

namespace tiz_teh_final_csharp_project.Extensions
{
    public static class EndpointExtensions
    {
        public static void Endpoints(this WebApplication app)
        {
            // Dynamically find and register all endpoint mappers
            var endpointMapperTypes = typeof(Program).Assembly
                .GetTypes()
                .Where(t => typeof(IEndpointMapper).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var mapperType in endpointMapperTypes)
            {
                var mapper = Activator.CreateInstance(mapperType) as IEndpointMapper;
                mapper?.Endpoints(app);
            }
        }
    }

    public interface IEndpointMapper
    {
        void Endpoints(WebApplication app);
    }
}