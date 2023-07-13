using System;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public enum ButtonCellState
    {
        Pushed,
        Normal,
        Disabled,
        Empty
    }
	
	public enum GridButtonArea
    {
        Nothing,
        Text,
        Image,
        Background
    }
	
	public enum GridButtonType
    {
        Normal,
        Header,
        LineNumber
    }
	
	public enum GridCheckBoxState
    {
        Checked,
        Unchecked,
        Indeterminate,
        Disabled,
        None
    }
	
	public enum GridColumnHeaderType
    {
        Text,
        Bitmap,
        TextAndBitmap,
        CheckBox,
        TextAndCheckBox
    }
	
	public enum GridColumnWidthType
    {
        InPixels,
        InAverageFontChar
    }
	
	public enum GridLineType
    {
        None,
        Solid
    }
	
	public enum GridSelectionType
    {
        SingleRow,
        SingleCell,
        SingleColumn,
        RowBlocks,
        CellBlocks,
        ColumnBlocks
    }
	
	public enum TextBitmapLayout
    {
        NotApplicable,
        TextLeftOfBitmap,
        TextRightOfBitmap
    }
}

