using Microsoft.Win32;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public abstract class GridColumn : IDisposable
    {
        // Fields
        public static readonly int CELL_CONTENT_OFFSET = 3;

        protected HorizontalAlignment m_myAlign;
        protected SolidBrush m_myBackgroundBrush;
        protected int m_myColType;
        protected int m_myColumnIndex;
        protected TextBitmapLayout m_myTextBmpLayout;
        protected SolidBrush m_myTextBrush;
        protected bool m_myWidthInChars;
        protected int m_myWidthInPixels;
        private int m_origWidthInPixels;
        protected bool m_withRightGridLine;
        protected bool m_withSelectionBk;
        protected static bool s_defaultRTL = false;
        protected static readonly int s_defaultWidthInPixels = 20;
        protected static SolidBrush s_DisabledCellBKBrush = new SolidBrush(SystemColors.Control);
        protected static SolidBrush s_DisabledCellForeBrush = new SolidBrush(SystemColors.GrayText);

        // Methods
        static GridColumn()
        {
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(GridColumn.OnUserPrefChanged);
        }

        protected GridColumn()
        {
        }

        protected GridColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex)
        {
            this.m_myColType = ci.ColumnType;
            this.m_myAlign = ci.ColumnAlignment;
            this.m_withRightGridLine = ci.IsWithRightGridLine;
            this.m_myBackgroundBrush = new SolidBrush(ci.BackgroundColor);
            this.m_myTextBrush = new SolidBrush(ci.TextColor);
            this.m_myWidthInPixels = (nWidthInPixels >= 0) ? nWidthInPixels : s_defaultWidthInPixels;
            this.m_myColumnIndex = colIndex;
            this.m_myTextBmpLayout = ci.TextBmpCellsLayout;
            this.m_withSelectionBk = ci.IsWithSelectionBackground;
            this.m_myWidthInChars = ci.WidthType == GridColumnWidthType.InAverageFontChar;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.m_myBackgroundBrush != null)
            {
                this.m_myBackgroundBrush.Dispose();
                this.m_myBackgroundBrush = null;
            }
            if (this.m_myTextBrush != null)
            {
                this.m_myTextBrush.Dispose();
                this.m_myTextBrush = null;
            }
        }

        public virtual void DrawCell(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.DrawCell(g, bkBrush, (SolidBrush)textBrush, textFont, rect, storage, nRowIndex);
        }

        public virtual void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
        }

        public virtual void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.DrawCell(g, s_DisabledCellBKBrush, s_DisabledCellForeBrush, textFont, rect, storage, nRowIndex);
        }

        public virtual AccessibleStates GetAccessibleState(long nRowIndex, IGridStorage storage)
        {
            return AccessibleStates.None;
        }

        public virtual string GetAccessibleValue(long nRowIndex, IGridStorage storage)
        {
            return "";
        }

        public virtual bool IsPointOverTextInCell(Point pt, Rectangle cellRect, IGridStorage storage, long row, Graphics g, Font f)
        {
            return true;
        }

        private static void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (s_DisabledCellBKBrush != null)
            {
                s_DisabledCellBKBrush.Dispose();
                s_DisabledCellBKBrush = null;
            }
            if (s_DisabledCellForeBrush != null)
            {
                s_DisabledCellForeBrush.Dispose();
                s_DisabledCellForeBrush = null;
            }
            s_DisabledCellBKBrush = new SolidBrush(SystemColors.Control);
            s_DisabledCellForeBrush = new SolidBrush(SystemColors.GrayText);
        }

        public virtual void PrintCell(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.PrintCell(g, bkBrush, (SolidBrush)textBrush, textFont, rect, storage, nRowIndex);
        }

        public virtual void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.DrawCell(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex);
        }

        public virtual void ProcessNewGridFont(Font newFont)
        {
        }

        public virtual void SetRTL(bool bRightToLeft)
        {
        }

        // Properties
        public SolidBrush BackgroundBrush
        {
            get
            {
                return this.m_myBackgroundBrush;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_myColumnIndex;
            }
            set
            {
                this.m_myColumnIndex = value;
            }
        }

        public int ColumnType
        {
            get
            {
                return this.m_myColType;
            }
        }

        internal static Brush DisabledBackgroundBrush
        {
            get
            {
                return s_DisabledCellBKBrush;
            }
        }

        public bool IsWidthInChars
        {
            get
            {
                return this.m_myWidthInChars;
            }
        }

        internal int OrigWidthInPixelsDuringResize
        {
            get
            {
                return this.m_origWidthInPixels;
            }
            set
            {
                this.m_origWidthInPixels = value;
            }
        }

        public bool RightGridLine
        {
            get
            {
                return this.m_withRightGridLine;
            }
        }

        public HorizontalAlignment TextAlign
        {
            get
            {
                return this.m_myAlign;
            }
        }

        public TextBitmapLayout TextBitmapLayout
        {
            get
            {
                return this.m_myTextBmpLayout;
            }
        }

        public SolidBrush TextBrush
        {
            get
            {
                return this.m_myTextBrush;
            }
            set
            {
                this.m_myTextBrush = value;
            }
        }

        public int WidthInPixels
        {
            get
            {
                return this.m_myWidthInPixels;
            }
            set
            {
                if (value < 0)
                {
                    this.m_myWidthInPixels = s_defaultWidthInPixels;
                }
                else
                {
                    this.m_myWidthInPixels = value;
                }
            }
        }

        public bool WithSelectionBk
        {
            get
            {
                return this.m_withSelectionBk;
            }
        }
    }

    public class GridColumnCollection : CollectionBase, IDisposable
    {
        // Methods
        public GridColumnCollection()
        {
        }

        public GridColumnCollection(GridColumnCollection value)
        {
            this.AddRange(value);
        }

        public GridColumnCollection(GridColumn[] value)
        {
            this.AddRange(value);
        }

        public int Add(GridColumn node)
        {
            return base.List.Add(node);
        }

        public void AddRange(GridColumn[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                this.Add(nodes[i]);
            }
        }

        public void AddRange(GridColumnCollection value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(GridColumn node)
        {
            return base.List.Contains(node);
        }

        public void CopyTo(GridColumn[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public void Dispose()
        {
            foreach (GridColumn column in base.List)
            {
                column.Dispose();
            }
            base.Clear();
        }

        public int IndexOf(GridColumn node)
        {
            return base.List.IndexOf(node);
        }

        public void Insert(int index, GridColumn node)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (this[i].ColumnIndex >= index)
                {
                    GridColumn column1 = this[i];
                    column1.ColumnIndex++;
                }
            }
            base.List.Insert(index, node);
        }

        public void Move(int fromIndex, int toIndex)
        {
            GridColumn column = this[fromIndex];
            base.RemoveAt(fromIndex);
            base.List.Insert(toIndex, column);
        }

        public void ProcessNewGridFont(Font newFont)
        {
            foreach (GridColumn column in base.List)
            {
                column.ProcessNewGridFont(newFont);
            }
        }

        public void Remove(GridColumn node)
        {
            base.List.Remove(node);
        }

        public void RemoveAtAndAdjust(int index)
        {
            int columnIndex = this[index].ColumnIndex;
            for (int i = 0; i < base.Count; i++)
            {
                if (this[i].ColumnIndex > columnIndex)
                {
                    GridColumn column1 = this[i];
                    column1.ColumnIndex--;
                }
            }
            base.RemoveAt(index);
        }

        public void SetRTL(bool bRightToLeft)
        {
            foreach (GridColumn column in base.List)
            {
                column.SetRTL(bRightToLeft);
            }
        }

        // Properties
        public GridColumn this[int index]
        {
            get
            {
                return (GridColumn)base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}