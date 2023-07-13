using System;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class EmbeddedControlContentsChangedEventArgs : EventArgs
    {
        private IGridEmbeddedControl m_EmbeddedControl;

        public EmbeddedControlContentsChangedEventArgs(IGridEmbeddedControl embCtrl)
        {
            this.m_EmbeddedControl = embCtrl;
        }

        public IGridEmbeddedControl EmbeddedControl
        {
            get
            {
                return this.m_EmbeddedControl;
            }
        }
    }

    public delegate void EmbeddedControlContentsChangedEventHandler(object sender, EmbeddedControlContentsChangedEventArgs args);
}

