namespace ArtheaEditor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using ArtheaEditor.Import.Rom;

    using ArtheaEngine.Model;

    using Object = ArtheaEngine.Model.Object;

    public partial class EditorForm : Form
    {
        #region Constructors

        public EditorForm()
        {
            InitializeComponent();

            Program.MainEditor = mainEditor;
        }

        #endregion Constructors

        #region Methods

        private void areasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainEditor.SetEditor(new AreaEditor());

            mainEditor.SetCollection(Area.List.ToList());
        }

        private void circleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Supported Yet.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void riversOfMudToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ImportDialog(new RomImporter()).ShowDialog();
        }

        private void rOTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Supported Yet.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void smaugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not Supported Yet.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        #endregion Methods
    }
}