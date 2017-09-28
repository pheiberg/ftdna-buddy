using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FtdnaBuddy.Ftdna;
using FtdnaBuddy.Ftdna.Model;
using Newtonsoft.Json;
using Xunit;

namespace UnitTests
{
    public class FtdnaConnectorTests
    {
        [Theory, AutoSubstituteData]
        public async void ListAllMatches_ShouldPopulateAllFields(MatchResults expected)
        {
            string json = JsonConvert.SerializeObject(expected, Formatting.Indented);
            
            var sut = new FtdnaConnector(
                new FakeHttpMessageHandler
                {
                    ResponseStatusCode = HttpStatusCode.OK,
                    ResponseContent = new StringContent(json)
                });

            var result = await sut.ListAllMatches();
            
            result.Data.Should().BeEquivalentTo(expected.Data);
        }
    }

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