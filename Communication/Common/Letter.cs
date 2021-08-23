using System;

namespace SCM.Framework.Communications.Common
{
    public sealed class Letter
    {
        public string By { get; init; }
        public CommunicationTypes Type { get; init; }
        public object Message { get; init; }
        public DateTime CreatedOn { get; set; }
     
     
        public Letter(string By, CommunicationTypes Type, object Message)
        {
            this.By = By;
            this.Type = Type;
            this.Message = Message;
        }
        public Letter(string @by, CommunicationTypes type, object message, DateTime createdOn):this(@by, type, message)
        {
            CreatedOn = createdOn;
            By = @by;
            Type = type;
            Message = message;
        }
        
    }
}