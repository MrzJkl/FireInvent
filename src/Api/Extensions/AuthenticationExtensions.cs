using FireInvent.Api.Authentication;

namespace FireInvent.Api.Extensions;

using FireInvent.Api.Helper;
using FireInvent.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

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
                    OnMessageReceived = context =>
                    {
                        // Extract raw JWT from Authorization header before validation
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            return Task.CompletedTask;

                        var token = authHeader.Substring("Bearer ".Length).Trim();
                        if (string.IsNullOrWhiteSpace(token))
                            return Task.CompletedTask;

                        try
                        {
                            var handler = new JwtSecurityTokenHandler();
                            // Read token without validating
                            var jwt = handler.ReadJwtToken(token);

                            // Prefer the Issuer property; fallback to claim lookup
                            var issuer = jwt.Issuer;
                            if (string.IsNullOrWhiteSpace(issuer))
                            {
                                issuer = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;
                            }

                            if (string.IsNullOrWhiteSpace(issuer))
                                return Task.CompletedTask;

                            var realm = TenantResolutionHelper.ExtractRealmFromIssuer(new System.Security.Claims.Claim(JwtRegisteredClaimNames.Iss, issuer));
                            if (!string.IsNullOrWhiteSpace(realm))
                            {
                                var newAuthority = context.Options.Authority?.Replace("master", realm);
                                context.Options.Authority = newAuthority;
                            }
                        }
                        catch
                        {
                            // Ignore malformed token at this stage; validation will handle errors later
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(AuthenticationExtensions));

                        logger.LogWarning(context.Exception, "Authentication failed.");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
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

                            logger.LogError(ex, "Error during synchronization of user from token claims.");
                        }
                    }
                };
            });

        return services;
    }
}
