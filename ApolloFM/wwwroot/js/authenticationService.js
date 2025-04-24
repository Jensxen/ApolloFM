window.AuthenticationService = {
    // Initialize the authentication service
    init: function () {
        console.log("AuthenticationService.init() called.");
        
        // Check if we have a token on load
        const token = this.getToken();
        if (token) {
            console.log("Found existing token on initialization");
            return true;
        }
        
        return false;
    },
    
    // Get current user information (placeholder implementation)
    getUser: function () {
        console.log("AuthenticationService.getUser() called.");
        return {
            name: "Test User",
            email: "testuser@example.com"
        };
    },
    
    // Store the Spotify access token
    setToken: function (token, expiresIn) {
        if (!token) return false;
        
        localStorage.setItem('spotify_access_token', token);
        
        // Store token expiry time if provided
        if (expiresIn) {
            const expiryTime = Date.now() + (expiresIn * 1000);
            localStorage.setItem('spotify_token_expiry', expiryTime.toString());
        }
        
        console.log("Access token stored successfully");
        return true;
    },
    
    // Retrieve the stored token
    getToken: function () {
        return localStorage.getItem('spotify_access_token');
    },
    
    // Check if the token is valid (not expired)
    isTokenValid: function () {
        const token = this.getToken();
        if (!token) return false;
        
        const expiryTime = localStorage.getItem('spotify_token_expiry');
        if (!expiryTime) return true; // No expiry time, assume valid
        
        return Date.now() < parseInt(expiryTime);
    },
    
    // Remove the stored token (logout)
    removeToken: function () {
        localStorage.removeItem('spotify_access_token');
        localStorage.removeItem('spotify_token_expiry');
        console.log("Token removed");
        return true;
    },
    
    // Handle authentication callback from Spotify
    handleAuthCallback: function () {
        // Parse URL hash or query parameters
        const params = new URLSearchParams(window.location.search);
        const token = params.get('token');
        
        if (token) {
            const expiresIn = params.get('expires_in');
            this.setToken(token, expiresIn);
            return true;
        }
        
        return false;
    },
    
    // Check if user is authenticated
    isAuthenticated: function () {
        return this.isTokenValid();
    },
    
    // Detect authentication errors
    checkForErrors: function () {
        const params = new URLSearchParams(window.location.search);
        const error = params.get('error');
        
        if (error) {
            console.error("Authentication error:", error);
            return error;
        }
        
        return null;
    },
    
    // Refresh the page after successful authentication
    redirectAfterAuth: function (redirectPath = '/dashboard') {
        if (this.isAuthenticated()) {
            // Clean the URL (remove token parameters)
            window.history.replaceState({}, document.title, '/');
            // Redirect to the specified path
            window.location.href = redirectPath;
            return true;
        }
        return false;
    }
};

// Run initialization when script loads
document.addEventListener('DOMContentLoaded', function() {
    window.AuthenticationService.init();
    
    // Check for authentication callback
    if (window.location.search.includes('token=')) {
        window.AuthenticationService.handleAuthCallback();
    }
    
    // Check for errors
    window.AuthenticationService.checkForErrors();
});
