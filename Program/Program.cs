using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NATS_Testing.Serializers;
using NATS.Client;


namespace Program
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            // var servers = new[] {"127.0.0.1:4222"};
            // var subject = "foo.baz";
            //
            //
            // var NATS = new Communication(c =>
            //     {
            //         c.Servers = servers;
            //         c.AllowReconnect = true;
            //         c.MaxReconnect = Options.ReconnectForever;
            //         c.PingInterval = 2000;
            //     },
            //     NATS_Testing.Serializers.UTF8Json.Serializer, NATS_Testing.Serializers.UTF8Json.Deserializer);
            //
            //
            //
            // var request = NATS.CreateRequester(subject, TimeSpan.FromMinutes(10), out var dispose);
            //
            // var cts = new CancellationTokenSource();
            //
            //
            // Action<string> listening = (name) =>
            // {
            //     var fetch = NATS.CreateIterator(subject, "mygroup");
            //
            //     try
            //     {
            //         foreach (var (msg, reply) in fetch(cts.Token))
            //         {
            //             switch (msg)
            //             {
            //                 case MyMessage _:
            //                     reply($"{name}, got message");
            //                     break;
            //                 default:
            //                     reply($"{name}: You sent X");
            //                     break;
            //             }
            //         }
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //     }
            // };
            //
            // var l1 = Task.Run(()=>listening("l1"));
            // var l2 = Task.Run(()=>listening("l2"));
            //
            //
            //
            //
            // var message = new MyMessage();
            // using (dispose)
            // {
            //     var sw = new Stopwatch();
            //     for (int i = 0; i < 2; i++)
            //     {
            //         message.Name = $"Hello World - {i}";
            //
            //         try
            //         {
            //             var data = await request(message, CancellationToken.None);
            //         }
            //         catch (Exception e)
            //         {
            //             Console.Error.WriteLine(e);
            //         }
            //     }
            // }
            //
            //
            // cts.Cancel();
            // await Task.WhenAll(l1, l2);


            var o1 = new YourMessage(1, "ali");
            var o2 = new MyMessage {Id = 1, Name = "ali"};

            var b1 = Protobuff.Serializer(o2);
            var ox = Protobuff.Deserializer(o2.GetType(), b1);

            var b2 = Protobuff.Serializer(o1);
            var oy = Protobuff.Deserializer(o2.GetType(), b2);
        }
    }
}