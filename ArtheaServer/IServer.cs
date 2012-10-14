using System;
using ArtheaEngine.Model;
using System.Collections.Generic;
using ArtheaEngine;

namespace ArtheaServer
{
	public enum ServerState
	{
		Initializing,
		Running,
		Stopped
	}

	public interface IServer
	{

		HashSet<IConnection> Connections
		{
			get;
		}

		ServerState State
		{
			get; set;
		}

		World World
		{
			get;
		}

		void Listen();

		void Start();

		void Stop();
	}
}

