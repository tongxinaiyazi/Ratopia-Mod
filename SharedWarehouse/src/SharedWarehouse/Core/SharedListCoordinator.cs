using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedWarehouse.Core
{
    internal sealed class SharedListCoordinator<TStorage, TEntry>
        where TStorage : class
    {
        private readonly Func<TStorage, int> _idSelector;
        private readonly Func<TStorage, List<TEntry>> _inventoryGetter;
        private readonly Action<TStorage, List<TEntry>> _inventorySetter;
        private readonly Func<IEnumerable<List<TEntry>>, List<TEntry>> _mergeInventories;
        private List<TStorage> _storages = new List<TStorage>();
        private int _singleViewDepth;

        public SharedListCoordinator(
            Func<TStorage, int> idSelector,
            Func<TStorage, List<TEntry>> inventoryGetter,
            Action<TStorage, List<TEntry>> inventorySetter,
            Func<IEnumerable<List<TEntry>>, List<TEntry>> mergeInventories)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
            _inventoryGetter = inventoryGetter ?? throw new ArgumentNullException(nameof(inventoryGetter));
            _inventorySetter = inventorySetter ?? throw new ArgumentNullException(nameof(inventorySetter));
            _mergeInventories = mergeInventories ?? throw new ArgumentNullException(nameof(mergeInventories));
        }

        public bool IsInitialized => Master != null && SharedInventory != null && _storages.Count > 0;

        public TStorage Master { get; private set; }

        public List<TEntry> SharedInventory { get; private set; }

        public void Initialize(IEnumerable<TStorage> storages)
        {
            EnsureNotInSingleView();

            var ordered = Normalize(storages);
            if (ordered.Count == 0)
            {
                ResetState();
                return;
            }

            var originalInventories = ordered
                .Select(storage => new KeyValuePair<TStorage, List<TEntry>>(storage, _inventoryGetter(storage)))
                .ToList();
            var merged = _mergeInventories(originalInventories.Select(pair => pair.Value));
            if (merged == null)
            {
                throw new InvalidOperationException("Inventory merger returned null.");
            }

            AssignWithRollback(ordered, merged, originalInventories);

            _storages = ordered;
            Master = ordered[0];
            SharedInventory = merged;
        }

        public void Attach(TStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            EnsureNotInSingleView();
            if (IndexOfReference(storage) >= 0)
            {
                if (IsInitialized)
                {
                    _inventorySetter(storage, SharedInventory);
                }

                return;
            }

            var expanded = new List<TStorage>(_storages) { storage };
            Initialize(expanded);
        }

        public bool DetachForDemolition(TStorage storage)
        {
            EnsureNotInSingleView();
            var index = IndexOfReference(storage);
            if (index < 0 || _storages.Count <= 1)
            {
                return false;
            }

            _inventorySetter(storage, new List<TEntry>());
            _storages.RemoveAt(index);
            Master = _storages.OrderBy(_idSelector).First();
            return true;
        }

        public void Remove(TStorage storage)
        {
            EnsureNotInSingleView();
            var index = IndexOfReference(storage);
            if (index < 0)
            {
                return;
            }

            var wasMaster = ReferenceEquals(_storages[index], Master);
            _storages.RemoveAt(index);
            if (_storages.Count == 0)
            {
                ResetState();
                return;
            }

            if (wasMaster)
            {
                Master = _storages.OrderBy(_idSelector).First();
            }
        }

        public IDisposable EnterSingleView()
        {
            return EnterSingleView(Master);
        }

        public IDisposable EnterSingleView(TStorage exposedStorage)
        {
            if (!IsInitialized)
            {
                return NoOpScope.Instance;
            }

            if (exposedStorage != null && IndexOfReference(exposedStorage) < 0)
            {
                throw new ArgumentException("The exposed storage is not registered with this coordinator.", nameof(exposedStorage));
            }

            var snapshots = _storages
                .Select(storage => new KeyValuePair<TStorage, List<TEntry>>(storage, _inventoryGetter(storage)))
                .ToList();
            var changed = new List<KeyValuePair<TStorage, List<TEntry>>>();

            try
            {
                foreach (var snapshot in snapshots)
                {
                    changed.Add(snapshot);
                    _inventorySetter(
                        snapshot.Key,
                        ReferenceEquals(snapshot.Key, exposedStorage) ? SharedInventory : new List<TEntry>());
                }
            }
            catch (Exception assignmentError)
            {
                var rollbackErrors = Restore(changed);
                if (rollbackErrors.Count > 0)
                {
                    rollbackErrors.Insert(0, assignmentError);
                    throw new AggregateException("Unable to enter single-inventory view and roll back cleanly.", rollbackErrors);
                }

                throw;
            }

            _singleViewDepth++;
            return new SingleViewScope(this, snapshots);
        }

        public void Reset()
        {
            EnsureNotInSingleView();
            ResetState();
        }

        private void ExitSingleView(List<KeyValuePair<TStorage, List<TEntry>>> snapshots)
        {
            if (_singleViewDepth <= 0)
            {
                return;
            }

            _singleViewDepth--;
            var errors = Restore(snapshots);
            if (errors.Count > 0)
            {
                throw new AggregateException("Unable to restore shared inventories after single-inventory view.", errors);
            }
        }

        private List<TStorage> Normalize(IEnumerable<TStorage> storages)
        {
            if (storages == null)
            {
                throw new ArgumentNullException(nameof(storages));
            }

            var seen = new HashSet<TStorage>(ReferenceEqualityComparer<TStorage>.Instance);
            return storages
                .Where(storage => storage != null && seen.Add(storage))
                .OrderBy(_idSelector)
                .ToList();
        }

        private void AssignWithRollback(
            IEnumerable<TStorage> storages,
            List<TEntry> inventory,
            List<KeyValuePair<TStorage, List<TEntry>>> originals)
        {
            var attempted = new List<KeyValuePair<TStorage, List<TEntry>>>();
            try
            {
                foreach (var storage in storages)
                {
                    var original = originals.First(pair => ReferenceEquals(pair.Key, storage));
                    attempted.Add(original);
                    _inventorySetter(storage, inventory);
                }
            }
            catch (Exception assignmentError)
            {
                var rollbackErrors = Restore(attempted);
                if (rollbackErrors.Count > 0)
                {
                    rollbackErrors.Insert(0, assignmentError);
                    throw new AggregateException("Unable to initialize shared inventories and roll back cleanly.", rollbackErrors);
                }

                throw;
            }
        }

        private List<Exception> Restore(IEnumerable<KeyValuePair<TStorage, List<TEntry>>> snapshots)
        {
            var errors = new List<Exception>();
            if (snapshots == null)
            {
                return errors;
            }

            foreach (var snapshot in snapshots)
            {
                try
                {
                    _inventorySetter(snapshot.Key, snapshot.Value);
                }
                catch (Exception error)
                {
                    errors.Add(error);
                }
            }

            return errors;
        }

        private int IndexOfReference(TStorage storage)
        {
            for (var index = 0; index < _storages.Count; index++)
            {
                if (ReferenceEquals(_storages[index], storage))
                {
                    return index;
                }
            }

            return -1;
        }

        private void EnsureNotInSingleView()
        {
            if (_singleViewDepth > 0)
            {
                throw new InvalidOperationException("Storage membership cannot change during a single-inventory view.");
            }
        }

        private void ResetState()
        {
            _storages = new List<TStorage>();
            Master = null;
            SharedInventory = null;
            _singleViewDepth = 0;
        }

        private sealed class SingleViewScope : IDisposable
        {
            private SharedListCoordinator<TStorage, TEntry> _owner;
            private readonly List<KeyValuePair<TStorage, List<TEntry>>> _snapshots;

            public SingleViewScope(
                SharedListCoordinator<TStorage, TEntry> owner,
                List<KeyValuePair<TStorage, List<TEntry>>> snapshots)
            {
                _owner = owner;
                _snapshots = snapshots;
            }

            public void Dispose()
            {
                var owner = _owner;
                if (owner == null)
                {
                    return;
                }

                _owner = null;
                owner.ExitSingleView(_snapshots);
            }
        }

        private sealed class NoOpScope : IDisposable
        {
            public static readonly NoOpScope Instance = new NoOpScope();

            private NoOpScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
