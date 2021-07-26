using System;
using System.Runtime.InteropServices;

namespace EJExceptions
{
    // https://docs.julialang.org/en/v1/manual/embedding/#Exceptions
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading();

            IntPtr v = Julia.jl_eval_string("[1, 2, 3]");
            var t = Julia.jl_typeof_str(v);
            Console.WriteLine(Marshal.PtrToStringAnsi(t));
            var tname = Julia.jl_typename_str(v);
            Console.WriteLine(Marshal.PtrToStringAnsi(tname));

            Julia.jl_eval_string("this_function_does_not_exist()");
            var jex = Julia.jl_exception_occurred();
            if (jex != IntPtr.Zero)
            {
                var tjex = Julia.jl_typeof_str(jex);
                Console.WriteLine(Marshal.PtrToStringAnsi(tjex));
            }

            Julia.jl_atexit_hook(0);
        }
    }
}
