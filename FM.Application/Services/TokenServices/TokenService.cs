// Example of correct TokenService implementation
public class TokenService
{
    private string _accessToken;
    private DateTime _expirationTime;
    private string _refreshToken;
    
    // Proper locking mechanism for thread safety
    private readonly object _lock = new object();

    public string GetAccessToken()
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _expirationTime)
            {
                return null;
            }
            return _accessToken;
        }
    }

    public void SetAccessToken(string accessToken, int expiresInSeconds)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));
            
        lock (_lock)
        {
            _accessToken = accessToken;
            _expirationTime = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        }
    }

    public string GetRefreshToken()
    {
        lock (_lock)
        {
            return _refreshToken;
        }
    }

    public void SetRefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentNullException(nameof(refreshToken));
            
        lock (_lock)
        {
            _refreshToken = refreshToken;
        }
    }

    public bool NeedsRefresh()
    {
        lock (_lock)
        {
            return string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow.AddMinutes(5) >= _expirationTime;
        }
    }

    public void ClearAccessTokens()
    {
        lock (_lock)
        {
            _accessToken = null;
            _refreshToken = null;
            _expirationTime = DateTime.MinValue;
        }
    }
}
