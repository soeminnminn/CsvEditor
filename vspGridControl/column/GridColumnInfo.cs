using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class GridColumnInfo
    {
        // Fields
        public Color BackgroundColor = SystemColors.Window;
        public HorizontalAlignment ColumnAlignment = HorizontalAlignment.Left;
        public int ColumnType = GridColumnType.Text;
        public int ColumnWidth = 20;
        public HorizontalAlignment HeaderAlignment = HorizontalAlignment.Center;
        public GridColumnHeaderType HeaderType = GridColumnHeaderType.Text;
        public bool IsHeaderClickable = true;
        public bool IsHeaderMergedWithRight = false;
        public bool IsUserResizable = true;
        public bool IsWithRightGridLine = true;
        public bool IsWithSelectionBackground = true;
        public float MergedHeaderResizeProportion = 0f;
        public TextBitmapLayout TextBmpCellsLayout = TextBitmapLayout.NotApplicable;
        public TextBitmapLayout TextBmpHeaderLayout = TextBitmapLayout.NotApplicable;
        public Color TextColor = SystemColors.WindowText;
        public GridColumnWidthType WidthType = GridColumnWidthType.InAverageFontChar;
    }

    public class GridColumnInfoCollection : CollectionBase
    {
        // Methods
        public GridColumnInfoCollection()
        {
        }

        public GridColumnInfoCollection(GridColumnInfoCollection value)
        {
            this.AddRange(value);
        }

        public GridColumnInfoCollection(GridColumnInfo[] value)
        {
            this.AddRange(value);
        }

        public int Add(GridColumnInfo columnInfo)
        {
            return base.List.Add(columnInfo);
        }

        public void AddRange(GridColumnInfo[] columnInfos)
        {
            for (int i = 0; i < columnInfos.Length; i++)
            {
                this.Add(columnInfos[i]);
            }
        }

        public void AddRange(GridColumnInfoCollection value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(GridColumnInfo columnInfo)
        {
            return base.List.Contains(columnInfo);
        }

        public void CopyTo(GridColumnInfo[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(GridColumnInfo columnInfo)
        {
            return base.List.IndexOf(columnInfo);
        }

        public void Insert(int index, GridColumnInfo columnInfo)
        {
            base.List.Insert(index, columnInfo);
        }

        public void Remove(GridColumnInfo columnInfo)
        {
            base.List.Remove(columnInfo);
        }

        // Properties
        public GridColumnInfo this[int index]
        {
            get
            {
                return (GridColumnInfo)base.List[index];
            }
            set
            {
                throw new InvalidOperationException(SRError.GridColumnInfoCollectionIsReadOnly);
            }
        }
    }
}