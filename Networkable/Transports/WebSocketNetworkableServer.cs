using Fleck;
using LiteNetLib.Utils;
using Networkable.Builtin;
using Networkable.Extensions;
using Networkable.Routing;
using Networkable.Serialization;

namespace Networkable.Transports;

public class WebSocketNetworkableServer : INetworkableListener
{
    private readonly Router<IWebSocketConnection> _router = new();
    private IWebSocketServer _webSocketServer;
    private readonly INetworkSerializer _serializer;
    private readonly NetDataWriter _writer = new();
    public readonly List<IWebSocketConnection> Sockets = new();

    public Router<IWebSocketConnection> Router => _router;
    
    public WebSocketNetworkableServer(
        INetworkSerializer serializer,
        IWebSocketServer webSocketServer
    )
    {
        _serializer = serializer;
        _webSocketServer = webSocketServer;
    }

    private void OnConnect(IWebSocketConnection client)
    {
        Sockets.Add(client);
        _router.ManualTrigger(client, new ClientConnected());
    }

    private void OnDisconnect(IWebSocketConnection client, string? reason = null)
    {
        _router.ManualTrigger(client, new ClientDisconnected(reason));
        
        if (Sockets.Contains(client))
            Sockets.Remove(client);
    }

    private void ReceiveMessage(IWebSocketConnection client, byte[] received)
    {
        var instance = _serializer.FromBytes(received, _router);
        _router.Trigger(client, instance);
    }

    public void Start()
    {
        _webSocketServer.Start(socket =>
        {
            socket.OnOpen = () => OnConnect(socket);

            socket.OnClose = () => OnDisconnect(socket);
            
            socket.OnBinary = data => ReceiveMessage(socket, data);
        });
    }

    public void Stop() => _webSocketServer.Dispose();
    
    public void SendTo<T>(IWebSocketConnection peer, T message)
    {
        byte[] bytes = _serializer.ToBytes(message);
        peer.Send(bytes);
    }
    
    public void SendTo<T>(Guid id, T message)
    {
        var peer = Sockets.First(p => p.GetId() == id);
        SendTo(peer, message);
    }
}