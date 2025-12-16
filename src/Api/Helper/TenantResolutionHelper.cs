using System.Security.Claims;

namespace FireInvent.Api.Helper
{
    public static class TenantResolutionHelper
    {
        public static string? ExtractRealmFromIssuer(Claim issuerClaim)
        {
            // Extract realm from issuer URL (e.g., "https://keycloak.example.com/realms/fire-dept-berlin" -> "fire-dept-berlin")
            string? realm = null;

            var issuerUri = new Uri(issuerClaim.Value);
            var pathSegments = issuerUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Look for "realms" segment and get the next one
            for (int i = 0; i < pathSegments.Length - 1; i++)
            {
                if (pathSegments[i].Equals("realms", StringComparison.OrdinalIgnoreCase))
                {
                    realm = pathSegments[i + 1];
                    break;
                }
            }

            return realm;
        }
    }
}
