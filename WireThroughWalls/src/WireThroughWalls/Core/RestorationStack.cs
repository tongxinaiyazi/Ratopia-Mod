using System;
using System.Collections.Generic;

namespace WireThroughWalls.Core
{
    internal sealed class RestorationStack : IDisposable
    {
        private readonly List<IDisposable> _scopes = new List<IDisposable>();
        private bool _disposed;

        internal void Push(IDisposable scope)
        {
            if (scope == null)
            {
                return;
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RestorationStack));
            }

            _scopes.Add(scope);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            List<Exception> errors = null;
            for (var index = _scopes.Count - 1; index >= 0; index--)
            {
                try
                {
                    _scopes[index].Dispose();
                }
                catch (Exception error)
                {
                    if (errors == null)
                    {
                        errors = new List<Exception>();
                    }

                    errors.Add(error);
                }
            }

            if (errors != null)
            {
                throw new AggregateException("One or more restoration scopes failed.", errors);
            }
        }
    }
}
