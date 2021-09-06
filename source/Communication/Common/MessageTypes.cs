namespace Trellis.Communications
{
    public enum MessageTypes: byte
    {
        /// <summary>
        /// Single Receiver or none - corresponds to fire and forget
        /// </summary>
        Message =10,

        /// <summary>
        /// Message is asking an action to be performed and acknowledged, an RPC but the response is always an ack
        /// </summary>
        Command = 20,

        /// <summary>
        /// A message which is a reply for the command sent. May contain some data or no data.
        /// </summary>
        Acknowledged = 21,
        
        /// <summary>
        /// Question and Answer - corresponds to RPC 
        /// </summary>
        Request = 22,
        
        /// <summary>
        /// A reply generated after receiving a request or command
        /// </summary>
        Response = 23,



        /// <summary>
        /// Multiple receivers
        /// </summary>
        Event = 30
    };
}