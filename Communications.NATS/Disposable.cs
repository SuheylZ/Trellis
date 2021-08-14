using System;
using Program;

namespace Communications.NATS
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
    
    public sealed class Disposable : IDisposable
    {
        readonly Action _action;

        public Disposable(Action action) => _action = action ?? throw new ArgumentNullException(nameof(action));

        public void Dispose()
        {
            _action();
        }

        public static implicit operator Disposable(Action act)
        {
            return new Disposable(act);
        }
    }
}