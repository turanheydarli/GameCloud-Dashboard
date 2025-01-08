using System.Net.Http.Headers;

namespace GameCloud.Dashboard.Security;

public class AuthenticationMessageHandler : DelegatingHandler
{
    private readonly JwtTokenHandler _tokenHandler;

    public AuthenticationMessageHandler(JwtTokenHandler tokenHandler)
    {
        _tokenHandler = tokenHandler;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _tokenHandler.Token;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}