using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatPrototipo.API.Hubs;

public class ChatHub : Hub
{
    private static readonly HashSet<string> OnlineUsers = new();
    private static readonly ConcurrentDictionary<string, string> ConnectionToUser = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> RoomUsers = new();

    public override Task OnConnectedAsync()
    {
        string? username = Context.GetHttpContext()?.Request.Query["username"];

        if (!string.IsNullOrEmpty(username))
        {
            OnlineUsers.Add(username);
            ConnectionToUser[Context.ConnectionId] = username;

            Clients.All.SendAsync("UpdateOnlineUsers", OnlineUsers.ToList());
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionToUser.TryRemove(Context.ConnectionId, out var username))
        {
            OnlineUsers.Remove(username);

            Clients.All.SendAsync("UpdateOnlineUsers", OnlineUsers.ToList());

            // Remove from rooms
            foreach (var room in RoomUsers.Keys)
            {
                if (RoomUsers[room].Contains(username))
                {
                    RoomUsers[room].Remove(username);
                    Clients.Group(room).SendAsync("UpdateOnlineUsersInRoom", RoomUsers[room].ToList());
                }
            }
        }

        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string username, string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);

        if (!RoomUsers.ContainsKey(room))
            RoomUsers[room] = new HashSet<string>();

        RoomUsers[room].Add(username);

        await Clients.Group(room).SendAsync("UpdateOnlineUsersInRoom", RoomUsers[room].ToList());

    }

    public async Task SendMessage(string message)
    {
        if (!ConnectionToUser.TryGetValue(Context.ConnectionId, out var username))
            return;

        // Get room by checking each room where this username is present
        var room = RoomUsers.FirstOrDefault(kvp => kvp.Value.Contains(username)).Key;

        if (room != null)
        {
            await Clients.Group(room)
                .SendAsync("ReceiveMessage", username, message);
        }
    }

    public async Task SendPrivateMessage(string to, string message)
    {
        if (!ConnectionToUser.TryGetValue(Context.ConnectionId, out var from))
            return;

        // Find connection of the target user
        var targetConnection = ConnectionToUser
            .Where(x => x.Value == to)
            .Select(x => x.Key)
            .FirstOrDefault();

        if (targetConnection != null)
        {
            await Clients.Client(targetConnection)
                .SendAsync("ReceivePrivateMessage", from, message);
        }
    }
}
