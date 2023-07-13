using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace S16.Collections
{
    public class ObservableList2D<T> : List2D<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Variables
        private static readonly string IndexerName = "Item[]";
        private const string CountString = "Count";

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        private Stack<Edit> undoStack = new Stack<Edit>();
        private Stack<Edit> redoStack = new Stack<Edit>();
        private bool recordTransactions = true;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        /// see <seealso cref="INotifyCollectionChanged"/>
        /// </remarks>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        public event EventHandler<ModificationEventArgs> Modified = null;
        #endregion

        #region Constructors
        public ObservableList2D()
            : base()
        {
        }

        public ObservableList2D(IEnumerable<T[]> collection)
            : base(collection)
        {
        }

        public ObservableList2D(T[,] values)
            : base(values)
        {
        }

        public ObservableList2D(int height, int width)
            : base(height, width)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Disallow reentrant attempts to change this collection. E.g. a event handler
        /// of the CollectionChanged event is not allowed to make changes to this collection.
        /// </summary>
        /// <remarks>
        /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
        /// <code>
        ///         using (BlockReentrancy())
        ///         {
        ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
        ///         }
        /// </code>
        /// </remarks>
        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        /// <summary> Check and assert for reentrant attempts to change this collection. </summary>
        /// <exception cref="InvalidOperationException"> raised when changing the collection
        /// while another collection change is still being notified to other listeners </exception>
        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        protected override void SetValues(int index, T[] items)
        {
            CheckReentrancy();
            T[] originalItem = GetValues(index);

            base.SetValues(index, items);

            RecordTransaction(
                () => SetValues(index, originalItem),
                () => SetValues(index, items)
            );

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, items, index);
        }

        protected override void SetValue(int y, int x, T item)
        {
            CheckReentrancy();

            T originalItem = GetValue(y, x);
            base.SetValue(y, x, item);

            RecordTransaction(
                () => SetValue(y, x, originalItem),
                () => SetValue(y, x, item)
            );

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new T[] { originalItem }, new T[] { item }, y);
        }

        protected override void InsertRange(int index, Enumeratorable collection)
        {
            CheckReentrancy();

            T[,] originalItem = collection.ToArray();
            base.InsertRange(index, collection);

            RecordTransaction(
                () => RemoveAt(index, collection.Count),
                () => InsertRange(index, collection)
            );

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, originalItem, index);
        }

        public override void RemoveAt(int index, int count = 1)
        {
            if (count < 1) throw new ArgumentException();

            CheckReentrancy();

            T[,] originalItem = new T[count, Width];
            for (int y = 0; y < count; y++)
            {
                T[] items = GetValues(y);
                for(int x = 0; x < items.Length; x++)
                {
                    originalItem[y, x] = items[x];
                }
            }

            base.RemoveAt(index, count);

            RecordTransaction(
                () => InsertRange(index, new Enumeratorable(originalItem)),
                () => RemoveAt(index, count)
            );

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, originalItem, index);
        }

        public override void Clear()
        {
            this.Clear(true);
        }

        public void Clear(bool recordHistory)
        {
            CheckReentrancy();

            T[,] originalItem = ToArray();

            base.Clear();

            if (recordHistory)
            {
                RecordTransaction(
                    () => InsertRange(0, new Enumeratorable(originalItem)),
                    () => Clear()
                );
            }

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        public override void AddColumn(int count = 1, IEnumerable<T[]> items = null)
        {
            CheckReentrancy();

            int index = m_size.Width;
            T[,] originalItem = null;
            using(var emu = new Enumeratorable(items))
            {
                originalItem = emu.ToArray();
            }
            base.AddColumn(count, items);

            RecordTransaction(
                () => RemoveColumn(index, count),
                () => AddColumn(count, items)
            );

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, originalItem, index);
        }

        public override void InsertColumn(int index, int count = 1, IEnumerable<T[]> items = null)
        {
            CheckReentrancy();

            T[,] originalItem = null;
            using (var emu = new Enumeratorable(items))
            {
                originalItem = emu.ToArray();
            }
            base.InsertColumn(index, count, items);

            RecordTransaction(
                () => RemoveColumn(index, count),
                () => InsertColumn(index, count, items)
            );

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, originalItem, index);
        }

        public override void RemoveColumn(int index, int count = 1)
        {
            if (count < 1) throw new ArgumentException();

            CheckReentrancy();

            T[,] originalItem = new T[Height, count];
            for (int y = 0; y < Height; y++)
            {
                T[] items = GetValues(y);
                for (int x = 0; x < (items.Length - index) && x < count; x++)
                {
                    originalItem[y, x] = items[index + x];
                }
            }

            base.RemoveColumn(index, count);

            RecordTransaction(
                () => InsertColumn(index, count, new Enumeratorable(originalItem)),
                () => RemoveColumn(index, count)
            );

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, originalItem, index);
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            T[] removedItem = this[oldIndex];

            base.RemoveAt(oldIndex);
            base.Insert(newIndex, removedItem);

            RecordTransaction(
                () => MoveItem(newIndex, oldIndex),
                () => MoveItem(oldIndex, newIndex)
            );

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }
        #endregion

        #region History Methods
        private void RecordTransaction(Action undoAction, Action redoAction)
        {
            Edit edit = new Edit(undoAction, redoAction);
            Modified?.Invoke(this, new ModificationEventArgs(edit));

            if (!recordTransactions) return;
            undoStack.Push(edit);
            redoStack.Clear();
        }

        private void RevertHistory(Edit edit, bool isUndo)
        {
            recordTransactions = false;
            edit.Invoke(isUndo);
            recordTransactions = true;
        }

        private void ProcessHistory(ref Stack<Edit> stack, ref Stack<Edit> opposite, bool isUndo)
        {
            try
            {
                var last = stack.Pop();
                opposite.Push(last);
                RevertHistory(last, isUndo);
            }
            catch (Exception)
            { }
        }

        public void SuspendHistory()
        {
            recordTransactions = false;
        }

        public void ResumeHistory()
        {
            recordTransactions = true;
        }

        public void ClearHistory()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public bool CanUndo
            => undoStack.Count > 0;

        public void Undo()
        {
            if (CanUndo)
                ProcessHistory(ref undoStack, ref redoStack, true);
        }

        public bool CanRedo
            => redoStack.Count > 0;

        public void Redo()
        {
            if (CanRedo)
                ProcessHistory(ref redoStack, ref undoStack, false);
        }
        #endregion

        #region Nested Types
        private class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy { get { return _busyCount > 0; } }

            int _busyCount;
        }

        internal readonly struct Edit
        {
            internal Action Undo { get; }
            internal Action Redo { get; }

            internal Edit(Action undo, Action redo)
            {
                Undo = undo;
                Redo = redo;
            }

            internal void Invoke(bool isUndo)
            {
                if (isUndo)
                    Undo();
                else
                    Redo();
            }
        }

        #region ModificationEventArgs Class
        public class ModificationEventArgs : EventArgs
        {
            public Action Action { get; }
            public Action OppositeAction { get; }

            public ModificationEventArgs(Action action, Action opposite)
            {
                Action = action;
                OppositeAction = opposite;
            }

            internal ModificationEventArgs(Edit edit)
            {
                Action = edit.Redo;
                OppositeAction = edit.Undo;
            }
        }
        #endregion

        #endregion
    }
}
