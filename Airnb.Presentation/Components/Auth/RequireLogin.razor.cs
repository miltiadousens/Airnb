using Microsoft.AspNetCore.Components;

namespace Airnb.Presentation.Components.Auth
{
    public partial class RequireLogin
    {
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [CascadingParameter] private AuthProvider Auth { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        protected override void OnInitialized()
        {
            if (Auth.IsLoggedIn)
                return;

            // Bed AuthProvider om at huske, hvor vi var – og send til en ren login-side
            Auth.RememberReturnUrl("/" + Navigation.ToBaseRelativePath(Navigation.Uri));
            Navigation.NavigateTo("/login");
        }
    }
}
