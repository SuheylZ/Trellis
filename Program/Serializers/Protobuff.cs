using System.IO;
using Common;

namespace NATS_Testing.Serializers
{
    public class Protobuff
    {
        public static Serializer Serializer
        {
            get
            {
                return (data) =>
                {
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, data);
                        ms.Close();
                        return ms.ToArray();
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
                        var obj = ProtoBuf.Serializer.Deserialize(type, ms);
                        ms.Close();
                        
                        return obj;
                    }
                };
            }
        }

    }
}