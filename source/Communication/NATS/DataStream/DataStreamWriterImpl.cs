using System;
using System.Threading.Tasks;
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
    public class DataStreamWriterImpl : IDataStreamWriter
    {
        readonly Func<Lazy<IConnection>> _connector;
        readonly Serializer _serialize;
        readonly Deserializer _deserialize;
        readonly string _stream;
        readonly string _sender;
        

        public DataStreamWriterImpl(string stream, Action<Options> configureOptions, ISerDes sd, string sender)
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

        static StreamInfo CreateStreamIfNoExists(IConnection cnn, string streamName, Duration duration)
        {
            var manager = cnn.CreateJetStreamManagementContext();
                
            var t = Sandbox.Try(() => manager.GetStreamInfo(streamName), ex =>
            {
                var builder = StreamConfiguration.Builder()
                    .WithName(streamName)
                    .AddSubjects($"{streamName}.*")
                    .WithStorageType(StorageType.File)
                    .WithDiscardPolicy(DiscardPolicy.Old)
                    .WithDuplicateWindow(duration)
                    .WithRetentionPolicy(RetentionPolicy.Limits);

                var info = builder.Build();
                return manager.AddStream(info);
            });

            return t;
        }
        
        public Func<string, object, Task<bool>> Compose()
        {
            var cnn = _connector().Value;
            var options = JetStreamOptions.Builder()
                .WithRequestTimeout(Duration.OfHours(2))
                .WithPublishNoAck(false)
                .Build();

            _ = CreateStreamIfNoExists(cnn, _stream, Duration.OfHours(2));
            
            var js = cnn.CreateJetStreamContext(options);
            
            return async (topic, data) =>
            {
                var bytes = _serialize(data);
                Msg msg = CreateMsg(topic, _sender, bytes);
                var ret = await js.PublishAsync(msg);
                return (ret.Stream == _stream && !ret.Duplicate);
            };
        }

        static Msg CreateMsg(string topic, string sender, byte[] bytes)
        {
            var msg = new Msg
            {
                Data = bytes,
                Subject = $"{_stream}.{topic}",
                Header = CreateHeader()
            };

            return msg;
            
            MsgHeader CreateHeader(DateTime createdOn, string subject, string sender, ulong id, )
        }

        public Func<T, Task> Compose<T>(string topic)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}