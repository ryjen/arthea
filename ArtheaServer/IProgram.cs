using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArtheaServer
{
    internal interface IProgram
    {
        void Run(Arguments args);

        void Exit();
    }
}
