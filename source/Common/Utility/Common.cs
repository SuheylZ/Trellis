

using System;

namespace Trellis.Utility
{


    // public delegate bool TrySerialize(object message, out byte[] binary);
    // public delegate bool TryDeserialize(byte[] binary, out object obj);


    /// <summary>
    /// Runs the code in sand box so any exceptions are not propagated outside and cause damage
    /// </summary>
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