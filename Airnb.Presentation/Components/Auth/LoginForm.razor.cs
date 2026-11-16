using Airnb.Presentation.Models;
using Microsoft.AspNetCore.Components;

namespace Airnb.Presentation.Components.Auth
{
    public partial class LoginForm
    {
        // ---- Data ned fra forælderen ----
        [Parameter] public bool IsBusy { get; set; }
        [Parameter] public string? ErrorMessage { get; set; }

        // ---- Besked op til forælderen ----
        [Parameter] public EventCallback<LoginModel> OnSubmit { get; set; }

        // ---- Lokal state: kun denne komponent bruger den ----
        private LoginModel model = new();
        private bool showPassword;

        // ---- Afledte værdier, som markup'en viser ----
        private string PasswordInputType => showPassword ? "text" : "password";
        private string ToggleLabel => showPassword ? "Skjul adgangskode" : "Vis adgangskode";
        private string SubmitLabel => IsBusy ? "Logger ind…" : "Log ind";
        private bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        // ---- Handlinger ----
        private void TogglePassword() => showPassword = !showPassword;

        private Task HandleValidSubmit() => OnSubmit.InvokeAsync(model);
    }
}
