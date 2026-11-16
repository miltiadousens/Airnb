using Airnb.Shared.Auth.DTO;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Airnb.Presentation.Services
{
    public class SessionStore
    {
        private const string Key = "airnb.session";
        private readonly IJSRuntime _js;

        public SessionStore(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SaveAsync(LoginResponse session)
        {
            var json = JsonSerializer.Serialize(session);
            await _js.InvokeVoidAsync("localStorage.setItem", Key, json);
        }

        public async Task<LoginResponse?> LoadAsync()
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", Key);
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<LoginResponse>(json);
            }
            catch (JsonException)
            {
                return null; // ødelagt eller gammelt format → lad som om der intet er
            }
        }

        public async Task ClearAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", Key);
        }
    }
}
