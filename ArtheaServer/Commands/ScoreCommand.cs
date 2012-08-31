using System;
using ArtheaServer.Commands;
using ArtheaEngine.Model;
using ArtheaEngine;

namespace ArtheaServer
{
	public class ScoreCommand : Command
	{
		public ScoreCommand () : base("score", "Displays information on your current character.")
		{
		}

		public override void Execute (Character ch, Argument argument)
		{
			ColumnFormatter cols = new ColumnFormatter (2, ch);

			cols.SetAlign(ColumnAlign.Center);

			cols.AddHeader (ch.Name);

			cols.SetBorder (ColumnBorder.Left);

			cols.Add ("~cDefense: ~W{0}", ch.Defense);

			cols.SetBorder (ColumnBorder.Left|ColumnBorder.Right);
			
			cols.Add ("~cLife: ~W{0}/{1}", ch.Vitals.Life, ch.Vitals.MaxLife);

			cols.SetBorder (ColumnBorder.Left);

			cols.Add ("~cLevel: ~W{0}", ch.Level);
			
			cols.SetBorder (ColumnBorder.Left|ColumnBorder.Right);
			
			cols.Add ("~cMana: ~W{0}/{1}", ch.Vitals.Mana, ch.Vitals.MaxMana);

			cols.SetBorder (ColumnBorder.Left);

			cols.Add ("~cRace: ~W{0}", ch.Race.Name);
			
			cols.SetBorder (ColumnBorder.Left|ColumnBorder.Right);

			cols.Add("~cEnergy: ~W{0}/{1}", ch.Vitals.Energy, ch.Vitals.MaxEnergy);

			cols.SetBorder (ColumnBorder.Left);

			cols.Add("~cSex: ~W{0}", ch.Sex);

			cols.SetAlign(ColumnAlign.Center);

			cols.AddHeader(1, "Screen");

			cols.SetBorder (ColumnBorder.Left);

			cols.Add ("~cSize: ~W{0}", ch.Size);

			cols.SetBorder (ColumnBorder.Left|ColumnBorder.Right);

			cols.Add ("~cWidth: ~W{0}", (ch as Player).Account.Connection.ScreenWidth);

			cols.SetBorder(ColumnBorder.Left);

			cols.Add("~cPosition: ~W{0}", ch.Position);

			cols.SetBorder (ColumnBorder.Left|ColumnBorder.Right);

			cols.Add ("~cHeight: ~W{0}", (ch as Player).Account.Connection.ScreenHeight);

			cols.AddHeader();

			ch.Page(cols.GetBuffer());
        }
	}
}

