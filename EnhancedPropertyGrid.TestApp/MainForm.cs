using EnhancedPropertyGrid.Attributes;
using EnhancedPropertyGrid.TestApp.Sample;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EnhancedPropertyGrid.TestApp
{
    public class MainForm : Form
    {
        private global::EnhancedPropertyGrid.EnhancedPropertyGrid _grid;
        private CheckBox _chkCategorized;
        private Button _btnRefresh;
        private Button _btnTheme;

        public MainForm()
        {
            Text = "EnhancedPropertyGrid 测试";
            Width = 600; Height = 700;

            var panel = new Panel { Dock = DockStyle.Fill };
            _grid = new global::EnhancedPropertyGrid.EnhancedPropertyGrid { Dock = DockStyle.Fill };
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            _chkCategorized = new CheckBox { Text = "按分类", Checked = true, AutoSize = true };
            _btnRefresh = new Button { Text = "刷新", AutoSize = true };
            _btnTheme = new Button { Text = "切换主题", AutoSize = true };

            toolbar.Controls.Add(_chkCategorized);
            toolbar.Controls.Add(_btnRefresh);
            toolbar.Controls.Add(_btnTheme);

            panel.Controls.Add(_grid);
            panel.Controls.Add(toolbar);

            Controls.Add(panel);

            var data = SampleOptions.CreateDemo();
            _grid.SelectedObject = data;

            _chkCategorized.CheckedChanged += (s, e) => { _grid.Categorized = _chkCategorized.Checked; _grid.SelectedObject = _grid.SelectedObject; };
            _btnRefresh.Click += (s, e) => _grid.RefreshValues();
            _btnTheme.Click += (s, e) =>
            {
                if (_grid.Theme == global::EnhancedPropertyGrid.Core.GridTheme.Light)
                    _grid.Theme = global::EnhancedPropertyGrid.Core.GridTheme.Dark;
                else
                    _grid.Theme = global::EnhancedPropertyGrid.Core.GridTheme.Light;
            };
        }
    }
}
