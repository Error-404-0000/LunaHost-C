using LunaHost;
using LunaHost.Attributes.MiddleWares;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using LunaHost.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost.MiddleWares.Security.DDoSProtection
{
    [AsMiddleWare]
    [AttributeUsage(AttributeTargets.Class)]
    public class Redirector : Attribute, IMiddleWare
    {
        private static readonly ConcurrentDictionary<int, RequestSrc> RequestSrcs = new();
        private const int MaxEntries = 300; // Maximum entries in the dictionary
        private const int MaxRequestsPerSecond = 30000; // Maximum requests allowed per second
        private const int TimeoutSeconds = 20; // Timeout duration in seconds
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);
        private static readonly object CleanupLock = new();
        private static DateTime LastCleanupTime = DateTime.UtcNow;

        public Task<IMiddleWareResult<IHttpResponse>> ExecuteAsync(HttpRequest request, dynamic? obj)
        {
            // Perform periodic cleanup of stale entries
            PerformCleanup();

            // Generate a unique hash for the request's source
            var hashcode = CacheLily.CacheHashCodeGenerator.GenerateCacheHashCode(request.SourceAddress);
            hashcode = hashcode < 0 ? -hashcode : hashcode;

            // Retrieve or create a RequestSrc entry in a thread-safe way
            var reqSrc = RequestSrcs.GetOrAdd(hashcode, _ => new RequestSrc(0, 0, 0));

            lock (reqSrc) // Ensure thread safety for this specific entry
            {
                var nowTicks = DateTime.UtcNow.Ticks;

                // Timeout handling
                if (nowTicks < reqSrc.TimeoutTime)
                {
                    return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(
                        HttpResponse.Forbidden($"Rate limit protection. Refresh in {TimeoutSeconds} seconds."),
                        false));
                }

                // Rate limiting
                if (reqSrc.TotalRequestsIn1000Ms >= MaxRequestsPerSecond)
                {
                    reqSrc.TimeoutTime = nowTicks + TimeSpan.FromSeconds(TimeoutSeconds).Ticks;
                    reqSrc.TotalRequestsIn1000Ms = 0;
                    return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(
                        HttpResponse.Forbidden($"Rate limit protection. Refresh in {TimeoutSeconds} seconds."),
                        false));
                }

                // Update request metadata
                if (reqSrc.LastRequestTime + TimeSpan.FromMilliseconds(1000).Ticks > nowTicks)
                {
                    reqSrc.TotalRequestsIn1000Ms++;
                }
                else
                {
                    reqSrc.TotalRequestsIn1000Ms = 1; // Reset count if outside the 1-second window
                }

                reqSrc.LastRequestTime = nowTicks;
            }

            // Allow the request to proceed
            return Task.FromResult<IMiddleWareResult<IHttpResponse>>(new MiddleWareResult<IHttpResponse>(default!, true));
        }

        /// <summary>
        /// Periodically removes stale entries to prevent memory bloat.
        /// </summary>
        private void PerformCleanup()
        {
            lock (CleanupLock)
            {
                if (DateTime.UtcNow - LastCleanupTime < CleanupInterval) return;

                var nowTicks = DateTime.UtcNow.Ticks;

                foreach (var key in RequestSrcs.Keys)
                {
                    if (RequestSrcs.TryGetValue(key, out var reqSrc) &&
                        nowTicks > reqSrc.LastRequestTime + TimeSpan.FromMinutes(5).Ticks)
                    {
                        RequestSrcs.TryRemove(key, out _);
                    }
                }

                LastCleanupTime = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Holds metadata for rate-limiting requests.
    /// </summary>
    class RequestSrc
    {
        public int TotalRequestsIn1000Ms { get; set; }
        public long LastRequestTime { get; set; }
        public long TimeoutTime { get; set; }

        public RequestSrc(int totalRequestsIn1000Ms, long lastRequestTime, long timeoutTime)
        {
            TotalRequestsIn1000Ms = totalRequestsIn1000Ms;
            LastRequestTime = lastRequestTime;
            TimeoutTime = timeoutTime;
        }
    }

}
