using DataModelLibrary.AuthRequestModels;
using LibraryWeb.Services;
using LibraryWeb.Services.Base;
using LibraryWeb.Services.Interfaces;

public class AuthenticationServiceClient : ApiBaseClient, IAuthenticationServiceClient
{
    private readonly AuthenticationStateService _authStateService;

    public AuthenticationServiceClient(HttpClient httpClient, AuthenticationStateService authStateService)
        : base(httpClient)
    {
        _authStateService = authStateService;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var result = await PostJsonAsync<AuthResponse>("api/Authentication/Login", request);

        if (result.IsSuccess && result.Data != null)
        {
            await _authStateService.SetAuthenticationState(result.Data.AccessToken);
        }

        return result;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var result = await PostJsonAsync<AuthResponse>("api/Authentication/Register", request);

        if (result.IsSuccess && result.Data != null)
        {
            await _authStateService.SetAuthenticationState(result.Data.AccessToken);
        }

        return result;
    }

    public Task<bool> IsAuthenticatedAsync() => _authStateService.IsAuthenticated();
    public Task LogoutAsync() => _authStateService.ClearAuthenticationState();
}
