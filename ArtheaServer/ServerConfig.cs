using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ArtheaEngine;
using ArtheaEngine.Model;

namespace ArtheaServer
{
    public class ServerConfig
    {
        public ServerConfig()
        {
            Worlds = new List<WorldConfig>();
        }

        public ServerConfig(int processId, IList<IServer> servers) : this()
        {
            foreach (IServer server in servers)
            {
                Worlds.Add(new WorldConfig(processId, server));
            }
        }

        public List<WorldConfig> Worlds { get; set; }
    }


    public class WorldConfig
    {
        public WorldConfig()
        {
            Connections = new List<ConnectionConfig>();
        }

        public WorldConfig(World world)
            : this()
        {
            Id = world.Id;
            Name = world.Name;
            Port = world.Port;
        }

        public WorldConfig(int processId, IServer server)
            : this(server.World)
        {
            foreach (IConnection c in server.Connections)
            {
                SocketInformation sockInfo = c.Socket.DuplicateAndClose(processId);

                Connections.Add(new ConnectionConfig {PlayerId = c.Playing.Id, SockInfo = sockInfo, State = c.State});
            }
        }

        public List<ConnectionConfig> Connections { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public short Port { get; set; }
    }


    public class ConnectionConfig : MarshalByRefObject
    {
        public string PlayerId { get; set; }

        public string State { get; set; }

        public SocketInformation SockInfo { get; set; }
    }
}