using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SystemNetHttpJsonSamples
{
    partial class Program
    {
        static async Task Main()
        {
            const string validUserUri = "https://example.com/users/1";
            const string postUserUri = "https://example.com/users";

            var httpClient = new HttpClient(new FakeEndpointHandler());

            User user = null;

            user = await InefficientStringApproach(validUserUri, httpClient);

            user = await StreamWithNewtonsoftJson(validUserUri, httpClient);

            user = await WebApiClient(validUserUri, httpClient);

            user = await StreamWithSystemTextJson(validUserUri, httpClient);

            user = await GetJsonHttpClient(validUserUri, httpClient);

            user = await GetJsonFromContent(validUserUri, httpClient);

            await PostJsonHttpClient(postUserUri, httpClient);

            await PostJsonContent(postUserUri, httpClient);
        }

        private static async Task<User> InefficientStringApproach(string uri, HttpClient httpClient)
        {
            var httpResponse = await httpClient.GetAsync(uri);

            if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                var jsonString = await httpResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<User>(jsonString);
            }
            else
            {
                Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
            }

            return null;
        }

        private static async Task<User> StreamWithNewtonsoftJson(string uri, HttpClient httpClient)
        {
            using var httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                JsonSerializer serializer = new JsonSerializer();

                try
                {
                    return serializer.Deserialize<User>(jsonReader);
                }
                catch(JsonReaderException)
                {
                    Console.WriteLine("Invalid JSON.");
                } 
            }
            else
            {
                Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
            }

            return null;
        }

        private static async Task<User> WebApiClient(string uri, HttpClient httpClient)
        {
            using var httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            try
            {
                return await httpResponse.Content.ReadAsAsync<User>();
            }
            catch // Could be ArgumentNullException or UnsupportedMediaTypeException
            {
                Console.WriteLine("HTTP Response was invalid or could not be deserialised.");
            }

            return null;
        }

        private static async Task<User> StreamWithSystemTextJson(string uri, HttpClient httpClient)
        {
            using var httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                try
                {
                    return await System.Text.Json.JsonSerializer.DeserializeAsync<User>(contentStream, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true });
                }
                catch (JsonException) // Invalid JSON
                {
                    Console.WriteLine("Invalid JSON.");
                }                
            }
            else
            {
                Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
            }

            return null;
        }

        private static async Task<User> GetJsonHttpClient(string uri, HttpClient httpClient)
        {
            try
            {
                var user = await httpClient.GetFromJsonAsync<User>(uri);

                return user;
            }
            catch (HttpRequestException) // Non success
            {
                Console.WriteLine("An error occurred.");
            }
            catch (NotSupportedException) // When content type is not valid
            {
                Console.WriteLine("The content type is not supported.");
            }
            catch (JsonException) // Invalid JSON
            {
                Console.WriteLine("Invalid JSON.");
            }
            
            return null;
        }

        private static async Task<User> GetJsonFromContent(string uri, HttpClient httpClient)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.TryAddWithoutValidation("some-header", "some-value");

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserialising
                
                try
                {
                    return await response.Content.ReadFromJsonAsync<User>();
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    Console.WriteLine("The content type is not supported.");
                }
                catch (JsonException) // Invalid JSON
                {
                    Console.WriteLine("Invalid JSON.");
                }
            }

            return null;
        }

        private static async Task PostJsonHttpClient(string uri, HttpClient httpClient)
        {
            var postUser = new User { Name = "Steve Gordon" };

            var postResponse = await httpClient.PostAsJsonAsync(uri, postUser);

            postResponse.EnsureSuccessStatusCode();
        }

        private static async Task PostJsonContent(string uri, HttpClient httpClient)
        {
            var postUser = new User { Name = "Steve Gordon" };

            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(postUser)
            };

            var postResponse = await httpClient.SendAsync(postRequest);

            postResponse.EnsureSuccessStatusCode();
        }
    }
}
