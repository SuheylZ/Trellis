using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using SCM.Framework.Communications.Common;
using Deserializer = SCM.Framework.Communications.Common.Deserializer;
using Serializer = SCM.Framework.Communications.Common.Serializer;

namespace SCM.Framework.Communications.NATS
{
    public class Communication
    {
        readonly Action<Options> _configureOptions;
        readonly Action<Exception> _errorHandler;

        readonly Serializer _serialize;
        readonly Deserializer _deserialize;

        readonly string _sender;
        readonly Func<Lazy<IConnection>> _establishConnection;


        public Communication(Action<Options> configureAction, ISerDes sd) : this(configureAction, sd, string.Empty, ex => { }) { }

        public Communication(Action<Options> configureOptions, ISerDes sd, string sender, Action<Exception> errorHandler)
        {
            _configureOptions = configureOptions;
            sd.GetSerializers(out _serialize, out _deserialize);

            _sender = sender;
            _errorHandler = errorHandler;

            _establishConnection = () =>
                new Lazy<IConnection>(() =>
                    {
                        var cf = new ConnectionFactory();
                        var options = ConnectionFactory.GetDefaultOptions();
                        _configureOptions(options);
                        return cf.CreateConnection(options);
                    }
                );
        }

        Msg CreateMessage<T>(string subject, T data, CommunicationTypes type)
        {
            var id = Guid.NewGuid().ToString();
            var binary = _serialize(data);
            var md = new Metadata(_sender, type, data);

            return new Msg
            {
                Data = binary,
                Header = md,
                Subject = subject,
                Reply = id
            };
        }

        bool TryGetMessage(ISyncSubscription subscription, out Msg message, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    message = null;
                    return false;
                }

