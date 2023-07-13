using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microsoft.SqlServer.Management.UI.Grid
{
    public class GridButtonColumn : GridTextColumn
    {
        private bool bGridHasLines;
        private bool bLineIndex;
        private ButtonWithForcedState m_forcedButton;

        protected GridButtonColumn()
        {
            this.bGridHasLines = true;
            this.m_forcedButton = new ButtonWithForcedState();
        }

        public GridButtonColumn(GridColumnInfo ci, int nWidthInPixels, int colIndex) : base(ci, nWidthInPixels, colIndex)
        {
            this.bGridHasLines = true;
            this.m_forcedButton = new ButtonWithForcedState();
        }

        private void AdjustButtonRect(ref Rectangle rect)
        {
            if (this.bGridHasLines)
            {
                rect.Width--;
                rect.Y++;
                rect.Height--;
            }
        }

        public void DrawButton(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, Bitmap buttomBmp, string buttonLabel, ButtonState btnState, bool bEnabled)
        {
            this.DrawButton(g, bkBrush, (SolidBrush) textBrush, textFont, rect, buttomBmp, buttonLabel, btnState, bEnabled);
        }

        public void DrawButton(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, Bitmap buttomBmp, string buttonLabel, ButtonState btnState, bool bEnabled)
        {
            this.DrawButton(g, bkBrush, textBrush, textFont, rect, buttomBmp, buttonLabel, btnState, bEnabled, false);
        }

        public void DrawButton(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, Bitmap buttomBmp, string buttonLabel, ButtonState btnState, bool bEnabled, bool useGdiPlus)
        {
            this.AdjustButtonRect(ref rect);
            g.FillRectangle(bkBrush, rect);
            if (useGdiPlus)
            {
                GridButton.Paint(g, rect, btnState, buttonLabel, buttomBmp, base.m_myAlign, base.m_myTextBmpLayout, bEnabled, textFont, textBrush, base.m_myStringFormat, (base.m_myStringFormat.FormatFlags & StringFormatFlags.DirectionRightToLeft) > 0, this.IsLineIndexButton ? GridButtonType.LineNumber : GridButtonType.Normal);
            }
            else
            {
                GridButton.Paint(g, rect, btnState, buttonLabel, buttomBmp, base.m_myAlign, base.m_myTextBmpLayout, bEnabled, textFont, textBrush, base.m_textFormat, (base.m_textFormat & TextFormatFlags.RightToLeft) > TextFormatFlags.GlyphOverhangPadding, this.IsLineIndexButton ? GridButtonType.LineNumber : GridButtonType.Normal);
            }
        }

        public override void DrawCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.DrawCellCommon(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex, true, false);
        }

        protected void DrawCellCommon(Graphics g, Brush bkBrush, Brush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex, bool bEnabled)
        {
            this.DrawCellCommon(g, bkBrush, (SolidBrush) textBrush, textFont, rect, storage, nRowIndex, bEnabled);
        }

        protected void DrawCellCommon(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex, bool bEnabled)
        {
            this.DrawCellCommon(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex, bEnabled, false);
        }

        protected void DrawCellCommon(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex, bool bEnabled, bool useGdiPlus)
        {
            ButtonCellState state;
            Bitmap image = null;
            string buttonLabel = null;
            ButtonState normal = ButtonState.Normal;
            storage.GetCellDataForButton(nRowIndex, base.m_myColumnIndex, out state, out image, out buttonLabel);
            switch (state)
            {
                case ButtonCellState.Empty:
                    this.AdjustButtonRect(ref rect);
                    g.FillRectangle(bkBrush, rect);
                    return;

                case ButtonCellState.Pushed:
                    normal = ButtonState.Pushed;
                    break;

                case ButtonCellState.Disabled:
                    normal = ButtonState.Inactive;
                    bEnabled = false;
                    break;
            }
            if (nRowIndex == this.m_forcedButton.RowIndex)
            {
                normal = this.m_forcedButton.State;
            }
            this.DrawButton(g, bkBrush, textBrush, textFont, rect, image, buttonLabel, normal, bEnabled, useGdiPlus);
        }

        public override void DrawDisabledCell(Graphics g, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.DrawCellCommon(g, GridColumn.s_DisabledCellBKBrush, GridColumn.s_DisabledCellForeBrush, textFont, rect, storage, nRowIndex, false);
        }

        public override string GetAccessibleValue(long nRowIndex, IGridStorage storage)
        {
            ButtonCellState state;
            Bitmap image = null;
            string buttonLabel = null;
            storage.GetCellDataForButton(nRowIndex, base.m_myColumnIndex, out state, out image, out buttonLabel);
            return buttonLabel;
        }

        public override void PrintCell(Graphics g, Brush bkBrush, SolidBrush textBrush, Font textFont, Rectangle rect, IGridStorage storage, long nRowIndex)
        {
            this.DrawCellCommon(g, bkBrush, textBrush, textFont, rect, storage, nRowIndex, true, true);
        }

        public override void ProcessNewGridFont(Font gridFont)
        {
        }

        public void SetForcedButtonState(long rowIndex, ButtonState state)
        {
            if (rowIndex == -1L)
            {
                this.m_forcedButton.RowIndex = -1L;
            }
            else
            {
                this.m_forcedButton.RowIndex = rowIndex;
                this.m_forcedButton.State = state;
            }
        }

        public void SetGridLinesMode(bool withLines)
        {
            this.bGridHasLines = withLines;
        }

        public bool IsLineIndexButton
        {
            get
            {
                return this.bLineIndex;
            }
            set
            {
                this.bLineIndex = value;
            }
        }

        private class ButtonWithForcedState
        {
            public long RowIndex = -1L;
            public ButtonState State;
        }
    }
}

