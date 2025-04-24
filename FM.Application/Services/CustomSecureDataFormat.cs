using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

public class CustomSecureDataFormat : ISecureDataFormat<AuthenticationProperties>
{
    private readonly IDataProtector _dataProtector;
    private readonly ILogger<CustomSecureDataFormat> _logger;

    public CustomSecureDataFormat(IDataProtectionProvider provider, ILogger<CustomSecureDataFormat> logger)
    {
        _dataProtector = provider.CreateProtector("OAuth.State");
        _logger = logger;
    }

    public string Protect(AuthenticationProperties data) =>
        Convert.ToBase64String(_dataProtector.Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data))));

    public string Protect(AuthenticationProperties data, string purpose) => Protect(data);

    public AuthenticationProperties Unprotect(string protectedText) =>
        JsonSerializer.Deserialize<AuthenticationProperties>(
            Encoding.UTF8.GetString(_dataProtector.Unprotect(Convert.FromBase64String(protectedText)))
        );

    public AuthenticationProperties Unprotect(string protectedText, string purpose) => Unprotect(protectedText);
}
