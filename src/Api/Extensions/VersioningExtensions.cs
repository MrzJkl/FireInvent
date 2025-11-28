using Asp.Versioning;

namespace FireInvent.Api.Extensions
{
    internal static class VersioningExtensions
    {
        internal static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(
                    options =>
                    {
                        options.DefaultApiVersion = new ApiVersion(1);
                        options.ReportApiVersions = true;
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.ApiVersionReader = new UrlSegmentApiVersionReader();
                    })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'V";
                    options.SubstituteApiVersionInUrl = true;
                });

            return services;
        }
    }
}
