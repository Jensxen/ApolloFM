using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace ApolloFM.JsInterop
{
    public class AuthInteropService : IDisposable
    {
        public AuthInteropService(AuthenticationStateProvider authStateProvider, NavigationManager navigationManager)
        {
            AuthInterop.Initialize(authStateProvider, navigationManager);
        }

        public void Dispose()
        {
            
        }
    }
}
