using System;
using System.Drawing.Printing;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    internal class GridPrintDocument : PrintDocument
    {
        private GridControl.GridPrinter m_gridPrinter;

        private GridPrintDocument()
        {
        }

        internal GridPrintDocument(GridControl.GridPrinter gridPrinter)
        {
            this.m_gridPrinter = gridPrinter;
        }

        protected override void OnPrintPage(PrintPageEventArgs ev)
        {
            base.OnPrintPage(ev);
            this.m_gridPrinter.PrintPage(ev);
        }
    }
}

