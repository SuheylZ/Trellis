using System;

namespace Trellis.Communications
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
    }
}

