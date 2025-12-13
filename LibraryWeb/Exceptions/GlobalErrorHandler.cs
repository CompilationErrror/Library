using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LibraryWeb.Exceptions
{
    public class GlobalErrorHandler
    {
        private readonly NavigationManager _nav;
        private readonly ISnackbar _snackbar;

        public GlobalErrorHandler(NavigationManager nav, ISnackbar snackbar)
        {
            _nav = nav;
            _snackbar = snackbar;
        }

        public void Handle(Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex}");
            _snackbar.Add("Something went wrong", Severity.Error);

            _nav.NavigateTo("/error", forceLoad: true);
        }
    }
}
