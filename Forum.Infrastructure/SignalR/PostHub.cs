using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Forum.Infrastructure.SignalR;

[Authorize]
public class PostHub : Hub
{
    public Task JoinPostGroup(Guid postId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(postId));
    }

    public static string GetGroupName(Guid postId) => $"post:{postId}";
}
