using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace EnhancedPropertyGrid.Core
{
    internal partial class ListEditorForm : Form
    {
        private object _originalList;
        private IList _workList;
        private Type _elementType;

        public object ResultList { get; private set; }

        public ListEditorForm(object list)
        {
            InitializeComponent();
            _originalList = list;
            (_workList, _elementType) = CreateWorkingList(list);
            LoadListToUI();

            lstItems.SelectedIndexChanged += (s, e) => UpdateEditorForSelection();
            btnAdd.Click += (s, e) => AddItem();
            btnRemove.Click += (s, e) => RemoveSelected();
            btnOk.Click += (s, e) => { SaveFromEditor(); ResultList = MaterializeResult(); this.DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; };
        }

        private void LoadListToUI()
        {
            lstItems.Items.Clear();
            foreach (var item in _workList)
            {
                lstItems.Items.Add(ItemToText(item));
            }
            if (lstItems.Items.Count > 0) lstItems.SelectedIndex = 0;
        }

        private string ItemToText(object item)
        {
            if (item == null) return "(null)";
            return item.ToString();
        }

        private void UpdateEditorForSelection()
        {
            SaveFromEditor();
            if (_elementType == typeof(string))
            {
                propertyGrid.Visible = false;
                return;
            }

            propertyGrid.Visible = true;
            var idx = lstItems.SelectedIndex;
            if (idx < 0 || idx >= _workList.Count) { propertyGrid.SelectedObject = null; return; }
            propertyGrid.SelectedObject = _workList[idx];
        }

        private void SaveFromEditor()
        {
            // No-op for string list (edited via prompt)
        }

        private void AddItem()
        {
            if (_elementType == typeof(string))
            {
                var text = Prompt("输入新项：", "添加字符串");
                if (text != null) { _workList.Add(text); lstItems.Items.Add(ItemToText(text)); }
                return;
            }

            object newItem = null;
            try
            {
                newItem = Activator.CreateInstance(_elementType);
            }
            catch { }
            if (newItem == null)
            {
                MessageBox.Show($"无法创建类型 {_elementType.Name} 的实例（需要无参构造函数）。", "添加失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _workList.Add(newItem);
            lstItems.Items.Add(ItemToText(newItem));
            lstItems.SelectedIndex = lstItems.Items.Count - 1;
        }

        private void RemoveSelected()
        {
            var idx = lstItems.SelectedIndex;
            if (idx < 0) return;
            _workList.RemoveAt(idx);
            lstItems.Items.RemoveAt(idx);
            if (idx < lstItems.Items.Count) lstItems.SelectedIndex = idx; else if (lstItems.Items.Count > 0) lstItems.SelectedIndex = lstItems.Items.Count - 1;
        }

        private (IList list, Type elementType) CreateWorkingList(object list)
        {
            if (list == null)
            {
                // default to List<string>
                return (new List<string>(), typeof(string));
            }

            var listType = list.GetType();
            Type elementType = null;
            if (listType.IsArray)
            {
                elementType = listType.GetElementType();
                var arr = (Array)list;
                var lo = new List<object>();
                foreach (var item in arr) lo.Add(item);
                return (new ArrayList(lo), elementType);
            }

            var iListOfT = listType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));
            if (iListOfT != null)
            {
                elementType = iListOfT.GetGenericArguments()[0];
                var tmpListType = typeof(List<>).MakeGenericType(elementType);
                var tmp = (IList)Activator.CreateInstance(tmpListType);
                foreach (var item in (IEnumerable)list) tmp.Add(item);
                return (tmp, elementType);
            }

            if (list is IList il)
            {
                // Non-generic IList, assume object
                elementType = typeof(object);
                var tmp = new ArrayList();
                foreach (var item in il) tmp.Add(item);
                return (tmp, elementType);
            }

            // Fallback
            return (new ArrayList(), typeof(object));
        }

        private object MaterializeResult()
        {
            if (_originalList == null)
            {
                // Return a List<T>
                var listType = typeof(List<>).MakeGenericType(_elementType);
                var result = (IList)Activator.CreateInstance(listType);
                foreach (var item in _workList) result.Add(item);
                return result;
            }

            var t = _originalList.GetType();
            if (t.IsArray)
            {
                var arr = Array.CreateInstance(_elementType, _workList.Count);
                for (int i = 0; i < _workList.Count; i++) arr.SetValue(_workList[i], i);
                return arr;
            }

            // Try to create same type instance
            try
            {
                var res = (IList)Activator.CreateInstance(t);
                foreach (var item in _workList) res.Add(item);
                return res;
            }
            catch
            {
                // Fallback to List<T>
                var listType = typeof(List<>).MakeGenericType(_elementType);
                var result = (IList)Activator.CreateInstance(listType);
                foreach (var item in _workList) result.Add(item);
                return result;
            }
        }

        private static string Prompt(string text, string caption)
        {
            using (var form = new Form())
            using (var label = new Label())
            using (var textBox = new TextBox())
            using (var buttonOk = new Button())
            using (var buttonCancel = new Button())
            {
                form.Text = caption;
                label.Text = text;
                buttonOk.Text = "确定";
                buttonCancel.Text = "取消";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 10, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new System.Drawing.Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}

