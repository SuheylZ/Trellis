using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common;
using NATS.Client;
using Deserializer = Common.Deserializer;
using Serializer = Common.Serializer;


namespace Communications.NATS
{
    public class Communication
    {
        // const string KCreatedOn = "createdOn";
        // const string KSentFrom = "sentFrom";
        // const string KCommunicationType = "communicationType";
        // const string KTypeName = "typeName";
        // const string KMessageId = "messageId";


        readonly Action<Options> _configureOptions;
        readonly Action<Exception> _errorHandler;
        
        readonly Serializer _serialize;
        readonly Deserializer  _deserialize;

        readonly string _sender;
 
        public Communication(Action<Options> configureAction, Common.Serializer serialize, Common.Deserializer deserialize) : this(configureAction, serialize, deserialize, string.Empty, ex => { })
        {
        }
        public Communication(Action<Options> configureOptions, Serializer serialize, Deserializer deserialize, string sender, Action<Exception> errorHandler)
        {
            _configureOptions = configureOptions;
            _serialize = serialize;
            _deserialize = deserialize;
            _sender = sender;
            _errorHandler = errorHandler;
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
            
            // MsgHeader CreateMessageHeader<T>(CommunicationTypes type, string id)
            // {
            //     var header = new MsgHeader();
            //     header[KMessageId] = id;
            //     header[KCreatedOn] = DateTime.UtcNow.ToIso8601String();
            //     header[KSentFrom] = _sender;
            //     header[KCommunicationType] = type.ToString();
            //     header[KTypeName] = typeof(T).FullName;
            //
            //     return header;
            // }
        }
        
        
        
        /// <summary>
        /// Creates a publisher with a disposer. Publisher is a simple function bound to a particular subject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="disposer">a function to cleanup the things later on. uses IDisposable</param>
        /// <returns></returns>
        public Func<object, ValueTask> CreatePublisher(string subject, out IDisposable disposer)
        {
            var cf = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();
            _configureOptions(options);

            var cnn = cf.CreateConnection(options);

            disposer = new Disposable(() =>
            {
                cnn.Close();
                cnn.Dispose();
            });

            return (data) =>
            {
                try
                {
                    var message = CreateMessage(subject, data, CommunicationTypes.FireAndForget);
                    cnn.Publish(message);
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
        public Func<CancellationToken, Task> CreateSubscription<T>(string subject, string group, Action<Metadata, object, Action<object>> handler)
        {
            var cf = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();
            _configureOptions(options);


            var cnn = cf.CreateConnection(options);
            var subscription = cnn.SubscribeAsync(subject, group, (_, args) =>
            {
                try
                {
                    var t1 = _deserialize(typeof(T), args.Message.Data);
                    
                    if (t1 is T data)
                    {
                        Metadata md = args.Message.Header;
                        Action<object> reply = obj => args.Message.Respond(_serialize(obj)); 

                        handler(md, data, reply);
                    }
                    else 
                        throw new InvalidCastException($"expected {typeof(T).FullName} but received {t1.GetType().FullName}");
                    
                }
                catch (Exception e)
                {
                    _errorHandler(e);
                }
            });

            
            
            
            var alreadyStarted = false;
            return (token) =>
            {
                if (alreadyStarted)
                    throw new Exception("Cannot start listening again!");

                subscription.Start();
                alreadyStarted = true;
                
                var tcs = new TaskCompletionSource();
                token.Register(() =>
                {
                    subscription.DrainAsync();
                    subscription.Unsubscribe();
                    subscription.Dispose();

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
            var cf = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();

            _configureOptions(options);

            var cnn = cf.CreateConnection(options);
            
            if (!int.TryParse($"{timeoutDuration.TotalMilliseconds}", out var milliseconds))
                milliseconds = int.MaxValue;
            
            disposer = new Disposable(() =>
            {
                cnn.Close();
                cnn.Dispose();
            });

            
            return async (data, token) =>
            {
                try
                {
                    var msg = CreateMessage(subject, data, CommunicationTypes.Query);
                    var t = await cnn.RequestAsync(msg, milliseconds, token).ConfigureAwait(false);
                    var ans = _deserialize(typeof(TReply), t.Data);

                    if (ans is TReply l1)
                        return l1;

                    throw new Exception("Not a letter");
                }
                catch (Exception e)
                {
                    throw;
                }
            };
        }

        

        public Func<CancellationToken, IEnumerable<(Metadata header, TMessage message, Action<object> reply)>> CreateResponder<TMessage>(string subject, string group)
        {
            var cf = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();
            _configureOptions(options);

            var cnn = cf.CreateConnection(options);
            var sw = new Stopwatch();

            var subscription = cnn.SubscribeSync(subject, group);

            return Fetcher;


            
            IEnumerable<(Metadata md, TMessage message, Action<object> reply)> Fetcher(CancellationToken token)
            {
                while (!token.IsCancellationRequested && subscription.IsValid)
                {
                    if (TryGetMessage(out var msg, token))
                    {
                        Metadata md = msg.Header;
                        var message = _deserialize(typeof(TMessage), msg.Data);
                        if (message is TMessage data)
                            yield return (md, data, ret =>msg.Respond(_serialize(ret)));
                    }
                }
                
                subscription.Unsubscribe();
                subscription.Dispose();
                
                bool TryGetMessage(out Msg message, CancellationToken token1)
                {
                    try
                    {
                        message = null;
                        
                        if (token1.IsCancellationRequested)
                            return false;

                        message = subscription.NextMessage(500);
                        return true;
                    }
                    catch (NATSTimeoutException ex)
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
                            Subject =  subscription.Subject
                        };
                        return false;
                    }
                }
            }
        }
    }
}