using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class MouseButtonClickedEventArgs : EventArgs
    {
        private MouseButtons m_Button;
        private Rectangle m_CellRect;
        private int m_ColumnIndex;
        private long m_RowIndex;
        private bool m_ShouldRedraw;

        protected MouseButtonClickedEventArgs()
        {
            this.m_ShouldRedraw = true;
        }

        public MouseButtonClickedEventArgs(long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn)
        {
            this.m_ShouldRedraw = true;
            this.m_RowIndex = nRowIndex;
            this.m_ColumnIndex = nColIndex;
            this.m_CellRect = rCellRect;
            this.m_Button = btn;
        }

        public MouseButtons Button
        {
            get
            {
                return this.m_Button;
            }
        }

        public Rectangle CellRect
        {
            get
            {
                return this.m_CellRect;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_ColumnIndex;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_RowIndex;
            }
        }

        public bool ShouldRedraw
        {
            get
            {
                return this.m_ShouldRedraw;
            }
            set
            {
                this.m_ShouldRedraw = value;
            }
        }
    }

    public delegate void MouseButtonClickedEventHandler(object sender, MouseButtonClickedEventArgs args);
}

