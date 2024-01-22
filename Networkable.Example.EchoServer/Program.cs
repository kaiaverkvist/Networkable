
using System.Net.Sockets;
using Networkable.Builtin;
using Networkable.Serialization;
using Networkable.Transports;

Console.WriteLine("Hello, World!");

var server = new LiteNetworkableServer(new JsonNetworkSerializer(), "echo_server");
server.Router.Register<ClientConnected>((client, connected) =>
{
    Console.WriteLine($"{client}: Connected");
    server.SendTo(client, Guid.NewGuid());
});
server.Router.Register<ClientDisconnected>((client, disconnected) =>
{
    Console.WriteLine($"{client}: Disconnected: {disconnected.Reason}");
});

server.Start();

while (!Console.KeyAvailable)
{
    server.Poll();
}
