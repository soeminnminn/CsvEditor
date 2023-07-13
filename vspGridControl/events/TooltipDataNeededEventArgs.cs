using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class TooltipDataNeededEventArgs : EventArgs
    {
        private int m_colIndex;
        private HitTestResult m_htResult;
        private long m_rowIndex;
        private string m_toolTip;

        public TooltipDataNeededEventArgs(HitTestResult ht, long rowIndex, int colIndex)
        {
            this.m_htResult = ht;
            this.m_rowIndex = rowIndex;
            this.m_colIndex = colIndex;
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_colIndex;
            }
        }

        public HitTestResult HitTest
        {
            get
            {
                return this.m_htResult;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_rowIndex;
            }
        }

        public string TooltipText
        {
            get
            {
                return this.m_toolTip;
            }
            set
            {
                this.m_toolTip = value;
            }
        }
    }

    public delegate void TooltipDataNeededEventHandler(object sender, TooltipDataNeededEventArgs a);
}

