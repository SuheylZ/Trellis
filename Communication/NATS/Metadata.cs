using System;
using SCM.Framework.Communications.Common;
using NATS.Client;


namespace SCM.Framework.Communications.NATS
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id">Message identity</param>
    /// <param name="CreatedOn">creation date of message</param>
    /// <param name="SentBy">service that sent message</param>
    /// <param name="Communication">Communication type (command, query, RPC event etc)</param>
    /// <param name="TypeName">name of the actual type that was serialized with assembly name</param>
    public record Metadata(Guid Id, DateTime CreatedOn, string SentBy, CommunicationTypes Communication, string TypeName, string subject)
    {
        public Metadata(string sentBy, CommunicationTypes communication, object data, string subject = ""): this(Guid.NewGuid(), DateTime.UtcNow, sentBy, communication, data.GetType().FullName, subject)
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

            return new Metadata(id, date, headers[HeaderNames.KSentFrom], (CommunicationTypes)commType, headers[HeaderNames.KTypeName], headers[HeaderNames.KSubject]);
        }

        public static implicit operator MsgHeader(Metadata md)
        {
            var header = new MsgHeader();
            
            header[HeaderNames.KMessageId] = md.Id.ToString();
            header[HeaderNames.KCreatedOn] = md.CreatedOn.ToIso8601String();
            header[HeaderNames.KSentFrom] = md.SentBy;
            header[HeaderNames.KCommunicationType] = md.Communication.ToString();
            header[HeaderNames.KTypeName] = md.TypeName;
            header[HeaderNames.KSubject] = string.Empty;

            return header;
        }
    }
}



 namespace SCM.Framework.Communications.NATS
 { }