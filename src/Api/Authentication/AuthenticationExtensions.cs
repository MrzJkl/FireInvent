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
#if DEBUG
                options.RequireHttpsMetadata = false;
#endif
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authOptions.ValidIssuers.Count != 0,
                    ValidateAudience = authOptions.ValidAudiences.Count != 0,
                    ValidIssuers = authOptions.ValidIssuers,
                    ValidAudiences = authOptions.ValidAudiences,
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
                    OnTokenValidated = context =>
                    {
                        _ = Task.Run(async () =>
                        {
                            var scopeFactory = context.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                            using var scope = scopeFactory.CreateScope();

                            try
                            {
                                var handler = scope.ServiceProvider.GetRequiredService<TokenValidatedHandler>();

                                await handler.HandleAsync(context);
                            }
                            catch (Exception ex)
                            {
                                var logger = scope.ServiceProvider
                                    .GetRequiredService<ILogger<TokenValidatedHandler>>();
                                
                                logger.LogError(ex, "Error during synchronization of user from claims.");
                            }
                        });

                        return Task.CompletedTask;
                    }

                };
            });

        return services;
    }
}
