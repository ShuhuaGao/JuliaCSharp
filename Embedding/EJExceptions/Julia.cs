// Main documentation:  https://docs.julialang.org/en/v1/manual/embedding/#Embedding-Julia

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EJExceptions
{
    // see https://github1s.com/JuliaLang/julia/blob/HEAD/src/julia.h
    class Julia
    {
        public const string JULIALIB = "libjulia";

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

        // jl_value_t *jl_call2(jl_function_t *f JL_MAYBE_UNROOTED, jl_value_t *a JL_MAYBE_UNROOTED, jl_value_t *b JL_MAYBE_UNROOTED);
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_call2(IntPtr function, IntPtr arg1, IntPtr arg2);


        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_call3(IntPtr function, IntPtr arg1, IntPtr arg2, IntPtr arg3);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_box_float64(double x);

        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double jl_unbox_float64(IntPtr v);

        // jl_value_t *jl_apply_array_type(jl_value_t *type, size_t dim)
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_apply_array_type(IntPtr type, UInt64 dim);

        // jl_array_t *jl_alloc_array_1d(jl_value_t *atype, size_t nr);
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_alloc_array_1d(IntPtr type, UInt64 n);

        // jl_value_t *jl_get_field(jl_value_t *o, const char *fld)
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr jl_get_field(IntPtr obj, string field);

        // jl_value_t *jl_get_nth_field(jl_value_t *v, size_t i)
        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_get_nth_field(IntPtr type, UInt64 i);

        // jl_value_t *jl_exception_occurred(void)
        [DllImport(JULIALIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_exception_occurred();

        // void jl_exception_clear(void)
        [DllImport(JULIALIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void jl_exception_clear();

        // const char *jl_typeof_str(jl_value_t *v)
        [DllImport(JULIALIB, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr jl_typeof_str(IntPtr v);


        [DllImport("libjulia.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr jl_typename_str(IntPtr value);


    }
}
