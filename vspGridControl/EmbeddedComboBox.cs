using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class EmbeddedComboBox : ComboBox, IGridEmbeddedControl, IGridEmbeddedControlManagement2, IGridEmbeddedControlManagement
    {
        protected int m_ColumnIndex;
        protected int m_MarginsWidth;
        protected StringFormat m_myStringFormat;
        protected long m_RowIndex;
        protected TextFormatFlags m_TextFormatFlags;

        public event ContentsChangedEventHandler ContentsChanged;

        protected EmbeddedComboBox()
        {
            this.m_RowIndex = -1L;
            this.m_ColumnIndex = -1;
        }

        public EmbeddedComboBox(Control parent, int MarginsWidth, ComboBoxStyle style)
        {
            this.m_RowIndex = -1L;
            this.m_ColumnIndex = -1;
            this.m_MarginsWidth = MarginsWidth;
            this.m_myStringFormat = new StringFormat(StringFormatFlags.LineLimit | StringFormatFlags.NoWrap);
            this.m_myStringFormat.HotkeyPrefix = HotkeyPrefix.None;
            this.m_myStringFormat.Trimming = StringTrimming.EllipsisCharacter;
            this.m_myStringFormat.LineAlignment = StringAlignment.Center;
            this.m_myStringFormat.Alignment = StringAlignment.Near;
            this.m_TextFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
            base.Parent = parent;
            base.DropDownStyle = style;
            base.Visible = false;
            base.IntegralHeight = false;
            base.DrawMode = DrawMode.OwnerDrawFixed;
            this.SetStringFormatRTL(this.RightToLeft == RightToLeft.Yes);
            if (style == ComboBoxStyle.DropDown)
            {
                base.TextChanged += new EventHandler(this.OnContentsChanged);
            }
            else
            {
                base.SelectedIndexChanged += new EventHandler(this.OnContentsChanged);
            }
        }

        public int AddDataAsString(string Item)
        {
            return base.Items.Add(Item);
        }

        public void ClearData()
        {
            base.Items.Clear();
            this.Text = "";
            if (base.IsHandleCreated)
            {
                base.Select(0, 1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.m_myStringFormat != null)
            {
                this.m_myStringFormat.Dispose();
                this.m_myStringFormat = null;
            }
            base.Dispose(disposing);
        }

        public string GetCurSelectionAsString()
        {
            return this.Text;
        }

        public int GetCurSelectionIndex()
        {
            return this.SelectedIndex;
        }

        private void OnContentsChanged(object sender, EventArgs args)
        {
            if (this.ContentsChanged != null)
            {
                this.ContentsChanged(this, EventArgs.Empty);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs a)
        {
            if (a.Index != -1)
            {
                a.DrawBackground();
                a.DrawFocusRectangle();
                Rectangle bounds = a.Bounds;
                bounds.Offset(2, 0);
                using (SolidBrush brush = new SolidBrush(a.ForeColor))
                {
                    object obj2 = base.Items[a.Index];
                    if (obj2 != null)
                    {
                        TextRenderer.DrawText(a.Graphics, obj2.ToString(), a.Font, bounds, brush.Color, this.m_TextFormatFlags);
                    }
                    else
                    {
                        TextRenderer.DrawText(a.Graphics, null, a.Font, bounds, brush.Color, this.m_TextFormatFlags);
                    }
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
        }

        protected override void OnRightToLeftChanged(EventArgs a)
        {
            base.OnRightToLeftChanged(a);
            this.SetStringFormatRTL(this.RightToLeft == RightToLeft.Yes);
        }

        public void PostProcessFocusFromKeyboard(System.Windows.Forms.Keys keyStroke, System.Windows.Forms.Keys modifiers)
        {
        }

        protected override bool ProcessKeyMessage(ref Message m)
        {
            System.Windows.Forms.Keys wParam = (System.Windows.Forms.Keys) ((int) m.WParam);
            System.Windows.Forms.Keys modifierKeys = Control.ModifierKeys;
            if (m.Msg == 0x102)
            {
                return this.ProcessKeyEventArgs(ref m);
            }
            System.Windows.Forms.Keys keys2 = wParam & System.Windows.Forms.Keys.KeyCode;
            System.Windows.Forms.Keys keys3 = keys2;
            if (keys3 != System.Windows.Forms.Keys.Tab)
            {
                if ((keys3 != System.Windows.Forms.Keys.Return) && (keys3 != System.Windows.Forms.Keys.Escape))
                {
                    return this.ProcessKeyEventArgs(ref m);
                }
            }
            else
            {
                return base.ProcessKeyPreview(ref m);
            }
            if (base.DroppedDown)
            {
                return this.ProcessKeyEventArgs(ref m);
            }
            return base.ProcessKeyPreview(ref m);
        }

        public void ReceiveChar(char c)
        {
            if (this.Enabled)
            {
                if (base.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    string str = "";
                    if (c.ToString() != null)
                    {
                        string str2 = c.ToString().ToLower(CultureInfo.CurrentUICulture);
                        string str3 = c.ToString().ToUpper(CultureInfo.CurrentUICulture);
                        for (int i = 0; i < base.Items.Count; i++)
                        {
                            str = base.Items[i].ToString();
                            if ((str != null) && (str.StartsWith(str2) || str.StartsWith(str3)))
                            {
                                this.SelectedIndex = i;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    this.Text = c.ToString();
                    base.SelectedText = string.Empty;
                    base.Select(1, 0);
                }
            }
        }

        public void ReceiveKeyboardEvent(KeyEventArgs ke)
        {
            if ((ke.KeyCode == System.Windows.Forms.Keys.F2) && (ke.Modifiers == System.Windows.Forms.Keys.None))
            {
                if (!base.ContainsFocus)
                {
                    base.Focus();
                }
                base.Select(this.Text.Length, 0);
            }
            else if ((ke.KeyCode == System.Windows.Forms.Keys.F4) || ((ke.KeyCode == System.Windows.Forms.Keys.Down) && ke.Alt))
            {
                if (!base.ContainsFocus)
                {
                    base.Focus();
                }
                base.DroppedDown = true;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            Rectangle bounds = base.Bounds;
            base.SetBoundsCore(x, y, width, height, specified);
            if ((bounds.Height != height) && base.IsHandleCreated)
            {
                base.ItemHeight = (height - SystemInformation.Border3DSize.Height) - (3 * SystemInformation.BorderSize.Height);
            }
        }

        public int SetCurSelectionAsString(string strNewSel)
        {
            int nIndex = base.FindStringExact(strNewSel);
            if (nIndex == -1)
            {
                if (base.DropDownStyle != ComboBoxStyle.DropDown)
                {
                    throw new InvalidOperationException(SRError.InvalidSelStringInEmbeddedCombo);
                }
                this.Text = strNewSel;
                base.Select(0, 1);
                return nIndex;
            }
            this.SetCurSelectionIndex(nIndex);
            return nIndex;
        }

        public void SetCurSelectionIndex(int nIndex)
        {
            if ((nIndex < 0) || (nIndex >= base.Items.Count))
            {
                throw new ArgumentException(SRError.InvalidSelIndexInEmbeddedCombo, "nIndex");
            }
            this.SelectedIndex = nIndex;
            base.Select(0, 1);
        }

        public void SetHorizontalAlignment(HorizontalAlignment alignment)
        {
        }

        private void SetStringFormatRTL(bool bRtl)
        {
            if (bRtl)
            {
                this.m_myStringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                this.m_TextFormatFlags |= TextFormatFlags.RightToLeft;
            }
            else
            {
                this.m_myStringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
                this.m_TextFormatFlags &= ~TextFormatFlags.RightToLeft;
            }
        }

        private void WmSetFont(ref Message m)
        {
            base.WndProc(ref m);
            if (base.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                IntPtr window = NativeMethods.GetWindow(base.Handle, 5);
                if (window != NativeMethods.NullIntPtr)
                {
                    NativeMethods.SendMessage(window, 0xd3, (IntPtr) 3, NativeMethods.Util.MAKELPARAM(this.m_MarginsWidth, this.m_MarginsWidth));
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x30)
            {
                this.WmSetFont(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.m_ColumnIndex;
            }
            set
            {
                this.m_ColumnIndex = value;
            }
        }

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }

        public long RowIndex
        {
            get
            {
                return this.m_RowIndex;
            }
            set
            {
                this.m_RowIndex = value;
            }
        }

        public bool WantMouseClick
        {
            get
            {
                return false;
            }
        }
    }
}

