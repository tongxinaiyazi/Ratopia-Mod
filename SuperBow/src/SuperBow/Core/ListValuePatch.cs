using System;
using System.Collections.Generic;

namespace SuperBow.Core
{
    public sealed class ListValuePatch : IDisposable
    {
        private readonly IList<float> _values;
        private readonly int _index;
        private readonly float _original;
        private readonly float _replacement;
        private bool _disposed;

        private ListValuePatch(
            IList<float> values,
            int index,
            float original,
            float replacement)
        {
            _values = values;
            _index = index;
            _original = original;
            _replacement = replacement;
        }

        public static bool TryApply(
            IList<float> values,
            int index,
            float replacement,
            out ListValuePatch patch)
        {
            patch = null;
            if (values == null || index < 0 || index >= values.Count)
            {
                return false;
            }

            var original = values[index];
            try
            {
                values[index] = replacement;
            }
            catch
            {
                return false;
            }

            patch = new ListValuePatch(values, index, original, replacement);
            return true;
        }

        public static bool TryApplyExpected(
            IList<float> values,
            int index,
            float expected,
            float replacement,
            out ListValuePatch patch)
        {
            patch = null;
            if (values == null || index < 0 || index >= values.Count ||
                Math.Abs(values[index] - expected) > 0.0001f)
            {
                return false;
            }

            return TryApply(values, index, replacement, out patch);
        }

        public static bool TryApplyExpectedOrAlreadySet(
            IList<float> values,
            int index,
            float expected,
            float replacement,
            out ListValuePatch patch)
        {
            patch = null;
            if (values == null || index < 0 || index >= values.Count)
            {
                return false;
            }

            if (Math.Abs(values[index] - replacement) <= 0.0001f)
            {
                return true;
            }

            return TryApplyExpected(values, index, expected, replacement, out patch);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_index < 0 || _index >= _values.Count ||
                !_values[_index].Equals(_replacement))
            {
                return;
            }

            _values[_index] = _original;
        }
    }
}
