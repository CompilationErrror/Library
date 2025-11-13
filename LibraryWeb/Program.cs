using Blazored.LocalStorage;
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

            builder.Services.AddHttpClient("AuthClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7121/");
            }).AddHttpMessageHandler<AuthenticationHandler>();

            builder.Services.AddScoped(sp =>
                sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthClient"));

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddMudServices();

            await builder.Build().RunAsync();
        }
    }
}
