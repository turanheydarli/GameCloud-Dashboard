namespace GameCloud.Dashboard.Security;

public class JwtTokenHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private string? _accessToken;

    public JwtTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Token
    {
        get => _httpContextAccessor.HttpContext?.Request.Cookies["GameCloud.Dashboard.Auth"] ?? string.Empty;
        set
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1) 
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("GameCloud.Dashboard.Auth", value, cookieOptions);
        }
    }


    public string GetToken()
    {
        if (string.IsNullOrEmpty(_accessToken))
            _accessToken = _httpContextAccessor.HttpContext?.Request.Cookies["GameCloud.Dashboard.Auth"];

        return _accessToken ?? string.Empty;
    }
}