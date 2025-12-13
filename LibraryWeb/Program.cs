using Blazored.LocalStorage;
using LibraryWeb.Exceptions;
using LibraryWeb.Pages.Error;
using LibraryWeb.Services;
using LibraryWeb.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace LibraryWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<AuthenticationHandler>();
            builder.Services.AddScoped<AuthenticationStateService>();

            builder.Services.AddScoped<IAuthenticationServiceClient, AuthenticationServiceClient>();
            builder.Services.AddScoped<IBookServiceClient, BookServiceClient>();
            builder.Services.AddScoped<IOrderServiceClient, OrderServiceClient>();
            builder.Services.AddScoped<IUserProfileServiceClient, UserProfileServiceClient>();

            builder.Services.AddScoped<ErrorBoundaryLogger>();
            builder.Services.AddScoped<GlobalErrorHandler>();
            builder.Services.AddSingleton<JsErrorHandler>();

            builder.Services.AddHttpClient("AuthClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7121/");
            }).AddHttpMessageHandler<AuthenticationHandler>();

            builder.Services.AddScoped(sp =>
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthClient"));

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddMudServices();

            var host = builder.Build();
            await host.RunAsync();

            var jsHandler = host.Services.GetRequiredService<JsErrorHandler>();
        }
    }
}
