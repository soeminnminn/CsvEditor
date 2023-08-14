using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace S16.Collections
{
    [Serializable, DebuggerDisplay("Count = {Count}")]
    public class List2D<T> : IEnumerable<T[]>, IEnumerable
    {
        #region Variables
        protected const uint MaxArrayLength = 0x7FEFFFFF;
        protected const int DefaultCapacity = 4;

        protected readonly static T[,] s_emptyArray = new T[0, 0];

        protected T[,] m_items;
        protected Size m_size;
        protected int m_version = 0;
        #endregion

        #region Constructors
        public List2D()
        {
            m_items = s_emptyArray;
            m_size = new Size();
        }

        public List2D(IEnumerable<T[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            int height = 0;
            int width = 0;
            using (IEnumerator<T[]> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    width = Math.Max(width, en.Current == null ? 0 : en.Current.Length);
                    height++;
                }
            }

            m_size = new Size(height, width);
            m_items = new T[height, width];

            if (height > 0 && width > 0)
            {
                using (IEnumerator<T[]> en = collection.GetEnumerator())
                {
                    int y = 0;
                    while (en.MoveNext())
                    {
                        T[] items = en.Current;
                        if (items != null)
                        {
                            for (int x = 0; x < items.Length && x < width; x++)
                            {
                                m_items[y, x] = items[x];
                            }
                        }
                        y++;
                    }
                }
            }
        }

        public List2D(T[,] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            int height = values.GetLength(0);
            int width = values.GetLength(1);

            m_size = new Size(height, width);
            m_items = new T[height, width];

            if (height > 0 && width > 0)
                Array.Copy(values, m_items, m_items.Length);
        }

        public List2D(int height, int width)
        {
            if (height < 0)
                throw new ArgumentOutOfRangeException("height", "NeedNonNegNum");

            if (width < 0)
                throw new ArgumentOutOfRangeException("width", "NeedNonNegNum");

            m_size = new Size(height, width);

            if (height == 0 && width == 0)
                m_items = s_emptyArray;
            else
                m_items = new T[height, width];
        }
        #endregion

        #region Properties
        public virtual T[] this[int index]
        {
            get => GetValues(index);
            set
            {
                SetValues(index, value);
                m_version++;
            }
        }

        public virtual T this[int y, int x]
        {
            get => GetValue(y, x);
            set
            {
                SetValue(y, x, value);
                m_version++;
            }
        }

        public virtual T this[Index index]
        {
            get => GetValue(index.Y, index.X);
            set
            {
                SetValue(index.Y, index.X, value);
                m_version++;
            }
        }

        public virtual int Width
        {
            get => m_size.Width;
            set
            {
                int width = m_items.GetLength(1);
                if (value != width)
                {
                    T[,] newItems = new T[m_size.Height, value];
                    if (m_size.Height > 0 && m_size.Width > 0)
                    {
                        for (int y = 0; y < m_size.Height; y++)
                        {
                            for (int x = 0; x < m_size.Width && x < value; x++)
                            {
                                newItems[y, x] = m_items[y, x];
                            }
                        }
                    }
                    m_items = newItems;
                }

                m_size.Width = value;
            }
        }

        public virtual int Height
        {
            get => m_size.Height;
            protected set
            {
                int height = m_items.GetLength(0);
                if (value != height)
                {
                    if (value > 0)
                    {
                        T[,] newItems = new T[value, m_size.Width];
                        if (m_size.Height > 0)
                            Array.Copy(m_items, newItems, m_size.Length);

                        m_items = newItems;
                    }
                    else
                        m_items = s_emptyArray;
                }
            }
        }

        public virtual int Count
        {
            get => m_size.Height;
        }

        public virtual int Length
        {
            get => m_size.Length;
        }
        #endregion

        #region Protected Methods
        protected static bool IsReferenceOrContainsReferences<TRef>()
        {
#if NET
            return RuntimeHelpers.IsReferenceOrContainsReferences<TRef>();
#else
            try
            {
                var method = typeof(RuntimeHelpers).GetMethod("IsReferenceOrContainsReferences", BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                {
                    var generic = method.MakeGenericMethod(typeof(TRef));
                    return generic.Invoke(null, null) == (object)true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            var t = typeof(TRef);
            return t.IsValueType || t.IsClass || t.IsAnsiClass || t.IsPointer;
#endif
        }

        protected virtual void EnsureCapacity(int min)
        {
            int height = m_items.GetLength(0);
            int width = m_items.GetLength(1);

            if (height < min)
            {
                int newCapacity = height == 0 ? DefaultCapacity : height * 2;

                if ((uint)(newCapacity * width) > MaxArrayLength) newCapacity = (int)(MaxArrayLength / width);
                if (newCapacity < min) newCapacity = min;

                Height = newCapacity;
            }
        }

        protected virtual void VerifyIndex(int index, int max)
        {
            if (index < 0 || index >= max)
                throw new IndexOutOfRangeException();
        }

        protected virtual T[] GetValues(int index)
        {
            VerifyIndex(index, m_size.Height);

            int width = m_size.Width;
            if (width == 0) return new T[0];

            T[] items = new T[width];
            for (int i = 0; i < width; i++)
            {
                items[i] = m_items[index, i];
            }
            return items;
        }

        protected virtual void SetValues(int index, T[] items)
        {
            VerifyIndex(index, m_size.Height);

            for (int i = 0; i < items.Length && i < m_size.Width; i++)
            {
                m_items[index, i] = items[i];
            }
        }

        protected virtual T GetValue(int y, int x)
        {
            VerifyIndex(x, m_size.Width);
            VerifyIndex(y, m_size.Height);

            return m_items[y, x];
        }

        protected virtual void SetValue(int y, int x, T item)
        {
            VerifyIndex(x, m_size.Width);
            VerifyIndex(y, m_size.Height);

            m_items[y, x] = item;
        }

        protected virtual void InsertRange(int index, Enumeratorable collection)
        {
            int size = m_size.Height;

            if ((uint)index > (uint)size)
                throw new ArgumentOutOfRangeException("index");

            if (collection == null)
                throw new ArgumentNullException("collection");

            if (collection.Count == int.MaxValue)
                throw new ArgumentOutOfRangeException("collection.count");

            int count = collection.Count;
            int len = m_size.Length;
            int height = m_items.GetLength(0);
            int width = m_size.Width;
            int idx = index * width;

            if ((size + count) >= height) EnsureCapacity(size + count);
            if (index < size)
                Array.Copy(m_items, idx, m_items, idx + width, len - idx);

            using (IEnumerator<T[]> en = collection.GetEnumerator())
            {
                int y = index;
                while (en.MoveNext() && y < (size + count))
                {
                    var items = en.Current;
                    for (int x = 0; x < items.Length && x < width; x++)
                    {
                        m_items[y, x] = items[x];
                    }
                    y++;
                }
            }

            m_size.Height = size + count;
        }
        #endregion

        #region Public Methods
        public virtual void Add(params T[] items)
        {
            using (Enumeratorable enumerable = new Enumeratorable(items, true))
            {
                InsertRange(m_size.Height, enumerable);
            }
            m_version++;
        }

        public virtual void Add(IEnumerable<T[]> collection)
        {
            using (Enumeratorable enumerable = new Enumeratorable(collection, true))
            {
                InsertRange(m_size.Height, enumerable);
            }
            m_version++;
        }

        public virtual void Insert(int index, params T[] items)
        {
            using (Enumeratorable enumerable = new Enumeratorable(items, true))
            {
                InsertRange(index, enumerable);
            }
            m_version++;
        }

        public virtual void Insert(int index, IEnumerable<T[]> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            using (Enumeratorable enumerable = new Enumeratorable(collection, true))
            {
                InsertRange(index, enumerable);
            }
            m_version++;
        }

        public virtual void RemoveAt(int index, int count = 1)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "NeedNonNegNum");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "NeedNonNegNum");

            int size = m_size.Height;
            if (size - index < count)
                throw new ArgumentException("InvalidOffLen");

            if (count > 0)
            {
                int len = m_size.Length;
                int width = m_size.Width;
                int idx = index * width;
                int srcIdx = idx + (count * width);
                int remain = len - srcIdx;

                if (index < size && remain > 0)
                    Array.Copy(m_items, srcIdx, m_items, idx, remain);

                m_size -= count;
                m_version++;

                if (IsReferenceOrContainsReferences<T>())
                    Array.Clear(m_items, m_size.Length, count * width);
            }
        }

        public virtual void Clear()
        {
            m_version++;
            if (IsReferenceOrContainsReferences<T>())
            {
                int size = m_items.Length;
                m_size.Height = 0;
                if (size > 0)
                    Array.Clear(m_items, 0, size);
            }
            else
                m_size.Height = 0;
        }

        #region Column Methods
        public virtual void AddColumn(int count = 1, IEnumerable<T[]> items = null)
        {
            int index = m_size.Width;

            if (items == null)
                InsertColumn(index, count);
            else
            {
                using (Enumeratorable emu = new Enumeratorable(items))
                {
                    InsertColumn(index, count, emu);
                }
            }
        }

        public virtual void InsertColumn(int index, int count = 1, IEnumerable<T[]> items = null)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            int size = m_size.Width;
            if ((uint)index > (uint)size)
                throw new ArgumentOutOfRangeException("index", "ListInsertColumn");

            int height = m_size.Height;
            int width = size + count;
            int end = index + count;

            T[,] array = new T[height, width];

            IEnumerator<T[]> emu = items != null ? items.GetEnumerator() : null;

            for (int y = 0; y < height; y++)
            {
                T[] colItems = emu != null && emu.MoveNext() ? emu.Current : new T[0];

                for (int x = 0; x < index; x++)
                {
                    array[y, x] = m_items[y, x];
                }

                for (int x = size - 1; x >= index; x--)
                {
                    array[y, x + count] = m_items[y, x];
                }

                int i = 0;
                for (int x = index; x < end; x++)
                {
                    if (i < colItems.Length)
                        array[y, x] = colItems[i];
                    else
                        array[y, x] = default(T);
                    i++;
                }
            }

            m_items = array;
            m_size.Width = width;
            m_version++;
        }

        public virtual void RemoveColumn(int index, int count = 1)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException("count");

            int size = m_size.Width;
            if ((uint)index > (uint)size)
                throw new ArgumentOutOfRangeException("index", "ListRemoveColumn");

            int height = m_size.Height;
            int width = size - count;

            T[,] array = new T[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = size - 1; x >= index; x--)
                {
                    if (x - count < 0) break;
                    array[y, x - count] = m_items[y, x];
                }
                for (int x = 0; x < index; x++)
                {
                    array[y, x] = m_items[y, x];
                }
            }

            m_items = array;
            m_size.Width = width;
            m_version++;
        }
        #endregion

        #region ForEach Methods
        public virtual void ForEach(Action<T[]> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            using (Enumeratorable emu = new Enumeratorable(this))
            {
                while (emu.MoveNext())
                {
                    action(emu.Current);
                }
            }
        }

        public virtual void ForEach(Action<T[], int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            using (Enumeratorable emu = new Enumeratorable(this))
            {
                while (emu.MoveNext())
                {
                    action(emu.Current, emu.Index);
                }
            }
        }

        public virtual void ForInColumn(int index, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (index < 0 || index >= m_size.Width)
                throw new IndexOutOfRangeException();

            for (int y = 0; y < m_size.Height; y++)
            {
                action(m_items[y, index]);
            }
        }

        public virtual void ForInColumn(int index, Action<T, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (index < 0 || index >= m_size.Width)
                throw new IndexOutOfRangeException();

            for (int y = 0; y < m_size.Height; y++)
            {
                action(m_items[y, index], y);
            }
        }

        public virtual void ForAllColumn(Action<T, int, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            for (int y = 0; y < m_size.Height; y++)
            {
                for (int x = 0; x < m_size.Width; x++)
                {
                    action(m_items[y, x], y, x);
                }
            }
        }
        #endregion

        #region Sort Methods
        public virtual void Sort(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= m_size.Width)
                throw new IndexOutOfRangeException();

            FunctorComparer fc = new FunctorComparer(this, columnIndex);
            T[,] sorted = fc.ToSortedArray(0, m_size.Height);
            Array.Copy(m_items, sorted, sorted.Length);
            m_version++;
        }

        public virtual void Sort(int columnIndex, IComparer<T> comparer)
            => Sort(columnIndex, 0, m_size.Height, comparer);

        public virtual void Sort(int columnIndex, int index, int count, IComparer<T> comparer)
        {
            if (columnIndex < 0 || columnIndex >= m_size.Width)
                throw new IndexOutOfRangeException();

            if (index < 0)
                throw new IndexOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            var size = m_size.Height;

            if (size - index < count)
                throw new ArgumentException();

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            FunctorComparer fc = new FunctorComparer(this, columnIndex, comparer);
            T[,] sorted = fc.ToSortedArray(index, count);
            Array.Copy(sorted, m_items, sorted.Length);
            m_version++;
        }

        public virtual void Sort(int columnIndex, Comparison<T> comparison)
        {
            if (columnIndex < 0 || columnIndex >= m_size.Width)
                throw new IndexOutOfRangeException();

            if (comparison == null)
                throw new ArgumentNullException("comparison");

            FunctorComparer fc = new FunctorComparer(this, columnIndex, comparison);
            T[,] sorted = fc.ToSortedArray(0, m_size.Height);
            Array.Copy(sorted, m_items, sorted.Length);
            m_version++;
        }

        public virtual void Sort(int columnIndex, Func<T, T, int> comparison)
        {
            if (columnIndex < 0 || columnIndex >= m_size.Width)
                throw new IndexOutOfRangeException();

            if (comparison == null)
                throw new ArgumentNullException("comparison");

            FunctorComparer fc = new FunctorComparer(this, columnIndex, comparison);
            T[,] sorted = fc.ToSortedArray(0, m_size.Height);
            Array.Copy(sorted, m_items, sorted.Length);
            m_version++;
        }
        #endregion

        #region IndexOf Methods
        public virtual Index IndexOf(T item)
        {
            int y = 0;
            using (Enumeratorable emu = new Enumeratorable(this))
            {
                while (emu.MoveNext())
                {
                    var x = Array.IndexOf(emu.Current, item);
                    if (x > -1)
                    {
                        return new Index(y, x);
                    }
                    y++;
                }
            }
            return Index.NotFound;
        }

        public virtual Index LastIndexOf(T item)
        {
            int y = m_size.Height - 1;
            using (Enumeratorable emu = new Enumeratorable(this, true))
            {
                while (emu.MoveNext())
                {
                    var x = Array.LastIndexOf(emu.Current, item);
                    if (x > -1)
                    {
                        return new Index(y, x);
                    }
                    y--;
                }
            }
            return Index.NotFound;
        }
        #endregion

        #region Find Methods
        #endregion

        #region Convertsion
        public virtual T[,] ToArray()
        {
            if (m_size.IsEmpty) return s_emptyArray;

            T[,] array = new T[m_size.Height, m_size.Width];

            if (m_items.GetLength(1) == m_size.Width)
                Array.Copy(m_items, array, m_size.Length);
            else
            {
                for (int y = 0; y < m_size.Height; y++)
                {
                    for (int x = 0; x < m_size.Width; x++)
                    {
                        array[y, x] = m_items[y, x];
                    }
                }
            }

            return array;
        }
        
        public virtual List<T[]> ToList()
        {
            List<T[]> list = new List<T[]>();
            using (Enumeratorable emu = new Enumeratorable(this))
            {
                while (emu.MoveNext())
                {
                    list.Add(emu.Current);
                }
            }
            return list;
        }
        #endregion

        #endregion

        #region Implemented Methods
        public virtual IEnumerator<T[]> GetEnumerator()
            => new Enumeratorable(this);

        IEnumerator<T[]> IEnumerable<T[]>.GetEnumerator()
            => new Enumeratorable(this);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumeratorable(this);
        #endregion

        #region Nested Types
        protected class Enumeratorable : IEnumerable<T[]>, IEnumerator<T[]>, IEnumerator
        {
            #region Variables
            private readonly int _height;
            private readonly int _version;

            private List2D<T> _list = null;
            private T[] _items = null;
            private T[] _itemsArray = null;
            private T[,] _items2dArray = null;
            private IEnumerator<T[]> _emu = null;

            private int _index = 0;
            private bool _reverse = false;
            private T[] _current = null;
            #endregion

            #region Properties
            public int Count
            {
                get => _height;
            }
            #endregion

            #region Constructors
            public Enumeratorable(T item, int height = 1)
            {
                _version = 0;
                _height = height;
                _items = new T[1];
                _items[0] = item;
            }

            public Enumeratorable(T[] items, bool horiz = true, int height = 1)
            {
                _version = 0;
                if (horiz)
                {
                    _height = height;
                    _items = items;
                }
                else
                {
                    _height = items.Length;
                    _itemsArray = items;
                }
            }

            public Enumeratorable(T[,] items)
            {
                _version = 0;
                _height = items.GetLength(0);
                _items2dArray = items;
            }

            public Enumeratorable(T[][] items)
            {
                _version = 0;
                _height = items.Length;
                _emu = items.GetEnumerator() as IEnumerator<T[]>;
            }

            public Enumeratorable(IEnumerable<T[]> items, bool forceCount = false)
            {
                _version = 0;
                if (items is ICollection<T[]> collection)
                    _height = collection.Count;
                else if (forceCount)
                    _height = items.Count();
                else
                    _height = int.MaxValue;

                _emu = items.GetEnumerator();
            }

            public Enumeratorable(IEnumerator<T[]> enumerator, bool forceCount = false)
            {
                _version = 0;
                if (forceCount)
                {
                    _height = 0;
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        _height++;
                    }
                }
                else
                    _height = int.MaxValue;

                enumerator.Reset();
                _emu = enumerator;
            }

            public Enumeratorable(List2D<T> list, bool reverse = false)
            {
                _version = list.m_version;
                _height = list.m_size.Height;
                _list = list;
                if (reverse)
                {
                    _reverse = true;
                    _index = _list.Height - 1;
                }
            }
            #endregion

            #region Properties
            public T[] Current => _current;

            public int Index => _index;

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _height + 1)
                        throw new InvalidOperationException();
                    return Current;
                }
            }
            #endregion

            #region Methods
            public IEnumerator<T[]> GetEnumerator() => this;

            IEnumerator IEnumerable.GetEnumerator() => this;

            public bool MoveNext()
            {
                if (_list != null)
                {
                    List2D<T> localList = _list;

                    int height = _list.m_size.Height;
                    int width = _list.m_size.Width;

                    if (_version == localList.m_version && ((uint)_index < (uint)height) && _index >= 0)
                    {
                        T[] arr = new T[width];
                        for (int i = 0; i < width; i++)
                        {
                            arr[i] = _list.m_items[_index, i];
                        }

                        _current = arr;
                        if (_reverse)
                            _index--;
                        else
                            _index++;

                        return true;
                    }
                }
                else if (_emu != null && _emu.MoveNext())
                {
                    _current = _emu.Current;
                    _index++;
                    return true;
                }
                else if (_emu == null && (uint)_index < (uint)_height)
                {
                    if (_items != null)
                    {
                        _current = _items;
                    }
                    else if (_itemsArray != null)
                    {
                        _current = new T[1];
                        _current[0] = _itemsArray[_index];
                    }
                    else if (_items2dArray != null)
                    {
                        int size = _items2dArray.GetLength(1);
                        _current = new T[size];
                        for (int i = 0; i < size; i++)
                        {
                            _current[i] = _items2dArray[_index, i];
                        }
                    }
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_list != null)
                {
                    if (_version != _list.m_version)
                        throw new InvalidOperationException();

                    if (_reverse)
                        _index = -1;
                    else
                        _index = _list.Height + 1;
                }
                else
                    _index = _height + 1;

                _current = null;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_reverse && _list != null)
                    _index = _list.Height - 1;
                else
                    _index = 0;

                if (_emu != null)
                    _emu.Reset();
            }

            public void Dispose()
            {
                if (_emu != null)
                    _emu.Dispose();
            }

            public T[,] ToArray()
            {
                if (_list != null) return _list.ToArray();

                if (_emu != null)
                {
                    if (_index == 0)
                    {
                        int len = 0;
                        List<T[]> list = new List<T[]>();
                        while(_emu.MoveNext())
                        {
                            len = Math.Max(len, _emu.Current.Length);
                            list.Add(_emu.Current);
                        }
                        _emu.Reset();

                        T[,] arr = new T[list.Count, len];
                        for (int y = 0; y < list.Count; y++)
                        {
                            T[] items = list[y];
                            for(int x = 0; x < len && x < items.Length; x++)
                            {
                                arr[y, x] = items[x];
                            }
                        }
                        return arr;
                    }
                }
                else if (_items != null)
                {
                    T[,] arr = new T[1, _items.Length];
                    for (int i = 0; i < _items.Length; i++)
                    {
                        arr[0, i] = _items[i];
                    }
                    return arr;
                }
                else if (_itemsArray != null)
                {
                    T[,] arr = new T[_itemsArray.Length, 1];
                    for(int i = 0; i < _itemsArray.Length; i++)
                    {
                        arr[i, 0] = _itemsArray[i];
                    }
                    return arr;
                }
                else if (_items2dArray != null)
                {
                    T[,] arr = new T[_items2dArray.GetLength(0), _items2dArray.GetLength(1)];
                    Array.Copy(_items2dArray, arr, _items2dArray.Length);
                    return arr;
                }

                return null;
            }
            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Size
        {
            #region Members
            public int Width;
            public int Height;

            public bool IsEmpty
            {
                get => Height == 0 || Width == 0;
            }

            public int Length
            {
                get => Width * Height;
            }
            #endregion

            #region Constructor
            public Size(int height, int width)
            {
                Width = width;
                Height = height;
            }
            #endregion

            #region Methods
            public override bool Equals(object obj)
            {
                if (obj is Size dd)
                {
                    return dd.Width == this.Width && dd.Height == this.Height;
                }
                else if (obj is Tuple<int, int> tp)
                {
                    return tp.Item2 == this.Width && tp.Item1 == this.Height;
                }
                else if (obj is int[] arr && arr.Length > 1)
                {
                    return arr[0] == this.Height && arr[1] == this.Width;
                }
                else if (obj is int i)
                {
                    return i == this.Height;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Width.GetHashCode() + Height.GetHashCode();
            }

            public override string ToString()
            {
                return $"{Height}x{Width}";
            }
            #endregion

            #region Operators
            public static implicit operator Size(int[] arr)
            {
                if (arr.Length < 2)
                    throw new IndexOutOfRangeException();
                return new Size(arr[0], arr[1]);
            }
            public static explicit operator int[](Size dimen)
                => new int[] { dimen.Height, dimen.Width };

            public static implicit operator Size(Tuple<int, int> arr)
                => new Size(arr.Item1, arr.Item2);
            public static explicit operator Tuple<int, int>(Size dimen)
                => new Tuple<int, int>(dimen.Height, dimen.Width);

            public static explicit operator int(Size dimen)
                => dimen.Length;

            public static Size operator +(Size a, int b)
                => new Size(a.Height + b, a.Width);
            public static Size operator -(Size a, int b)
                => new Size(a.Height - b, a.Width);

            public static Size operator ++(Size a)
                => new Size(a.Height + 1, a.Width);
            public static Size operator --(Size a)
                => new Size(a.Height - 1, a.Width);

            public static bool operator true(Size a)
                => a.Height > 0 && a.Width > 0;
            public static bool operator false(Size a)
                => a.Height == 0 || a.Width == 0;

            public static bool operator ==(Size a, Size b)
                => a.Height == b.Height && a.Width == b.Width;
            public static bool operator !=(Size a, Size b)
                => a.Height != b.Height || a.Width != b.Width;

            public static bool operator ==(Size a, int b)
                => a.Height == b;
            public static bool operator !=(Size a, int b)
                => a.Height != b;

            public static bool operator >(Size a, Size b)
                => a.Height > b.Height || a.Width > b.Width;
            public static bool operator <(Size a, Size b)
                => a.Height < b.Height || a.Width > b.Width;

            public static bool operator >(Size a, int b)
                => a.Height > b;
            public static bool operator <(Size a, int b)
                => a.Height < b;

            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Index
        {
            #region Members
            public int X;
            public int Y;

            public int Idx => Y * X;
            #endregion

            #region Constructor
            public Index(int y, int x)
            {
                Y = y;
                X = x;
            }
            #endregion

            #region Methods
            public override bool Equals(object obj)
            {
                if (obj is Index dd)
                {
                    return dd.Y == this.Y && dd.X == this.X;
                }
                else if (obj is Tuple<int, int> tp)
                {
                    return tp.Item2 == this.Y && tp.Item1 == this.X;
                }
                else if (obj is int[] arr && arr.Length > 1)
                {
                    return arr[0] == this.Y && arr[1] == this.X;
                }
                else if (obj is int i)
                {
                    return i == this.Y;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Y.GetHashCode() + X.GetHashCode();
            }

            public override string ToString()
            {
                return $"{X},{Y}";
            }

            internal static Index NotFound => new Index(-1, -1);
            #endregion

            #region Operators
            public static implicit operator Index(int[] arr)
            {
                if (arr.Length < 2)
                    throw new IndexOutOfRangeException();
                return new Index(arr[0], arr[1]);
            }
            public static explicit operator int[](Index index)
                => new int[] { index.Y, index.X };

            public static implicit operator Index(Tuple<int, int> arr)
                => new Index(arr.Item1, arr.Item2);
            public static explicit operator Tuple<int, int>(Index index)
                => new Tuple<int, int>(index.Y, index.X);

            public static explicit operator int(Index index)
                => index.Idx;
            #endregion
        }

        protected class FunctorComparer : IComparer<int>
        {
            #region Variables
            private readonly int _version;
            private readonly int[] _tagArray;
            private readonly int _sortIndex;
            private List2D<T> _list;

            private IComparer<T> comparer;
            private Comparison<T> comparison = null;
            private Func<T, T, int> comparisonFn = null;
            #endregion

            #region Constructor
            public FunctorComparer(List2D<T> list, int sortIndex)
            {
                _sortIndex = sortIndex;
                comparer = Comparer<T>.Default;

                _version = list.m_version;
                _list = list;

                int size = list.Height;
                _tagArray = new int[size];
                for (int i = 0; i < size; ++i) _tagArray[i] = i;
            }

            public FunctorComparer(List2D<T> list, int sortIndex, IComparer<T> comparer)
                : this(list, sortIndex)
            {
                if (comparer != null)
                    this.comparer = comparer;
            }

            public FunctorComparer(List2D<T> list, int sortIndex, Comparison<T> comparison)
                : this(list, sortIndex)
            {   
                this.comparison = comparison;
            }

            public FunctorComparer(List2D<T> list, int sortIndex, Func<T, T, int> comparison)
                : this(list, sortIndex)
            {
                comparisonFn = comparison;
            }
            #endregion

            #region Methods
            public int Compare(int x, int y)
            {
                int col = _sortIndex;
                if (col < 0) return 0;

                T a = _list.m_items[x, col];
                T b = _list.m_items[y, col];

                if (comparisonFn != null)
                    return comparisonFn(a, b);
                else if (comparison != null)
                    return comparison(a, b);
                else
                    return comparer.Compare(a, b);
            }

            public T[,] ToSortedArray(int index, int length)
            {
                Array.Sort(_tagArray, index, length, this);

                int height = _list.Height;
                int width = _list.Width;

                T[,] result = new T[height, width];
                for (int y = 0; y < height; y++)
                {
                    if (_version != _list.m_version) break;

                    for (int x = 0; x < width; x++)
                    {
                        result[y, x] = _list.m_items[_tagArray[y], x];
                    }
                }
                return result;
            }
            #endregion
        }
        #endregion
    }
}
