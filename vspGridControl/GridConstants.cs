using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class GridConstants
    {
        // Fields
        private static Bitmap s_CheckedBitmap;
        private static Bitmap s_DisabledBitmap;
        private static Bitmap s_IntermidiateBitmap;
        private static Bitmap s_UncheckedBitmap;

        public const int StandardCheckBoxSize = 13;

        public const string TName = "GridControl";

        [CLSCompliant(false)]
        public const uint trERR = 0x40000000;
        [CLSCompliant(false)]
        public const uint trL1 = 1;
        [CLSCompliant(false)]
        public const uint trL2 = 2;
        [CLSCompliant(false)]
        public const uint trL3 = 4;
        [CLSCompliant(false)]
        public const uint trL4 = 8;
        [CLSCompliant(false)]
        public const uint trWARN = 0x20000000;

        // Methods
        internal static void AdjustFormatFlagsForAlignment(ref TextFormatFlags inputFlags, HorizontalAlignment ha)
        {
            switch (ha)
            {
                case HorizontalAlignment.Left:
                    inputFlags &= ~TextFormatFlags.Right;
                    inputFlags &= ~TextFormatFlags.HorizontalCenter;
                    return;

                case HorizontalAlignment.Right:
                    inputFlags &= ~TextFormatFlags.GlyphOverhangPadding;
                    inputFlags &= ~TextFormatFlags.HorizontalCenter;
                    inputFlags |= TextFormatFlags.Right;
                    return;

                case HorizontalAlignment.Center:
                    inputFlags &= ~TextFormatFlags.Right;
                    inputFlags &= ~TextFormatFlags.GlyphOverhangPadding;
                    inputFlags |= TextFormatFlags.HorizontalCenter;
                    return;
            }
        }

        private static void GetIntermidiateCheckboxBitmap(Bitmap bmp)
        {
            Rectangle bounds = new Rectangle(0, 0, 13, 13);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(Color.Transparent);
                if (Application.RenderWithVisualStyles)
                {
                    VisualStyleElement checkBox = DrawManager.GetCheckBox(ButtonState.Flat);
                    if ((checkBox != null) && VisualStyleRenderer.IsElementDefined(checkBox))
                    {
                        new VisualStyleRenderer(checkBox).DrawBackground(graphics, bounds);
                        return;
                    }
                }
                ControlPaint.DrawMixedCheckBox(graphics, bounds, ButtonState.Checked);
            }
        }

        private static void GetStdCheckBitmap(Bitmap bmp, ButtonState state)
        {
            Rectangle bounds = new Rectangle(0, 0, StandardCheckBoxSize, StandardCheckBoxSize);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(Color.Transparent);
                if (Application.RenderWithVisualStyles)
                {
                    VisualStyleElement element = DrawManager.GetCheckBox(state);
                    if ((element != null) && VisualStyleRenderer.IsElementDefined(element))
                    {
                        new VisualStyleRenderer(element).DrawBackground(graphics, bounds);
                        return;
                    }
                }
                ControlPaint.DrawCheckBox(graphics, bounds, state);
            }
        }

        internal static void RegenerateCheckBoxBitmaps()
        {
            Bitmap checkedCheckBoxBitmap = CheckedCheckBoxBitmap;
            Bitmap uncheckedCheckBoxBitmap = UncheckedCheckBoxBitmap;
            Bitmap intermidiateCheckBoxBitmap = IntermidiateCheckBoxBitmap;
            Bitmap disabledCheckBoxBitmap = DisabledCheckBoxBitmap;
        }

        // Properties
        public static TextFormatFlags DefaultTextFormatFlags
        {
            get
            {
                return (TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.WordEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
            }
        }

        public static Bitmap CheckedCheckBoxBitmap
        {
            get
            {
                if (s_CheckedBitmap == null)
                {
                    s_CheckedBitmap = new Bitmap(StandardCheckBoxSize, StandardCheckBoxSize);
                    GetStdCheckBitmap(s_CheckedBitmap, ButtonState.Checked);
                }
                return s_CheckedBitmap;
            }
        }

        public static Bitmap DisabledCheckBoxBitmap
        {
            get
            {
                if (s_DisabledBitmap == null)
                {
                    s_DisabledBitmap = new Bitmap(StandardCheckBoxSize, StandardCheckBoxSize);
                    GetStdCheckBitmap(s_DisabledBitmap, ButtonState.Inactive);
                }
                return s_DisabledBitmap;
            }
        }

        public static Bitmap IntermidiateCheckBoxBitmap
        {
            get
            {
                if (s_IntermidiateBitmap == null)
                {
                    s_IntermidiateBitmap = new Bitmap(StandardCheckBoxSize, StandardCheckBoxSize);
                    GetIntermidiateCheckboxBitmap(s_IntermidiateBitmap);
                }
                return s_IntermidiateBitmap;
            }
        }

        public static Bitmap UncheckedCheckBoxBitmap
        {
            get
            {
                if (s_UncheckedBitmap == null)
                {
                    s_UncheckedBitmap = new Bitmap(StandardCheckBoxSize, StandardCheckBoxSize);
                    GetStdCheckBitmap(s_UncheckedBitmap, ButtonState.Normal);
                }
                return s_UncheckedBitmap;
            }
        }
    }

    public class EditableCellType
    {
        public const int ComboBox = 2;
        public const int Editor = 1;
        public const int FirstCustomEditableCellType = 0x400;
        public const int ListBox = 3;
        public const int ReadOnly = 0;
        public const int SpinBox = 4;
    }

    public class GridColumnType
    {
        // Fields
        public const int Text = 1;
        public const int Button = 2;
        public const int Bitmap = 3;
        public const int Checkbox = 4;
        public const int Hyperlink = 5;
        public const int LineNumber = 6;
        public const int FirstCustomColumnType = 0x400;
    }
}