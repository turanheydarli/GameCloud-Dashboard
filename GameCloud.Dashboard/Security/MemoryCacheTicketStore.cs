using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace GameCloud.Dashboard.Security;

public class MemoryCacheTicketStore : ITicketStore
{
    private const string KeyPrefix = "AuthTicket_";
    private readonly IMemoryCache _cache;

    public MemoryCacheTicketStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = KeyPrefix + Guid.NewGuid().ToString("N");
        await RenewAsync(key, ticket);
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = ticket.Properties.ExpiresUtc,
            SlidingExpiration = TimeSpan.FromHours(1)
        };

        _cache.Set(key, ticket, options);

        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        _cache.TryGetValue(key, out AuthenticationTicket? ticket);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}