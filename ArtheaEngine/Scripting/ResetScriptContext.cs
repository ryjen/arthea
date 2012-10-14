using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using NLog;
using System;
using System.IO;
using ArtheaEngine.Model;
using Object = ArtheaEngine.Model.Object;

namespace ArtheaEngine.Scripting
{

	public class ResetScript : ScriptContext
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private Object lastOBJ;
		private NonPlayer lastNPC;

		public Room Room
		{
			get; private set;
		}

		public ResetScript(Room room) : base(new ScriptContext(room)) {
			Room = room;
		}

		public override void Apply (ArtheaScript script)
		{
			base.Apply(script);

			script.SetFunction("npc", new Action<string, int>(Npc));
			script.SetFunction("obj", new Action<string, int>(Obj));
			script.SetFunction("give", new Action<string, int>(Give));
			script.SetFunction("put", new Action<string, int>(Put));
			script.SetFunction("equip", new Action<string,string>(Equip));
			script.SetFunction("echo", new Action<string>(Echo));
		}

		public void Echo(string msg)
		{
			foreach (var ch in Room.Characters)
				ch.WriteLine(msg);
		}

		public void Equip (string where, string objRef)
		{
			WearLocation loc = (WearLocation)Enum.Parse (typeof(WearLocation), where, true);
			
			lastOBJ = World.FindObject (objRef);

			if (lastOBJ == null) {
				Log.Error ("object {0} not found for reset {1}", lastOBJ, Room.Id);
				return;
			}
			
			foreach (var co in lastNPC.Carrying)
			{
				if (co.WearLocation == loc) return;
			}
			
			lastOBJ.WearLocation = loc;
			
			lastOBJ.CarriedBy = lastNPC;
			
			lastNPC.Carrying.Add(lastOBJ);
		}

		
		public void Give (string objRef, int max = 1)
		{
			if (lastNPC == null)
				return;

			int count = 0;

			lastOBJ = World.FindObject (objRef);

			if (lastOBJ == null) {
				Log.Error ("object {0} was not found for reset {1}", objRef, Room.Id);
				return;
			}

			foreach (Object o in lastNPC.Carrying)
			{
				if (o.Equals (lastOBJ) ) count++;
			}
			
			if (count >= max) return;
			
			lastNPC.Carrying.Add(lastOBJ);
			lastOBJ.CarriedBy = lastNPC;
		}
		
		public void Npc (string npcRef, int max = 1)
		{
			int count = 0;

			lastNPC = World.FindNPC(npcRef);

			if (lastNPC == null) {
				Log.Error ("No such npc {0} for reset {1}", npcRef, Room.Id);
				return;
			}
			
			foreach (Character ch in Room.Characters)
			{
				if (ch.Equals(lastNPC)) count++;
			}
			
			if (count >= max) return;
			
			Room.Characters.Add(lastNPC);
		}
		
		public void Obj (string objRef, int max = 1)
		{
			int count = 0;

			lastOBJ = World.FindObject(objRef);
			
			if (lastOBJ == null) {
				Log.Error ("Invalid object '{0}' for reset {1}", objRef, Room.Id);
				return;
			}
			
			foreach (Object o in Room.Objects)
			{
				if (o.Equals(lastOBJ)) count++;
			}
			
			if (count >= max) return;
			
			Room.Objects.Add(lastOBJ);
		}
		
		public void Put (string objRef, int max = 1)
		{
			if(lastOBJ == null)
				return;

			int count = 0;

			var newObj = World.FindObject(objRef);
			
			if (newObj == null) {
				Log.Error ("Invalid object '{0}' for reset {1}", objRef, Room.Id);
				return;
			}
			
			foreach (Object o in lastOBJ.Contents)
			{
				if (o.Equals(newObj)) count++;
			}
			
			if (count >= max) return;
			
			lastOBJ.Contents.Add(newObj);
			newObj.Inside = lastOBJ;
		}

	}
}