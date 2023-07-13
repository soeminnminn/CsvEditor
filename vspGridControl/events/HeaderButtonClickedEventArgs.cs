using System;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class HeaderButtonClickedEventArgs : EventArgs
    {
        private MouseButtons m_Button;
        private int m_ColumnIndex;
        private GridButtonArea m_headerArea;
        private bool m_RepaintWholeGrid;

        protected HeaderButtonClickedEventArgs()
        {
        }

        public HeaderButtonClickedEventArgs(int nColIndex, MouseButtons btn, GridButtonArea headerArea)
        {
            this.m_ColumnIndex = nColIndex;
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

        public bool RepaintWholeGrid
        {
            get
            {
                return this.m_RepaintWholeGrid;
            }
            set
            {
                this.m_RepaintWholeGrid = value;
            }
        }
    }

    public delegate void HeaderButtonClickedEventHandler(object sender, HeaderButtonClickedEventArgs args);
}

