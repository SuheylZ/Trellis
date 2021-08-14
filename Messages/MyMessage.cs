using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace Program
{

    public interface IRestoreFromLetter
    {
        object Restore(IDictionary<object, object> map);
    }
    
    [MessagePackObject()]
    [Serializable]
    public class MyMessage
    {
        [Key(0)] public int Id { get; set; }
        [Key(1)] public string Name { get; set; }
    }
}