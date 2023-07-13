using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class GridCheckBoxColumn : GridBitmapColumn
    {
        protected int m_CurrentCheckSize;
        protected Bitmap m_CheckedBitmap;
        protected Bitmap m_DisabledBitmap;
        protected Bitmap m_IntermidiateBitmap;
        protected Bitmap m_UncheckedBitmap;

        protected GridCheckBoxColumn()
        {
            this.m_CurrentCheckSize = GridConstants.StandardCheckBoxSize;
        }

        public GridCheckBoxColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
            : base(ci, nWidthInPixels, colIndex)
        {
            this.m_CurrentCheckSize = GridConstants.StandardCheckBoxSize;
            this.m_CheckedBitmap = GridConstants.CheckedCheckBoxBitmap;
            this.m_UncheckedBitmap = GridConstants.UncheckedCheckBoxBitmap;
            this.m_IntermidiateBitmap = GridConstants.IntermidiateCheckBoxBitmap;
            this.m_DisabledBitmap = GridConstants.DisabledCheckBoxBitmap;
            this.CalcCheckboxSize();
        }

        public Bitmap BitmapFromGridCheckBoxState(GridCheckBoxState state)
        {
            Bitmap disabledBitmap = null;
            if (state == GridCheckBoxState.Checked)
            {
                return this.m_CheckedBitmap;
            }
            if (state == GridCheckBoxState.Unchecked)
            {
                return this.m_UncheckedBitmap;
            }
            if (state == GridCheckBoxState.Indeterminate)
            {
                return this.m_IntermidiateBitmap;
            }
            if (state == GridCheckBoxState.Disabled)
            {
                disabledBitmap = this.m_DisabledBitmap;
            }
            return disabledBitmap;
        }

        private void CalcCheckboxSize()
        {
            this.m_CurrentCheckSize = this.m_CheckedBitmap.Size.Height;
            if (this.m_UncheckedBitmap.Size.Height > this.m_CurrentCheckSize)
            {
                this.m_CurrentCheckSize = this.m_UncheckedBitmap.Size.Height;
            }
            if (this.m_IntermidiateBitmap.Size.Height > this.m_CurrentCheckSize)
            {
                this.m_CurrentCheckSize = this.m_IntermidiateBitmap.Size.Height;
            }
            if (this.m_DisabledBitmap.Size.Height > this.m_CurrentCheckSize)
            {
                this.m_CurrentCheckSize = this.m_DisabledBitmap.Size.Height;
            }
        }

        public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            GridCheckBoxState cellDataForCheckBox = storage.GetCellDataForCheckBox(nRowIndex, base.m_myColumnIndex);
            g.FillRectangle(bkBrush, rect);
            Bitmap myBmp = this.BitmapFromGridCheckBoxState(cellDataForCheckBox);
            this.DrawBitmap(g, bkBrush, rect, myBmp, true);
        }

        public override void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            GridCheckBoxState cellDataForCheckBox = storage.GetCellDataForCheckBox(nRowIndex, base.m_myColumnIndex);
            g.FillRectangle(GridColumn.s_DisabledCellBKBrush, rect);
            Bitmap myBmp = this.BitmapFromGridCheckBoxState(cellDataForCheckBox);
            this.DrawBitmap(g, GridColumn.s_DisabledCellBKBrush, rect, myBmp, false);
        }

        public override AccessibleStates GetAccessibleState(long nRowIndex, IGridStorage storage)
        {
            switch (storage.GetCellDataForCheckBox(nRowIndex, base.m_myColumnIndex))
            {
                case GridCheckBoxState.Checked:
                    return AccessibleStates.Checked;

                case GridCheckBoxState.Unchecked:
                    return AccessibleStates.None;

                case GridCheckBoxState.Indeterminate:
                    return AccessibleStates.Mixed;
            }
            return base.GetAccessibleState(nRowIndex, storage);
        }

        public override string GetAccessibleValue(long nRowIndex, IGridStorage storage)
        {
            return storage.GetCellDataForCheckBox(nRowIndex, base.m_myColumnIndex).ToString();
        }

        public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            GridCheckBoxState cellDataForCheckBox = storage.GetCellDataForCheckBox(nRowIndex, base.m_myColumnIndex);
            g.FillRectangle(bkBrush, rect.X - 1, rect.Y, rect.Width, rect.Height);
            Bitmap myBmp = this.BitmapFromGridCheckBoxState(cellDataForCheckBox);
            this.DrawBitmap(g, bkBrush, rect, myBmp, true);
        }

        public void SetCheckboxBitmaps(Bitmap checkedState, Bitmap uncheckedState, Bitmap indeterminateState, Bitmap disabledState)
        {
            if (checkedState != null)
            {
                this.m_CheckedBitmap = checkedState;
            }
            if (uncheckedState != null)
            {
                this.m_UncheckedBitmap = uncheckedState;
            }
            if (indeterminateState != null)
            {
                this.m_IntermidiateBitmap = indeterminateState;
            }
            if (disabledState != null)
            {
                this.m_DisabledBitmap = disabledState;
            }
            this.CalcCheckboxSize();
        }

        public int CheckBoxHeight
        {
            get
            {
                return this.m_CurrentCheckSize;
            }
        }
    }
}

