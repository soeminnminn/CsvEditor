using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    internal static class DrawManager
    {
        private static Pen borderPen;

        static DrawManager()
        {
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(DrawManager.SystemEvents_UserPreferenceChanged);
        }

        public static bool DrawNCBorder(ref Message m)
        {
            bool flag = false;
            int num = NativeMethods.GetWindowLong(m.HWnd, -16).ToInt32();
            if (((m.Msg != 0x85) || ((num & 0x800000) == 0)) || ((NativeMethods.GetWindowLong(m.HWnd, -20).ToInt32() & 0x200) != 0))
            {
                return flag;
            }
            IntPtr hrgnClip = (m.WParam != ((IntPtr) 1)) ? m.WParam : IntPtr.Zero;
            if (!(m.HWnd != IntPtr.Zero))
            {
                return flag;
            }
            int flags = 0x50401;
            if (hrgnClip != IntPtr.Zero)
            {
                flags |= 0x80;
            }
            IntPtr handle = UnsafeNativeMethods.GetDCEx(m.HWnd, hrgnClip, flags);
            if (!(handle != IntPtr.Zero))
            {
                return flag;
            }
            NativeMethods.RECT rect = new NativeMethods.RECT();
            NativeMethods.GetWindowRect(m.HWnd, ref rect);
            rect.right -= rect.left;
            rect.left = 0;
            rect.bottom -= rect.top;
            rect.top = 0;
            if (((num & 0x100000) != 0) || ((num & 0x200000) != 0))
            {
                m.Result = UnsafeNativeMethods.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
            }
            IntPtr ptr3 = NativeMethods.CreatePen(0, 1, ColorTranslator.ToWin32(BorderColor));
            IntPtr ptr4 = NativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr3));
            NativeMethods.POINT pt = new NativeMethods.POINT();
            NativeMethods.MoveToEx(new HandleRef(null, handle), rect.left, rect.top, pt);
            NativeMethods.LineTo(new HandleRef(null, handle), rect.left, rect.bottom - 1);
            NativeMethods.LineTo(new HandleRef(null, handle), rect.right - 1, rect.bottom - 1);
            NativeMethods.LineTo(new HandleRef(null, handle), rect.right - 1, rect.top);
            NativeMethods.LineTo(new HandleRef(null, handle), rect.left, rect.top);
            NativeMethods.SelectObject(new HandleRef(null, handle), new HandleRef(null, ptr4));
            NativeMethods.DeleteObject(new HandleRef(null, ptr3));
            UnsafeNativeMethods.ReleaseDC(m.HWnd, handle);
            return true;
        }

        public static VisualStyleElement GetButton(ButtonState state)
        {
            VisualStyleElement normal = null;
            if (!Application.RenderWithVisualStyles)
            {
                return normal;
            }
            normal = VisualStyleElement.Button.PushButton.Normal;
            if (state != ButtonState.Inactive)
            {
                if (state != ButtonState.Pushed)
                {
                    return normal;
                }
            }
            else
            {
                return VisualStyleElement.Button.PushButton.Disabled;
            }
            return VisualStyleElement.Button.PushButton.Pressed;
        }

        public static VisualStyleElement GetCheckBox(ButtonState state)
        {
            if (Application.RenderWithVisualStyles)
            {
                CheckState checkState = ((state & ButtonState.Flat) == ButtonState.Flat) ? CheckState.Indeterminate : (((state & ButtonState.Checked) == ButtonState.Checked) ? CheckState.Checked : CheckState.Unchecked);
                bool inactive = (state & ButtonState.Inactive) == ButtonState.Inactive;
                bool pushed = (state & ButtonState.Pushed) == ButtonState.Pushed;
                switch (checkState)
                {
                    case CheckState.Unchecked:
                        if (!inactive)
                        {
                            if (pushed)
                            {
                                return VisualStyleElement.Button.CheckBox.UncheckedPressed;
                            }
                            return VisualStyleElement.Button.CheckBox.UncheckedNormal;
                        }
                        return VisualStyleElement.Button.CheckBox.UncheckedDisabled;

                    case CheckState.Checked:
                        if (!inactive)
                        {
                            if (pushed)
                            {
                                return VisualStyleElement.Button.CheckBox.CheckedPressed;
                            }
                            return VisualStyleElement.Button.CheckBox.CheckedNormal;
                        }
                        return VisualStyleElement.Button.CheckBox.CheckedDisabled;

                    case CheckState.Indeterminate:
                        if (!inactive)
                        {
                            if (pushed)
                            {
                                return VisualStyleElement.Button.CheckBox.MixedPressed;
                            }
                            return VisualStyleElement.Button.CheckBox.MixedNormal;
                        }
                        return VisualStyleElement.Button.CheckBox.MixedDisabled;
                }
            }
            return null;
        }

        public static VisualStyleElement GetHeader(ButtonState state)
        {
            VisualStyleElement normal = null;
            if (!Application.RenderWithVisualStyles)
            {
                return normal;
            }
            normal = VisualStyleElement.Header.Item.Normal;
            if (state != ButtonState.Inactive)
            {
                if (state != ButtonState.Pushed)
                {
                    return normal;
                }
            }
            else
            {
                return VisualStyleElement.Header.Item.Normal;
            }
            return VisualStyleElement.Header.Item.Pressed;
        }

        public static VisualStyleElement GetLineIndexButton(ButtonState state)
        {
            VisualStyleElement normal = null;
            if (!Application.RenderWithVisualStyles)
            {
                return normal;
            }
            normal = VisualStyleElement.Header.ItemRight.Normal;
            switch (state)
            {
                case ButtonState.Inactive:
                    normal = VisualStyleElement.Header.ItemRight.Normal;
                    break;

                case ButtonState.Pushed:
                    normal = VisualStyleElement.Header.ItemRight.Pressed;
                    break;
            }
            if (!VisualStyleRenderer.IsElementDefined(normal))
            {
                normal = VisualStyleElement.Header.ItemLeft.Normal;
                switch (state)
                {
                    case ButtonState.Inactive:
                        normal = VisualStyleElement.Header.ItemLeft.Normal;
                        break;

                    case ButtonState.Pushed:
                        normal = VisualStyleElement.Header.ItemLeft.Pressed;
                        break;
                }
            }
            if (VisualStyleRenderer.IsElementDefined(normal))
            {
                return normal;
            }
            normal = VisualStyleElement.Header.Item.Normal;
            if (state != ButtonState.Inactive)
            {
                if (state != ButtonState.Pushed)
                {
                    return normal;
                }
            }
            else
            {
                return VisualStyleElement.Header.Item.Normal;
            }
            return VisualStyleElement.Header.Item.Pressed;
        }

        public static VisualStyleElement GetTreePlusMinus(ButtonState state)
        {
            if (Application.RenderWithVisualStyles)
            {
                CheckState checkState = ((state & ButtonState.Checked) == ButtonState.Checked) ? CheckState.Checked : CheckState.Unchecked;
                switch (checkState)
                {
                    case CheckState.Unchecked:
                        return VisualStyleElement.TreeView.Glyph.Closed;
                    case CheckState.Checked:
                        return VisualStyleElement.TreeView.Glyph.Opened;
                    default:
                        break;
                }
            }
            return null;
        }

        private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            borderPen = null;
        }

        public static System.Drawing.Color BorderColor
        {
            get
            {
                if (Application.RenderWithVisualStyles)
                {
                    return VisualStyleInformation.TextControlBorder;
                }
                return SystemColors.ControlDark;
            }
        }

        public static Pen BorderPen
        {
            get
            {
                if (!Application.RenderWithVisualStyles)
                {
                    return SystemPens.ControlDark;
                }
                if (borderPen == null)
                {
                    borderPen = new Pen(VisualStyleInformation.TextControlBorder);
                }
                return borderPen;
            }
        }
    }
}

