using System.Windows.Forms.VisualStyles;

namespace CPortTerminal
{
    /// <summary>
    /// A checkbox that keeps its caption legible when the control is disabled.
    /// Windows normally replaces disabled checkbox text with a low-contrast gray.
    /// </summary>
    internal sealed class ReadableCheckBox : CheckBox
    {
        private static readonly Color DisabledTextColor = Color.FromArgb(220, 220, 205);

        public ReadableCheckBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            CheckBoxState state = Enabled
                ? (Checked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal)
                : (Checked ? CheckBoxState.CheckedDisabled : CheckBoxState.UncheckedDisabled);
            Size glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, state);
            Point glyphLocation = new(0, (Height - glyphSize.Height) / 2);
            CheckBoxRenderer.DrawCheckBox(e.Graphics, glyphLocation, state);

            Rectangle textBounds = new(glyphSize.Width + 4, 0, Width - glyphSize.Width - 4, Height);
            Color textColor = Enabled ? ForeColor : DisabledTextColor;
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, textBounds, textColor, BackColor);
            }
        }
    }
}
