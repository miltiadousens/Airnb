using Airnb.Presentation.Components.Auth;
using Airnb.Presentation.Models;
using Airnb.Shared.Auth.DTO;
using Airnb.Shared.Auth.Requests;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Json;

namespace Airnb.Presentation.Pages
{
    public partial class Login
    {
        // ---- Services fra Program.cs ----
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [CascadingParameter] private AuthProvider Auth { get; set; } = default!;

        // ---- State, som siden ejer ----
        private bool isBusy;
        private string? errorMessage;

        // Læses fra adressen: /login?expired=true
        [SupplyParameterFromQuery(Name = "expired")]
        public bool Expired { get; set; }
        private bool showExpiredNotice;

        private async Task HandleLogin(LoginModel model)
        {
            isBusy = true;
            errorMessage = null;
            showExpiredNotice = false;   // beskeden har gjort sin pligt, når brugeren prøver igen

            try
            {
                // 1. Oversæt formularens model til API'ets kontrakt
                var request = new LoginRequest(model.Email, model.Password);

                // 2. Send den til serveren
                var response = await Http.PostAsJsonAsync("api/auth/login", request);

                // 3. Forkert login → 401 fra jeres GlobalExceptionHandler
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    errorMessage = "Forkert e-mail eller adgangskode.";
                    return;
                }

                // 4. Alt andet, der ikke er en succes
                if (!response.IsSuccessStatusCode)
                {
                    errorMessage = "Noget gik galt. Prøv igen om lidt.";
                    return;
                }

                // 5. Succes → læs svaret og giv det til ejeren af auth-state
                var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (login is null)
                {
                    errorMessage = "Uventet svar fra serveren.";
                    return;
                }

                await Auth.SignInAsync(login);
                Navigation.NavigateTo(Auth.TakeReturnUrl());   // i stedet for NavigateTo("/")
            }
            catch (HttpRequestException)
            {
                // Serveren svarede slet ikke (kører ikke, forkert port, CORS osv.)
                errorMessage = "Kan ikke få forbindelse til serveren. Kører API'et?";
            }
            finally
            {
                isBusy = false;
            }

        }
        protected override void OnInitialized()
        {
            // Kom brugeren hertil, fordi sessionen udløb?
            showExpiredNotice = Expired;
        }
    }
}
