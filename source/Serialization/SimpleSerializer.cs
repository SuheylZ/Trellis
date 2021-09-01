using System;
using System.Text;
using System.Text.Json;

namespace Trellis.Serialization
{
    public class SimpleSerializer
    {
        public static Serializer Serializer
        {
            get
            {
                return (data) =>
                {
                    var str = JsonSerializer.Serialize(data);
                    return Encoding.UTF8.GetBytes(str);
                };
            }
        }

        public static Deserializer Deserializer
        {
            get
            {
                return (type, data) =>
                {
                    
                        var s1= Encoding.UTF8.GetString(data);
                        var r1 = JsonSerializer.Deserialize(s1, type);
                        
                        if (r1.GetType() == type)
                        {
                            return Convert.ChangeType(r1, type);
                             
                        }

                        return null;


                    
                };
            }
        }
    }
}