using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Communications.NATS;
using NATS.Client;
using Deserializer = Common.Deserializer;
using Serializer = Common.Serializer;


namespace Program
{
    public class Communication
    {
        
        
        readonly Action<Options> _configureOptions;
        
        readonly Common.Serializer _serialize;
        readonly Common.Deserializer  _deserialize;

        readonly string _sender;

        public Communication(Action<Options> configureAction, Common.Serializer serialize, Common.Deserializer deserialize) : this(configureAction, serialize, deserialize, string.Empty)
        {
        }

        public Communication(Action<Options> configureOptions, Serializer serialize, Deserializer deserialize, string sender)
        {
            _configureOptions = configureOptions;
            _serialize = serialize;
            _deserialize = deserialize;
            _sender = sender;
        }

        
        
        /// <summary>
        /// Creates a publisher with a disposer. Publisher is a simple function bound to a particular subject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="disposer">a function to cleanup the things later on. uses IDisposable</param>
        /// <returns></returns>
        public Func<object, Task> CreatePublisher(string subject, out IDisposable disposer)
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
                    var letter = new Letter(_sender, CommunicationTypes.FireAndForget, data);
                    var binary = _serialize(letter);
                    
                    cnn.Publish(subject, Guid.NewGuid().ToString(), binary);
                    return Task.CompletedTask;
                }
                catch (Exception e)
                {
                    return Task.FromException(e);
                }
            };
        }

        
        /// <summary>
        /// Creates a Requester a Request function that takes a message and returns a reply
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="timeoutDuration">duration after which the timeout exception occurs</param>
        /// <param name="disposer"></param>
        /// <returns>the message that should be returned</returns>
        public Func<object, CancellationToken, Task<Letter>> CreateRequester(string subject, TimeSpan timeoutDuration, out Disposable disposer)
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
                    var letter = new Letter(_sender, CommunicationTypes.Query, data);
                    var binary = _serialize(letter);

                    //var msg = CreateMessage(subject, binary, CommunicationTypes.Query);
                    var t = await cnn.RequestAsync(subject, binary, milliseconds, token).ConfigureAwait(false);
                    var ans = _deserialize(typeof(Letter), t.Data);

                    if (ans is Letter l1)
                        return l1;

                    throw new Exception("Not a letter");

                }
                catch (Exception e)
                {
                    throw e;
                }
            };
        }

        // Msg CreateMessage(string subject, byte[] binary, CommunicationTypes type)
        // {
        //     var header = new MsgHeader();
        //     
        //     header["sentAt"] = DateTime.UtcNow.ToIsoString();
        //     header["sender"] = _sender;
        //     header["messagetype"] = type.ToString();
        //
        //     return new Msg
        //     {
        //         Data = binary,
        //         Header = header,
        //         Subject = subject,
        //         Reply = Guid.NewGuid().ToString()
        //     };
        // }

        public Func<CancellationToken, Task> CreateListener(string subject, string group, Action<object, Action<object>> handler)
        {
            var cf = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();
            _configureOptions(options);


            var cnn = cf.CreateConnection(options);
            var subscription = cnn.SubscribeAsync(subject, group, (_, args) =>
            {
                try
                {
                    var t1 = _deserialize(typeof(Letter), args.Message.Data);
                    if (t1 is Letter letterReceived)
                    {
                        Action<object> reply = ret =>
                        {
                            var letterReplied = new Letter("", CommunicationTypes.FireAndForget, ret, letterReceived.CreatedOn);
                            var binary = _serialize(letterReplied);
                            args.Message.Respond(binary);
                        };
                     
                        handler(letterReceived.Message, reply);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
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
        
        
        
        
        public Func<CancellationToken, IEnumerable<(object message, Action<object> reply)>> CreateIterator(string subject, string group)
        {
            var cf = new ConnectionFactory();
            var options = ConnectionFactory.GetDefaultOptions();
            _configureOptions(options);


            var cnn = cf.CreateConnection(options);
            var sw = new Stopwatch();

            var subscription = cnn.SubscribeSync(subject, group);

            return Fetcher;

            bool TryGetMessage(out Msg message, CancellationToken token)
            {

                try
                {
                    if (token.IsCancellationRequested)
                        throw new TaskCanceledException();

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
                        Data = _serialize(new Letter(subject, CommunicationTypes.FireAndForget, ex)), 
                        Reply = string.Empty, 
                        Subject =  subscription.Subject
                    };
                    return false;
                }
            }
            
            IEnumerable<(object message, Action<object> reply)> Fetcher(CancellationToken token)
            {
                while (!token.IsCancellationRequested && subscription.IsValid)
                {
                    if (TryGetMessage(out var msg, token))
                    {
                        var message = _deserialize(typeof(Letter), msg.Data);

                        if (message is Letter letter)
                        {
                            Action<object> reply = ret =>
                            {
                                var binary = _serialize(new Letter("", CommunicationTypes.FireAndForget, ret,letter.CreatedOn));
                                msg.Respond(binary);
                            };
                        
                            yield return (letter.Message, reply);
                        }
                    }
                }
                
                subscription.Unsubscribe();
                subscription.Dispose();
            }
        }
    }
}