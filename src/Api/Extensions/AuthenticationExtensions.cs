namespace FireInvent.Api.Extensions;

using FireInvent.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        string scheme,
        AuthenticationOptions authOptions)
    {
        services
            .AddAuthentication(scheme)
            .AddJwtBearer(scheme, options =>
            {
                options.Authority = authOptions.Authority;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authOptions.ValidIssuers.Count != 0,
                    ValidateAudience = authOptions.ValidAudiences.Count != 0,
                    ValidIssuers = authOptions.ValidIssuers,
                    ValidAudiences = authOptions.ValidAudiences,
                    ValidateLifetime = authOptions.ValidateLifetime,
                    ClockSkew = TimeSpan.FromSeconds(authOptions.ClockSkewSeconds),
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(AuthenticationExtensions));

                        logger.LogWarning(context.Exception, "Authentication failed.");
                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
