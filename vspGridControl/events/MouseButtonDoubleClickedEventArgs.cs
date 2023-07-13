using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class MouseButtonDoubleClickedEventArgs : EventArgs
    {
        private MouseButtons m_Button;
        private Rectangle m_CellRect;
        private int m_ColumnIndex;
        private GridButtonArea m_headerArea;
        private HitTestResult m_htRes;
        private long m_RowIndex;

        protected MouseButtonDoubleClickedEventArgs()
        {
        }

        public MouseButtonDoubleClickedEventArgs(HitTestResult htRes, long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn, GridButtonArea headerArea)
        {
            this.m_htRes = htRes;
            this.m_RowIndex = nRowIndex;
            this.m_ColumnIndex = nColIndex;
            this.m_CellRect = rCellRect;
            this.m_Button = btn;
            this.m_headerArea = headerArea;
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

        public GridButtonArea HeaderArea
        {
            get
            {
                return this.m_headerArea;
            }
        }

        public HitTestResult HitTest
        {
            get
            {
                return this.m_htRes;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_RowIndex;
            }
        }
    }

    public delegate void MouseButtonDoubleClickedEventHandler(object sender, MouseButtonDoubleClickedEventArgs args);
}

