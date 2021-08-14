using System;
using MessagePack;

namespace Communications.NATS
{
    
    [MessagePackObject]
    public sealed class Letter
    {
        [Key(0)] public string By { get; init; }
        [Key(1)] public CommunicationTypes Type { get; init; }
        [Key(2)] public object Message { get; init; }
        [Key(3)]  public DateTime CreatedOn { get; set; }
        
        
        public Letter(string By, CommunicationTypes Type, object Message)
        {
            this.By = By;
            this.Type = Type;
            this.Message = Message;
        }
        
      

        public Letter(DateTime createdOn, string @by, CommunicationTypes type, object message):this(@by, type, message)
        {
            CreatedOn = createdOn;
            By = @by;
            Type = type;
            Message = message;
        }
        
        [IgnoreMember] internal TimeSpan Duraction => DateTime.UtcNow.Subtract(CreatedOn);
    }
}