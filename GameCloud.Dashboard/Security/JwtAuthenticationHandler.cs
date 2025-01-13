using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameCloud.Dashboard.Security;

public class JwtAuthenticationOptions : AuthenticationSchemeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromHours(1);
}

public class JwtAuthenticationHandler : AuthenticationHandler<JwtAuthenticationOptions>
{
    private readonly JwtTokenHandler _tokenHandler;
    private readonly ILogger<JwtAuthenticationHandler> _logger;

    public JwtAuthenticationHandler(
        IOptionsMonitor<JwtAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        JwtTokenHandler tokenHandler
        
    )
        : base(options, logger, encoder, clock)
    {
        _tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
        _logger = logger.CreateLogger<JwtAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var token = _tokenHandler.Token;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Authentication failed: No token provided");
                return AuthenticateResult.NoResult();
            }

            var validationParameters = GetTokenValidationParameters();
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                _logger.LogWarning("Authentication failed: Token format is invalid");
                return AuthenticateResult.Fail("Invalid token format");
            }

            ClaimsPrincipal principal;
            SecurityToken validatedToken;
            try
            {
                principal = handler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Authentication failed: Token has expired");
                return AuthenticateResult.Fail("Token has expired");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Authentication failed: Invalid token signature");
                return AuthenticateResult.Fail("Invalid token signature");
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning(ex, "Authentication failed: Token validation failed");
                return AuthenticateResult.Fail("Token validation failed");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Authentication failed: Missing required claims");
                return AuthenticateResult.Fail("Missing required claims");
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication");
            return AuthenticateResult.Fail("An unexpected error occurred");
        }
    }

    private TokenValidationParameters GetTokenValidationParameters()
    {
        var keyBytes = Encoding.UTF8.GetBytes(Options.SecretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    }
}