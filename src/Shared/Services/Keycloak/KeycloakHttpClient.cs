using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services.Keycloak;

public class KeycloakHttpClient
{
    private const int TokenExpiryBufferSeconds = 30;
    private const int DefaultTokenExpirySeconds = 300;

    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakHttpClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly object _tokenLock = new();

    public KeycloakHttpClient(
        HttpClient httpClient,
        IOptions<KeycloakAdminOptions> options,
        ILogger<KeycloakHttpClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ValidateConfiguration();

        _httpClient.BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public JsonSerializerOptions JsonOptions => _jsonOptions;
    public string Realm => _options.Realm;

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Url))
            throw new InvalidOperationException("Keycloak URL is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AdminUsername))
            throw new InvalidOperationException("Keycloak admin username is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AdminPassword))
            throw new InvalidOperationException("Keycloak admin password is not configured.");

        if (string.IsNullOrWhiteSpace(_options.Realm))
            throw new InvalidOperationException("Keycloak realm is not configured.");
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        lock (_tokenLock)
        {
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
                return;
        }

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _options.AdminUsername!,
            ["password"] = _options.AdminPassword!
        });

        var stopwatch = Stopwatch.StartNew();
        var endpoint = "realms/master/protocol/openid-connect/token";

        try
        {
            var response = await _httpClient.PostAsync(endpoint, tokenRequest);
            stopwatch.Stop();

            LogRequest("POST", endpoint, response.StatusCode, stopwatch.ElapsedMilliseconds);

            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(_jsonOptions);
            _accessToken = tokenResponse?.AccessToken
                ?? throw new InvalidOperationException("Failed to obtain access token from Keycloak.");

            lock (_tokenLock)
            {
                _tokenExpiry = DateTime.UtcNow.AddSeconds(
                    (tokenResponse.ExpiresIn ?? DefaultTokenExpirySeconds) - TokenExpiryBufferSeconds);
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            _logger.LogDebug("Successfully authenticated with Keycloak. Token expires at {TokenExpiry}", _tokenExpiry);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to authenticate with Keycloak after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        await EnsureAuthenticatedAsync();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetAsync(requestUri);
            stopwatch.Stop();

            LogRequest("GET", requestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GET request to {RequestUri} failed after {ElapsedMs}ms", requestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
    {
        await EnsureAuthenticatedAsync();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);
            stopwatch.Stop();

            LogRequest("POST", requestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "POST request to {RequestUri} failed after {ElapsedMs}ms", requestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        await EnsureAuthenticatedAsync();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, value, _jsonOptions);
            stopwatch.Stop();

            LogRequest("POST", requestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "POST request to {RequestUri} failed after {ElapsedMs}ms", requestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
    {
        await EnsureAuthenticatedAsync();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.PutAsJsonAsync(requestUri, value, _jsonOptions);
            stopwatch.Stop();

            LogRequest("PUT", requestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "PUT request to {RequestUri} failed after {ElapsedMs}ms", requestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        await EnsureAuthenticatedAsync();

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.DeleteAsync(requestUri);
            stopwatch.Stop();

            LogRequest("DELETE", requestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "DELETE request to {RequestUri} failed after {ElapsedMs}ms", requestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void LogRequest(string method, string endpoint, System.Net.HttpStatusCode statusCode, long elapsedMilliseconds)
    {
        var statusCodeValue = (int)statusCode;
        var logLevel = statusCodeValue >= 500 ? LogLevel.Error
                     : statusCodeValue >= 400 ? LogLevel.Warning
                     : LogLevel.Information;

        _logger.Log(logLevel,
            "Keycloak API: {Method} {Endpoint} => {StatusCode} ({StatusCodeValue}) in {ElapsedMs}ms",
            method, endpoint, statusCode, statusCodeValue, elapsedMilliseconds);
    }
}
