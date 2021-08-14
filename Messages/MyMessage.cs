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
    
    [Serializable]
    public class MyMessage
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}