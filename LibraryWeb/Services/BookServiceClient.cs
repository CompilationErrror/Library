using DataModelLibrary.Models;
using LibraryWeb.Services.Interfaces;
using LibraryWeb.Services.Base;
using Microsoft.AspNetCore.Components.Forms;

namespace LibraryWeb.Services
{
    public class BookServiceClient : ApiBaseClient, IBookServiceClient
    {
        public BookServiceClient(HttpClient httpClient) : base(httpClient) { }

        public Task<ApiResponse<List<Book>>> GetBooksAsync()
            => GetAsync<List<Book>>("api/Book");

        public Task<ApiResponse<Book>> GetBookByIdAsync(int bookId)
            => GetAsync<Book>($"api/Book/{bookId}"); 

        public Task<ApiResponse<string>> GetBookCoverAsync(int bookId)
            => GetAsync<string>($"api/CoverImages/{bookId}");

        public async Task<ApiResponse<string>> UploadBookCoverAsync(int bookId, IBrowserFile file)
        {
            var content = new MultipartFormDataContent
            {
                { new StreamContent(file.OpenReadStream()), "coverImg", file.Name }
            };

            var response = await _httpClient.PostAsync($"api/CoverImages/{bookId}", content);
            return await HandleResponseAsync<string>(response);
        }

        public Task<ApiResponse<bool>> DeleteBookCoverAsync(int bookId)
            => DeleteAsync<bool>($"api/CoverImages/{bookId}");

        public Task<ApiResponse<bool>> OrderBookAsync(int bookId)
            => PostJsonAsync<bool>($"api/Orders/{bookId}", null);

        public Task<ApiResponse<Book>> AddBookAsync(Book book)
            => PostJsonAsync<Book>("api/Book", book);

        public Task<ApiResponse<Book>> UpdateBookAsync(Book book)
            => PutJsonAsync<Book>("api/Book", book);

        public async Task<ApiResponse<bool>> DeleteBookAsync(int bookId)
        {
            await DeleteBookCoverAsync(bookId);
            return await DeleteAsync<bool>($"api/Book/{bookId}");
        }
    }
}
