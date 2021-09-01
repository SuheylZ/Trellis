using System;
using System.IO;
using MessagePack;

namespace Trellis.Serialization
{
    /// <summary>
    /// Serializer signature
    /// </summary>
    public delegate byte[] Serializer(object message);
    /// <summary>
    /// Deserializer signature
    /// </summary>
    public delegate object Deserializer(Type type, byte[] data);
    
    
    
    public class MessagePack
    {
        public static Serializer Serializer
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