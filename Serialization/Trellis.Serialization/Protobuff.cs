using System.IO;
using Trellis.Communications.Common;

namespace Trellis.Serialization
{
    public class Protobuff
    {
        public static Trellis.Communications.Common.Serializer Serializer
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