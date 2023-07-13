using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class GridHeader : IDisposable
    {
        private GridButton m_cachedButton = new GridButton();
        private System.Drawing.Font m_headerFont;
        private HeaderItemCollection m_Items = new HeaderItemCollection();

        public void DeleteItem(int nIndex)
        {
            this.m_Items.RemoveAt(nIndex);
        }

        public void Dispose()
        {
        }

        public void InsertHeaderItem(int nIndex, GridColumnInfo info)
        {
            HeaderItem node = new HeaderItem(info);
            this.m_Items.Insert(nIndex, node);
        }

        public void Move(int fromIndex, int toIndex)
        {
            this.m_Items.Move(fromIndex, toIndex);
        }

        public void Reset()
        {
            this.m_Items.Clear();
        }

        public void SetHeaderItemInfo(int nIndex, string strText, Bitmap bmp, GridCheckBoxState checkboxState)
        {
            HeaderItem item = this.m_Items[nIndex];
            if (item.MergedWithRight)
            {
                throw new ArgumentException(SRError.ShouldBeNoDataForMergedColumHeader, "nIndex");
            }
            item.Bmp = bmp;
            item.Text = strText;
            item.CheckboxState = checkboxState;
        }

        public void SetHeaderItemState(int nIndex, bool bPushed)
        {
            HeaderItem item = this.m_Items[nIndex];
            if (item.MergedWithRight)
            {
                throw new ArgumentException(SRError.CannotSetMergeItemState, "nIndex");
            }
            item.Pushed = bPushed;
        }

        public System.Drawing.Font Font
        {
            get
            {
                return this.m_headerFont;
            }
            set
            {
                this.m_headerFont = value;
            }
        }

        public GridButton HeaderGridButton
        {
            get
            {
                return this.m_cachedButton;
            }
        }

        public HeaderItem this[int nIndex]
        {
            get
            {
                return this.m_Items[nIndex];
            }
        }

        public class HeaderItem
        {
            private HorizontalAlignment m_Align;
            private bool m_bClickable;
            private bool m_bMergedWithRight;
            private Bitmap m_Bmp;
            private bool m_bPushed;
            private bool m_bResizable;
            private GridCheckBoxState m_checkboxState;
            private float m_mergedHeaderResizeProportion;
            private string m_strText;
            private TextBitmapLayout m_textBmpLayout;
            private GridColumnHeaderType m_Type;

            private HeaderItem()
            {
                this.m_checkboxState = GridCheckBoxState.None;
            }

            public HeaderItem(GridColumnInfo ci)
            {
                this.m_checkboxState = GridCheckBoxState.None;
                this.m_Type = ci.HeaderType;
                this.m_Align = ci.HeaderAlignment;
                this.m_bResizable = ci.IsUserResizable;
                this.m_bMergedWithRight = ci.IsHeaderMergedWithRight;
                this.m_textBmpLayout = ci.TextBmpHeaderLayout;
                this.m_mergedHeaderResizeProportion = ci.MergedHeaderResizeProportion;
                this.m_bClickable = ci.IsHeaderClickable;
            }

            public HorizontalAlignment Align
            {
                get
                {
                    return this.m_Align;
                }
            }

            public Bitmap Bmp
            {
                get
                {
                    if ((this.m_Type != GridColumnHeaderType.CheckBox) && (this.m_Type != GridColumnHeaderType.TextAndCheckBox))
                    {
                        return this.m_Bmp;
                    }
                    switch (this.m_checkboxState)
                    {
                        case GridCheckBoxState.Checked:
                            return GridConstants.CheckedCheckBoxBitmap;

                        case GridCheckBoxState.Unchecked:
                            return GridConstants.UncheckedCheckBoxBitmap;

                        case GridCheckBoxState.Indeterminate:
                            return GridConstants.IntermidiateCheckBoxBitmap;

                        case GridCheckBoxState.Disabled:
                            return GridConstants.DisabledCheckBoxBitmap;

                        case GridCheckBoxState.None:
                            return null;
                    }
                    return null;
                }
                set
                {
                    this.m_Bmp = value;
                }
            }

            public GridCheckBoxState CheckboxState
            {
                get
                {
                    return this.m_checkboxState;
                }
                set
                {
                    this.m_checkboxState = value;
                }
            }

            public bool Clickable
            {
                get
                {
                    return this.m_bClickable;
                }
            }

            public float MergedHeaderResizeProportion
            {
                get
                {
                    return this.m_mergedHeaderResizeProportion;
                }
                set
                {
                    this.m_mergedHeaderResizeProportion = value;
                }
            }

            public bool MergedWithRight
            {
                get
                {
                    return this.m_bMergedWithRight;
                }
            }

            public bool Pushed
            {
                get
                {
                    return this.m_bPushed;
                }
                set
                {
                    this.m_bPushed = value;
                }
            }

            public bool Resizable
            {
                get
                {
                    return this.m_bResizable;
                }
            }

            public string Text
            {
                get
                {
                    return this.m_strText;
                }
                set
                {
                    this.m_strText = value;
                }
            }

            public TextBitmapLayout TextBmpLayout
            {
                get
                {
                    return this.m_textBmpLayout;
                }
            }

            public GridColumnHeaderType Type
            {
                get
                {
                    return this.m_Type;
                }
            }
        }

        public class HeaderItemCollection : CollectionBase
        {
            public HeaderItemCollection()
            {
            }

            public HeaderItemCollection(GridHeader.HeaderItemCollection value)
            {
                this.AddRange(value);
            }

            public HeaderItemCollection(GridHeader.HeaderItem[] value)
            {
                this.AddRange(value);
            }

            public int Add(GridHeader.HeaderItem node)
            {
                return base.List.Add(node);
            }

            public void AddRange(GridHeader.HeaderItem[] nodes)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    this.Add(nodes[i]);
                }
            }

            public void AddRange(GridHeader.HeaderItemCollection value)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    this.Add(value[i]);
                }
            }

            public bool Contains(GridHeader.HeaderItem node)
            {
                return base.List.Contains(node);
            }

            public void CopyTo(GridHeader.HeaderItem[] array, int index)
            {
                base.List.CopyTo(array, index);
            }

            public int IndexOf(GridHeader.HeaderItem node)
            {
                return base.List.IndexOf(node);
            }

            public void Insert(int index, GridHeader.HeaderItem node)
            {
                base.List.Insert(index, node);
            }

            public void Move(int fromIndex, int toIndex)
            {
                GridHeader.HeaderItem node = this[fromIndex];
                base.RemoveAt(fromIndex);
                this.Insert(toIndex, node);
            }

            public void Remove(GridHeader.HeaderItem node)
            {
                base.List.Remove(node);
            }

            public GridHeader.HeaderItem this[int index]
            {
                get
                {
                    return (GridHeader.HeaderItem) base.List[index];
                }
                set
                {
                    base.List[index] = value;
                }
            }
        }
    }
}

