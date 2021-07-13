using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_eval_string(string str);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void jl_atexit_hook(int status);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_symbol(string str);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_get_global(IntPtr module, IntPtr sym);
    }
}
