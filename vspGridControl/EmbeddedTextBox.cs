using System;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class EmbeddedTextBox : TextBox, IGridEmbeddedControl, IGridEmbeddedControlManagement2, IGridEmbeddedControlManagement
    {
        protected bool m_alwaysShowContextMenu;
        protected int m_ColumnIndex;
        protected int m_MarginsWidth;
        protected long m_RowIndex;

        public event ContentsChangedEventHandler ContentsChanged;

        protected EmbeddedTextBox()
        {
            this.m_RowIndex = -1L;
            this.m_ColumnIndex = -1;
            this.m_alwaysShowContextMenu = true;
        }

        public EmbeddedTextBox(Control parent, int MarginsWidth)
        {
            this.m_RowIndex = -1L;
            this.m_ColumnIndex = -1;
            this.m_alwaysShowContextMenu = true;
            this.m_MarginsWidth = MarginsWidth;
            base.Parent = parent;
            this.Multiline = false;
            this.Text = "";
            base.Visible = false;
            this.AutoSize = false;
            base.BorderStyle = Application.RenderWithVisualStyles ? BorderStyle.Fixed3D : BorderStyle.FixedSingle;
            base.TextChanged += new EventHandler(this.OnTextChanged);
        }

        public int AddDataAsString(string Item)
        {
            this.SetDataInternal(Item);
            return 0;
        }

        public void ClearData()
        {
            this.Text = "";
            base.PasswordChar = '\0';
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

        private void OnTextChanged(object sender, EventArgs args)
        {
            if (this.ContentsChanged != null)
            {
                this.ContentsChanged(this, args);
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

        protected override bool ProcessKeyMessage(ref Message m)
        {
            System.Windows.Forms.Keys wParam = (System.Windows.Forms.Keys) ((int) m.WParam);
            System.Windows.Forms.Keys modifierKeys = Control.ModifierKeys;
            if (((wParam | modifierKeys) == System.Windows.Forms.Keys.Return) || ((wParam | modifierKeys) == System.Windows.Forms.Keys.Escape))
            {
                return ((m.Msg == 0x102) || this.ProcessKeyPreview(ref m));
            }
            if (m.Msg != 0x102)
            {
                switch ((wParam & System.Windows.Forms.Keys.KeyCode))
                {
                    case System.Windows.Forms.Keys.Prior:
                    case System.Windows.Forms.Keys.Next:
                    case System.Windows.Forms.Keys.Tab:
                    case System.Windows.Forms.Keys.Up:
                    case System.Windows.Forms.Keys.Down:
                        return this.ProcessKeyPreview(ref m);
                }
            }
            return this.ProcessKeyEventArgs(ref m);
        }

        public void ReceiveChar(char c)
        {
            if (!base.ReadOnly)
            {
                this.Text = c.ToString();
                this.SelectedText = string.Empty;
                base.Select(1, 0);
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

        private void WmSetFont(ref Message m)
        {
            base.WndProc(ref m);
            NativeMethods.SendMessage(base.Handle, 0xd3, (IntPtr) 3, NativeMethods.Util.MAKELPARAM(this.m_MarginsWidth, this.m_MarginsWidth));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x30:
                    this.WmSetFont(ref m);
                    return;

                case 0x7b:
                    if ((this.ContextMenu == null) && (!this.m_alwaysShowContextMenu || (this.ContextMenu != null)))
                    {
                        break;
                    }
                    base.WndProc(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public bool AlwaysShowContextMenu
        {
            get
            {
                return this.m_alwaysShowContextMenu;
            }
            set
            {
                this.m_alwaysShowContextMenu = value;
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
                return true;
            }
        }
    }
}

