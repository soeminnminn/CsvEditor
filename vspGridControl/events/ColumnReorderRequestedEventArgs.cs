using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class ColumnReorderRequestedEventArgs : EventArgs
    {
        private bool m_bAllowReorder;
        private int m_colIndex = -1;

        public ColumnReorderRequestedEventArgs(int nColumnIndex, bool reordableByDefault)
        {
            this.m_colIndex = nColumnIndex;
            this.m_bAllowReorder = reordableByDefault;
        }

        public bool AllowReorder
        {
            get
            {
                return this.m_bAllowReorder;
            }
            set
            {
                this.m_bAllowReorder = value;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_colIndex;
            }
        }
    }

    public delegate void ColumnReorderRequestedEventHandler(object sender, ColumnReorderRequestedEventArgs a);
}

