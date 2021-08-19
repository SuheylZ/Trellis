using System.IO;
using Common;
using Utf8Json;

namespace SCM.Framework.Serializer
{
    public class Utf8Json
    {
        public static Common.Serializer Serializer
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