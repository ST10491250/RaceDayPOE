using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace RaceDayMVC.Services;
public class ApiException(HttpStatusCode status, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}
public class RaceDayApi(IHttpClientFactory factory, IWebHostEnvironment environment)
{
    private HttpClient Client => factory.CreateClient("RaceDayApi");
    public async Task<T> Get<T>(string path)
    {
        using var response = await Client.GetAsync(path);
        await Check(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new JsonException("Empty API response.");
    }
    public async Task<T> Post<T>(string path, object payload)
    {
        using var response = await Client.PostAsJsonAsync(path, payload);
        await Check(response);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new JsonException("Empty API response.");
    }
    public async Task Put(string path, object payload)
    {
        using var response = await Client.PutAsJsonAsync(path, payload);
        await Check(response);
    }
    public async Task Delete(string path) {
        using var response=await Client.DeleteAsync(path); await Check(response);
    }
    private async Task Check(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        string message = response.StatusCode switch {
            HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
            HttpStatusCode.Forbidden => "You do not have permission to access this event.",
            HttpStatusCode.NotFound => "The requested record could not be found.",
            _ => "The API could not complete this request. Please try again."
        };
        if ((int)response.StatusCode < 500 || environment.IsDevelopment())
        {
            try {
                using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (body.RootElement.TryGetProperty("message", out var value)) message = value.GetString() ?? message;
                else if (body.RootElement.TryGetProperty("errors", out var errors))
                    message = string.Join(" ", errors.EnumerateObject().SelectMany(p => p.Value.EnumerateArray()).Select(v => v.GetString()));
            } catch (JsonException) { }
        }
        if (environment.IsDevelopment()) message = $"API {(int)response.StatusCode} at {response.RequestMessage?.RequestUri?.AbsolutePath}: {message}";
        throw new ApiException(response.StatusCode, message);
    }
}
