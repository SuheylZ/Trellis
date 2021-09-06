using System;

namespace Trellis.Communications
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id">Message identity</param>
    /// <param name="CreatedOn">creation date of message</param>
    /// <param name="Sender">service that sent message</param>
    /// <param name="messageType">Communication type (command, query, RPC event etc)</param>
    /// <param name="TypeName">name of the actual type that was serialized with assembly name</param>
    /// <param name="ReplyTo">name of the message to reply to</param>
    public record Metadata(Guid Id, DateTime CreatedOn, string Sender, MessageTypes MessageType, string TypeName, string SentFrom, string ReplyTo);
}

