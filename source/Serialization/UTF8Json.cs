using System.IO;
using Utf8Json;

namespace Trellis.Serialization
{
    public class Utf8Json
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
                        var ret1 = JsonSerializer.Deserialize<object>(ms);
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