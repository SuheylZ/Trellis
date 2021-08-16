using System;
using Common;
using NATS.Client;


namespace Communications.NATS
{

    public static class HeaderNames
    {
        public const string KCreatedOn = "createdOn";
        public const string KSentFrom = "sentFrom";
        public const string KCommunicationType = "communicationType";
        public const string KTypeName = "typeName";
        public const string KMessageId = "messageId";
    }
    
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id">Message identity</param>
    /// <param name="CreatedOn">creation date of message</param>
    /// <param name="SentBy">service that sent message</param>
    /// <param name="Communication">Communication type (command, query, RPC event etc)</param>
    /// <param name="TypeName">name of the actual type that was serialized with assembly name</param>
    public record Metadata(Guid Id, DateTime CreatedOn, string SentBy, CommunicationTypes Communication, string TypeName)
    {
        public Metadata(string sentBy, CommunicationTypes communication, object data): this(Guid.NewGuid(), DateTime.UtcNow, sentBy, communication, data.GetType().FullName)
        {
        }

        public static implicit operator Metadata(MsgHeader headers)
        {
            if (!Guid.TryParse(headers[HeaderNames.KMessageId], out var id))
                id = Guid.Empty;
            
            if (!DateTime.Now.TryParseIso8601String(headers[HeaderNames.KCreatedOn], out var date))
                date = DateTime.Now;

            if(!Enum.TryParse(typeof(CommunicationTypes), headers[HeaderNames.KCommunicationType], out var commType))
                commType = CommunicationTypes.FireAndForget;

            return new Metadata(id, date, headers[HeaderNames.KSentFrom], (CommunicationTypes)commType, headers[HeaderNames.KTypeName]);
        }

        public static implicit operator MsgHeader(Metadata md)
        {
            var header = new MsgHeader();
            header[HeaderNames.KMessageId] = md.Id.ToString();
            header[HeaderNames.KCreatedOn] = md.CreatedOn.ToIso8601String();
            header[HeaderNames.KSentFrom] = md.SentBy;
            header[HeaderNames.KCommunicationType] = md.Communication.ToString();
            header[HeaderNames.KTypeName] = md.TypeName;
            
            return header;
        }
    }
}



// namespace Communications.NATS
// {
// public sealed class Letter
// {
//     public string By { get; init; }
//     public CommunicationTypes Type { get; init; }
//     public object Message { get; init; }
//     public DateTime CreatedOn { get; set; }
//     
//     
//     public Letter(string By, CommunicationTypes Type, object Message)
//     {
//         this.By = By;
//         this.Type = Type;
//         this.Message = Message;
//     }
//     
//   
//     public Letter(string @by, CommunicationTypes type, object message, DateTime createdOn):this(@by, type, message)
//     {
//         CreatedOn = createdOn;
//         By = @by;
//         Type = type;
//         Message = message;
//     }
//     
//     [IgnoreMember] internal TimeSpan Duraction => DateTime.UtcNow.Subtract(CreatedOn);
// }
//}