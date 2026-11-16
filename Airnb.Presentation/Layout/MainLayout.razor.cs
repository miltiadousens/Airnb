using Airnb.Presentation.Components.Auth;
using Microsoft.AspNetCore.Components;

namespace Airnb.Presentation.Layout
{
    public partial class MainLayout
    {
        [CascadingParameter] private AuthProvider Auth { get; set; } = default!;  //fra components AuthProvider.razor.cs, for at kunne læse login state
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private bool IsLoggedIn => Auth.IsLoggedIn;
        private string LoggedInUntil => Auth.ExpiresAtLocal?.ToString("HH:mm") ?? "";

        private async Task Logout()
        {
            await Auth.SignOutAsync();
            Navigation.NavigateTo("/login");
        }
    }
}
