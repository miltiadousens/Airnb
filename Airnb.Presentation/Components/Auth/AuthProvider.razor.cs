using Airnb.Presentation.Services;
using Airnb.Shared.Auth.DTO;
using Airnb.Shared.Auth.Requests;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Airnb.Presentation.Components.Auth
{
    public partial class AuthProvider
    {
        // Det, der står mellem <AuthProvider> og </AuthProvider>
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Inject] private SessionStore Store { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        // ---- State: alle kan læse, kun AuthProvider kan ændre ----
        public LoginResponse? Session { get; private set; }
        public bool IsLoggedIn => Session is not null;
        public DateTime? ExpiresAtLocal => Session?.AccessTokenExpiresAt.ToLocalTime();
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private string? returnUrl; // Hvor brugeren ville hen, før hun blev sendt til login (kun i hukommelsen)
        private bool isLoaded;
        private Task<bool>? refreshing;        // NY: en fornyelse, der er i gang

        // ---- Kører én gang, når appen starter (også efter F5) ----
        protected override async Task OnInitializedAsync()
        {
            var saved = await Store.LoadAsync();

            if (saved is not null)
            {
                ApplySession(saved);           // ÆNDRET: var Session = saved;

                if (saved.AccessTokenExpiresAt <= DateTime.UtcNow)
                {
                    await RefreshAsync();
                }
            }

            isLoaded = true;
        }

        // ---- Børnene beder om ændringer her ----
        public async Task SignInAsync(LoginResponse session)
        {
            ApplySession(session);             // ÆNDRET: var Session = session;
            await Store.SaveAsync(session);
            StateHasChanged();
        }

        public async Task SignOutAsync()
        {
            ApplySession(null);                // ÆNDRET: var Session = null;
            await Store.ClearAsync();
            StateHasChanged();
        }
        // Sessionen kunne ikke fornyes → log ud OG fortæl brugeren hvorfor
        private async Task ExpireSessionAsync()
        {
            await SignOutAsync();
            Navigation.NavigateTo("/login?expired=true");
        }

        // NY: Kald før et API-kald, der kræver login.
        // Fornyer automatisk, hvis tokenet er udløbet eller tæt på.
        public async Task<bool> EnsureValidTokenAsync()
        {
            if (Session is null)
                return false;

            if (Session.AccessTokenExpiresAt > DateTime.UtcNow.AddSeconds(30))
                return true;

            return await RefreshAsync();
        }

        // ÆNDRET: Venter på en fornyelse, der allerede er i gang, i stedet for at starte en ny.
        public async Task<bool> RefreshAsync()
        {
            if (refreshing is not null)
                return await refreshing;

            refreshing = RefreshCoreAsync();
            try
            {
                return await refreshing;
            }
            finally
            {
                refreshing = null;
            }
        }

        // ÆNDRET: Det var din gamle RefreshAsync. Kun navnet og "private" er nyt.
        private async Task<bool> RefreshCoreAsync()
        {
            if (Session is null)
                return false;

            try
            {
                var request = new RefreshRequest(Session.RefreshToken);
                var response = await Http.PostAsJsonAsync("api/auth/refresh", request);

                if (!response.IsSuccessStatusCode)
                {
                    await ExpireSessionAsync();
                    return false;
                }

                var fresh = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (fresh is null)
                {
                    await ExpireSessionAsync();
                    return false;
                }

                await SignInAsync(fresh);
                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        // NY: Det ENESTE sted, sessionen sættes. Sørger for, at HttpClient sender det rigtige token med.
        private void ApplySession(LoginResponse? session)
        {
            Session = session;
            Http.DefaultRequestHeaders.Authorization = session is null
                ? null
                : new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        public void RememberReturnUrl(string url)
        {
            returnUrl = url;
        }

        public string TakeReturnUrl()
        {
            var url = returnUrl ?? "/";
            returnUrl = null;
            return url;
        }

    }
  
}
