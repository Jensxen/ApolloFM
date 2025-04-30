// ApolloFM/wwwroot/js/authHelper.js

window.authHelper = {
    // Store token and update auth state
    storeTokenAndAuthenticate: function (token, refreshToken, expiresIn) {
        console.log("Storing tokens in localStorage...");
        localStorage.setItem('spotify_access_token', token);
        
        if (refreshToken) {
            localStorage.setItem('spotify_refresh_token', refreshToken);
        }
        
        if (expiresIn) {
            const expiryTime = Date.now() + (expiresIn * 1000);
            localStorage.setItem('spotify_token_expiry', expiryTime.toString());
        }
        
        console.log("Tokens stored in localStorage");
        
        // Force authentication state refresh if DotNet is available
        if (window.DotNet) {
            try {
                DotNet.invokeMethodAsync('ApolloFM', 'ForceAuthenticationStateRefresh')
                    .then(() => console.log("Authentication state refresh requested"))
                    .catch(err => console.error("Error refreshing auth state:", err));
            } catch (e) {
                console.error("Error invoking .NET method:", e);
            }
        }
        
        return true;
    },
    
    // Check if token exists
    hasToken: function() {
        const token = localStorage.getItem('spotify_access_token');
        return !!token;
    },
    
    // Get access token
    getAccessToken: function() {
        return localStorage.getItem('spotify_access_token');
    },
    
    // Get refresh token
    getRefreshToken: function() {
        return localStorage.getItem('spotify_refresh_token');
    },
    
    // Check if token is expired
    isTokenExpired: function() {
        const expiryTime = localStorage.getItem('spotify_token_expiry');
        if (!expiryTime) return false; // No expiry time set
        
        // Add a buffer of 5 minutes to refresh before actual expiry
        return Date.now() > (parseInt(expiryTime) - (5 * 60 * 1000));
    },
    
    // Clear all tokens
    clearTokens: function() {
        localStorage.removeItem('spotify_access_token');
        localStorage.removeItem('spotify_refresh_token');
        localStorage.removeItem('spotify_token_expiry');
        console.log("All tokens cleared");
        return true;
    }
};

// Register a global function to force auth update that our HTML can call
window.forceAuthUpdate = function() {
    console.log("forceAuthUpdate called from redirect page");
    if (window.DotNet) {
        DotNet.invokeMethodAsync('ApolloFM', 'ForceAuthenticationStateRefresh')
            .then(() => console.log("Authentication state refreshed from global function"))
            .catch(err => console.error("Error in global refresh:", err));
    }
};

// Helper to extract tokens from URL hash or search params
window.getTokenFromUrl = function() {
    // Try hash first (implicit flow)
    const hash = window.location.hash.substring(1);
    if (hash) {
        const params = new URLSearchParams(hash);
        const token = params.get("access_token");
        if (token) {
            return {
                accessToken: token,
                refreshToken: params.get("refresh_token"),
                expiresIn: params.get("expires_in")
            };
        }
    }
    
    // Then try query params (authorization code flow)
    const queryParams = new URLSearchParams(window.location.search);
    const code = queryParams.get("code");
    if (code) {
        return { code: code };
    }
    
    return null;
};

console.log("authHelper.js loaded successfully");
