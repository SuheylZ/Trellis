

using System;

namespace Common
{
    public delegate byte[] Serializer(object message);
    public delegate object Deserializer(Type type, byte[] data);

    public delegate bool TrySerialize(object message, out byte[] binary);
    public delegate bool TryDeserialize(byte[] binary, out object obj);

}