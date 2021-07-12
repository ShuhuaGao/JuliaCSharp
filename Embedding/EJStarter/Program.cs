using System;

namespace EJStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading();
            Console.WriteLine("Hello World!");
        }
    }
}
