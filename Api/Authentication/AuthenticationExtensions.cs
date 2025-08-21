namespace FireInvent.Api.Authentication;

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
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authOptions.ValidIssuers.Any(),
                    ValidateAudience = authOptions.ValidAudiences.Any(),
                    ValidIssuers = authOptions.ValidIssuers,
                    ValidAudiences = authOptions.ValidAudiences,
                    RoleClaimType = "roles",
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");

                        logger.LogWarning(context.Exception, "Authentication failed.");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var handler = context.HttpContext.RequestServices
                            .GetRequiredService<TokenValidatedHandler>();

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await handler.HandleAsync(context);
                            }
                            catch (Exception ex)
                            {
                                var logger = context.HttpContext.RequestServices
                                    .GetRequiredService<ILogger<TokenValidatedHandler>>();
                                
                                logger.LogError(ex, "Error during synchronization of user from claims.");
                            }
                        });

                        await handler.HandleAsync(context);
                    }

                };
            });

        return services;
    }
}
