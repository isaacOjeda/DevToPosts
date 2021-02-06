using System.Threading.Tasks;
using RestEase;

namespace RestEaseExample
{
    [Header("User-Agent", "RestEase")]
    public interface IGitHubApi
    {
        [Get("users/{userId}")]
        Task<User> GetUserAsync([Path] string userId);
    }
}