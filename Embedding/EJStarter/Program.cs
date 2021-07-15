using System;
using System.Runtime.InteropServices;

namespace EJStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading();


            // Julia default IO output is redirected here automatically
            Julia.jl_eval_string("println(sin(2.34))");


            // get sin(2.34): the concise way
            // jl_eval_string returns a jl_value_t*, which is a pointer to a heap-allocated Julia object.
            IntPtr r = Julia.jl_eval_string("sin(2.34)");
            Console.WriteLine($"r_unboxed = {Julia.jl_unbox_float64(r)}");


            // get sin(2.34) with jl_call
            IntPtr sin = Julia.jl_eval_string("sin");
            IntPtr arg = Julia.jl_box_float64(2.34);
            IntPtr pRes = Julia.jl_call1(sin, arg);
            // memory not safe here!!
            double res = Julia.jl_unbox_float64(pRes);
            Console.WriteLine($"sin(2.34) = {res}");


            // call Pkg.envdir()
            Julia.jl_eval_string("using Pkg");
            IntPtr envdir = Julia.jl_eval_string("Pkg.envdir"); // the function
            IntPtr p1 = Julia.jl_call0(envdir); // return jl_value_t*
            IntPtr p2 = Julia.jl_string_ptr(p1); //get const char* from jl_value_t* since the true return is a string
            string envdirRes = Marshal.PtrToStringAnsi(p2);  // const char* --> C# string
            Console.WriteLine($"Pkg.envdir() -> {envdirRes}");


            // get sin(2.34): a complicated way
            IntPtr mBase = Julia.jl_eval_string("Base");
            IntPtr symSin = Julia.jl_symbol("sin");
            IntPtr sinFunc = Julia.jl_get_global(mBase, symSin); // get `Base.sin`
            IntPtr ret = Julia.jl_call1(sinFunc, Julia.jl_box_float64(2.34));
            // not memory safe here: see https://docs.julialang.org/en/v1/manual/embedding/#Memory-Management
            double ret_unboxed = Julia.jl_unbox_float64(ret);
            Console.WriteLine($"ret_unboxed = {ret_unboxed}");


            Julia.jl_atexit_hook(0);
        }
    }
}
