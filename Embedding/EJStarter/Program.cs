using System;

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


            // get sin(2.34): the verbose but more general way
            IntPtr mBase = Julia.jl_eval_string("Base");
            IntPtr symSin = Julia.jl_symbol("sin");
            IntPtr sin = Julia.jl_get_global(mBase, symSin); // get `Base.sin`
            IntPtr ret = Julia.jl_call1(sin, Julia.jl_box_float64(2.34));
            // not memory safe here: see https://docs.julialang.org/en/v1/manual/embedding/#Memory-Management
            double ret_unboxed = Julia.jl_unbox_float64(ret);
            Console.WriteLine($"ret_unboxed = {ret_unboxed}");




            Julia.jl_atexit_hook(0);
            Console.WriteLine("Hello World!");
        }
    }
}
