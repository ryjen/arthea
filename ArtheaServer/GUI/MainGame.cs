namespace ArtheaServer
{
    using ArtheaEngine;
    using ArtheaEngine.Model;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;


    public partial class MainGame : Form
    {
        #region Constructors

        public MainGame()
        {
            InitializeComponent();

            foreach (var entry in DbManager.Query<World>())
            {
				var saved = RebootHelper.Recover(entry);

                var mi = new ToolStripMenuItem(entry.Name);

				ToolStripMenuItem miStart;

				if(saved == null)
                	miStart = new ToolStripMenuItem("Start") { Tag = entry };
				else
					miStart = new ToolStripMenuItem("Stop") { Tag = saved };

                miStart.Click += miStart_Click;
                mi.DropDownItems.Add(miStart);

                worldToolStripMenuItem.DropDownItems.Add(mi);
            }
        }

        #endregion Constructors

        #region Methods

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var buf = new StringBuilder();

            var assName = Assembly.GetAssembly(typeof(World)).GetName(false);

            var vrs = assName.Version;

            buf.Append(assName.Name).Append(": v");
            buf.AppendFormat("{0}.{1}.{2}", vrs.Major, vrs.Minor, vrs.Build);
            buf.AppendLine();
            buf.AppendLine();

            assName = Assembly.GetAssembly(typeof (TelnetServer)).GetName(false);

            vrs = assName.Version;

            buf.Append(assName.Name).Append(": v");
            buf.AppendFormat("{0}.{1}.{2}", vrs.Major, vrs.Minor, vrs.Build);
            buf.AppendLine();
            buf.AppendLine();

            buf.AppendLine("Copyright " + DateTime.Now.ToString("yyyy") + " Ryan Jennings.");

            MessageBox.Show(buf.ToString(), "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var s in TelnetServer.List.Reverse())
            {
                s.Stop();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState && minimizeToTrayToolStripMenuItem.Checked)
            {
                Hide();
                notifier.Visible = true;
            }
        }

        private void miStart_Click(object sender, EventArgs e)
        {
            var mi = sender as ToolStripMenuItem;

            if (mi == null) return;

            if (mi.Tag is World)
            {
                var world = mi.Tag as World;
                mi.Tag = StartWorld(world);
                mi.Text = "&Stop";
            }
            else if (mi.Tag is TelnetServer)
            {
                var server = mi.Tag as TelnetServer;
                mi.Tag = StopWorld(server);
                mi.Text = "&Start";
            }
        }

        private void notifierContextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            mainMenu.Items.Add(worldToolStripMenuItem);
        }

        private void notifierContextMenu_Opening(object sender, CancelEventArgs e)
        {
            notifierContextMenu.Items.Add(worldToolStripMenuItem);
        }

        private void notifier_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreMainForm();
        }

        private void Output_Enter(object sender, EventArgs e)
        {
            mainMenu.Focus();
        }

        private void RestoreMainForm()
        {
            Show();
            notifier.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreMainForm();
        }

        private TelnetServer StartWorld (World world)
		{
			var server = new TelnetServer (world);
			StartServer (server);
			return server;
		}

		private void StartServer(TelnetServer server)
		{
			World world = server.World;

            var tabPage = new TabPage(world.Name);
            tabPage.Tag = world.Id;

            var output = new RichTextBox();
            output.Enter += Output_Enter;
            output.Name = world.Name + "_" + world.Id;
            output.Dock = DockStyle.Fill;
            tabPage.Controls.Add(output);

            RTLogTarget target = new RTLogTarget(output);
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";

            SimpleConfigurator.ConfigureForTargetLogging(target);

            tabControl1.TabPages.Add(tabPage);

            server.Start();
        }

        private World StopWorld(TelnetServer server)
        {
            server.Stop();
            foreach (TabPage tabPage in tabControl1.TabPages)
            {
                if (tabPage.Tag.Equals(server.World.Id))
                {
                    tabControl1.TabPages.Remove(tabPage);
                    break;
                }
            }
            return server.World;
        }

        #endregion Methods
    }

    public sealed class RTLogTarget : TargetWithLayout
    {
        #region Constructors

        public RTLogTarget(RichTextBox box)
        {
            this.Display = box;
        }

        #endregion Constructors

        #region Delegates

        public delegate void AppendTextDelegate(string text);

        #endregion Delegates

        #region Properties

        public RichTextBox Display
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public void AppendLine(string text)
        {
            Display.AppendText(text);
            Display.AppendText(Environment.NewLine);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);

            if (Display.InvokeRequired)
            {
                Display.Invoke(new AppendTextDelegate(AppendLine), logMessage);
            }
            else
            {
                AppendLine(logMessage);
            }
        }

        #endregion Methods
    }

    public class WorldConfig
    {
        #region Constructors

        public WorldConfig()
        {
            Connections = new List<ConnectionConfig>();
        }

		public WorldConfig (World world) : this()
		{
			Id = world.Id;
			Name = world.Name;
			Port = world.Port;
		}

		public WorldConfig(TelnetServer server) : this(server.World)
		{
				foreach(var c in server.Connections)
				{
					var sockInfo = c.Socket.DuplicateAndClose(Process.GetCurrentProcess().Id);

					Connections.Add(new ConnectionConfig { PlayerId = c.Playing.Id, SockInfo = sockInfo });
				}
		}

        #endregion Constructors

        #region Properties

        public List<ConnectionConfig> Connections
        {
            get; set;
        }

        public string Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public short Port
        {
            get;
            set;
        }

        #endregion Properties

        #region Nested Types

        public class ConnectionConfig
        {
            #region Properties

            public string PlayerId
            {
                get;
                set;
            }

            public SocketInformation SockInfo
            {
                get;
                set;
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}