using System;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class GridLineNumberColumn : GridButtonColumn
    {
        public GridLineNumberColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
            : base(ci, nWidthInPixels, colIndex)
        {
            IsLineIndexButton = true;
        }
    }
}
