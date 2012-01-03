namespace ArtheaEditor
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using ArtheaEditor.Import;
    using ArtheaEditor.Import.Rom;

    using NLog;
    using NLog.Config;
    using NLog.Targets;

    public partial class ImportDialog : Form
    {
        #region Fields

        private IImporter _importer;

        #endregion Fields

        #region Constructors

        public ImportDialog(IImporter importer)
        {
            InitializeComponent();

            _importer = importer;
        }

        #endregion Constructors

        #region Methods

        private void btnCommit_Click(object sender, EventArgs e)
        {
            _importer.Commit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {

                RichTextBoxTarget target = new RichTextBoxTarget();
                target.ControlName = txtImportLog.Name;
                target.FormName = this.Name;

                target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

                SimpleConfigurator.ConfigureForTargetLogging(target);

                var fi = new FileInfo(textBox1.Text);

                _importer.Process(fi);

                btnCommit.Enabled = true;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Please specify a file first!");
            }
            catch (ImportException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Methods
    }
}