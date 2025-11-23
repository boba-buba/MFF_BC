using dns_netcore;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;

class RecursiveResolver : IRecursiveResolver
{
    private IDNSClient dnsClient;

    private ConcurrentDictionary<string, IP4Addr> _cache = new ConcurrentDictionary<string, IP4Addr>();
    private ConcurrentDictionary<string, Task<IP4Addr>> _tasks = new ConcurrentDictionary<string, Task<IP4Addr>>();

    public RecursiveResolver(IDNSClient client)
    {
        this.dnsClient = client;
    }

    public async Task<IP4Addr> ResolveRecursive(string domain)
    {
        if (_cache.TryGetValue(domain, out IP4Addr cachedAddr))
        {
            return await Task<IP4Addr>.Run(async () => { return await ReverseDomain(cachedAddr, domain); });
        }

        if (!_tasks.ContainsKey(domain))
        {
            _tasks.TryAdd(domain, Task<IP4Addr>.Run(async () => { return await ResolveDomain(domain); }));
        }
        var res = await _tasks[domain]; // Task<IP4Addr>.Run(async () => { return await ResolveDomain(domain);});
        _tasks.TryRemove(domain, out _);
        return res;
    }

    public async Task<IP4Addr> ReverseDomain(IP4Addr cached, string domain)
    {
        try
        {
            var res = await dnsClient.Reverse(cached);
            if (res == domain)
            {
                return cached;
            }
        }
        catch (Exception)
        {

        }
        _cache.TryRemove(domain, out _);
        var t = ResolveDomain(domain);
        return await t;
    }
    public async Task<IP4Addr> ResolveDomain(string domain)
    {

        var domains = domain.Split('.', 2);

        if (domains.Length == 1)
        {
            var parent = dnsClient.GetRootServers()[0];
            var subAddress = dnsClient.Resolve(parent, domains[0]);
            var sub = await subAddress;
            _cache.TryAdd(domain, sub);
            return sub;
        }
        else
        {
            var parent = await ResolveRecursive(domains[1]);
            var subAddress = dnsClient.Resolve(parent, domains[0]);
            var sub = await subAddress;
            _cache.TryAdd(domain, sub);
            return sub;
        }

    }
}
