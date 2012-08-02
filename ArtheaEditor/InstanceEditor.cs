namespace ArtheaEditor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;

    public abstract partial class InstanceEditor : UserControl, IEditor
    {
        #region Fields

        private object _editing;
        private Dictionary<PropertyInfo, IEditField> _fields = new Dictionary<PropertyInfo, IEditField>();

        #endregion Fields

        #region Constructors

        public InstanceEditor()
        {
            InitializeComponent();

            Dock = DockStyle.Fill;
        }

        #endregion Constructors

        #region Methods

        public void AddRow(PropertyInfo pi, IEditField field)
        {
            var label = new Label { Text = pi.Name + ":" };

            tableLayoutPanel1.Controls.Add(label, 0, _fields.Count);

            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            tableLayoutPanel1.Controls.Add(field as Control, 1, _fields.Count);

            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _fields.Add(pi, field);
        }

        public void Update(object value)
        {
            _editing = value;

            UpdateFields();
        }

        protected void UpdateFields()
        {
            foreach (var entry in _fields)
            {
                entry.Value.Update(entry.Key.GetValue(_editing, null));
            }
        }

        #endregion Methods
    }
}