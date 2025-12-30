using System.Windows.Forms;

namespace EnhancedPropertyGrid.Core
{
    partial class ListEditorForm
    {
        private System.ComponentModel.IContainer components = null;
        private ListBox lstItems;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnOk;
        private Button btnCancel;
        private Panel pnlRight;
        private global::EnhancedPropertyGrid.EnhancedPropertyGrid propertyGrid;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstItems = new ListBox();
            this.btnAdd = new Button();
            this.btnRemove = new Button();
            this.btnOk = new Button();
            this.btnCancel = new Button();
            this.pnlRight = new Panel();
            this.propertyGrid = new global::EnhancedPropertyGrid.EnhancedPropertyGrid();
            this.SuspendLayout();

            // lstItems
            this.lstItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.lstItems.Location = new System.Drawing.Point(12, 12);
            this.lstItems.Size = new System.Drawing.Size(220, 316);

            // btnAdd
            this.btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnAdd.Text = "添加";
            this.btnAdd.Location = new System.Drawing.Point(12, 336);
            this.btnAdd.Size = new System.Drawing.Size(60, 28);

            // btnRemove
            this.btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnRemove.Text = "删除";
            this.btnRemove.Location = new System.Drawing.Point(78, 336);
            this.btnRemove.Size = new System.Drawing.Size(60, 28);

            // btnOk
            this.btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnOk.Text = "确定";
            this.btnOk.Location = new System.Drawing.Point(606, 336);
            this.btnOk.Size = new System.Drawing.Size(80, 28);

            // btnCancel
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.Text = "取消";
            this.btnCancel.Location = new System.Drawing.Point(692, 336);
            this.btnCancel.Size = new System.Drawing.Size(80, 28);

            // pnlRight
            this.pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.pnlRight.Location = new System.Drawing.Point(238, 12);
            this.pnlRight.Size = new System.Drawing.Size(534, 316);
            this.pnlRight.BorderStyle = BorderStyle.FixedSingle;

            // propertyGrid
            this.propertyGrid.Dock = DockStyle.Fill;
            this.pnlRight.Controls.Add(this.propertyGrid);

            // Form
            this.ClientSize = new System.Drawing.Size(784, 381);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstItems);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "列表编辑器";

            this.ResumeLayout(false);
        }
    }
}
