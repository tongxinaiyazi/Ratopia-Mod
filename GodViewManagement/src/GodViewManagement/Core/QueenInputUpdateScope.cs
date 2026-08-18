using System;

namespace GodViewManagement
{
    internal sealed class QueenInputUpdateScope
    {
        private int _depth;

        public bool IsActive
        {
            get { return _depth > 0; }
        }

        public IDisposable Enter()
        {
            _depth++;
            return new Lease(this);
        }

        private void Exit()
        {
            if (_depth > 0)
            {
                _depth--;
            }
        }

        private sealed class Lease : IDisposable
        {
            private QueenInputUpdateScope _scope;

            public Lease(QueenInputUpdateScope scope)
            {
                _scope = scope;
            }

            public void Dispose()
            {
                if (_scope == null)
                {
                    return;
                }

                _scope.Exit();
                _scope = null;
            }
        }
    }
}
