using System;
using System.Collections.Generic;
using System.Linq;
using SharedWarehouse.Core;
using Xunit;

namespace SharedWarehouse.Tests
{
    public sealed class SharedListCoordinatorTests
    {
        [Fact]
        public void MergeUnique_combines_types_and_preserves_reservations_and_first_state()
        {
            var first = new List<FakeEntry>
            {
                Entry("Wood", "Normal", 11, 12),
                Entry("Stone", "Normal", 21),
            };
            var second = new List<FakeEntry>
            {
                Entry("Wood", "Reserved", 13),
                Entry("Iron", "Normal", 31, 32),
            };

            var result = Merge(first, second);

            Assert.Equal(new[] { "Wood", "Stone", "Iron" }, result.Select(entry => entry.Type));
            Assert.Equal("Normal", result[0].State);
            Assert.Equal(new[] { 11, 12, 13 }, result[0].Reservations);
            Assert.Equal(new[] { 21 }, result[1].Reservations);
            Assert.Equal(new[] { 31, 32 }, result[2].Reservations);
        }

        [Fact]
        public void MergeUnique_counts_an_aliased_list_only_once()
        {
            var shared = new List<FakeEntry> { Entry("Wood", "Normal", 1, 2) };

            var result = Merge(shared, shared, shared);

            Assert.Single(result);
            Assert.Equal(new[] { 1, 2 }, result[0].Reservations);
        }

        [Fact]
        public void Initialize_selects_lowest_id_and_aliases_every_inventory()
        {
            var storage20 = Storage(20, Entry("Stone", "Normal", 2));
            var storage10 = Storage(10, Entry("Wood", "Normal", 1));
            var storage30 = Storage(30);
            var coordinator = Coordinator();

            coordinator.Initialize(new[] { storage20, storage10, storage30 });

            Assert.Same(storage10, coordinator.Master);
            Assert.Same(coordinator.SharedInventory, storage10.Inventory);
            Assert.Same(storage10.Inventory, storage20.Inventory);
            Assert.Same(storage10.Inventory, storage30.Inventory);
            Assert.Equal(2, coordinator.SharedInventory.Count);
        }

        [Fact]
        public void Initialize_can_run_twice_without_multiplying_inventory()
        {
            var first = Storage(1, Entry("Wood", "Normal", 1, 2));
            var second = Storage(2);
            var coordinator = Coordinator();

            coordinator.Initialize(new[] { first, second });
            coordinator.Initialize(new[] { first, second });

            Assert.Single(coordinator.SharedInventory);
            Assert.Equal(new[] { 1, 2 }, coordinator.SharedInventory[0].Reservations);
        }

        [Fact]
        public void Initialize_rolls_back_all_assignments_when_a_setter_fails()
        {
            var first = Storage(1, Entry("Wood", "Normal", 1));
            var second = Storage(2, Entry("Stone", "Normal", 2));
            var firstOriginal = first.Inventory;
            var secondOriginal = second.Inventory;
            var coordinator = Coordinator((storage, inventory) =>
            {
                if (ReferenceEquals(storage, second) && !ReferenceEquals(inventory, secondOriginal))
                {
                    throw new InvalidOperationException("setter failed");
                }

                storage.Inventory = inventory;
            });

            Assert.Throws<InvalidOperationException>(() => coordinator.Initialize(new[] { first, second }));

            Assert.Same(firstOriginal, first.Inventory);
            Assert.Same(secondOriginal, second.Inventory);
            Assert.False(coordinator.IsInitialized);
        }

        [Fact]
        public void Attach_merges_preexisting_inventory_and_rebinds_all_storages()
        {
            var first = Storage(1, Entry("Wood", "Normal", 1));
            var added = Storage(2, Entry("Wood", "Normal", 2), Entry("Stone", "Normal", 3));
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { first });

            coordinator.Attach(added);

            Assert.Same(first.Inventory, added.Inventory);
            Assert.Equal(new[] { 1, 2 }, first.Inventory.Single(entry => entry.Type == "Wood").Reservations);
            Assert.Equal(new[] { 3 }, first.Inventory.Single(entry => entry.Type == "Stone").Reservations);
        }

        [Fact]
        public void Single_view_exposes_inventory_only_on_master_and_supports_nesting()
        {
            var master = Storage(1, Entry("Wood", "Normal", 1));
            var secondary = Storage(2);
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { secondary, master });
            var shared = coordinator.SharedInventory;

            using (coordinator.EnterSingleView())
            {
                Assert.Same(shared, master.Inventory);
                Assert.Empty(secondary.Inventory);

                using (coordinator.EnterSingleView())
                {
                    Assert.Empty(secondary.Inventory);
                }

                Assert.Empty(secondary.Inventory);
            }

