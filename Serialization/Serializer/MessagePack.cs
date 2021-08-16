using System.IO;
using System.Text;
using Common;
using MessagePack;

namespace NATS_Testing.Serializers
{
    public class MessagePack
    {
        public static Common.Serializer Serializer
        {
            get
            {
                return (data) =>
                {
                    using (var ms = new MemoryStream())
                    {
                        MessagePackSerializer.Serialize(data.GetType(), ms, data);
                        ms.Close();
                        return ms.GetBuffer();
                    }
                };
            }
        }

        public static Deserializer Deserializer
        {
            get
            {
                return (type, data) =>
                {
                    using (var ms = new MemoryStream(data))
                    {
                        var ret1 = MessagePackSerializer.Deserialize(type, ms);
                        ms.Close();
                        return ret1;
                    }
                };
            }
        }

        
    }
}