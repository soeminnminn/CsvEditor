using System;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class SelectionManager
    {
        private bool m_bOnlyOneSelItem = true;
        private int m_curColIndex = -1;
        private long m_curRowIndex = -1L;
        private int m_curSelBlockIndex = -1;
        private BlockOfCellsCollection m_selBlocks = new BlockOfCellsCollection();
        private GridSelectionType m_selType = GridSelectionType.SingleCell;

        public void Clear()
        {
            this.Clear(true);
        }

        public void Clear(bool bClearCurrentCell)
        {
            this.m_selBlocks.Clear();
            this.ResetCurrentBlock();
            if (bClearCurrentCell)
            {
                this.m_curColIndex = -1;
                this.m_curRowIndex = -1L;
            }
        }

        private int GetSelBlockIndexForCell(long nRowIndex, int nColIndex)
        {
            for (int i = 0; i < this.m_selBlocks.Count; i++)
            {
                if (this.IsCellSelectedInt(this.m_selBlocks[i], nRowIndex, nColIndex))
                {
                    return i;
                }
            }
            return -1;
        }

        public int GetSelecttionBlockNumberForCell(long nRowIndex, int nColIndex)
        {
            for (int i = 0; i < this.m_selBlocks.Count; i++)
            {
                if (this.IsCellSelectedInt(this.m_selBlocks[i], nRowIndex, nColIndex))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool IsCellSelected(long nRowIndex, int nColIndex)
        {
            foreach (BlockOfCells cells in this.m_selBlocks)
            {
                if (this.IsCellSelectedInt(cells, nRowIndex, nColIndex))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsCellSelectedInt(BlockOfCells block, long nRowIndex, int nColIndex)
        {
            if ((this.m_selType == GridSelectionType.CellBlocks) || (this.m_selType == GridSelectionType.SingleCell))
            {
                if (block.Contains(nRowIndex, nColIndex))
                {
                    return true;
                }
            }
            else if ((this.m_selType == GridSelectionType.ColumnBlocks) || (this.m_selType == GridSelectionType.SingleColumn))
            {
                if ((nColIndex >= block.X) && (nColIndex <= block.Right))
                {
                    return true;
                }
            }
            else if ((nRowIndex >= block.Y) && (nRowIndex <= block.Bottom))
            {
                return true;
            }
            return false;
        }

        public void ResetCurrentBlock()
        {
            this.m_curSelBlockIndex = -1;
        }

        public bool SetCurrentCell(long rowIndex, int columnIndex)
        {
            if (this.m_curSelBlockIndex == -1)
            {
                return false;
            }
            BlockOfCells cells = this.m_selBlocks[this.m_curSelBlockIndex];
            if (!cells.Contains(rowIndex, columnIndex))
            {
                return false;
            }
            this.m_curColIndex = columnIndex;
            this.m_curRowIndex = rowIndex;
            cells.SetOriginalCell(rowIndex, columnIndex);
            return true;
        }

        private void SplitOneColumnsBlock(long nRowIndex, int nColIndex, int nIndexOfBlock)
        {
            BlockOfCells cells = this.m_selBlocks[nIndexOfBlock];
            if (cells.Width == 1)
            {
                this.m_selBlocks.RemoveAt(nIndexOfBlock);
            }
            else if (cells.X == nColIndex)
            {
                cells.X++;
            }
            else if (cells.Right == nColIndex)
            {
                cells.Width--;
            }
            else
            {
                this.m_selBlocks.RemoveAt(nIndexOfBlock);
                BlockOfCells node = new BlockOfCells(cells.Y, cells.X);
                node.UpdateBlock(cells.Bottom, nColIndex - 1);
                this.m_selBlocks.Add(node);
                node = new BlockOfCells(cells.Y, nColIndex + 1);
                node.UpdateBlock(cells.Bottom, cells.Right);
                this.m_selBlocks.Add(node);
            }
        }

        private void SplitOneRowsBlock(long nRowIndex, int nColIndex, int nIndexOfBlock)
        {
            BlockOfCells cells = this.m_selBlocks[nIndexOfBlock];
            if (cells.Height == 1L)
            {
                this.m_selBlocks.RemoveAt(nIndexOfBlock);
            }
            else if (cells.Y == nRowIndex)
            {
                cells.Y += 1L;
            }
            else if (cells.Bottom == nRowIndex)
            {
                cells.Height -= 1L;
            }
            else
            {
                this.m_selBlocks.RemoveAt(nIndexOfBlock);
                BlockOfCells node = new BlockOfCells(cells.Y, cells.X);
                node.UpdateBlock(nRowIndex - 1L, cells.Right);
                this.m_selBlocks.Add(node);
                node = new BlockOfCells(nRowIndex + 1L, cells.X);
                node.UpdateBlock(cells.Bottom, cells.Right);
                this.m_selBlocks.Add(node);
            }
        }

        public void StartNewBlock(long nRowIndex, int nColIndex)
        {
            this.m_curRowIndex = nRowIndex;
            this.m_curColIndex = nColIndex;
            BlockOfCells node = new BlockOfCells(nRowIndex, nColIndex);
            if (this.m_bOnlyOneSelItem)
            {
                this.m_selBlocks.Clear();
            }
            this.m_curSelBlockIndex = this.m_selBlocks.Add(node);
        }

        public bool StartNewBlockOrExcludeCell(long nRowIndex, int nColIndex)
        {
            int selBlockIndexForCell = this.GetSelBlockIndexForCell(nRowIndex, nColIndex);
            if (((selBlockIndexForCell == -1) || this.m_bOnlyOneSelItem) || (this.m_selType == GridSelectionType.CellBlocks))
            {
                this.StartNewBlock(nRowIndex, nColIndex);
                return true;
            }
            this.ResetCurrentBlock();
            while (selBlockIndexForCell != -1)
            {
                if (this.m_selType == GridSelectionType.ColumnBlocks)
                {
                    this.SplitOneColumnsBlock(nRowIndex, nColIndex, selBlockIndexForCell);
                }
                else
                {
                    this.SplitOneRowsBlock(nRowIndex, nColIndex, selBlockIndexForCell);
                }
                selBlockIndexForCell = this.GetSelBlockIndexForCell(nRowIndex, nColIndex);
            }
            if (this.m_selBlocks.Count == 0)
            {
                this.m_curRowIndex = -1L;
                this.m_curColIndex = -1;
            }
            return false;
        }

        public void UpdateCurrentBlock(long nRowIndex, int nColIndex)
        {
            if ((this.m_bOnlyOneSelItem || (this.m_selBlocks.Count == 0)) || (this.m_curSelBlockIndex == -1))
            {
                this.StartNewBlock(nRowIndex, nColIndex);
            }
            else
            {
                this.m_selBlocks[this.m_curSelBlockIndex].UpdateBlock(nRowIndex, nColIndex);
            }
        }

        public int CurrentColumn
        {
            get
            {
                return this.m_curColIndex;
            }
            set
            {
                this.m_curColIndex = value;
            }
        }

        public long CurrentRow
        {
            get
            {
                return this.m_curRowIndex;
            }
            set
            {
                this.m_curRowIndex = value;
            }
        }

        public int CurrentSelectionBlockIndex
        {
            get
            {
                return this.m_curSelBlockIndex;
            }
        }

        public int LastUpdatedColumn
        {
            get
            {
                if (this.m_curSelBlockIndex == -1)
                {
                    return -1;
                }
                return this.m_selBlocks[this.m_curSelBlockIndex].LastUpdatedCol;
            }
        }

        public long LastUpdatedRow
        {
            get
            {
                if (this.m_curSelBlockIndex == -1)
                {
                    return -1L;
                }
                return this.m_selBlocks[this.m_curSelBlockIndex].LastUpdatedRow;
            }
        }

        public bool OnlyOneCellSelected
        {
            get
            {
                if (this.SelectedBlocks.Count != 1)
                {
                    return false;
                }
                return ((this.SelectedBlocks[0].Width == 1) && (this.SelectedBlocks[0].Height == 1L));
            }
        }

        public bool OnlyOneSelItem
        {
            get
            {
                return this.m_bOnlyOneSelItem;
            }
        }

        public BlockOfCellsCollection SelectedBlocks
        {
            get
            {
                return this.m_selBlocks;
            }
        }

        public GridSelectionType SelectionType
        {
            get
            {
                return this.m_selType;
            }
            set
            {
                if (this.m_selType != value)
                {
                    this.m_selType = value;
                    this.m_bOnlyOneSelItem = ((this.m_selType == GridSelectionType.SingleCell) || (this.m_selType == GridSelectionType.SingleColumn)) || (this.m_selType == GridSelectionType.SingleRow);
                }
            }
        }

        internal bool SingleRowOrColumnSelectedInMultiSelectionMode
        {
            get
            {
                if (this.m_selBlocks.Count != 1)
                {
                    return false;
                }
                return (((this.m_selBlocks[0].Height == 1L) && (this.m_selType == GridSelectionType.RowBlocks)) || ((this.m_selBlocks[0].Width == 1) && (this.m_selType == GridSelectionType.ColumnBlocks)));
            }
        }
    }
}

