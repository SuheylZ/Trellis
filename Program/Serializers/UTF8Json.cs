using System;
using System.IO;
using Common;
using Communications.NATS;
using Utf8Json;

namespace NATS_Testing.Serializers
{
    public class UTF8Json
    {
        public static Serializer Serializer
        {
            get
            {
                return (data) =>
                {
                    var str = JsonSerializer.Serialize(data);
                    return str;
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
                        var ret1 = JsonSerializer.Deserialize<Letter>(ms);
                        ms.Close();
                        if (ret1.GetType() == type)
                        {
                            return ret1;
                        }

                        return null;


                    }
                };
            }
        }

        
    }
}