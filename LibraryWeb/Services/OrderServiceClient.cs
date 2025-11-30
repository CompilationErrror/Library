using DataModelLibrary.Models;
using LibraryWeb.Services.Base;
using LibraryWeb.Services.Interfaces;
using System.Net.Http.Json;

namespace LibraryWeb.Services
{
    public class OrderServiceClient : ApiBaseClient, IOrderServiceClient
    {
        public OrderServiceClient(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<OrderedBook>>> GetOrderedBooksAsync()
            => GetAsync<List<OrderedBook>>("api/Orders");

        public async Task<ApiResponse<bool>> ReturnBooksAsync(List<int> bookIds)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "api/Orders")
            {
                Content = JsonContent.Create(bookIds)
            };
            var response = await _httpClient.SendAsync(request);
            return await HandleResponseAsync<bool>(response);
        }

        public Task<ApiResponse<bool>> OrderBookAsync(int bookId)
            => PostJsonAsync<bool>($"api/Orders/{bookId}", null);
    }
}