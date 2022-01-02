using Microsoft.AspNetCore.Identity;

namespace WebApiJwt.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}