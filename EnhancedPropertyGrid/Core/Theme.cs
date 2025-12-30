using System.Drawing;

namespace EnhancedPropertyGrid.Core
{
    public class GridTheme
    {
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }
        public Color LineColor { get; set; }
        public Color CategoryBackColor { get; set; }
        public Color CategoryForeColor { get; set; }
        public Color AlternateBackColor { get; set; }
        public Color EditorBackColor { get; set; }
        public Color EditorForeColor { get; set; }

        public static GridTheme Light { get; } = new GridTheme
        {
            BackColor = SystemColors.Window,
            ForeColor = SystemColors.WindowText,
            LineColor = SystemColors.ControlLight,
            CategoryBackColor = SystemColors.ControlDark,
            CategoryForeColor = SystemColors.ControlLightLight,
            AlternateBackColor = Color.FromArgb(250, 250, 250),
            EditorBackColor = SystemColors.Window,
            EditorForeColor = SystemColors.WindowText
        };

        public static GridTheme Dark { get; } = new GridTheme
        {
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(220, 220, 220),
            LineColor = Color.FromArgb(60, 60, 60),
            CategoryBackColor = Color.FromArgb(45, 45, 48),
            CategoryForeColor = Color.FromArgb(240, 240, 240),
            AlternateBackColor = Color.FromArgb(35, 35, 35),
            EditorBackColor = Color.FromArgb(40, 40, 40),
            EditorForeColor = Color.FromArgb(220, 220, 220)
        };
    }
}
