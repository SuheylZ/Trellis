using System.IO;
using SCM.Framework.Communications.Common;
using MessagePack;

namespace SCM.Framework.Serialization
{
    public class MessagePack
    {
        public static Communications.Common.Serializer Serializer
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