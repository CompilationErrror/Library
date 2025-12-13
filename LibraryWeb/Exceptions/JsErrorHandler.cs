using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LibraryWeb.Exceptions
{
    public class JsErrorHandler
    {
        private readonly NavigationManager _nav;

        public JsErrorHandler(NavigationManager nav)
        {
            _nav = nav;
        }

        [JSInvokable("HandleJsError")]
        public Task HandleError(string message)
        {
            Console.Error.WriteLine($"JS Error: {message}");
            _nav.NavigateTo("/error", forceLoad: true);
            return Task.CompletedTask;
        }
    }
}
