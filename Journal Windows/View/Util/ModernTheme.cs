using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JournalTrace.View.Util
{
    public static class ModernTheme
    {
        // DWM API для тёмного заголовка окна
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        // Цветовая палитра
        public static readonly Color BackgroundDark = Color.FromArgb(30, 30, 30);
        public static readonly Color BackgroundMedium = Color.FromArgb(45, 45, 48);
        public static readonly Color BackgroundLight = Color.FromArgb(62, 62, 66);
        public static readonly Color BackgroundHover = Color.FromArgb(70, 70, 74);
        public static readonly Color BackgroundPressed = Color.FromArgb(80, 80, 84);
        public static readonly Color Accent = Color.FromArgb(0, 122, 204);
        public static readonly Color AccentHover = Color.FromArgb(28, 151, 234);
        public static readonly Color TextPrimary = Color.FromArgb(241, 241, 241);
        public static readonly Color TextSecondary = Color.FromArgb(153, 153, 153);
        public static readonly Color Border = Color.FromArgb(67, 67, 70);

        public static readonly Font FontRegular = new Font("Segoe UI", 9F);
        public static readonly Font FontBold = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        public static readonly Font FontLarge = new Font("Segoe UI", 11F, FontStyle.Bold);

        public static void ApplyToForm(Form form)
        {
            form.BackColor = BackgroundDark;
            form.ForeColor = TextPrimary;
            form.Font = FontRegular;
            
            // Применяем тёмный заголовок окна (Windows 10 1809+)
            SetDarkTitleBar(form.Handle);
            
            ApplyToControls(form.Controls);
        }

        private static void SetDarkTitleBar(IntPtr handle)
        {
            try
            {
                int darkMode = 1;
                DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            }
            catch
            {
                // Игнорируем ошибки на старых версиях Windows
            }
        }

        public static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ApplyToControl(ctrl);
                if (ctrl.HasChildren)
                    ApplyToControls(ctrl.Controls);
            }
        }

        public static void ApplyToControl(Control ctrl)
        {
            ctrl.ForeColor = TextPrimary;
            ctrl.Font = FontRegular;

            if (ctrl is MenuStrip menu)
            {
                menu.BackColor = BackgroundMedium;
                menu.ForeColor = TextPrimary;
                menu.Renderer = new ModernMenuRenderer();
            }
            else if (ctrl is Label lbl)
            {
                lbl.BackColor = Color.Transparent;
                if (lbl.Font.Bold)
                    lbl.Font = FontBold;
            }
            else if (ctrl is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Border;
                btn.FlatAppearance.BorderSize = 1;
                btn.BackColor = BackgroundLight;
                btn.FlatAppearance.MouseOverBackColor = BackgroundHover;
                btn.FlatAppearance.MouseDownBackColor = BackgroundPressed;
                btn.Cursor = Cursors.Hand;
            }
            else if (ctrl is TextBox txt)
            {
                txt.BackColor = BackgroundMedium;
                txt.ForeColor = TextPrimary;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (ctrl is ComboBox cmb)
            {
                cmb.BackColor = BackgroundMedium;
                cmb.ForeColor = TextPrimary;
                cmb.FlatStyle = FlatStyle.Flat;
            }
            else if (ctrl is ListBox lst)
            {
                lst.BackColor = BackgroundMedium;
                lst.ForeColor = TextPrimary;
                lst.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (ctrl is Panel pnl)
            {
                pnl.BackColor = BackgroundDark;
            }
            else if (ctrl is FlowLayoutPanel flp)
            {
                flp.BackColor = BackgroundMedium;
            }
            else if (ctrl is DataGridView dgv)
            {
                StyleDataGridView(dgv);
            }
            else if (ctrl is TreeView tv)
            {
                tv.BackColor = BackgroundMedium;
                tv.ForeColor = TextPrimary;
                tv.BorderStyle = BorderStyle.None;
                tv.LineColor = Border;
            }
            else if (ctrl is ProgressBar)
            {
                // ProgressBar не поддерживает кастомные цвета напрямую
            }
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = BackgroundMedium;
            dgv.ForeColor = TextPrimary;
            dgv.GridColor = Border;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.RowHeadersVisible = false;

            dgv.DefaultCellStyle.BackColor = BackgroundMedium;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = Accent;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = FontRegular;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = BackgroundLight;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontBold;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = BackgroundLight;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
        }

        public static void ApplyToContextMenu(ContextMenuStrip cms)
        {
            cms.BackColor = BackgroundMedium;
            cms.ForeColor = TextPrimary;
            cms.Renderer = new ModernMenuRenderer();
        }
    }

    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new ModernColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = ModernTheme.TextPrimary;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                using (var brush = new SolidBrush(ModernTheme.BackgroundHover))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
                }
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => ModernTheme.Border;
        public override Color MenuItemBorder => ModernTheme.BackgroundHover;
        public override Color MenuItemSelected => ModernTheme.BackgroundHover;
        public override Color MenuItemSelectedGradientBegin => ModernTheme.BackgroundHover;
        public override Color MenuItemSelectedGradientEnd => ModernTheme.BackgroundHover;
        public override Color MenuItemPressedGradientBegin => ModernTheme.BackgroundPressed;
        public override Color MenuItemPressedGradientEnd => ModernTheme.BackgroundPressed;
        public override Color MenuStripGradientBegin => ModernTheme.BackgroundMedium;
        public override Color MenuStripGradientEnd => ModernTheme.BackgroundMedium;
        public override Color ToolStripDropDownBackground => ModernTheme.BackgroundMedium;
        public override Color ImageMarginGradientBegin => ModernTheme.BackgroundMedium;
        public override Color ImageMarginGradientMiddle => ModernTheme.BackgroundMedium;
        public override Color ImageMarginGradientEnd => ModernTheme.BackgroundMedium;
        public override Color SeparatorDark => ModernTheme.Border;
        public override Color SeparatorLight => ModernTheme.Border;
        public override Color CheckBackground => ModernTheme.Accent;
        public override Color CheckSelectedBackground => ModernTheme.AccentHover;
    }
}
