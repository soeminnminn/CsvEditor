using System;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class StandardKeyProcessingEventArgs : EventArgs
    {
        private System.Windows.Forms.Keys m_Key;
        private System.Windows.Forms.Keys m_Modifiers;
        private bool m_ShouldHandle;

        protected StandardKeyProcessingEventArgs()
        {
            this.m_ShouldHandle = true;
        }

        public StandardKeyProcessingEventArgs(KeyEventArgs ke)
        {
            this.m_ShouldHandle = true;
            this.m_Key = ke.KeyCode;
            this.m_Modifiers = ke.Modifiers;
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

    public delegate void StandardKeyProcessingEventHandler(object sender, StandardKeyProcessingEventArgs args);
}

