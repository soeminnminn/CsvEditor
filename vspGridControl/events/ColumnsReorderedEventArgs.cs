using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class ColumnsReorderedEventArgs : EventArgs
    {
        private int m_newColumnIndex = -1;
        private int m_origColumnIndex = -1;

        public ColumnsReorderedEventArgs(int origIndex, int newIndex)
        {
            this.m_origColumnIndex = origIndex;
            this.m_newColumnIndex = newIndex;
        }

        public int NewColumnIndex
        {
            get
            {
                return this.m_newColumnIndex;
            }
        }

        public int OriginalColumnIndex
        {
            get
            {
                return this.m_origColumnIndex;
            }
        }
    }

    public delegate void ColumnsReorderedEventHandler(object sender, ColumnsReorderedEventArgs a);
}

