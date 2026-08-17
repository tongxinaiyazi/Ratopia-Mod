using System;
using System.Collections.Generic;

namespace StrongerWorkDistance.Core
{
    public static class AtomicListUpdater
    {
        public static void ReplaceBoth<T>(IList<T> first, IList<T> second, IReadOnlyList<T> replacement)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            if (replacement == null)
            {
                throw new ArgumentNullException(nameof(replacement));
            }

            var firstSnapshot = new List<T>(first);
            var secondSnapshot = new List<T>(second);

            try
            {
                Replace(first, replacement);
                Replace(second, replacement);
            }
            catch
            {
                Replace(first, firstSnapshot);
                Replace(second, secondSnapshot);
                throw;
            }
        }

        private static void Replace<T>(IList<T> target, IReadOnlyList<T> values)
        {
            target.Clear();
            for (var index = 0; index < values.Count; index++)
            {
                target.Add(values[index]);
            }
        }
    }
}
