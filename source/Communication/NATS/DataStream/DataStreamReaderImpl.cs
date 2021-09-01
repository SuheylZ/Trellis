using System;
using System.Collections.Generic;
using System.Threading;
using NATS.Client;
using NATS.Client.Internals;
using NATS.Client.JetStream;
using Trellis.Communications.DataStreams;
using Trellis.Serialization;
using Trellis.Utility;
using Deserializer = Trellis.Serialization.Deserializer;
using Serializer = Trellis.Serialization.Serializer;

namespace Trellis.Communications.NATS.DataStream
{
    public class DataStreamReaderImpl: IDataStreamReader
    {
        readonly Func<Lazy<IConnection>> _connector;
        readonly Serializer _serialize;
        readonly Deserializer _deserialize;
        readonly string _stream;
        readonly string _sender;
 
        

        public DataStreamReaderImpl(string stream, Action<Options> configureOptions, ISerDes sd, string sender)
        {
            sd.GetSerializers(out _serialize, out _deserialize);

            _stream = stream;
            _sender = sender;

            _connector = () =>
                new Lazy<IConnection>(() =>
                    {
                        var cf = new ConnectionFactory();
                        var options = ConnectionFactory.GetDefaultOptions();
                        configureOptions(options);
                        return cf.CreateConnection(options);
                    }
                );
        }
        
        
        public Func<CancellationToken, IAsyncEnumerable<(string tag, Func<Type, object> reverser, IDataStreamTransaction transactor)>> Compose()
        {




            return Iterate;

            IAsyncEnumerable<(string tag, Func<Type, object> reverser, IDataStreamTransaction transactor)> Iterate(CancellationToken token)
            {
                var cnn = _connector().Value;
                var manager = cnn.CreateJetStreamManagementContext();

                var t = Sandbox.Try(() => manager.GetStreamInfo(_stream), ex =>
                {
                    var builder = StreamConfiguration.Builder()
                        .WithName(_stream)
                        .AddSubjects($"{_stream}.*")
                        .WithStorageType(StorageType.File)
                        .WithDiscardPolicy(DiscardPolicy.Old)
                        .WithDuplicateWindow(Duration.OfDays(5))
                        .WithRetentionPolicy(RetentionPolicy.Limits);

                    var info = builder.Build();
                    return manager.AddStream(info);
                });

                var options = JetStreamOptions.Builder()
                    .WithRequestTimeout(Duration.OfHours(2))
                    .WithPublishNoAck(false)
                    .Build();

                var js = cnn.CreateJetStreamContext(options);
                js.



            }
        }

        public Func<CancellationToken, IAsyncEnumerable<(T message, IDataStreamTransaction transactor)>> Compose<T>(string tag)
        {
            throw new NotImplementedException();
        }


        static object GetStreamInfo(IConnection cnn, string stream)
        {
            var mgr = cnn.CreateJetStreamManagementContext();
            var info = mgr.GetStreamInfo(stream);
            
        }
    }
}