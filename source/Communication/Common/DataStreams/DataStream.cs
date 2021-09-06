using System;
using System.Collections.Generic;
using System.Data;

namespace Trellis.Communications.DataStreams
{
    /// <summary>
    /// Identifies a data stream with all its topics and types associated with topics
    /// </summary>
    public class DataStream
    {
        readonly string _streamName;
        readonly Dictionary<string, HashSet<Type>> _topics2Types;
        readonly HashSet<Type> _types;

        public DataStream(string streamName)
        {
            _streamName = streamName;
            _topics2Types = new Dictionary<string, HashSet<Type>>();
            _types = new HashSet<Type>();
        }

        public void Add(string topic, Type type)
        {
            if (_types.Contains(type))
                throw new DuplicateNameException("Type is duplicated for a topic. A type can only be used for a single topic");

            _types.Add(type);
            var realTopic = topic.Trim().ToLower();
            if (!_topics2Types.ContainsKey(realTopic))
                _topics2Types.Add(realTopic, new HashSet<Type> { type });
            else
                _topics2Types[realTopic].Add(type);
        }


        public IEnumerable<string> Topics => _topics2Types.Keys;
        public IEnumerable<Type> Types => _types;

    }

    public interface IGenericObserver
    {
        void Observe(object obj);
    }
    
    public interface IDataStreamObserver<T> : IObserver<T>, IGenericObserver
    {
        void IGenericObserver.Observe(object obj)
        {
            if (obj is T message)
                OnNext(message);
            else
                throw new Exception("given type is not of correct type");
        }
    }
}