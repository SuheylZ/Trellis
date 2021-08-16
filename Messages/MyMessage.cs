using System.Collections.Generic;
using ProtoBuf;

namespace Program
{

    public interface IRestoreFromLetter
    {
        object Restore(IDictionary<object, object> map);
    }
    
    
    //[Serializable]
    [ProtoContract(Name = "MyMessage", SkipConstructor = false)]
    public class MyMessage
    {
        [ProtoMember(1, Name="Id")] 
        public int Id { get; set; }
        [ProtoMember(2, Name = "Name")] 
        public string Name { get; set; }
    }


    public record YourMessage(int Age, string Name);
    
    
}