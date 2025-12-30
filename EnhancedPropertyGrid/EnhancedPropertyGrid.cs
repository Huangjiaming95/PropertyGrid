using EnhancedPropertyGrid.Attributes;
using EnhancedPropertyGrid.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EnhancedPropertyGrid
{
    public class PropertyValueChangedEventArgs : EventArgs
    {
        public string PropertyName { get; }
        public object OldValue { get; }
        public object NewValue { get; }
        public PropertyValueChangedEventArgs(string propertyName, object oldValue, object newValue)
        {
            PropertyName = propertyName; OldValue = oldValue; NewValue = newValue;
        }
    }

    public class EnhancedPropertyGrid : UserControl
    {
        private Panel _scrollPanel;
        private TableLayoutPanel _table;
        private Label _descriptionLabel;
        private SplitContainer _split;

        private object _selectedObject;
        private readonly List<Action> _refreshers = new List<Action>();
        private readonly EditorOptions _options = new EditorOptions();
        private GridTheme _theme = GridTheme.Light;

        public event EventHandler<PropertyValueChangedEventArgs> PropertyValueChanged;

        public EnhancedPropertyGrid()
        {
            InitializeUI();
        }

        public object SelectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                Rebuild();
            }
        }

        public bool Categorized { get; set; } = true;

        public GridTheme Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                ApplyTheme();
                Rebuild();
            }
        }

        public bool ShowDescription { get => _split.Panel2Collapsed == false; set => _split.Panel2Collapsed = !value; }

        // 集合显示选项
        // (Removed unused options: ShowListSummary, ShowListPreview, ListPreviewMaxItems, CollectionMode)

        public void RefreshValues()
        {
            foreach (var r in _refreshers) r();
            UpdateVisibility();
        }

        private void ApplyTheme()
        {
            if (_scrollPanel != null) _scrollPanel.BackColor = Theme.BackColor;
            if (_descriptionLabel != null)
            {
                _descriptionLabel.BackColor = Theme.LineColor;
                _descriptionLabel.ForeColor = Theme.ForeColor;
            }
        }

        private void InitializeUI()
        {
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 280,
                Panel2Collapsed = false
            };
            Controls.Add(_split);

            _scrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            // Enable double buffering for smoother scrolling
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, _scrollPanel, new object[] { true });
            _table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 4, 8, 8) };
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _scrollPanel.Controls.Add(_table);
            _split.Panel1.Controls.Add(_scrollPanel);

            _descriptionLabel = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, Padding = new Padding(8), BackColor = Theme.LineColor, ForeColor = Theme.ForeColor, Text = "" };
            _split.Panel2.Controls.Add(_descriptionLabel);
            ApplyTheme();
        }

        private void Rebuild()
        {
            _table.SuspendLayout();
            _refreshers.Clear();
            _table.Controls.Clear();
            _table.RowStyles.Clear();
            _table.RowCount = 0;

            if (_selectedObject == null)
            {
                _table.ResumeLayout();
                return;
            }

            var props = TypeDescriptor.GetProperties(_selectedObject)
                .Cast<PropertyDescriptor>()
                .Where(p => p.IsBrowsable)
                .OrderBy(p => Categorized ? (p.Category ?? string.Empty) : string.Empty)
                .ThenBy(p => p.DisplayName)
                .ToList();

            string lastCategory = null;
            foreach (var pd in props)
            {
                var item = new PropertyItem(_selectedObject, pd);

                if (Categorized)
                {
                    var cat = pd.Category ?? string.Empty;
                    if (!string.Equals(cat, lastCategory, StringComparison.Ordinal))
                    {
                        lastCategory = cat;
                        AddCategoryHeader(string.IsNullOrEmpty(cat) ? "未分类" : cat);
                    }
                }

                AddPropertyRow(item);
            }

            _table.ResumeLayout();
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (_selectedObject == null) return;

            foreach (Control c in _table.Controls)
            {
                if (c.Tag is PropertyItem item)
                {
                    bool visible = true;
                    foreach (var attr in item.Attributes.OfType<VisibleWhenAttribute>())
                    {
                        var prop = _selectedObject.GetType().GetProperty(attr.PropertyName);
                        if (prop != null)
                        {
                            var val = prop.GetValue(_selectedObject);
                            bool match = object.Equals(val, attr.Value);
                            if (attr.Inverse) match = !match;
                            if (!match) visible = false;
                        }
                    }
                    c.Visible = visible;
                }
            }
        }

        private void AddCategoryHeader(string category)
        {
            var lbl = new Label
            {
                Text = category,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font(Font, FontStyle.Bold),
                BackColor = Theme.CategoryBackColor,
                ForeColor = Theme.CategoryForeColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };

            var span = new Panel { Height = lbl.Height, Dock = DockStyle.Top };

            _table.RowCount += 1;
            _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _table.Controls.Add(lbl, 0, _table.RowCount - 1);
            _table.SetColumnSpan(lbl, 2);
        }

        private void AddPropertyRow(PropertyItem item)
        {
            var nameLabel = new Label
            {
                Text = item.DisplayName,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Padding = new Padding(6, 4, 6, 4),
                ForeColor = Theme.ForeColor,
                Tag = item // Tag for visibility check
            };
            nameLabel.Cursor = Cursors.Hand;

            var editor = Editors.CreateEditor(item, _options, Theme, out var apply, out var refresh, (pi, oldVal, newVal) =>
            {
                PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(pi.Name, oldVal, newVal));
                UpdateVisibility();
            });
            editor.Tag = item; // Tag for visibility check

            // hover to show description
            void updateDesc(object s, EventArgs e) => _descriptionLabel.Text = item.Description;
            nameLabel.MouseEnter += updateDesc;
            editor.MouseEnter += updateDesc;

            _table.RowCount += 1;
            _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _table.Controls.Add(nameLabel, 0, _table.RowCount - 1);
            _table.Controls.Add(editor, 1, _table.RowCount - 1);

            _refreshers.Add(refresh);
        }
    }
}
