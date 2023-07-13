using System;
using System.Collections;
using System.Reflection;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class BlockOfCells
    {
        private long m_Bottom;
        private int m_OriginalX;
        private long m_OriginalY;
        private int m_Right;
        private int m_X;
        private long m_Y;

        private BlockOfCells()
        {
            this.m_X = -1;
            this.m_Y = -1L;
            this.m_Right = -1;
            this.m_Bottom = -1L;
            this.m_OriginalX = -1;
            this.m_OriginalY = -1L;
        }

        public BlockOfCells(long nRowIndex, int nColIndex)
        {
            this.m_X = -1;
            this.m_Y = -1L;
            this.m_Right = -1;
            this.m_Bottom = -1L;
            this.m_OriginalX = -1;
            this.m_OriginalY = -1L;
            this.InitNewBlock(nRowIndex, nColIndex);
        }

        public bool Contains(long nRowIndex, int nColIndex)
        {
            return ((((nColIndex >= this.m_X) && (nColIndex <= this.m_Right)) && (nRowIndex >= this.m_Y)) && (nRowIndex <= this.m_Bottom));
        }

        private void InitNewBlock(long nRowIndex, int nColIndex)
        {
            this.m_OriginalX = this.m_X = this.m_Right = nColIndex;
            this.m_OriginalY = this.m_Y = this.m_Bottom = nRowIndex;
        }

        internal void SetOriginalCell(long rowIndex, int columnIndex)
        {
            if (!this.Contains(rowIndex, columnIndex))
            {
                throw new ArgumentException("", "rowIndex or columnIndex");
            }
            if (!this.IsEmpty)
            {
                int width = this.Width;
                long height = this.Height;
                this.m_OriginalY = rowIndex;
                this.m_OriginalX = columnIndex;
            }
        }

        internal void UpdateBlock(long nRowIndex, int nColIndex)
        {
            if (this.IsEmpty)
            {
                this.InitNewBlock(nRowIndex, nColIndex);
            }
            else
            {
                if (nRowIndex < this.m_OriginalY)
                {
                    this.m_Bottom = this.m_OriginalY;
                    this.m_Y = nRowIndex;
                }
                else
                {
                    this.m_Y = this.m_OriginalY;
                    this.m_Bottom = nRowIndex;
                }
                if (nColIndex < this.m_OriginalX)
                {
                    this.m_Right = this.m_OriginalX;
                    this.m_X = nColIndex;
                }
                else
                {
                    this.m_X = this.m_OriginalX;
                    this.m_Right = nColIndex;
                }
            }
        }

        public long Bottom
        {
            get
            {
                return this.m_Bottom;
            }
        }

        public long Height
        {
            get
            {
                if (this.IsEmpty)
                {
                    return 0L;
                }
                return ((this.m_Bottom - this.m_Y) + 1L);
            }
            set
            {
                if (value <= 0L)
                {
                    this.m_Y = this.m_Bottom = -1L;
                }
                else
                {
                    this.m_Bottom = (this.m_Y + value) - 1L;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (this.m_X != -1)
                {
                    return (this.m_Y == -1L);
                }
                return true;
            }
        }

        internal int LastUpdatedCol
        {
            get
            {
                if (this.m_X == this.m_OriginalX)
                {
                    return this.m_Right;
                }
                return this.m_X;
            }
        }

        internal long LastUpdatedRow
        {
            get
            {
                if (this.m_Y == this.m_OriginalY)
                {
                    return this.m_Bottom;
                }
                return this.m_Y;
            }
        }

        public int OriginalX
        {
            get
            {
                return this.m_OriginalX;
            }
        }

        public long OriginalY
        {
            get
            {
                return this.m_OriginalY;
            }
        }

        public int Right
        {
            get
            {
                return this.m_Right;
            }
        }

        public int Width
        {
            get
            {
                if (this.IsEmpty)
                {
                    return 0;
                }
                return ((this.m_Right - this.m_X) + 1);
            }
            set
            {
                if (value <= 0)
                {
                    this.m_X = this.m_Right = -1;
                }
                else
                {
                    this.m_Right = (this.m_X + value) - 1;
                }
            }
        }

        public int X
        {
            get
            {
                return this.m_X;
            }
            set
            {
                this.m_X = value;
            }
        }

        public long Y
        {
            get
            {
                return this.m_Y;
            }
            set
            {
                this.m_Y = value;
            }
        }
    }

    public class BlockOfCellsCollection : CollectionBase, IDisposable
    {
        public BlockOfCellsCollection()
        {
        }

        public BlockOfCellsCollection(BlockOfCellsCollection value)
        {
            this.AddRange(value);
        }

        public BlockOfCellsCollection(BlockOfCells[] value)
        {
            this.AddRange(value);
        }

        public int Add(BlockOfCells node)
        {
            return base.List.Add(node);
        }

        public void AddRange(BlockOfCells[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                this.Add(nodes[i]);
            }
        }

        public void AddRange(BlockOfCellsCollection value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(BlockOfCells node)
        {
            return base.List.Contains(node);
        }

        public void CopyTo(BlockOfCells[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public void Dispose()
        {
            base.Clear();
        }

        public int IndexOf(BlockOfCells node)
        {
            return base.List.IndexOf(node);
        }

        public void Insert(int index, BlockOfCells node)
        {
            base.List.Insert(index, node);
        }

        public void Remove(BlockOfCells node)
        {
            base.List.Remove(node);
        }

        public BlockOfCells this[int index]
        {
            get
            {
                return (BlockOfCells)base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

