using System;
using System.Text;
using Newtonsoft.Json;
using Trellis.Utility;

namespace Trellis.Serialization
{
    public class NewtonSoft
    {
        public static byte[] Serialize(Type type, object obj)
        {
            // step 1: Convert to a JSON string
            var json = JsonConvert.SerializeObject(obj, type, new JsonSerializerSettings{});
            

            // step 2: convert stri5ng to bytes
            var binary = Encoding.UTF8.GetBytes(json);

            return binary;

        }

        public static object Deserialize(Type type, byte[] binary)
        {
            var finalObject = new object();
            // Binary to string
            var json = Encoding.UTF8.GetString(binary, 0, binary.Length);

            // String to JSON
           
            
            Sandbox.Try(() =>
            {
                var instance = JsonConvert.DeserializeObject(json, type);
                finalObject = instance;
            }, _ => finalObject = null);
            
            if(finalObject ==null)
                Sandbox.Try(()=>
                {
                    var instance = JsonConvert.DeserializeObject(json);
                    finalObject = Convert.ChangeType(instance, type);
                }, ex => 
                {
                    finalObject = JsonConvert.DeserializeObject(json);
                });
            
            return finalObject;
        }
        
    }
}