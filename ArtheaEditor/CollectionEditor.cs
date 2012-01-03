namespace ArtheaEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class CollectionEditor : UserControl
    {
        #region Fields

        IEditor _editor;
        private Stack<CollectionData> _history = new Stack<CollectionData>();

        #endregion Fields

        #region Constructors

        public CollectionEditor()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        #endregion Constructors

        #region Properties

        public object SelectedItem
        {
            get { return lstCollection.SelectedItem; }
        }

        #endregion Properties

        #region Methods

        public void Restore()
        {
            var data = _history.Pop();

            SetEditor(data.Editor as Control);
            SetCollection(data.Data);
        }

        public void Save()
        {
            _history.Push(new CollectionData(lstCollection.DataSource, _editor));
        }

        public void SetCollection(ICollection items)
        {
            lstCollection.DataSource = null;
            lstCollection.DataSource = items;
        }

        public void SetEditor(Control ctl)
        {
            _editor = ctl as IEditor;
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(ctl);
        }

        private void lstCollection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor == null || lstCollection.SelectedItem == null) return;

            _editor.Update(lstCollection.SelectedItem);
        }

        #endregion Methods

        #region Nested Types

        private struct CollectionData
        {
            #region Fields

            public ICollection Data;
            public IEditor Editor;

            #endregion Fields

            #region Constructors

            public CollectionData(object data, IEditor editor)
            {
                Data = data as ICollection;
                Editor = editor;
            }

            #endregion Constructors
        }

        #endregion Nested Types
    }
}