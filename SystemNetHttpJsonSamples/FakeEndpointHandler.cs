using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SystemNetHttpJsonSamples
{
    partial class Program
    {
        internal class FakeEndpointHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpResponseMessage responseMessage = null;

                if (request.Method == HttpMethod.Get && request.RequestUri.AbsolutePath == "/users/1")
                {
                    responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"id\":1,\"name\":\"Steve Gordon\",\"username\":\"stevejgordon\",\"email\":\"stevejgordon@example.com\",\"address\":{\"street\":\"1 Madeup Place\",\"city\":\"Sometown\",\"postcode\":\"AA1 1AA\",\"geo\":{\"lat\":\"0.0000\",\"lng\":\"0.0000\"}},\"phone\":\"00000 000000\",\"website\":\"stevejgordon.co.uk\",\"company\":{\"name\":\"An Impressive Name\",\"catchPhrase\":\"Building .NET Code everyday!\"}}", Encoding.UTF8, "application/json")
                    };
                }

                if (request.Method == HttpMethod.Get && request.RequestUri.AbsolutePath == "/users/2")
                {
                    responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
                }

                if (request.Method == HttpMethod.Post && request.RequestUri.AbsolutePath == "/users")
                {
                    responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.Created);
                }

                return Task.FromResult(responseMessage);
            }
        }
    }
}
