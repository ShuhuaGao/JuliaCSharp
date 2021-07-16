using System;
using System.Runtime.InteropServices;

namespace EJMemory
{
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading();

            // Create a global `IdDict` during the initialization.
            IntPtr REFS = Julia.jl_eval_string("const REFS = IdDict()");
            IntPtr setindex_ = Julia.jl_eval_string("setindex!");
            // `var` is a `Vector{Float64}`, which is mutable.
            IntPtr var = Julia.jl_eval_string("[sqrt(2.0); sqrt(4.0); sqrt(6.0)]");
            // To protect `var`, add its reference to `REFS`.
            Julia.jl_call3(setindex_, REFS, var, var);
            // read the array data with unsafe code and a raw pointer
            unsafe
            {
                double* data = (double*)Marshal.ReadIntPtr(var).ToPointer();
                for (int i = 0; i < 3; i++)
                    Console.WriteLine(*(data + i));
            }


            Julia.jl_atexit_hook(0);
        }
    }
}
