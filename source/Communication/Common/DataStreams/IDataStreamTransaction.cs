using System;

namespace Trellis.Communications.DataStreams
{
    public interface IDataStreamTransaction
    {
        void Done();
        void Fail(Exception ex);
    }
}