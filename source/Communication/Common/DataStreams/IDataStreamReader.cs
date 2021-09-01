using System;
using System.Collections.Generic;
using System.Threading;

namespace Trellis.Communications.DataStreams
{
    public interface IDataStreamReader
    {
        Func<CancellationToken, IAsyncEnumerable<(string tag, Func<Type, object> reverser, IDataStreamTransaction transactor)>> Compose();
        Func<CancellationToken, IAsyncEnumerable<(T message, IDataStreamTransaction transactor)>> Compose<T>(string tag);
    }
}