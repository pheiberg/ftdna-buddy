using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FtdnaBuddy.Ftdna
{
    public class ThrottlingMessageHandler : DelegatingHandler, IDisposable
    {
        public int Delay { get; }
        DateTime _lastThrottledCall = DateTime.MinValue;

        public ThrottlingMessageHandler(int delay, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            Delay = delay;
        }

        /// <summary>
        /// Gets or sets the throttling filter. Requests are throttled if filter returns true.
        /// </summary>
        /// <value>The filter.</value>
        public Func<HttpRequestMessage, bool> Filter { get; set; } = x => true;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool throttled = Filter(request);
            TimeSpan diff = DateTime.UtcNow - _lastThrottledCall;
            if (throttled && diff.TotalMilliseconds < Delay)
            {
                await Task.Delay(Delay - (int)diff.TotalMilliseconds);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (throttled)
            {
                _lastThrottledCall = DateTime.UtcNow;
            }

            return response;
        }
    }
}