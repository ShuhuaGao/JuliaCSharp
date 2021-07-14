// Main documentation:  https://docs.julialang.org/en/v1/manual/embedding/#Embedding-Julia

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EJStarter
{
    // see https://github1s.com/JuliaLang/julia/blob/HEAD/src/julia.h
    class Julia
    {
        // normally, "libjulia.dll" can be found automatically
        // check https://stackoverflow.com/questions/8836093/how-can-i-specify-a-dllimport-path-at-runtime/8861895
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void jl_init__threading();

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr jl_eval_string(string str);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void jl_atexit_hook(int status);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr jl_symbol(string str);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_get_global(IntPtr module, IntPtr sym);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_call1(IntPtr function, IntPtr arg);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_box_float64(double x);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double jl_unbox_float64(IntPtr v);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr jl_exception_occurred();

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr jl_typeof_str(IntPtr value);
    }
}
