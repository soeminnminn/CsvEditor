using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class GridSpecialEventArgs : MouseButtonDoubleClickedEventArgs
    {
        private object customData;
        private int eventType;
        public const int HyperlinkClick = 0;

        public GridSpecialEventArgs(int eventType, object data, HitTestResult htRes, long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn, GridButtonArea headerArea) : base(htRes, nRowIndex, nColIndex, rCellRect, btn, headerArea)
        {
            this.eventType = eventType;
            this.customData = data;
        }

        public object Data
        {
            get
            {
                return this.customData;
            }
        }

        public int EventType
        {
            get
            {
                return this.eventType;
            }
        }
    }

    public delegate void GridSpecialEventHandler(object sender, GridSpecialEventArgs sea);
}

