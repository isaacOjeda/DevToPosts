using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class ChatHub : Hub
{
    public async Task JoinGroup(string groupName, string userName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        await Clients.Group(groupName).SendAsync("NewUser", $"{userName} entró al canal");
    }

    public async Task LeaveGroup(string groupName, string userName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("LeftUser", $"{userName} salió del canal");
    }

    public async Task SendMessage(NewMessage message)
    {
        await Clients.Group(message.GroupName).SendAsync("NewMessage", message);
    }
}

public record NewMessage(string UserName, string Message, string GroupName);