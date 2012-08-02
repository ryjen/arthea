namespace ArtheaEditor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public partial class NpcEditor : InstanceEditor
    {
        #region Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AddRow(PropertyHelper<NonPlayer>.GetProperty(x => x.Keyword()), new TextField { ReadOnly = true });
            AddRow(PropertyHelper<NonPlayer>.GetProperty(x => x.Name), new TextField());
            AddRow(PropertyHelper<NonPlayer>.GetProperty(x => x.Description), new TextField { Multiline = true });
        }

        #endregion Methods
    }
}