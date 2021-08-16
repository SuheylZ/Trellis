using System;

namespace Communications.NATS
{
    public sealed class Disposable : IDisposable
    {
        readonly Action _action;

        public Disposable(Action action) => _action = action ?? throw new ArgumentNullException(nameof(action));

        public void Dispose()
        {
            _action();
        }

        public static implicit operator Disposable(Action act)
        {
            return new Disposable(act);
        }
    }
}