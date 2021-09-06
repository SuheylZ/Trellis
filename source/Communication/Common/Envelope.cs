using System;
using Trellis.Serialization;

namespace Trellis.Communications
{
    public record Envelope(Metadata Headers, byte[] Data);
    


    public class EnvelopeCreator
    {
        readonly Serializer _serialize;
        
        Envelope NewRequest(string sender, string subject, object message)
        {
            var id = Guid.NewGuid();
            var sentOn = DateTime.UtcNow;
            var type = message.GetType().FullName;
            var mtype = MessageTypes.Request;
            
            // serialize data
            var binary = _serialize(message);

            var headers = new Metadata(id, sentOn, sender, mtype, type, subject, id.ToString());
            return new Envelope(headers, binary);
        }
        
        Envelope NewReply(Metadata headers, string sender, object message)
        {
            var id = Guid.NewGuid();
            var sentOn = DateTime.UtcNow;
            var type = message.GetType().FullName;
            var mtype = MessageTypes.Request;
            
            // serialize data
            var binary = _serialize(message);

            var headers2 = new Metadata(id, sentOn, sender, MessageTypes.Response, type, headers.ReplyTo, string.Empty);
            return new Envelope(headers2, binary);
        }
    }
}