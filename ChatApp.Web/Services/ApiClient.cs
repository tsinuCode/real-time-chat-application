using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ChatApp.Web.Services;

public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiClientResult<TResponse>> PostAsync<TRequest, TResponse>(
        string path, TRequest payload, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            ApplyAuthHeader(request);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ApiClientResult<TResponse>.Failure(
                    ExtractErrorMessage(body, response.StatusCode),
                    TryDeserialize<TResponse>(body));
            }

            var data = JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
            return ApiClientResult<TResponse>.Success(data!);
        }
        catch (Exception ex)
        {
            return ApiClientResult<TResponse>.Failure(ex.Message);
        }
    }

    public async Task<ApiClientResult<TResponse>> GetAsync<TResponse>(
        string path, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            ApplyAuthHeader(request);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ApiClientResult<TResponse>.Failure(
                    ExtractErrorMessage(body, response.StatusCode),
                    TryDeserialize<TResponse>(body));
            }

            var data = JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
            return ApiClientResult<TResponse>.Success(data!);
        }
        catch (Exception ex)
        {
            return ApiClientResult<TResponse>.Failure(ex.Message);
        }
    }

    private static TResponse? TryDeserialize<TResponse>(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
        }
        catch
        {
            return default;
        }
    }

    private static string ExtractErrorMessage(string body, System.Net.HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Request failed with status {(int)statusCode}.";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(message.GetString()))
            {
                var errors = CollectErrors(root);
                return errors.Count > 0
                    ? $"{message.GetString()} {string.Join(" ", errors)}"
                    : message.GetString()!;
            }

            if (root.TryGetProperty("title", out var title) &&
                title.ValueKind == JsonValueKind.String)
            {
                var errors = CollectErrors(root);
                return errors.Count > 0
                    ? string.Join(" ", errors)
                    : title.GetString() ?? $"Request failed with status {(int)statusCode}.";
            }

            var fallbackErrors = CollectErrors(root);
            if (fallbackErrors.Count > 0)
            {
                return string.Join(" ", fallbackErrors);
            }
        }
        catch
        {
            // Fall through to generic message.
        }

        return $"Request failed with status {(int)statusCode}.";
    }

    private static List<string> CollectErrors(JsonElement root)
    {
        var errors = new List<string>();

        if (!root.TryGetProperty("errors", out var errorsElement))
        {
            return errors;
        }

        if (errorsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in errorsElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var text = item.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        errors.Add(text);
                    }
                }
            }
        }
        else if (errorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in errorsElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in property.Value.EnumerateArray())
                {
                    var text = item.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        errors.Add(text);
                    }
                }
            }
        }

        return errors;
    }

    private void ApplyAuthHeader(HttpRequestMessage request)
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

public class ApiClientResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiClientResult<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    public static ApiClientResult<T> Failure(string message, T? data = default) =>
        new() { IsSuccess = false, ErrorMessage = message, Data = data };
}
