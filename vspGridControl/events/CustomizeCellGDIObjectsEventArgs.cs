using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class CustomizeCellGDIObjectsEventArgs : EventArgs
    {
        private SolidBrush m_bkBrush;
        private Font m_cellFont;
        private int m_columnIndex;
        private long m_rowIndex;
        private SolidBrush m_textBrush;

        internal void SetRowAndColumn(long nRowIndex, int nColIndex)
        {
            this.m_rowIndex = nRowIndex;
            this.m_columnIndex = nColIndex;
        }

        public SolidBrush BKBrush
        {
            get
            {
                return this.m_bkBrush;
            }
            set
            {
                this.m_bkBrush = value;
            }
        }

        public Font CellFont
        {
            get
            {
                return this.m_cellFont;
            }
            set
            {
                this.m_cellFont = value;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_columnIndex;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_rowIndex;
            }
        }

        public SolidBrush TextBrush
        {
            get
            {
                return this.m_textBrush;
            }
            set
            {
                this.m_textBrush = value;
            }
        }
    }

    public delegate void CustomizeCellGDIObjectsEventHandler(object sender, CustomizeCellGDIObjectsEventArgs args);
}

