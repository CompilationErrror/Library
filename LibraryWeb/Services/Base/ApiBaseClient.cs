using DataModelLibrary;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibraryWeb.Services.Base
{
    public abstract class ApiBaseClient
    {
        protected readonly HttpClient _httpClient;

        protected ApiBaseClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(string))
                {
                    var text = await response.Content.ReadAsStringAsync();
                    return ApiResponse<T>.Success((T)(object)text);
                }

                if (typeof(T) == typeof(bool))
                {
                    return ApiResponse<T>.Success((T)(object)true);
                }

                var data = await response.Content.ReadFromJsonAsync<T>();
                return ApiResponse<T>.Success(data!);
            }

            var errorMessage = await ExtractErrorMessageAsync(response);

            return ApiResponse<T>.Failure(errorMessage, (int)response.StatusCode);
        }

        protected async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            return await HandleResponseAsync<T>(response);
        }

        protected async Task<ApiResponse<T>> PostJsonAsync<T>(string url, object? data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);
            return await HandleResponseAsync<T>(response);
        }

        protected async Task<ApiResponse<T>> PutJsonAsync<T>(string url, object data)
        {
            var response = await _httpClient.PutAsJsonAsync(url, data);
            return await HandleResponseAsync<T>(response);
        }

        protected async Task<ApiResponse<T>> DeleteAsync<T>(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            return await HandleResponseAsync<T>(response);
        }

        private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.TryGetProperty("message", out var messageProp)
                ? messageProp.GetString() ?? "Unknown error occurred."
                : "Unknown error occurred.";
        }
    }
}