            Assert.Same(shared, master.Inventory);
            Assert.Same(shared, secondary.Inventory);
        }

        [Fact]
        public void Single_view_restores_aliases_after_an_exception()
        {
            var master = Storage(1, Entry("Wood", "Normal", 1));
            var secondary = Storage(2);
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { master, secondary });
            var shared = coordinator.SharedInventory;

            Assert.Throws<InvalidOperationException>((Action)(() =>
            {
                using (coordinator.EnterSingleView())
                {
                    throw new InvalidOperationException("simulated failure");
                }
            }));

            Assert.Same(shared, master.Inventory);
            Assert.Same(shared, secondary.Inventory);
        }

        [Fact]
        public void Single_view_can_expose_a_specific_secondary_storage()
        {
            var master = Storage(1, Entry("Wood", "Normal", 1));
            var secondary = Storage(2);
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { master, secondary });
            var shared = coordinator.SharedInventory;

            using (coordinator.EnterSingleView(secondary))
            {
                Assert.Empty(master.Inventory);
                Assert.Same(shared, secondary.Inventory);
            }

            Assert.Same(shared, master.Inventory);
            Assert.Same(shared, secondary.Inventory);
        }

        [Fact]
        public void Single_view_can_hide_inventory_from_every_storage()
        {
            var master = Storage(1, Entry("Wood", "Normal", 1));
            var secondary = Storage(2);
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { master, secondary });
            var shared = coordinator.SharedInventory;

            using (coordinator.EnterSingleView(null))
            {
                Assert.Empty(master.Inventory);
                Assert.Empty(secondary.Inventory);
            }

            Assert.Same(shared, master.Inventory);
            Assert.Same(shared, secondary.Inventory);
        }

        [Fact]
        public void DetachForDemolition_empties_nonlast_storage_and_reselects_master()
        {
            var storage10 = Storage(10, Entry("Wood", "Normal", 1));
            var storage20 = Storage(20);
            var storage30 = Storage(30);
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { storage30, storage20, storage10 });
            var shared = coordinator.SharedInventory;

            var detached = coordinator.DetachForDemolition(storage10);

            Assert.True(detached);
            Assert.Empty(storage10.Inventory);
            Assert.Same(storage20, coordinator.Master);
            Assert.Same(shared, storage20.Inventory);
            Assert.Same(shared, storage30.Inventory);
        }

        [Fact]
        public void DetachForDemolition_leaves_last_storage_for_vanilla_demolition()
        {
            var only = Storage(1, Entry("Wood", "Normal", 1));
            var coordinator = Coordinator();
            coordinator.Initialize(new[] { only });
            var inventory = only.Inventory;

            var detached = coordinator.DetachForDemolition(only);

            Assert.False(detached);
            Assert.Same(inventory, only.Inventory);
            Assert.True(coordinator.IsInitialized);

            coordinator.Remove(only);
            Assert.False(coordinator.IsInitialized);
        }

        private static SharedListCoordinator<FakeStorage, FakeEntry> Coordinator(
            Action<FakeStorage, List<FakeEntry>> setter = null)
        {
            return new SharedListCoordinator<FakeStorage, FakeEntry>(
                storage => storage.Id,
                storage => storage.Inventory,
                setter ?? ((storage, inventory) => storage.Inventory = inventory),
                inventories => Merge(inventories.ToArray()));
        }

        private static List<FakeEntry> Merge(params List<FakeEntry>[] inventories)
        {
            return InventoryMerger.MergeUnique(
                inventories,
                entry => entry.Type,
                entry => Entry(entry.Type, entry.State, entry.Reservations.ToArray()),
                (target, source) => target.Reservations.AddRange(source.Reservations));
        }

        private static FakeStorage Storage(int id, params FakeEntry[] entries)
        {
            return new FakeStorage(id, new List<FakeEntry>(entries));
        }

        private static FakeEntry Entry(string type, string state, params int[] reservations)
        {
            return new FakeEntry(type, state, new List<int>(reservations));
        }

        private sealed class FakeStorage
        {
            public FakeStorage(int id, List<FakeEntry> inventory)
            {
                Id = id;
                Inventory = inventory;
            }

            public int Id { get; }

            public List<FakeEntry> Inventory { get; set; }
        }

        private sealed class FakeEntry
        {
            public FakeEntry(string type, string state, List<int> reservations)
            {
                Type = type;
                State = state;
                Reservations = reservations;
            }

            public string Type { get; }

            public string State { get; }

            public List<int> Reservations { get; }
        }
    }
}
