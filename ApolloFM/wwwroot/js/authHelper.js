// wwwroot/authHelper.js

window.authHelpers = {
    // Store token and update auth state
    storeTokenAndAuthenticate: function (token) {
        console.log("Storing token in localStorage...");
        localStorage.setItem('spotify_access_token', token)
        console.log("Token stored in localStorage");
        
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
    
    // Get token
    getToken: function() {
        return localStorage.getItem('spotify_access_token');
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

window.getTokenFromHash = () => {
    const hash = window.location.hash.substring(1);
    const params = new URLSearchParams(hash);
    return params.get("access_token");
};


