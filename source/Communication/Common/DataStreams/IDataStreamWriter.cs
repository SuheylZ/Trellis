using System;
using System.Threading.Tasks;

namespace Trellis.Communications.DataStreams
{
    public interface IDataStreamWriter 
    {
        Func<string, object, Task<bool>> Compose();
        Func<T, Task> Compose<T>(string topic);
        void Dispose();
    }
}