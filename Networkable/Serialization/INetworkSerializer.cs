using Networkable.Routing;

namespace Networkable.Serialization;

public interface INetworkSerializer
{
    byte[] ToBytes(object message);
    object? FromBytes(byte[] bytes, IRouter router);
}