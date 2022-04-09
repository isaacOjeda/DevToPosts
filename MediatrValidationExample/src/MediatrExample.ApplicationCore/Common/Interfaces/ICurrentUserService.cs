namespace MediatrExample.ApplicationCore.Common.Interfaces;
public interface ICurrentUserService
{
    CurrentUser User { get; }

    bool IsInRole(string roleName);
}

public record CurrentUser(string Id, string UserName, bool IsAuthenticated);