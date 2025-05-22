//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.JSInterop;

//public static class AuthHelpers
//{
//    private static AuthenticationStateProvider _authStateProvider;

//    // Initialiser AuthHelpers med en AuthenticationStateProvider
//    public static void Initialize(AuthenticationStateProvider authStateProvider)
//    {
//        _authStateProvider = authStateProvider;
//    }

//    // Force en opdatering af authentication state
//    [JSInvokable("ForceAuthenticationStateRefresh")]
//    public static void ForceAuthenticationStateRefresh()
//    {
//        if (_authStateProvider is SpotifyAuthenticationStateProvider spotifyAuth)
//        {
//            Console.WriteLine("AuthHelpers: Forcing authentication state refresh");
//            spotifyAuth.TriggerAuthenticationStateChanged();
//        }
//        else
//        {
//            Console.WriteLine("AuthHelpers: AuthenticationStateProvider is not SpotifyAuthenticationStateProvider");
//        }
//    }
//}