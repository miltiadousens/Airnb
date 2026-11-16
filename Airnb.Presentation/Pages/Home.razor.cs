using Airnb.Presentation.Components.Auth;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace Airnb.Presentation.Pages
{
    public partial class Home
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [CascadingParameter] private AuthProvider Auth { get; set; } = default!;

        private List<ClaimDto>? claims;
        private string? message;

        private async Task TestToken()
        {
            claims = null;
            message = null;

            // 1. Sørg for et gyldigt token (fornyer selv, hvis nødvendigt)
            if (!await Auth.EnsureValidTokenAsync())
            {
                message = "Du er ikke logget ind, eller din session er udløbet.";
                return;
            }

            // 2. Kald et endpoint, der kræver login
            var response = await Http.GetAsync("api/auth/me");
            if (!response.IsSuccessStatusCode)
            {
                message = $"API'et svarede {(int)response.StatusCode}.";
                return;
            }

            claims = await response.Content.ReadFromJsonAsync<List<ClaimDto>>();
        }

        // Svarer til { Type, Value }, som AuthController.Me returnerer
        private record ClaimDto(string Type, string Value);
    }
}
