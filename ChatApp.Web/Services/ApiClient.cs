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
                    $"Request failed with status {(int)response.StatusCode}.");
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
                    $"Request failed with status {(int)response.StatusCode}.");
            }

            var data = JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
            return ApiClientResult<TResponse>.Success(data!);
        }
        catch (Exception ex)
        {
            return ApiClientResult<TResponse>.Failure(ex.Message);
        }
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

    public static ApiClientResult<T> Failure(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}
