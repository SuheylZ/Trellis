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
        /// <param name="headers">headers from NATS Msg</param>
        /// <returns>Metadata</returns>
        public static Metadata ToMetadata(this MsgHeader headers)
        {
            if (!Guid.TryParse(headers[HeaderNames.KMessageId], out var id))
                id = Guid.Empty;
            
            if (!DateTime.Now.TryParseIso8601String(headers[HeaderNames.KCreatedOn], out var date))
                date = DateTime.Now;

            if(!Enum.TryParse(typeof(CommunicationTypes), headers[HeaderNames.KCommunicationType], out var commType))
                commType = CommunicationTypes.FireAndForget;

            return new Metadata(id, date, headers[HeaderNames.KSentFrom], (CommunicationTypes)commType, headers[HeaderNames.KTypeName], headers[HeaderNames.KSubject]);
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
            header[HeaderNames.KSentFrom] = md.SentBy;
            header[HeaderNames.KCommunicationType] = md.Communication.ToString();
            header[HeaderNames.KTypeName] = md.TypeName;
            header[HeaderNames.KSubject] = string.Empty;

            return header;
        }
    }
}
