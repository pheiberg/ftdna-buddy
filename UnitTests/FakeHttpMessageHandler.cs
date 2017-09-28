using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    class FakeHttpMessageHandler : HttpMessageHandler
    {
        
        public HttpStatusCode ResponseStatusCode { get; set; }
        public HttpContent ResponseContent { get; set; }
        
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = ResponseStatusCode,
                Content = ResponseContent
            };
            return Task.Run(() => response, cancellationToken);
        }
    }
}