namespace Trellis.Serialization
{
    public interface ISerDes
    {
        byte[] Serialize(object obj);
        T Deserialize<T>(byte[] data);
        
        void GetSerializers(out Serializer ser, out Deserializer des);
    }

    /// <summary>
    /// Providers serialization and deserialization together.
    /// You can build your own serialization.
    /// </summary>
    public sealed class SerDes : ISerDes
    {
        readonly Serializer _serializer;
        readonly Deserializer _deserializer;

        public SerDes(Serializer serializer, Deserializer deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public byte[] Serialize(object obj) => _serializer(obj);
        public T Deserialize<T>(byte[] data) => (T)_deserializer(typeof(T), data);
        public void GetSerializers(out Serializer ser, out Deserializer des)
        {
            ser = _serializer;
            des = _deserializer;
        }
    }
}