using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SystemNetHttpJsonSamples
{
    class Program
    {
        static async Task Main()
        {
            const string user1Uri = "https://jsonplaceholder.typicode.com/users/1";
            const string postUserUri = "https://jsonplaceholder.typicode.com/users";

            var httpClient = new HttpClient();

            try
            {
                var user = await httpClient.GetFromJsonAsync<User>(user1Uri);

                // Do stuff with the user data
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("An error occurred.");
            }            

            var response = await httpClient.GetAsync(user1Uri);

            if(response.IsSuccessStatusCode)
            {
                var user2 = await response.Content.ReadFromJsonAsync<User>();

                // Do stuff with the user data
            }
            else
            {
                Console.WriteLine("Request was unsuccessful.");
            }

            var postUser = new User { Name = "Steve Gordon" };

            var postResponse1 = await httpClient.PostAsJsonAsync(postUserUri, postUser);

            postResponse1.EnsureSuccessStatusCode();

            var postRequest = new HttpRequestMessage(HttpMethod.Post, postUserUri)
            {
                Content = JsonContent.Create(postUser)
            };

            var postResponse2 = await httpClient.SendAsync(postRequest);

            postResponse2.EnsureSuccessStatusCode();
        }
    }
}
