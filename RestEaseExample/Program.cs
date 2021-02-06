using System;
using System.Threading.Tasks;
using RestEase;

namespace RestEaseExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IGitHubApi api = RestClient.For<IGitHubApi>("https://api.github.com");

            User user = await api.GetUserAsync("isaacOjeda");

            Console.WriteLine($"Name: {user.Name}. Blog: {user.Blog}. CreatedAt: {user.CreatedAt}");
            Console.ReadLine();
        }
    }
}
