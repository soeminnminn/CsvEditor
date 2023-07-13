using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class EmbeddedSpinBox : NumericUpDown, IGridEmbeddedControl, IGridEmbeddedControlManagement2, IGridEmbeddedControlManagement, IGridEmbeddedSpinControl
    {
        protected int m_ColumnIndex;
        protected int m_MarginsWidth;
        protected int m_myDefWidth;
        protected long m_RowIndex;

        public event ContentsChangedEventHandler ContentsChanged;

        protected EmbeddedSpinBox()
        {
            this.m_RowIndex = -1L;
            this.m_ColumnIndex = -1;
            this.m_myDefWidth = 120;
        }

        public EmbeddedSpinBox(Control parent, int MarginsWidth)
        {
            this.m_RowIndex = -1L;
            this.m_ColumnIndex = -1;
            this.m_myDefWidth = 120;
            this.m_MarginsWidth = MarginsWidth;
            this.OnFontChanged(new EventArgs());
            base.Parent = parent;
            base.Value = this.Minimum;
            base.Visible = false;
            base.ValueChanged += new EventHandler(this.OnValueChanged);
            foreach (Control control in base.Controls)
            {
                if (control is TextBox)
                {
                    control.TextChanged += new EventHandler(this.OnValueChanged);
                }
            }
        }

        public int AddDataAsString(string Item)
        {
            this.SetDataInternal(Item);
            return 0;
        }

        public void ClearData()
        {
            this.Text = this.Minimum.ToString(CultureInfo.CurrentUICulture);
            base.Select(0, 0);
        }

        public string GetCurSelectionAsString()
        {
            return this.Text;
        }

        public int GetCurSelectionIndex()
        {
            return 0;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            foreach (Control control in base.Controls)
            {
                if (control is TextBox)
                {
                    NativeMethods.SendMessage(control.Handle, 0xd3, (IntPtr) 3, NativeMethods.Util.MAKELPARAM(this.m_MarginsWidth, this.m_MarginsWidth));
                }
            }
        }

        protected override void OnTextBoxResize(object source, EventArgs e)
        {
            int height = base.Height;
            base.OnTextBoxResize(source, e);
            base.Height = height;
        }

        private void OnValueChanged(object sender, EventArgs args)
        {
            if (this.ContentsChanged != null)
            {
                this.ContentsChanged(this, EventArgs.Empty);
            }
        }

        public void PostProcessFocusFromKeyboard(System.Windows.Forms.Keys keyStroke, System.Windows.Forms.Keys modifiers)
        {
            string text = this.Text;
            if (text != null)
            {
                base.Select(0, text.Length);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyPreview(ref Message m)
        {
            System.Windows.Forms.Keys wParam = (System.Windows.Forms.Keys) ((int) m.WParam);
            System.Windows.Forms.Keys modifierKeys = Control.ModifierKeys;
            if (((wParam | modifierKeys) == System.Windows.Forms.Keys.Return) || ((wParam | modifierKeys) == System.Windows.Forms.Keys.Escape))
            {
                if (m.Msg == 0x102)
                {
                    return true;
                }
                base.ValidateEditText();
                return base.ProcessKeyPreview(ref m);
            }
            if (m.Msg != 0x102)
            {
                switch ((wParam & System.Windows.Forms.Keys.KeyCode))
                {
                    case System.Windows.Forms.Keys.Prior:
                    case System.Windows.Forms.Keys.Next:
                    case System.Windows.Forms.Keys.Tab:
                        return base.ProcessKeyPreview(ref m);
                }
            }
            return this.ProcessKeyEventArgs(ref m);
        }

        public void ReceiveChar(char c)
        {
            if (!base.ReadOnly)
            {
                try
                {
                    this.Text = c.ToString();
                    base.Select(1, 0);
                }
                catch (Exception)
                {
                }
            }
        }

        public void ReceiveKeyboardEvent(KeyEventArgs ke)
        {
            bool flag = false;
            if (!base.ReadOnly && (ke.Modifiers == System.Windows.Forms.Keys.None))
            {
                if (ke.KeyCode == System.Windows.Forms.Keys.F2)
                {
                    base.Select(this.Text.Length, 0);
                    flag = true;
                }
                else if ((ke.KeyCode == System.Windows.Forms.Keys.Back) || (ke.KeyCode == System.Windows.Forms.Keys.Delete))
                {
                    this.Text = string.Empty;
                    flag = true;
                }
                if (!base.ContainsFocus && flag)
                {
                    base.Focus();
                }
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            Rectangle bounds = base.Bounds;
            if (((bounds.X != x) || (bounds.Y != y)) || ((bounds.Width != width) || (bounds.Height != height)))
            {
                if (base.Handle != NativeMethods.NullIntPtr)
                {
                    int flags = 20;
                    if ((bounds.X == x) && (bounds.Y == y))
                    {
                        flags |= 2;
                    }
                    if ((bounds.Width == width) && (bounds.Height == height))
                    {
                        flags |= 1;
                    }
                    SafeNativeMethods.SetWindowPos(base.Handle, NativeMethods.NullIntPtr, x, y, width, height, flags);
                }
                else
                {
                    base.UpdateBounds(x, y, width, height);
                }
            }
        }

        public int SetCurSelectionAsString(string strNewSel)
        {
            this.SetDataInternal(strNewSel);
            return 0;
        }

        public void SetCurSelectionIndex(int nIndex)
        {
        }

        private void SetDataInternal(string myText)
        {
            this.Text = myText;
            base.Select(0, 0);
        }

        public void SetHorizontalAlignment(HorizontalAlignment alignment)
        {
            base.TextAlign = alignment;
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

        protected override Size DefaultSize
        {
            get
            {
                return new Size(this.m_myDefWidth, base.Height);
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

        public new decimal Increment
        {
            get
            {
                return base.Increment;
            }
            set
            {
                base.Increment = value;
            }
        }

        public new decimal Maximum
        {
            get
            {
                return base.Maximum;
            }
            set
            {
                base.Maximum = value;
            }
        }

        public new decimal Minimum
        {
            get
            {
                return base.Minimum;
            }
            set
            {
                base.Minimum = value;
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
                return true;
            }
        }
    }
}

