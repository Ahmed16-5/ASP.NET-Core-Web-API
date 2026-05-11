using Microsoft.AspNetCore.SignalR;

namespace ASP.NET_Core_Web_API.Hubs;

public class StudyGroupHub : Hub
{
    public async Task JoinGroup(string groupId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

    public async Task LeaveGroup(string groupId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
}