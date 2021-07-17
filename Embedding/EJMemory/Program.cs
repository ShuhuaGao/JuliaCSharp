using System;
using System.Runtime.InteropServices;


namespace EJMemory
{
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading();

            #region keep an object live in Julia
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
            #endregion


            #region pass data from C# to Julia
            Console.WriteLine("\n Test array from C# to Julia");
            // get the `Array{Float64, 1}` type
            IntPtr juliaDLL = Win32.LoadLibraryA("libjulia.dll");
            IntPtr pJlFloat64 = Win32.GetProcAddress(juliaDLL, "jl_float64_type");
            IntPtr jlFloat64 = Marshal.ReadIntPtr(pJlFloat64);
            IntPtr arrayType = Julia.jl_apply_array_type(jlFloat64, 1);

            // pin a C# array with `GCHandle`
            double[] csArray = new double[] { 3.14, 1.2, 4.5, 6.7, -10 };
            GCHandle h = GCHandle.Alloc(csArray, GCHandleType.Pinned);
            // wrap the array into a `jl_array_t*` (interpreted as a Vector{Float64} in Julia)
            IntPtr jlArray = Julia.jl_ptr_to_array_1d(arrayType, h.AddrOfPinnedObject(), csArray.Length, 0);
            // apply in-place reverse!
            IntPtr reverse_ = Julia.jl_eval_string("reverse!");
            Julia.jl_call1(reverse_, jlArray);
            // check the result
            foreach (double ele in csArray)
                Console.WriteLine(ele);
            // free the handle
            h.Free();

            #endregion

            Julia.jl_atexit_hook(0);
        }
    }
}
