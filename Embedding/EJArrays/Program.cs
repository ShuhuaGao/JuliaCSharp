using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace EJArrays
{
    class Program
    {
        static void Main(string[] _)
        {
            // always initialize Julia first
            Julia.jl_init__threading();


            #region get an exported extern variable
            IntPtr juliaDLL = Win32.LoadLibraryA("libjulia.dll");
            Debug.Assert(juliaDLL != IntPtr.Zero);

            //`pJlFloat64` is a pointer to the pointer variable `jl_float64_type`
            IntPtr pJlFloat64 = Win32.GetProcAddress(juliaDLL, "jl_float64_type");
            // dereference `pJlFloat64` to obtain the pointer variable
            // Use `PtrToStructure` in general, but better to use the dedicated `ReadIntPtr`
            //IntPtr jlFloat64 = Marshal.PtrToStructure<IntPtr>(pJlFloat64);
            IntPtr jlFloat64 = Marshal.ReadIntPtr(pJlFloat64);
            Debug.Assert(jlFloat64 != IntPtr.Zero);

            Win32.FreeLibrary(juliaDLL);
            #endregion


            #region create, fill, and print a 1D array
            // NOT memory safe in this part
            // allocate an array managed by Julia
            IntPtr arrayType = Julia.jl_apply_array_type(jlFloat64, 1);
            IntPtr jlVector = Julia.jl_alloc_array_1d(arrayType, 5);  // an array of length 5
            // fill the array in Julia
            IntPtr fill_ = Julia.jl_eval_string("fill!");
            Julia.jl_call2(fill_, jlVector, Julia.jl_box_float64(3.14));
            // get the pointer to the data member of the `jl_array_t`
            IntPtr jlVectorData = Marshal.ReadIntPtr(jlVector);
            // access the array in C#: copy from Julia to C# managed memory
            double[] csVector = new double[5];
            Marshal.Copy(jlVectorData, csVector, 0, 5);
            foreach (var item in csVector)
            {
                Console.WriteLine(item);
            }

            #endregion


            #region add 1 to the above array
            Console.WriteLine("Add 1 to the array");
            IntPtr dotAdd = Julia.jl_eval_string(".+");
            IntPtr jlVector1 = Julia.jl_call2(dotAdd, jlVector, Julia.jl_box_float64(1.0)); // return an array
            IntPtr jlVector1Data = Marshal.ReadIntPtr(jlVector1);
            for (int i = 0; i < 5; i++)
            {
                IntPtr p = IntPtr.Add(jlVector1Data, sizeof(double) * i);
                Console.WriteLine(Marshal.PtrToStructure<double>(p));
            }
            #endregion


            Julia.jl_atexit_hook(0);
        }
    }
}
