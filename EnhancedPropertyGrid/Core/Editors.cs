using EnhancedPropertyGrid.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace EnhancedPropertyGrid.Core
{
    internal static class Editors
    {
        public static Control CreateEditor(PropertyItem item,
            EditorOptions options,
            GridTheme theme,
            out Action applyToModel,
            out Action refreshFromModel,
            Action<PropertyItem, object, object> onValueChanged)
        {
            var type = item.PropertyType;
            var attrs = item.Attributes;

            Control editor = null;

            // File path
            var fileAttr = attrs.OfType<FilePathAttribute>().FirstOrDefault();
            if (fileAttr != null)
            {
                editor = CreateFileEditor(item, fileAttr, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            // Folder path
            else if (attrs.OfType<FolderPathAttribute>().Any())
            {
                var folderAttr = attrs.OfType<FolderPathAttribute>().First();
                editor = CreateFolderEditor(item, folderAttr, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            // Bool first: always use CheckBox (not ComboBox)
            else if ((Nullable.GetUnderlyingType(type) ?? type) == typeof(bool))
            {
                editor = CreateCheckboxEditor(item, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            // Standard values from TypeConverter (after bool)
            else if (item.Converter != null && item.Converter.GetStandardValuesSupported())
            {
                editor = CreateComboEditor(item, item.Converter.GetStandardValues(), theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            else if (type.IsEnum)
            {
                var values = Enum.GetValues(type).Cast<object>().ToList();
                editor = CreateComboEditor(item, values, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
            {
                editor = CreateIntegerEditor(item, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            else if (type == typeof(decimal))
            {
                editor = CreateDecimalEditor(item, 2, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            else if (type == typeof(double) || type == typeof(float))
            {
                editor = CreateDecimalTextEditor(item, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            else if (type == typeof(DateTime))
            {
                editor = CreateDateTimeEditor(item, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            // List editor (IList or generic IList<>)
            else if (typeof(IList).IsAssignableFrom(type) || ImplementsGenericInterface(type, typeof(IList<>)))
            {
                editor = CreateListDialogEditor(item, options, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }
            else
            {
                // Fallback: textbox with converter
                editor = CreateTextEditor(item, theme, out applyToModel, out refreshFromModel, onValueChanged);
            }

            return editor;
        }

        private static void ApplyValidation(Control control, PropertyItem item, GridTheme theme)
        {
            var error = item.GetValidationErrors();
            if (!string.IsNullOrEmpty(error))
            {
                control.BackColor = Color.MistyRose;
                // Optional: Set tooltip
            }
            else
            {
                control.BackColor = theme.EditorBackColor;
            }
        }

        private static bool ImplementsGenericInterface(Type type, Type interfaceType)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        private static Control CreateCheckboxEditor(PropertyItem item, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var chk = new CheckBox { AutoSize = true, Anchor = AnchorStyles.Left, Enabled = !item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor };
            Action onRefresh = () => chk.Checked = (bool)(item.GetValue() ?? false);
            Action onApply = () => { var old = item.GetValue(); if (!item.IsReadOnly) { item.SetValue(chk.Checked); changed?.Invoke(item, old, chk.Checked); } };
            refresh = onRefresh;
            apply = onApply;
            chk.CheckedChanged += (s, e) => onApply();
            onRefresh();
            return chk;
        }

        private static Control CreateComboEditor(PropertyItem item, ICollection values, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right, Enabled = !item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, FlatStyle = FlatStyle.Flat };
            foreach (var v in values) cmb.Items.Add(v);
            Action onRefresh = () =>
            {
                var current = item.GetValue();
                cmb.SelectedItem = current;
                ApplyValidation(cmb, item, theme);
            };
            Action onApply = () =>
            {
                var old = item.GetValue();
                if (!item.IsReadOnly)
                {
                    var value = cmb.SelectedItem;
                    if (value == null && cmb.Items.Count > 0) value = cmb.Items[0];
                    item.SetValue(value);
                    changed?.Invoke(item, old, value);
                    ApplyValidation(cmb, item, theme);
                }
            };
            refresh = onRefresh;
            apply = onApply;
            cmb.SelectedIndexChanged += (s, e) => onApply();
            onRefresh();
            return cmb;
        }

        private static Control CreateIntegerEditor(PropertyItem item, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var num = new NumericUpDown
            {
                Minimum = decimal.MinValue / 2,
                Maximum = decimal.MaxValue / 2,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Enabled = !item.IsReadOnly,
                ForeColor = theme.EditorForeColor,
                BackColor = theme.EditorBackColor
            };
            Action onRefresh = () =>
            {
                var v = item.GetValue();
                num.Value = v == null ? 0 : System.Convert.ToDecimal(v);
                ApplyValidation(num, item, theme);
            };
            Action onApply = () =>
            {
                var old = item.GetValue();
                if (!item.IsReadOnly)
                {
                    object v = System.Convert.ChangeType(num.Value, item.PropertyType);
                    item.SetValue(v);
                    changed?.Invoke(item, old, v);
                    ApplyValidation(num, item, theme);
                }
            };
            refresh = onRefresh;
            apply = onApply;
            num.ValueChanged += (s, e) => onApply();
            onRefresh();
            return num;
        }

        private static Control CreateDecimalEditor(PropertyItem item, int decimals, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var num = new NumericUpDown
            {
                Minimum = -1000000000m,
                Maximum = 1000000000m,
                DecimalPlaces = decimals,
                Increment = 0.1m,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Enabled = !item.IsReadOnly,
                ForeColor = theme.EditorForeColor,
                BackColor = theme.EditorBackColor
            };
            Action onRefresh = () =>
            {
                var v = item.GetValue();
                num.Value = v == null ? 0 : System.Convert.ToDecimal(v);
                ApplyValidation(num, item, theme);
            };
            Action onApply = () =>
            {
                var old = item.GetValue();
                if (!item.IsReadOnly)
                {
                    object v = Converters.ChangeType(num.Value, item.PropertyType);
                    item.SetValue(v);
                    changed?.Invoke(item, old, v);
                    ApplyValidation(num, item, theme);
                }
            };
            refresh = onRefresh;
            apply = onApply;
            num.ValueChanged += (s, e) => onApply();
            onRefresh();
            return num;
        }

        private static Control CreateDecimalTextEditor(PropertyItem item, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var tb = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Enabled = !item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, BorderStyle = BorderStyle.FixedSingle };
            Action onRefresh = () =>
            {
                var v = item.GetValue();
                tb.Text = v?.ToString() ?? string.Empty;
                ApplyValidation(tb, item, theme);
            };
            Action onApply = () =>
            {
                var old = item.GetValue();
                if (!item.IsReadOnly)
                {
                    object v = null;
                    try { v = Converters.ChangeType(tb.Text, item.PropertyType); }
                    catch { /* ignore invalid */ }
                    if (v != null)
                    {
                        item.SetValue(v);
                        changed?.Invoke(item, old, v);
                        ApplyValidation(tb, item, theme);
                    }
                }
            };
            refresh = onRefresh;
            apply = onApply;
            tb.Leave += (s, e) => onApply();
            tb.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { onApply(); e.SuppressKeyPress = true; } };
            onRefresh();
            return tb;
        }

        private static Control CreateDateTimeEditor(PropertyItem item, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var dt = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss", ShowUpDown = true, Anchor = AnchorStyles.Left | AnchorStyles.Right, Enabled = !item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor };
            Action onRefresh = () =>
            {
                var v = item.GetValue();
                dt.Value = v is DateTime d ? d : DateTime.Now;
                ApplyValidation(dt, item, theme);
            };
            Action onApply = () =>
            {
                var old = item.GetValue();
                if (!item.IsReadOnly)
                {
                    var v = dt.Value;
                    item.SetValue(v);
                    changed?.Invoke(item, old, v);
                    ApplyValidation(dt, item, theme);
                }
            };
            refresh = onRefresh;
            apply = onApply;
            dt.ValueChanged += (s, e) => onApply();
            onRefresh();
            return dt;
        }

        private static Control CreateTextEditor(PropertyItem item, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var tb = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Enabled = !item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, BorderStyle = BorderStyle.FixedSingle };
            Action onRefresh = () =>
            {
                var v = item.GetValue();
                tb.Text = v?.ToString() ?? string.Empty;
                ApplyValidation(tb, item, theme);
            };
            Action onApply = () =>
            {
                var old = item.GetValue();
                if (!item.IsReadOnly)
                {
                    object v;
                    try { v = Converters.ChangeType(tb.Text, item.PropertyType, item.Converter); }
                    catch { v = tb.Text; }
                    item.SetValue(v);
                    changed?.Invoke(item, old, v);
                    ApplyValidation(tb, item, theme);
                }
            };
            refresh = onRefresh;
            apply = onApply;
            tb.Leave += (s, e) => onApply();
            tb.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { onApply(); e.SuppressKeyPress = true; } };
            onRefresh();
            return tb;
        }

        private static Control CreateFileEditor(PropertyItem item, FilePathAttribute attr, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var panel = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill, AutoSize = true };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var tb = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, BorderStyle = BorderStyle.FixedSingle };
            var btn = new Button { Text = "...", Width = 30, Anchor = AnchorStyles.Right, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, FlatStyle = FlatStyle.Flat };
            panel.Controls.Add(tb, 0, 0);
            panel.Controls.Add(btn, 1, 0);

            Action onRefresh = () =>
            {
                tb.Text = item.GetValue()?.ToString() ?? string.Empty;
                ApplyValidation(tb, item, theme);
            };
            Action onApply = () => { var old = item.GetValue(); if (!item.IsReadOnly) { item.SetValue(tb.Text); changed?.Invoke(item, old, tb.Text); ApplyValidation(tb, item, theme); } };
            refresh = onRefresh;
            apply = onApply;

            btn.Click += (s, e) =>
            {
                if (item.IsReadOnly) return;
                if (attr.SaveDialog)
                {
                    using (var sd = new SaveFileDialog { Filter = attr.Filter, Title = attr.Title })
                    {
                        if (sd.ShowDialog() == DialogResult.OK)
                        {
                            tb.Text = sd.FileName; onApply();
                        }
                    }
                }
                else
                {
                    using (var od = new OpenFileDialog { Filter = attr.Filter, Title = attr.Title })
                    {
                        if (od.ShowDialog() == DialogResult.OK)
                        {
                            tb.Text = od.FileName; onApply();
                        }
                    }
                }
            };

            onRefresh();
            return panel;
        }

        private static Control CreateFolderEditor(PropertyItem item, FolderPathAttribute attr, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var panel = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill, AutoSize = true };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var tb = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, ReadOnly = item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, BorderStyle = BorderStyle.FixedSingle };
            var btn = new Button { Text = "...", Width = 30, Anchor = AnchorStyles.Right, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, FlatStyle = FlatStyle.Flat };
            panel.Controls.Add(tb, 0, 0);
            panel.Controls.Add(btn, 1, 0);

            Action onRefresh = () =>
            {
                tb.Text = item.GetValue()?.ToString() ?? string.Empty;
                ApplyValidation(tb, item, theme);
            };
            Action onApply = () => { var old = item.GetValue(); if (!item.IsReadOnly) { item.SetValue(tb.Text); changed?.Invoke(item, old, tb.Text); ApplyValidation(tb, item, theme); } };
            refresh = onRefresh;
            apply = onApply;

            btn.Click += (s, e) =>
            {
                if (item.IsReadOnly) return;
                using (var fb = new FolderBrowserDialog { Description = attr.Description })
                {
                    if (fb.ShowDialog() == DialogResult.OK)
                    {
                        tb.Text = fb.SelectedPath; onApply();
                    }
                }
            };

            onRefresh();
            return panel;
        }

        private static Control CreateListDialogEditor(PropertyItem item, EditorOptions options, GridTheme theme, out Action apply, out Action refresh, Action<PropertyItem, object, object> changed)
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var btn = new Button { Text = "...", Anchor = AnchorStyles.Right, Width = 30, Enabled = !item.IsReadOnly, ForeColor = theme.EditorForeColor, BackColor = theme.EditorBackColor, FlatStyle = FlatStyle.Flat };
            var lbl = new Label { AutoEllipsis = true, Anchor = AnchorStyles.Left | AnchorStyles.Right, TextAlign = ContentAlignment.MiddleLeft, ForeColor = theme.EditorForeColor };

            panel.Controls.Add(lbl, 0, 0);
            panel.Controls.Add(btn, 1, 0);

            Action updateSummary = () =>
            {
                var list = item.GetValue() as IEnumerable;
                if (list == null) { lbl.Text = "(null)"; }
                else { var count = 0; foreach (var _ in list) count++; lbl.Text = $"(Collection) Count: {count}"; }
            };
            Action onApply = () => { };
            refresh = updateSummary;
            apply = onApply;

            btn.Click += (s, e) =>
            {
                if (item.IsReadOnly) return;
                var listValue = item.GetValue();
                using (var dlg = new ListEditorForm(listValue))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        var old = listValue;
                        item.SetValue(dlg.ResultList);
                        changed?.Invoke(item, old, dlg.ResultList);
                        updateSummary();
                    }
                }
            };

            // Subscribe to collection changes for realtime preview if supported
            if (options.SubscribeListChanges)
            {
                void trySubscribe(object col)
                {
                    if (col == null) return;
                    try
                    {
                        // IBindingList
                        if (col is System.ComponentModel.IBindingList bl)
                        {
                            bl.ListChanged += (s, e) => { updateSummary(); };
                            return;
                        }
                        // INotifyCollectionChanged (via reflection to avoid WPF reference)
                        var evt = col.GetType().GetEvent("CollectionChanged");
                        if (evt != null)
                        {
                            var handlerType = evt.EventHandlerType;
                            var invoker = new System.EventHandler((s, e) => updateSummary());
                            // create delegate with compatible signature if possible
                            var del = Delegate.CreateDelegate(handlerType, invoker.Target, invoker.Method, false);
                            if (del != null)
                            {
                                evt.AddEventHandler(col, del);
                            }
                        }
                    }
                    catch { /* ignore subscribe failures */ }
                }
                trySubscribe(item.GetValue());
            }

            updateSummary();
            return panel;
        }


    }
}
