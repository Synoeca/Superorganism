#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Superorganism.Core.Inventory
{
    /// <summary>
    /// Represents a collection of inventory items with change notifications.
    /// </summary>
    public class Inventory : ICollection<InventoryItem>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Event triggered when a property changes
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Event triggered when the collection changes
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// A private backing field for Items property
        /// </summary>
        private ICollection<InventoryItem> _items = new List<InventoryItem>();

        /// <summary>
        /// Gets or sets the collection of inventory items
        /// </summary>
        public ICollection<InventoryItem> Items
        {
            get => _items;
            set
            {
                List<InventoryItem> old = _items.ToList();
                _items = value;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, old, _items.ToList()));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        /// <summary>
        /// Gets the number of items in the inventory
        /// </summary>
        public int Count => Items.Count;

        /// <summary>
        /// Indicates whether the current inventory is read-only
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item">The item to add to the inventory</param>
        public void Add(InventoryItem item)
        {
            // Check if item already exists (by name)
            InventoryItem existingItem = Items.FirstOrDefault(i => i.Name == item?.Name);
            if (existingItem != null)
            {
                // If it exists, just increase quantity
                if (item != null) existingItem.Quantity += item.Quantity;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, existingItem, existingItem));
            }
            else
            {
                // Add new item
                Items.Add(item);
                if (item != null)
                {
                    item.PropertyChanged += HandleItemPropertyChanged;
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>
        /// Clears all items from the inventory
        /// </summary>
        public void Clear()
        {
            foreach (InventoryItem item in _items)
            {
                item.PropertyChanged -= HandleItemPropertyChanged;
            }
            Items.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }

        /// <summary>
        /// Determines whether the inventory contains a specific item
        /// </summary>
        /// <param name="item">The item to locate in the inventory</param>
        /// <returns>true if the item is found; otherwise, false</returns>
        public bool Contains(InventoryItem item)
        {
            return Items.Contains(item);
        }

        /// <summary>
        /// Checks if the inventory contains an item with the specified name
        /// </summary>
        /// <param name="itemName">The name of the item to locate</param>
        /// <returns>true if an item with the name is found; otherwise, false</returns>
        public bool ContainsName(string itemName)
        {
            return Items.Any(i => i.Name == itemName);
        }

        /// <summary>
        /// Copies the elements of the inventory to an array, starting at a particular index
        /// </summary>
        /// <param name="array">The destination array</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins</param>
        public void CopyTo(InventoryItem[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the inventory
        /// </summary>
        /// <returns>An enumerator for the inventory</returns>
        public IEnumerator<InventoryItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Removes the first occurrence of an item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>true if successfully removed; otherwise, false</returns>
        public bool Remove(InventoryItem item)
        {
            int index = _items.ToList().IndexOf(item);
            if (index == -1)
            {
                return false;
            }

            bool result = Items.Remove(item);
            if (item != null)
            {
                item.PropertyChanged -= HandleItemPropertyChanged;
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            return result;
        }

        /// <summary>
        /// Removes a specific quantity of an item, potentially removing it entirely if quantity reaches zero
        /// </summary>
        /// <param name="item">The item to reduce</param>
        /// <param name="quantity">The quantity to remove</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool RemoveQuantity(InventoryItem item, int quantity)
        {
            if (!Contains(item)) return false;

            item.Quantity -= quantity;

            // If quantity drops to zero or below, remove the item entirely
            if (item.Quantity <= 0)
            {
                return Remove(item);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item));
            return true;
        }

        /// <summary>
        /// Uses a specific item (reduces quantity by 1)
        /// </summary>
        /// <param name="item">The item to use</param>
        /// <returns>true if successful; otherwise, false</returns>
        public bool UseItem(InventoryItem item)
        {
            return RemoveQuantity(item, 1);
        }

        /// <summary>
        /// Tries to find an item by name
        /// </summary>
        /// <param name="itemName">The name of the item to find</param>
        /// <returns>The item if found; otherwise, null</returns>
        public InventoryItem FindByName(string itemName)
        {
            return Items.FirstOrDefault(i => i.Name == itemName);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the inventory
        /// </summary>
        /// <returns>An enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Handles property changed events from inventory items
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="args">The event data</param>
        private void HandleItemPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (sender is InventoryItem item)
            {
                // If quantity dropped to 0 or below, remove the item
                if (args.PropertyName == nameof(InventoryItem.Quantity) && item.Quantity <= 0)
                {
                    Remove(item);
                }
                else
                {
                    // Notify that an item has changed
                    int index = _items.ToList().IndexOf(item);
                    if (index >= 0)
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Replace, item, item, index));
                    }
                }
            }
        }
    }
}