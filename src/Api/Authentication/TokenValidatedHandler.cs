namespace FireInvent.Api.Authentication
{
    using FireInvent.Shared.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Caching.Memory;
    using System.IdentityModel.Tokens.Jwt;

    public class TokenValidatedHandler(
        IMemoryCache cache,
        IUserService userService)
    {
        private const string SeenJTICachePrefix = "seen_jti";

        public async Task HandleAsync(TokenValidatedContext context)
        {
            if (context.Principal is null)
                return;

            var jti = context.Principal.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti)) return;

            var cacheKey = $"{SeenJTICachePrefix}:{jti}";
            if (cache.TryGetValue(cacheKey, out _)) return;

            await userService.SyncUserFromClaimsAsync(context.Principal);

            var exp = long.Parse(
                context.Principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value
            );

            cache.Set(
                cacheKey,
                true,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(exp)
                });
        }
    }
}
