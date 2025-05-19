// ApolloFM/wwwroot/js/authenticationService.js

window.AuthenticationService = {
    // Initialize the authentication service
    init: function () {
        console.log("AuthenticationService.init() called.");
        
        // Check for callback URL
        this.handleCallbackIfPresent();
        
        // Check for valid token
        if (this.isTokenValid()) {
            console.log("Found valid token on initialization");
            return true;
        }
        
        return false;
    },
    
    // Handle any authentication callbacks on the current page
    handleCallbackIfPresent: function() {
        // Check for code or token in URL
        const tokenInfo = window.getTokenFromUrl();
        if (tokenInfo) {
            console.log("Found token information in URL");
            
            if (tokenInfo.code) {
                // Authorization code flow - needs backend exchange
                console.log("Found authorization code, sending to backend");
                // This would typically be handled by your Blazor code
            } else if (tokenInfo.accessToken) {
                // Implicit flow - can handle directly
                console.log("Found access token in URL hash");
                window.authHelper.storeTokenAndAuthenticate(
                    tokenInfo.accessToken, 
                    tokenInfo.refreshToken, 
                    tokenInfo.expiresIn
                );
                
                // Clean URL
                window.history.replaceState({}, document.title, '/');
            }
        }
    },
    
    // Store the Spotify access token
    setToken: function (token, refreshToken, expiresIn) {
        return window.authHelper.storeTokenAndAuthenticate(token, refreshToken, expiresIn);
    },
    
    // Retrieve the stored token
    getToken: function () {
        return window.authHelper.getAccessToken();
    },
    
    // Check if the token is valid (not expired)
    isTokenValid: function () {
        const token = this.getToken();
        if (!token) return false;
        return !window.authHelper.isTokenExpired();
    },
    
    // Remove the stored token (logout)
    logout: function () {
        window.authHelper.clearTokens();
        
        // Force authentication state refresh
        if (window.DotNet) {
            DotNet.invokeMethodAsync('ApolloFM', 'ForceAuthenticationStateRefresh')
                .catch(err => console.error("Error refreshing auth state after logout:", err));
        }
        
        return true;
    },
    
    // Check if user is authenticated
    isAuthenticated: function () {
        return this.isTokenValid();
    },
    
    // Check for authentication errors
    checkForErrors: function () {
        const params = new URLSearchParams(window.location.search);
        const error = params.get('error');
        
        if (error) {
            console.error("Authentication error:", error);
            return error;
        }
        
        return null;
    }
};

// Run initialization when script loads
document.addEventListener('DOMContentLoaded', function() {
    console.log("DOM loaded, initializing auth service");
    window.AuthenticationService.init();
});

console.log("authenticationService.js loaded successfully");