                message = subscription.NextMessage(500);
                return true;
            }
            catch (NATSTimeoutException _)
            {
                message = null;
                return false;
            }
            catch (Exception ex)
            {
                message = new Msg
                {
                    Data = _serialize(ex),
                    Reply = string.Empty,
                    Subject = subscription.Subject
                };
                return false;
            }
        }
        
        
        
        
        public bool CommunicationIsWorking()
        {
            var result = false;

            var t0 = _establishConnection();
            using (var cnn = t0.Value)
            {
                result = cnn.State == ConnState.CONNECTED;


                cnn.Close();
            }

            return result;
        }

        /// <summary>
        /// Creates a publisher with a disposer. Publisher is a simple function bound to a particular subject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="disposer">a function to cleanup the things later on. uses IDisposable</param>
        /// <returns></returns>
        public Func<object, ValueTask> CreatePublisher(string subject, out IDisposable disposer)
        {
            var cnn = _establishConnection();

            disposer = new Disposable(() =>
            {
                if (cnn.IsValueCreated)
                {
                    cnn.Value.Close();
                    cnn.Value.Dispose();
                }
            });

            return (data) =>
            {
                try
                {
                    var message = CreateMessage(subject, data, CommunicationTypes.FireAndForget);
                    cnn.Value.Publish(message);
                    return ValueTask.CompletedTask;
                }
                catch (Exception e)
                {
                    return ValueTask.FromException(e);
                }
            };
        }


        /// <summary>
        /// Creates an async subscription that gets called automatically when a message is received.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Func<CancellationToken, Task> CreateAutoListener<TRequest>(string subject, string group, Action<Metadata, TRequest, Action<object>> handler)
        {
            var cnn = _establishConnection();

            var alreadyStarted = false;
            return (token) =>
            {
                if (alreadyStarted)
                    throw new Exception("Cannot start listening again!");

                var subscription = cnn.Value.SubscribeAsync(subject, group, (_, args) =>
                {
                    try
                    {
                        var t1 = _deserialize(typeof(TRequest), args.Message.Data);

                        if (t1 is TRequest data)
                        {
                            Metadata md = args.Message.Header;
                            Action<object> reply = obj => args.Message.Respond(_serialize(obj));

                            handler(md, data, reply);
                        }
                        else
                            throw new InvalidCastException($"expected {typeof(TRequest).FullName} but received {t1.GetType().FullName}");
                    }
                    catch (Exception e)
                    {
                        _errorHandler(e);
                    }
                });

                subscription.Start();
                alreadyStarted = true;

                var tcs = new TaskCompletionSource();
                token.Register(() =>
                {
                    if (cnn.IsValueCreated)
                    {
                        subscription.DrainAsync().GetAwaiter().GetResult();
                        subscription.Unsubscribe();
                        subscription.Dispose();

                        cnn.Value.Close();
                        cnn.Value.Dispose();
                    }

                    tcs.SetResult();
                    alreadyStarted = false;
                });

                return tcs.Task;
            };
        }


        /// <summary>
        /// Creates a Requester a Request function that takes a message and returns a reply
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="timeoutDuration">duration after which the timeout exception occurs</param>
        /// <param name="disposer"></param>
        /// <returns>the message that should be returned</returns>
        public Func<object, CancellationToken, Task<TReply>> CreateRequester<TReply>(string subject, TimeSpan timeoutDuration, out Disposable disposer)
        {
            var cnn = _establishConnection();

            if (!int.TryParse($"{timeoutDuration.TotalMilliseconds}", out var milliseconds))
                milliseconds = int.MaxValue;

            disposer = new Disposable(() =>
            {
                if (cnn.IsValueCreated)
                {
                    cnn.Value.Close();
                    cnn.Value.Dispose();
                }
            });


            return async (data, token) =>
            {
                var msg = CreateMessage(subject, data, CommunicationTypes.Query);
                var t = await cnn.Value.RequestAsync(msg, milliseconds, token).ConfigureAwait(false);
                var ans = _deserialize(typeof(TReply), t.Data);

                if (ans is TReply l1)
                    return l1;
                else
                    throw new InvalidCastException($"Not of type {typeof(TReply).FullName}");
            };
        }


        /// <summary>
        /// Creates an iterator for listening to the incomming messages on a topic.
        /// </summary>
        /// <param name="subject">name of the queue to listen to</param>
        /// <param name="group">group name</param>
        /// <typeparam name="TMessage">type of the message that will be received on this queue</typeparam>
        /// <returns></returns>
        public Func<CancellationToken, IEnumerable<(Metadata header, TMessage message, Action<object> reply)>> CreateListeningIterator<TMessage>(string subject, string group)
        {
            return Fetcher;

            IEnumerable<(Metadata md, TMessage message, Action<object> reply)> Fetcher(CancellationToken token)
            {
                var cnn = _establishConnection().Value;
                using (cnn)
                {
                    using (var subscription = cnn.SubscribeSync(subject, group))
                    {
                        while (!token.IsCancellationRequested && subscription.IsValid)
                        {
                            if (TryGetMessage(subscription, out var msg, token))
                            {
                                Metadata md = msg.Header;
                                var message = _deserialize(typeof(TMessage), msg.Data);
                                if (message is TMessage data)
                                    yield return (md, data, ret => msg.Respond(_serialize(ret)));
                            }
                        }
                        subscription.Unsubscribe();
                    }
                    cnn.Close();
                }
            }
        }

      

        /// <summary>
        /// Creates a generic iterator for listening to the incomming messages on a topic.
        /// Messages are delivered in NATS core format.
        /// </summary>
        /// <param name="subject">name of the queue to listen to</param>
        /// <param name="group">group name</param>
        /// <typeparam name="TMessage">type of the message that will be received on this queue</typeparam>
        /// <returns></returns>
        public Func<CancellationToken, IEnumerable<(Metadata header, Msg payload, Action<object> reply)>> CreateGenericListeningIterator(string subject, string group)
        {
            return Fetcher;

            IEnumerable<(Metadata header, Msg payload, Action<object> reply)> Fetcher(CancellationToken token)
            {
                var cnn = _establishConnection().Value;
                using (cnn)
                {
                    using (var subscription = cnn.SubscribeSync(subject, group))
                    {
                        while (!token.IsCancellationRequested && subscription.IsValid)
                        {
                            if (TryGetMessage(subscription, out var msg, token))
                            {
                                Metadata md = msg.Header;
                                yield return (header: md, payload: msg, ret => msg.Respond(_serialize(ret)));
                            }
                        }
                        subscription.Unsubscribe();
                    }
                    cnn.Close();
                }
            }
        }
    }
}