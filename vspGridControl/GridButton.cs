using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public sealed class GridButton
    {
        // Fields
        internal static readonly int ExtraHorizSpace = SystemInformation.Border3DSize.Width;
        private TextFormatFlags m_cacheFormat = GridConstants.DefaultTextFormatFlags;
        private StringFormat m_cacheGdiPlusFormat = new StringFormat(StringFormatFlags.NoWrap);
        private bool m_RTL;
        private SolidBrush m_textBrush = new SolidBrush(SystemColors.ControlText);
        private Font m_textFont = Control.DefaultFont;
        private static SolidBrush s_DisabledButtonTextBrush = new SolidBrush(SystemColors.GrayText);
        private static readonly int s_imageTextGap = 4;

        // Methods
        static GridButton()
        {
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(GridButton.OnUserPrefChanged);
        }

        public GridButton()
        {
            this.m_cacheGdiPlusFormat.LineAlignment = StringAlignment.Center;
            this.m_cacheGdiPlusFormat.HotkeyPrefix = HotkeyPrefix.None;
            this.m_cacheGdiPlusFormat.Trimming = StringTrimming.EllipsisCharacter;
            this.m_cacheFormat &= ~TextFormatFlags.SingleLine;
        }

        public static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref StringFormat sFormat, out int nStringWidth)
        {
            TextFormatFlags flags = ConvertStringFormatIntoTextFormat(sFormat, false);
            return CalculateInitialContentsRect(g, r, text, bmp, contentsAlignment, textFont, bRtl, ref flags, ref sFormat, out nStringWidth);
        }

        public static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref TextFormatFlags sFormat, out int nStringWidth)
        {
            StringFormat gdiPlusFormat = new StringFormat();
            return CalculateInitialContentsRect(g, r, text, bmp, contentsAlignment, textFont, bRtl, ref sFormat, ref gdiPlusFormat, out nStringWidth);
        }

        private static Rectangle CalculateInitialContentsRect(Graphics g, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, Font textFont, bool bRtl, ref TextFormatFlags sFormat, ref StringFormat gdiPlusFormat, out int nStringWidth)
        {
            Rectangle rectangle;
            nStringWidth = 0;
            Size size = new Size(0, 0);
            if ((text != null) && (text.Length > 0))
            {
                SizeF ef;
                if (gdiPlusFormat != null)
                {
                    ef = g.MeasureString(text, textFont);
                }
                else
                {
                    Size proposedSize = new Size(0x7fffffff, 0x7fffffff);
                    ef = (SizeF)TextRenderer.MeasureText(g, text, textFont, proposedSize, sFormat);
                }
                size.Width = (int)Math.Ceiling((double)ef.Width);
                size.Height = (int)Math.Ceiling((double)ef.Height);
                nStringWidth = size.Width;
            }
            if (bmp != null)
            {
                size.Width += bmp.Width;
                size.Height = Math.Max(size.Height, bmp.Height);
                if ((text != null) && (text != ""))
                {
                    size.Width += s_imageTextGap;
                }
            }
            int num = (r.Height - size.Height) / 2;
            if (((contentsAlignment == HorizontalAlignment.Left) && !bRtl) || ((contentsAlignment == HorizontalAlignment.Right) && bRtl))
            {
                rectangle = new Rectangle(r.X + ExtraHorizSpace, r.Y + num, size.Width, size.Height);
                GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Left);
                if (gdiPlusFormat != null)
                {
                    gdiPlusFormat.Alignment = StringAlignment.Near;
                }
            }
            else if (((contentsAlignment == HorizontalAlignment.Right) && !bRtl) || ((contentsAlignment == HorizontalAlignment.Left) && bRtl))
            {
                rectangle = new Rectangle(((r.X + r.Width) - ExtraHorizSpace) - size.Width, r.Y + num, size.Width, size.Height);
                GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Right);
                if (gdiPlusFormat != null)
                {
                    gdiPlusFormat.Alignment = StringAlignment.Far;
                }
            }
            else
            {
                int num2 = (int)Math.Ceiling((double)Math.Max((float)((r.Width - size.Width) / 2f), (float)0f));
                rectangle = new Rectangle(r.X + num2, r.Y + num, size.Width, size.Height);
                GridConstants.AdjustFormatFlagsForAlignment(ref sFormat, HorizontalAlignment.Center);
                if (gdiPlusFormat != null)
                {
                    gdiPlusFormat.Alignment = StringAlignment.Center;
                }
            }
            rectangle.X = Math.Max(rectangle.X, r.X + ExtraHorizSpace);
            rectangle.Width = Math.Min(rectangle.Width, r.Width - (2 * ExtraHorizSpace));
            rectangle.Y = Math.Max(rectangle.Y, r.Y);
            rectangle.Height = Math.Min(rectangle.Height, r.Height);
            return rectangle;
        }

        private static TextFormatFlags ConvertStringFormatIntoTextFormat(StringFormat sf, bool adjustStringAlign)
        {
            TextFormatFlags defaultTextFormatFlags = GridConstants.DefaultTextFormatFlags;
            if (sf != null)
            {
                if ((sf.FormatFlags & StringFormatFlags.DirectionRightToLeft) == StringFormatFlags.DirectionRightToLeft)
                {
                    defaultTextFormatFlags |= TextFormatFlags.RightToLeft;
                }
                else
                {
                    defaultTextFormatFlags &= ~TextFormatFlags.RightToLeft;
                }
                if (!adjustStringAlign)
                {
                    return defaultTextFormatFlags;
                }
                HorizontalAlignment left = HorizontalAlignment.Left;
                if (sf.Alignment == StringAlignment.Center)
                {
                    left = HorizontalAlignment.Center;
                }
                else if (sf.Alignment == StringAlignment.Far)
                {
                    left = HorizontalAlignment.Right;
                }
                GridConstants.AdjustFormatFlagsForAlignment(ref defaultTextFormatFlags, left);
            }
            return defaultTextFormatFlags;
        }

        private static void DrawBitmap(Graphics g, Bitmap bmp, Rectangle rect, bool bEnabled)
        {
            if (bmp != null)
            {
                if (bEnabled)
                {
                    g.DrawImage(bmp, rect);
                }
                else
                {
                    ControlPaint.DrawImageDisabled(g, bmp, rect.X, rect.Y, SystemColors.Control);
                }
            }
        }

        public GridButtonArea HitTest(Graphics g, Point p, Rectangle r, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout)
        {
            return HitTest(g, p, r, text, bmp, contentsAlignment, tbLayout, this.m_textFont, this.m_textBrush, this.m_cacheFormat, this.m_RTL);
        }

        public static GridButtonArea HitTest(Graphics g, Point point, Rectangle buttonRect, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, Font textFont, Brush textBrush, StringFormat sFormat, bool bRtl)
        {
            return HitTest(g, point, buttonRect, text, bmp, contentsAlignment, tbLayout, textFont, (SolidBrush)textBrush, ConvertStringFormatIntoTextFormat(sFormat, true), bRtl);
        }

        public static GridButtonArea HitTest(Graphics g, Point point, Rectangle buttonRect, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl)
        {
            return PaintButtonOrHitTest(g, buttonRect, ButtonState.Normal, text, bmp, contentsAlignment, tbLayout, textFont, null, sFormat, bRtl, true, point, false, null);
        }

        private static void OnUserPrefChanged(object sender, UserPreferenceChangedEventArgs pref)
        {
            if (s_DisabledButtonTextBrush != null)
            {
                s_DisabledButtonTextBrush.Dispose();
                s_DisabledButtonTextBrush = null;
            }
            s_DisabledButtonTextBrush = new SolidBrush(SystemColors.GrayText);
        }

        public void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled)
        {
            Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, this.m_textFont, this.m_textBrush, this.m_cacheFormat, this.m_RTL);
        }

        public void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, GridButtonType buttonType)
        {
            Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, this.m_textFont, this.m_textBrush, this.m_cacheFormat, this.m_RTL, buttonType);
        }

        internal void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, bool useGdiPlus)
        {
            this.Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, useGdiPlus, GridButtonType.Normal);
        }

        internal void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, bool useGdiPlus, GridButtonType buttonType)
        {
            if (useGdiPlus)
            {
                Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, this.m_textFont, this.m_textBrush, this.m_cacheGdiPlusFormat, this.m_RTL, buttonType);
            }
            else
            {
                Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, this.m_textFont, this.m_textBrush, this.m_cacheFormat, this.m_RTL, buttonType);
            }
        }

        public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, Font textFont, Brush textBrush, StringFormat sFormat, bool bRtl)
        {
            Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, (SolidBrush)textBrush, ConvertStringFormatIntoTextFormat(sFormat, true), bRtl, sFormat, GridButtonType.Normal);
        }

        public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl)
        {
            Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, textBrush, sFormat, bRtl, null, GridButtonType.Normal);
        }

        public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, Font textFont, Brush textBrush, StringFormat sFormat, bool bRtl, GridButtonType buttonType)
        {
            Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, (SolidBrush)textBrush, ConvertStringFormatIntoTextFormat(sFormat, true), bRtl, sFormat, buttonType);
        }

        public static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, GridButtonType buttonType)
        {
            Paint(g, r, state, text, bmp, contentsAlignment, tbLayout, bEnabled, textFont, textBrush, sFormat, bRtl, null, buttonType);
        }

        private static void Paint(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, bool bEnabled, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, StringFormat gdiPlusFormat, GridButtonType buttonType)
        {
            if (!r.IsEmpty)
            {
                SolidBrush brush = bEnabled ? textBrush : s_DisabledButtonTextBrush;
                PaintButtonFrame(g, r, state, buttonType);
                PaintButtonOrHitTest(g, r, state, text, bmp, contentsAlignment, tbLayout, textFont, brush, sFormat, bRtl, bEnabled, Point.Empty, true, gdiPlusFormat);
            }
        }

        private static void PaintButtonFrame(Graphics g, Rectangle r, ButtonState state, GridButtonType buttonType)
        {
            if (Application.RenderWithVisualStyles)
            {
                VisualStyleElement button = null;
                Rectangle bounds = r;
                Rectangle rect = r;
                switch (buttonType)
                {
                    case GridButtonType.Normal:
                        button = DrawManager.GetButton(state);
                        break;

                    case GridButtonType.Header:
                        button = DrawManager.GetHeader(state);
                        rect.Width++;
                        bounds.Width++;
                        break;

                    case GridButtonType.LineNumber:
                        button = DrawManager.GetLineIndexButton(state);
                        bounds.Width *= 2;
                        break;
                }
                if ((button != null) && VisualStyleRenderer.IsElementDefined(button))
                {
                    Region clip = g.Clip;
                    using (Region region2 = new Region(rect))
                    {
                        g.Clip = region2;
                        new VisualStyleRenderer(button).DrawBackground(g, bounds);
                        g.Clip = clip;
                    }
                    return;
                }
            }
            ControlPaint.DrawButton(g, r, state);
        }

        private static GridButtonArea PaintButtonOrHitTest(Graphics g, Rectangle r, ButtonState state, string text, Bitmap bmp, HorizontalAlignment contentsAlignment, TextBitmapLayout tbLayout, Font textFont, SolidBrush textBrush, TextFormatFlags sFormat, bool bRtl, bool bEnabled, Point ptToHitTest, bool bPaint, StringFormat gdiPlusFormat)
        {
            int nStringWidth = 0;
            Rectangle layoutRectangle = CalculateInitialContentsRect(g, r, text, bmp, contentsAlignment, textFont, bRtl, ref sFormat, ref gdiPlusFormat, out nStringWidth);
            if (layoutRectangle.Width <= 0)
            {
                return GridButtonArea.Background;
            }
            if ((text != null) && (text != ""))
            {
                if (bmp == null)
                {
                    if (!bPaint)
                    {
                        if (layoutRectangle.Contains(ptToHitTest))
                        {
                            return GridButtonArea.Text;
                        }
                        return GridButtonArea.Background;
                    }
                    if (gdiPlusFormat != null)
                    {
                        g.DrawString(text, textFont, textBrush, layoutRectangle, gdiPlusFormat);
                    }
                    else
                    {
                        TextRenderer.DrawText(g, text, textFont, layoutRectangle, textBrush.Color, sFormat);
                    }
                }
                else
                {
                    Rectangle rectangle2;
                    Rectangle rectangle3;
                    int num2 = Math.Max((layoutRectangle.Height - bmp.Height) / 2, 0);
                    if (tbLayout != TextBitmapLayout.TextRightOfBitmap)
                    {
                        rectangle3 = new Rectangle(layoutRectangle.X, layoutRectangle.Y, Math.Min(nStringWidth, layoutRectangle.Width), layoutRectangle.Height);
                        if (bPaint)
                        {
                            if (gdiPlusFormat != null)
                            {
                                g.DrawString(text, textFont, textBrush, rectangle3, gdiPlusFormat);
                            }
                            else
                            {
                                TextRenderer.DrawText(g, text, textFont, rectangle3, textBrush.Color, sFormat);
                            }
                        }
                        else if (rectangle3.Contains(ptToHitTest))
                        {
                            return GridButtonArea.Text;
                        }
                        rectangle2 = new Rectangle(rectangle3.Right + s_imageTextGap, layoutRectangle.Y + num2, (layoutRectangle.Width - rectangle3.Width) - s_imageTextGap, bmp.Height);
                        if (rectangle2.X < layoutRectangle.Right)
                        {
                            rectangle2.Width = Math.Min(rectangle2.Width, layoutRectangle.Right - rectangle2.X);
                            rectangle2.Height = Math.Min(rectangle2.Height, layoutRectangle.Height);
                            if (bPaint)
                            {
                                DrawBitmap(g, bmp, rectangle2, bEnabled);
                            }
                            else if (rectangle2.Contains(ptToHitTest))
                            {
                                return GridButtonArea.Image;
                            }
                        }
                        if (!bPaint)
                        {
                            return GridButtonArea.Background;
                        }
                    }
                    else
                    {
                        rectangle2 = new Rectangle(layoutRectangle.X, layoutRectangle.Y + num2, Math.Min(bmp.Width, layoutRectangle.Width), bmp.Height);
                        rectangle2.Height = Math.Min(rectangle2.Height, layoutRectangle.Height);
                        if (bPaint)
                        {
                            DrawBitmap(g, bmp, rectangle2, bEnabled);
                        }
                        else if (rectangle2.Contains(ptToHitTest))
                        {
                            return GridButtonArea.Image;
                        }
                        rectangle3 = new Rectangle(rectangle2.Right + s_imageTextGap, layoutRectangle.Y, (layoutRectangle.Width - rectangle2.Width) - s_imageTextGap, layoutRectangle.Height);
                        if (rectangle3.X < layoutRectangle.Right)
                        {
                            rectangle3.Width = Math.Min(rectangle3.Width, layoutRectangle.Right - rectangle3.X);
                            if (bPaint)
                            {
                                if (gdiPlusFormat != null)
                                {
                                    g.DrawString(text, textFont, textBrush, rectangle3, gdiPlusFormat);
                                }
                                else
                                {
                                    TextRenderer.DrawText(g, text, textFont, rectangle3, textBrush.Color, sFormat);
                                }
                            }
                            else if (rectangle3.Contains(ptToHitTest))
                            {
                                return GridButtonArea.Text;
                            }
                        }
                        if (!bPaint)
                        {
                            return GridButtonArea.Background;
                        }
                    }
                }
            }
            else if (bmp != null)
            {
                if (!bPaint)
                {
                    if (layoutRectangle.Contains(ptToHitTest))
                    {
                        return GridButtonArea.Image;
                    }
                    return GridButtonArea.Background;
                }
                DrawBitmap(g, bmp, layoutRectangle, bEnabled);
            }
            else if (!bPaint)
            {
                return GridButtonArea.Background;
            }
            return GridButtonArea.Nothing;
        }

        // Properties
        public static int ButtonAdditionalHeight
        {
            get
            {
                return ((SystemInformation.Border3DSize.Width * 2) + 2);
            }
        }

        public bool RTL
        {
            get
            {
                return this.m_RTL;
            }
            set
            {
                if (this.m_RTL != value)
                {
                    this.m_RTL = value;
                    if (value)
                    {
                        this.m_cacheFormat |= TextFormatFlags.RightToLeft;
                        this.m_cacheGdiPlusFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    }
                    else
                    {
                        this.m_cacheFormat &= ~TextFormatFlags.RightToLeft;
                        this.m_cacheGdiPlusFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
                    }
                }
            }
        }

        public Brush TextBrush
        {
            get
            {
                return this.m_textBrush;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                SolidBrush brush = value as SolidBrush;
                if (brush == null)
                {
                    throw new ArgumentException(SRError.OnlySolidBrush, "value");
                }
                this.m_textBrush = brush;
            }
        }

        public Font TextFont
        {
            get
            {
                return this.m_textFont;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                this.m_textFont = value;
            }
        }
    }
}