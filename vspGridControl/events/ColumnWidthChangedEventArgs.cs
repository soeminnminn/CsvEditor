using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class ColumnWidthChangedEventArgs : EventArgs
    {
        private int m_ColumnIndex;
        private int m_NewColumnWidth;

        protected ColumnWidthChangedEventArgs()
        {
        }

        public ColumnWidthChangedEventArgs(int nColIndex, int nNewColWidth)
        {
            this.m_ColumnIndex = nColIndex;
            this.m_NewColumnWidth = nNewColWidth;
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_ColumnIndex;
            }
        }

        public int NewColumnWidth
        {
            get
            {
                return this.m_NewColumnWidth;
            }
        }
    }

    public delegate void ColumnWidthChangedEventHandler(object sender, ColumnWidthChangedEventArgs args);
}

