namespace Trellis.Communications.Common
{
    public enum CommunicationTypes
    {
        /// <summary>
        /// Single Receiver
        /// </summary>
        FireAndForget,

        /// <summary>
        /// Action asked to be done and acknowledged
        /// </summary>
        Command,

        /// <summary>
        /// Question and Answer
        /// </summary>
        Query,

        /// <summary>
        /// RPC but collects the response
        /// </summary>
        ScatterAndGather,

        /// <summary>
        /// Multiple receivers
        /// </summary>
        Event
    };
}