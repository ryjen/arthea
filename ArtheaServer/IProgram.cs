namespace ArtheaServer
{
    internal interface IProgram
    {
        void Run(Arguments args);

        void Exit();
    }
}