using System;

namespace EJStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading(); // correct
            Julia.jl_eval_string("println(sqrt(3.14))");

            IntPtr mBase = Julia.jl_eval_string("Base");
            IntPtr symSin = Julia.jl_symbol("sin");
            IntPtr sin = Julia.jl_get_global(mBase, symSin);

            Julia.jl_atexit_hook(0);
            Console.WriteLine("Hello World!");
        }
    }
}
