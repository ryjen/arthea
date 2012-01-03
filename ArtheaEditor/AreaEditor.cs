namespace ArtheaEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;

    using ArtheaEngine;
    using ArtheaEngine.Model;

    public partial class AreaEditor : InstanceEditor
    {
        #region Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AddRow(PropertyHelper<Area>.GetProperty(x => x.Id), new TextField { ReadOnly = true });
            AddRow(PropertyHelper<Area>.GetProperty(x => x.Name), new TextField());
            AddRow(PropertyHelper<Area>.GetProperty(x => x.Description), new TextField { Multiline = true });
            AddRow(PropertyHelper<Area>.GetProperty(x => x.Credits), new TextField());
            AddRow(PropertyHelper<Area>.GetProperty(x => x.World), new SelectorField());
            AddRow(PropertyHelper<Area>.GetProperty(x => x.NonPlayers), new CollectionField(new NpcEditor()));
        }

        #endregion Methods
    }
}