

using System;

namespace Common
{
    public delegate byte[] Serializer(object message);
    public delegate object Deserializer(Type type, byte[] data);

    public delegate bool TrySerialize(object message, out byte[] binary);
    public delegate bool TryDeserialize(byte[] binary, out object obj);


    public static class Sandbox
    {
        public static void Try(Action act, Action<Exception> handler = null)
        {
            try
            {
                act();
            }
            catch (Exception ex)
            {
                handler?.Invoke(ex);
            }
        }
        public static T Try<T>(Func<T> func, Func<Exception, T> handler = null)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if(handler!=null)
                  return handler(ex);
                
                return default(T);
            }
        }
    }
}