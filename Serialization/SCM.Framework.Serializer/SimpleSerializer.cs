using System;
using System.Text;
using Common;

namespace SCM.Framework.Serializer
{
    public class SimpleSerializer
    {
        public static Common.Serializer Serializer
        {
            get
            {
                return (data) =>
                {
                    var str = System.Text.Json.JsonSerializer.Serialize(data);
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
                        var r1 = System.Text.Json.JsonSerializer.Deserialize(s1, type);
                        
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