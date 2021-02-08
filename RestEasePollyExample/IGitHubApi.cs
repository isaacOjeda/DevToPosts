using System.Threading.Tasks;
using RestEase;

namespace RestEasePollyExample
{
    [Header("User-Agent", "RestEase")]
    public interface IGitHubApi
    {
        [Get("users/{userId}")]
        Task<User> GetUserAsync([Path] string userId);
    }
}