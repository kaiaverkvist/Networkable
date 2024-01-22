namespace Networkable.Transports;

public interface INetworkableListener
{
    void Start();
    void Stop();
    
    public void Poll()
    {
        //Empty default implementation
    }
}