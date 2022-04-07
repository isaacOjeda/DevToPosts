namespace MediatrValidationExample.Services;
public interface ICurrentUserService
{
    CurrentUser User { get; }

    bool IsInRole(string roleName);
}

public record CurrentUser(string Id, string UserName);