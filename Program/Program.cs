using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SCM.Framework.Communications.NATS;
using NATS.Client;
using SCM.Framework.Communications.Common;
using SCM.Framework.Serialization;


namespace Program
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var servers = new[] {"127.0.0.1:4222"};
            var subject = "foo.baz";

            var serdes = new SerDes((obj) => NewtonSoft.Serialize(obj.GetType(), obj), (typ, x) => NewtonSoft.Deserialize(typ, x));
            
            var NATS = new NATSCommunicationImpl(c =>
                {
                    c.Servers = servers;
                    c.AllowReconnect = true;
                    c.MaxReconnect = Options.ReconnectForever;
                    c.PingInterval = 2000;
                }, serdes);
            
            
            
            
            
            var cts = new CancellationTokenSource();
           
            Action<string> listening = (name) =>
            {
                var listen = NATS.CreateGenericListeningIterator(subject, "mygroup");
            
                try
                {
                    foreach (var (headers, msg, reply) in listen(cts.Token))
                    {
                        switch (msg.Subject)
                        {
                            case "foo.baz":
                                if (DateTime.Now.Ticks % 2 == 0)
                                    reply((object) 234);
                                else
                                    reply(new {H = "helo"});
                                break;
                            
                            default:
                                reply($"{name}: You sent X");
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
            
            var l1 = Task.Run(()=>listening("l1"));
            var l2 = Task.Run(()=>listening("l2"));
            
            
            
            
            var message = new MyMessage();
            
            var request = NATS.CreateRequester<object>(subject, TimeSpan.FromMinutes(10), out var dispose);
            using (dispose)
            {
                var sw = new Stopwatch();
                for (int i = 0; i < 2000; i++)
                {
                    message.Name = $"Hello World - {i}";
            
                    try
                    {
                        var data = await request(message, CancellationToken.None);
                        Console.WriteLine($"{i}. {data}");
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e);
                    }
                }
            }


            try
            {
                cts.Cancel();
                await Task.WhenAll(l1, l2);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // var o1 = new YourMessage(1, "ali");
            // var o2 = new MyMessage {Id = 1, Name = "ali"};
            //
            // var b1 = Protobuff.SCM.Framework.Serialization(o2);
            // var ox = Protobuff.Deserializer(o2.GetType(), b1);
            //
            // var b2 = Protobuff.SCM.Framework.Serialization(o1);
            // var oy = Protobuff.Deserializer(o2.GetType(), b2);
        }
    }
}