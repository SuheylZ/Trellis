using System;
using NATS.Client;
using Trellis.Utility;

namespace Trellis.Communications.NATS
{
    public static class Extension
    {
        /// <summary>
        /// Converts the NATS headers to Metadata
        /// </summary>
        /// <param name="msg">NATS Msg</param>
        /// <returns>Metadata</returns>
        public static Metadata ToMetadata(this Msg msg)
        {
            var headers = msg.Header;
            
            if (!Guid.TryParse(headers[HeaderNames.KMessageId], out var id))
                id = Guid.Empty;
            
            if (!DateTime.Now.TryParseIso8601String(headers[HeaderNames.KCreatedOn], out var date))
                date = DateTime.Now;

            if(!Enum.TryParse(typeof(MessageTypes), headers[HeaderNames.KCommunicationType], out var commType))
                commType = MessageTypes.Message;

            return new Metadata(id, date, headers[HeaderNames.KSentFrom], (MessageTypes)commType, headers[HeaderNames.KTypeName], msg.Subject, msg.Reply);
        }

        /// <summary>
        /// Converts Metadata to NATS headers
        /// </summary>
        /// <param name="md">metadata to be used</param>
        /// <returns>MsgHeader</returns>
        public static MsgHeader ToMsgHeader(this Metadata md)
        {
            var header = new MsgHeader();
            
            header[HeaderNames.KMessageId] = md.Id.ToString();
            header[HeaderNames.KCreatedOn] = md.CreatedOn.ToIso8601String();
            header[HeaderNames.KSentFrom] = md.Sender;
            header[HeaderNames.KCommunicationType] = md.TypeName;
            header[HeaderNames.KTypeName] = md.TypeName;
            header[HeaderNames.KSubject] = md.SentFrom;
            header[HeaderNames.KReplyTo] = md.ReplyTo;

            return header;
        }
    }
}
