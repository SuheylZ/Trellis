using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trellis.Communications.Common;
using Deserializer = Trellis.Serialization.Deserializer;

namespace Trellis.Communications.NATS
{
    public interface ICommunication
    {
        /// <summary>
        /// Creates a publisher with a disposer. Publisher is a simple function bound to a particular subject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="disposer">a function to cleanup the things later on. uses IDisposable</param>
        /// <returns></returns>
        Func<object, ValueTask> CreatePublisher(string subject, out IDisposable disposer);

        /// <summary>
        /// Creates an async subscription that gets called automatically when a message is received.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        Func<CancellationToken, Task> CreateAutoListener<TRequest>(string subject, string group, Action<Metadata, TRequest, Action<object>> handler);

        /// <summary>
        /// Creates a Requester a Request function that takes a message and returns a reply
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="timeoutDuration">duration after which the timeout exception occurs</param>
        /// <param name="disposer"></param>
        /// <returns>the message that should be returned</returns>
        Func<object, CancellationToken, Task<TReply>> CreateRequester<TReply>(string subject, TimeSpan timeoutDuration, out Disposable disposer);

        /// <summary>
        /// Creates an iterator for listening to the incomming messages on a topic.
        /// </summary>
        /// <param name="subject">name of the queue to listen to</param>
        /// <param name="group">group name</param>
        /// <typeparam name="TMessage">type of the message that will be received on this queue</typeparam>
        /// <returns></returns>
        Func<CancellationToken, IEnumerable<(Metadata header, Func<Type, object> unpacker, Action<object> reply)>> CreateListeningIterator(string subject, string group);

        // /// <summary>
        // /// Creates a generic iterator for listening to the incomming messages on a topic.
        // /// Messages are delivered in NATS core format.
        // /// </summary>
        // /// <param name="subject">name of the queue to listen to</param>
        // /// <param name="group">group name</param>
        // /// <typeparam name="TMessage">type of the message that will be received on this queue</typeparam>
        // /// <returns></returns>
        // Func<CancellationToken, IEnumerable<(Metadata header, Func<Type, object> unpacker, Action<object> reply)>> CreateGenericListeningIterator(string subject, string group);
    }
}