namespace ArtheaEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    public interface IEditField : IEditor
    {
        #region Methods

        object Retrieve();

        #endregion Methods
    }

    public class CollectionField : Button, IEditField
    {
        #region Fields

        object _data;
        Control _editor;

        #endregion Fields

        #region Constructors

        public CollectionField(IEditor editor)
        {
            _editor = editor as Control;

            Click += StartEditor;
        }

        #endregion Constructors

        #region Methods

        public object Retrieve()
        {
            return _data;
        }

        public void Update(object value)
        {
            _data = value;

            if (value == null || !(value is ICollection))
            {
                Text = "0 items";
                return;
            }

            Text = (value as ICollection).Count + " items";
        }

        private void StartEditor(object o, EventArgs e)
        {
            Program.MainEditor.Save();

            var back = new Button();
            back.Text = "Back";
            back.Dock = DockStyle.Bottom;
            back.Click += (o2, e2) => { Program.MainEditor.Restore(); _editor.Controls.Remove(back); };
            _editor.Controls.Add(back);

            Program.MainEditor.SetEditor(_editor);
            Program.MainEditor.SetCollection(_data as ICollection);
        }

        #endregion Methods
    }

    public class SelectorField : Button, IEditField
    {
        #region Fields

        private object _data;

        #endregion Fields

        #region Constructors

        public SelectorField()
        {
            Click += SelectItem;
        }

        #endregion Constructors

        #region Methods

        public object Retrieve()
        {
            return _data;
        }

        public void Update(object value)
        {
            _data = value;
            Text = value == null ? "None" : value.ToString();
        }

        private void SelectItem(object o, EventArgs e)
        {
            MessageBox.Show("Not supported yet.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        #endregion Methods
    }

    public class TextField : TextBox, IEditField
    {
        #region Constructors

        public TextField()
        {
            Dock = DockStyle.Fill;
        }

        #endregion Constructors

        #region Methods

        public object Retrieve()
        {
            return Text;
        }

        public void Update(object value)
        {
            Text = value == null ? string.Empty : value.ToString();
        }

        #endregion Methods
    }
}