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
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/api-status";
            
            // Create a diagnostic object to return in case of error
            var diagnostics = new
            {
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ClientInfo = "Blazor WebAssembly",
                Message = "Attempting to connect to API..."
            };

            Console.WriteLine($"Attempting to connect to API at: {apiUrl}");

            // Try to connect to the API
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/api-status", timeoutCts.Token);
            
            // If successful, return the response
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API response: {result}");
            return result;
        }
        catch (TaskCanceledException)
        {
            // Handle timeout specifically
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/api-status";
            var isAzure = apiUrl.Contains("azurewebsites.net");
            
            var errorInfo = new
            {
                Error = "Timeout",
                Message = "The API request timed out after 15 seconds.",
                PossibleCauses = isAzure 
                    ? new[]
                    {
                        "The Azure API app might be in a cold start state.",
                        "The Azure API app might be experiencing high load.",
                        "There might be network latency between the client and Azure."
                    }
                    : new[]
                    {
                        "The API server is not running.",
                        "The API server is running but is taking too long to respond."
                    },
                Suggestions = isAzure
                    ? new[]
                    {
                        "Wait a moment and try again (Azure apps may need to warm up).",
                        "Check if the Azure API app is running in the Azure portal.",
                        "Check the Azure API app's logs for any issues."
                    }
                    : new[]
                    {
                        "Ensure the API project (Tournaments.Api) is running.",
                        $"Check if the API is accessible at {apiUrl} in a browser."
                    },
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/api-status";
            var isAzure = apiUrl.Contains("azurewebsites.net");
            
            var errorInfo = new
            {
                Error = "Connection Error",
                Message = ex.Message,
                StatusCode = ex.StatusCode?.ToString() ?? "Unknown",
                PossibleCauses = isAzure
                    ? new[]
                    {
                        "The Azure API app might not be running.",
                        "The Azure API app URL might be incorrect.",
                        "CORS policy might be preventing the connection.",
                        "The Azure API app might be experiencing issues."
                    }
                    : new[]
                    {
                        "The API server is not running.",
                        "The API server is running on a different port than expected.",
                        "CORS policy is preventing the connection."
                    },
                Suggestions = isAzure
                    ? new[]
                    {
                        "Check if the Azure API app is running in the Azure portal.",
                        $"Verify the API URL in the configuration: {apiUrl}",
                        "Check the Azure API app's CORS configuration.",
                        "Check the Azure API app's logs for any issues."
                    }
                    : new[]
                    {
                        "Ensure the API project (Tournaments.Api) is running.",
                        $"Check if the API is accessible at {apiUrl} in a browser.",
                        "Verify the API's CORS policy allows requests from the web application."
                    },
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            // Handle other errors
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/api-status";
            var errorInfo = new
            {
                Error = "Unexpected Error",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetDatabaseStatusAsync()
    {
        try
        {
            // Add a timeout to the request
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status";
            
            // Try to connect to the API
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/status", timeoutCts.Token);
            
            // If successful, return the response
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException)
        {
            // Handle timeout specifically
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status";
            var isAzure = apiUrl.Contains("azurewebsites.net");
            
            var errorInfo = new
            {
                Error = "Database Connection Timeout",
                Message = "The database status request timed out after 15 seconds.",
                PossibleCauses = isAzure
                    ? new[]
                    {
                        "The Azure SQL Database might be in a cold start state.",
                        "The Azure API app might be experiencing high load.",
                        "The database connection might be misconfigured."
                    }
                    : new[]
                    {
                        "The API server is not running.",
                        "The database is not responding.",
                        "The database connection is misconfigured."
                    },
                Suggestions = isAzure
                    ? new[]
                    {
                        "Wait a moment and try again (Azure services may need to warm up).",
                        "Check the Azure SQL Database status in the Azure portal.",
                        "Verify the database connection string in the API's configuration."
                    }
                    : new[]
                    {
                        "Ensure the API project (Tournaments.Api) is running.",
                        "Check if the database server is running.",
                        "Verify the database connection string in the API's configuration."
                    },
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status";
            var isAzure = apiUrl.Contains("azurewebsites.net");
            
            var errorInfo = new
            {
                Error = "Database Connection Error",
                Message = ex.Message,
                StatusCode = ex.StatusCode?.ToString() ?? "Unknown",
                PossibleCauses = isAzure
                    ? new[]
                    {
                        "The Azure API app might not be running.",
                        "The Azure SQL Database might not be accessible.",
                        "The database connection might be misconfigured."
                    }
                    : new[]
                    {
                        "The API server is not running.",
                        "The database is not accessible.",
                        "The database connection is misconfigured."
                    },
                Suggestions = isAzure
                    ? new[]
                    {
                        "Check if the Azure API app is running in the Azure portal.",
                        "Check the Azure SQL Database status in the Azure portal.",
                        "Verify the database connection string in the API's configuration."
                    }
                    : new[]
                    {
                        "Ensure the API project (Tournaments.Api) is running.",
                        "Check if the database server is running.",
                        "Verify the database connection string in the API's configuration."
                    },
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            // Handle other errors
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/status";
            var errorInfo = new
            {
                Error = "Database Status Error",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                ApiUrl = apiUrl,
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
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset";
            
            var response = await _httpClient.PostAsync($"{ApiEndpoint}/reset", null, timeoutCts.Token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException)
        {
            // Handle timeout specifically
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset";
            var isAzure = apiUrl.Contains("azurewebsites.net");
            
            var errorInfo = new
            {
                Error = "Database Reset Timeout",
                Message = "The request to reset the database timed out after 15 seconds.",
                PossibleCauses = isAzure
                    ? new[]
                    {
                        "The Azure API app might be experiencing high load.",
                        "The database operation is taking longer than expected.",
                        "The operation might not be allowed in the Azure environment."
                    }
                    : new[]
                    {
                        "The API server is not running.",
                        "The database operation is taking longer than expected."
                    },
                Suggestions = isAzure
                    ? new[]
                    {
                        "Check if the Azure API app is running in the Azure portal.",
                        "Check the Azure API app's logs for any issues.",
                        "Verify that the API is configured to allow database resets in the Azure environment."
                    }
                    : new[]
                    {
                        "Ensure the API project (Tournaments.Api) is running.",
                        "Try again later or check the API logs for any issues."
                    },
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            // Handle HTTP request errors
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset";
            var isAzure = apiUrl.Contains("azurewebsites.net");
            
            var errorInfo = new
            {
                Error = "Database Reset Error",
                Message = ex.Message,
                StatusCode = ex.StatusCode?.ToString() ?? "Unknown",
                PossibleCauses = isAzure
                    ? new[]
                    {
                        "The Azure API app might not be running.",
                        "The Azure SQL Database might not be accessible.",
                        "The operation might not be allowed in the Azure environment."
                    }
                    : new[]
                    {
                        "The API server is not running.",
                        "The database is not accessible.",
                        "The operation is not allowed in the current environment."
                    },
                Suggestions = isAzure
                    ? new[]
                    {
                        "Check if the Azure API app is running in the Azure portal.",
                        "Check the Azure SQL Database status in the Azure portal.",
                        "Verify that the API is configured to allow database resets in the Azure environment."
                    }
                    : new[]
                    {
                        "Ensure the API project (Tournaments.Api) is running.",
                        "Check if the database server is running.",
                        "Verify that the API is running in a development or local environment."
                    },
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            // Handle other errors
            var apiUrl = $"{_httpClient.BaseAddress}{ApiEndpoint}/reset";
            var errorInfo = new
            {
                Error = "Database Reset Error",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                ApiUrl = apiUrl,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions { WriteIndented = true });
        }
    }
} 