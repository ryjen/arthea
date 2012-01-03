using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArtheaEngine.Domain;
using Object = ArtheaEngine.Domain.Objects.Object;
using Db4objects.Db4o;

namespace ArtheaServer
{
    public class World : IWorld
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public short Port { get; set; }
        public IList<Object> Objects { get; set; }
        public IList<Player> Players { get; set; }
        public IList<IConnection> Connections { get; set; }
        public IList<Character> Characters { get; set; }
        public IList<Area> Areas { get; set; }
        public EntityRepository Repository { get; set; }

        public Room GetDefaultRoom()
        {
            return null;
        }
    }
}
