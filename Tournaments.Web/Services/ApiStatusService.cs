using System.Net.Http.Json;
using System.Text.Json;

namespace Tournaments.Web.Services;

/// <summary>
/// Implementation of the IApiStatusService interface.
/// </summary>
public class ApiStatusService : IApiStatusService
{
    private readonly HttpClient _httpClient;
    private const string ApiEndpoint = "api/TestUtility";

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiStatusService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public ApiStatusService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<string> GetStatusAsync()
    {
        try
        {
            // Add a timeout to the request
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            // Create a diagnostic object to return in case of error
            var diagnostics = new
            {
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ClientInfo = "Blazor WebAssembly",
                Message = "Attempting to connect to API..."
            };

            // Try to connect to the API
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/status", timeoutCts.Token);
            
            // If successful, return the response
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException)
        {
            // Handle timeout specifically
            var errorInfo = new
            {
                Error = "Connection Timeout",
                Message = "The request to the API timed out after 10 seconds.",
                PossibleCauses = new[]
                {
                    "The API server is not running.",
                    "The API server is running but is not responding.",
                    "The API server is running on a different port than expected (http://localhost:5241)."
                },
                Suggestions = new[]
                {
                    "Ensure the API project (Tournaments.Api) is running.",
                    "Check if the API is accessible at http://localhost:5241/api/TestUtility/status in a browser.",
                    "Verify there are no firewall or network issues blocking the connection."
                },
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors
            var errorInfo = new
            {
                Error = "Connection Error",
                Message = ex.Message,
                StatusCode = ex.StatusCode?.ToString() ?? "Unknown",
                PossibleCauses = new[]
                {
                    "The API server is not running.",
                    "The API server is running on a different port than expected (http://localhost:5241).",
                    "CORS policy is preventing the connection."
                },
                Suggestions = new[]
                {
                    "Ensure the API project (Tournaments.Api) is running.",
                    "Check if the API is accessible at http://localhost:5241/api/TestUtility/status in a browser.",
                    "Verify the API's CORS policy allows requests from the web application."
                },
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            // Handle other errors
            var errorInfo = new
            {
                Error = "Unexpected Error",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <inheritdoc/>
    public async Task<string> ResetDatabaseAsync()
    {
        try
        {
            // Add a timeout to the request
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            var response = await _httpClient.PostAsync($"{ApiEndpoint}/reset", null, timeoutCts.Token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException)
        {
            // Handle timeout specifically
            var errorInfo = new
            {
                Error = "Connection Timeout",
                Message = "The request to reset the database timed out after 10 seconds.",
                PossibleCauses = new[]
                {
                    "The API server is not running.",
                    "The database operation is taking longer than expected."
                },
                Suggestions = new[]
                {
                    "Ensure the API project (Tournaments.Api) is running.",
                    "Try again later or check the API logs for any issues."
                },
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors
            var errorInfo = new
            {
                Error = "Connection Error",
                Message = ex.Message,
                StatusCode = ex.StatusCode?.ToString() ?? "Unknown",
                PossibleCauses = new[]
                {
                    "The API server is not running.",
                    "The API server rejected the request.",
                    "The database is not accessible."
                },
                Suggestions = new[]
                {
                    "Ensure the API project (Tournaments.Api) is running.",
                    "Check if the API is accessible at http://localhost:5241/api/TestUtility/status in a browser.",
                    "Verify the database connection in the API project."
                },
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            // Handle other errors
            var errorInfo = new
            {
                Error = "Unexpected Error",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                ApiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
    }
} 