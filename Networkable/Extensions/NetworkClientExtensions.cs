using Fleck;
using LiteNetLib;

namespace Networkable.Extensions;

public static class NetworkClientExtensions
{
    public static Guid GetId(this IWebSocketConnection socket) => socket.ConnectionInfo.Id;
    
    public static Guid GetId(this NetPeer socket)
    {
        if (socket.Tag != null)
            return (Guid)socket.Tag;

        socket.Tag = System.Guid.NewGuid();
        return (Guid)socket.Tag;
    }
}