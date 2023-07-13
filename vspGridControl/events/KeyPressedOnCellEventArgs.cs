using System;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class KeyPressedOnCellEventArgs : EventArgs
    {
        private int m_ColumnIndex;
        private System.Windows.Forms.Keys m_Key;
        private System.Windows.Forms.Keys m_Modifiers;
        private long m_RowIndex;

        protected KeyPressedOnCellEventArgs()
        {
        }

        public KeyPressedOnCellEventArgs(long nCurRow, int nCurCol, System.Windows.Forms.Keys k, System.Windows.Forms.Keys m)
        {
            this.m_RowIndex = nCurRow;
            this.m_ColumnIndex = nCurCol;
            this.m_Key = k;
            this.m_Modifiers = m;
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_ColumnIndex;
            }
        }

        public System.Windows.Forms.Keys Key
        {
            get
            {
                return this.m_Key;
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
    }

    public delegate void KeyPressedOnCellEventHandler(object sender, KeyPressedOnCellEventArgs args);
}

