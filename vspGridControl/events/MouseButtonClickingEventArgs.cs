using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class MouseButtonClickingEventArgs : EventArgs
    {
        private MouseButtons m_Button;
        private Rectangle m_CellRect;
        private int m_ColumnIndex;
        private System.Windows.Forms.Keys m_Modifiers;
        private long m_RowIndex;
        private bool m_ShouldHandle;

        protected MouseButtonClickingEventArgs()
        {
            this.m_ShouldHandle = true;
        }

        public MouseButtonClickingEventArgs(long nRowIndex, int nColIndex, Rectangle rCellRect, System.Windows.Forms.Keys mod, MouseButtons btn)
        {
            this.m_ShouldHandle = true;
            this.m_RowIndex = nRowIndex;
            this.m_ColumnIndex = nColIndex;
            this.m_CellRect = rCellRect;
            this.m_Modifiers = mod;
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

        public System.Windows.Forms.Keys Modifiers
        {
            get
            {
                return this.m_Modifiers;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_RowIndex;
            }
        }

        public bool ShouldHandle
        {
            get
            {
                return this.m_ShouldHandle;
            }
            set
            {
                this.m_ShouldHandle = value;
            }
        }
    }

    public delegate void MouseButtonClickingEventHandler(object sender, MouseButtonClickingEventArgs args);
}

