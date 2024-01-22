using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Networkable.Builtin;
using Networkable.Extensions;
using Networkable.Routing;
using Networkable.Serialization;

namespace Networkable.Transports;

public class LiteNetworkableServer : INetworkableListener
{
    private readonly Router<NetPeer> _router = new();
    private readonly INetworkSerializer _serializer;
    private readonly EventBasedNetListener _listener = new();
    private readonly NetManager _manager;
    
    // Configuration options
    private readonly string _versionKey;
    private readonly uint _maxConnections;
    private readonly uint _pollRateMs;
    private readonly uint _port;
    private readonly IPAddress? _addressIPv4;
    private readonly IPAddress? _addressIPv6;
    private readonly NetDataWriter _writer = new();

    // Fired when a socket error occurs
    public event NetworkError? OnNetworkError;
    public delegate void NetworkError(IPEndPoint endpoint, SocketError socketError);

    public List<NetPeer> Peers => _manager.ConnectedPeerList;
    
    public Router<NetPeer> Router => _router;

    public LiteNetworkableServer(
        INetworkSerializer serializer,
        string versionKey,
        uint maxConnections = 1000,
        uint pollRateMs = 10,
        uint port = 3022,
        IPAddress? addressIPv4 = null,
        IPAddress? addressIPv6 = null
    )
    {
        _serializer = serializer;
        _manager = new NetManager(_listener);
        
        _versionKey = versionKey;
        _maxConnections = maxConnections;
        _pollRateMs = pollRateMs;
        _port = port;
        _addressIPv4 = addressIPv4;
        _addressIPv6 = addressIPv6;
        
        _listener.ConnectionRequestEvent += ListenerOnConnectionRequestEvent;
        _listener.PeerConnectedEvent += ListenerOnPeerConnectedEvent;
        _listener.PeerDisconnectedEvent += ListenerOnPeerDisconnectedEvent;
        _listener.NetworkReceiveEvent += ListenerOnNetworkReceiveEvent;
        _listener.NetworkErrorEvent += ListenerOnNetworkErrorEvent;
    }
    
    public void Start() => _manager.Start(_addressIPv4, _addressIPv6, (int)_port);
    public void Stop() => _manager.Stop(true);

    public void Poll()
    {
        _manager.PollEvents();
        Thread.Sleep((int)_pollRateMs);
    }

    private void ListenerOnNetworkErrorEvent(IPEndPoint endpoint, SocketError socketerror)
        => OnNetworkError?.Invoke(endpoint, socketerror);

    private void ListenerOnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod)
    {
        byte[] bytes = reader.GetRemainingBytes();

        var instance = _serializer.FromBytes(bytes, _router);
        _router.Trigger(peer, instance);
    }

    private void ListenerOnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        var message = new ClientDisconnected(disconnectInfo.Reason.ToString());
        _router.ManualTrigger(peer, message);
    }

    private void ListenerOnPeerConnectedEvent(NetPeer peer)
    {
        var message = new ClientConnected();
        _router.ManualTrigger(peer, message);
    }

    private void ListenerOnConnectionRequestEvent(ConnectionRequest request)
    {
        if (_manager.ConnectedPeersCount < _maxConnections)
            request.AcceptIfKey(_versionKey);
        else
            request.Reject();
    }

    public void Connect(string host, uint port)
        => _manager.Connect(host, (int)port, _versionKey);

    public void SendTo<T>(NetPeer peer, T message, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableUnordered)
    {
        _writer.Reset();

        byte[] bytes = _serializer.ToBytes(message);
        
        _writer.Put(bytes);
        peer.Send(_writer, deliveryMethod);
    }
    
    public void SendTo<T>(Guid id, T message)
    {
        var peer = Peers.First(p => p.GetId() == id);
        SendTo(peer, message);
    }
}