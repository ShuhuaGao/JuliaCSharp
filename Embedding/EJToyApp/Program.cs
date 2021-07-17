using System;
using MathNet.Numerics.LinearAlgebra;
using System.Runtime.InteropServices;

namespace EJToyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Julia.jl_init__threading();
            // for Julia GC rooting
            IntPtr REFS = Julia.jl_eval_string("const REFS = IdDict()");
            IntPtr setindex_ = Julia.jl_eval_string("setindex!");
            IntPtr delete_ = Julia.jl_eval_string("delete!");

            // get the `Array` type
            IntPtr juliaDLL = Win32.LoadLibraryA("libjulia.dll");
            IntPtr pJlFloat64 = Win32.GetProcAddress(juliaDLL, "jl_float64_type");
            IntPtr jlFloat64 = Marshal.ReadIntPtr(pJlFloat64);
            IntPtr VectorFloat64 = Julia.jl_apply_array_type(jlFloat64, 1);
            IntPtr MatrixFloat64 = Julia.jl_apply_array_type(jlFloat64, 2);

            // build a random matrix and vector in MathNet (column major)
            Matrix<double> A = Matrix<double>.Build.Random(4, 4);
            Vector<double> b = Vector<double>.Build.Random(4);
            Console.WriteLine($"A = {A}");
            Console.WriteLine($"b = {b}");

            unsafe
            {
                fixed (double* pb = b.AsArray())  // pin `b` 
                {
                    // wrap b's data into a Julia vector
                    IntPtr jlb = Julia.jl_ptr_to_array_1d(VectorFloat64, (IntPtr)pb, b.Count, 0);
                    Julia.jl_call3(setindex_, REFS, jlb, jlb); // keep `jlb` live
                    // allocate a 2D Julia matrix
                    IntPtr jlA = Julia.jl_alloc_array_2d(MatrixFloat64, A.RowCount, A.ColumnCount);
                    Julia.jl_call3(setindex_, REFS, jlA, jlA);
                    IntPtr jlAData = Julia.jl_array_data(jlA);  // pointer to its data
                    // copy data from A to jlA
                    Marshal.Copy(A.AsColumnMajorArray(), 0, jlAData, A.RowCount * A.ColumnCount);
                    // (optionally) print to verify jlA
                    Julia.jl_call1(Julia.jl_eval_string("println"), jlA);
                    // solve Ax = b
                    IntPtr jlfunc = Julia.jl_eval_string("\\");
                    IntPtr jlx = Julia.jl_call2(jlfunc, jlA, jlb);
                    Julia.jl_call3(setindex_, REFS, jlx, jlx);
                    // retrieve the data of jlx to C#
                    Vector<double> x = Vector<double>.Build.Dense(A.ColumnCount);
                    Marshal.Copy(Julia.jl_array_data(jlx), x.AsArray(), 0, x.Count);
                    Console.WriteLine($"x = {x}");
                    // after finish, allow Julia to garbage collect variables
                    Julia.jl_call2(delete_, REFS, jlb);
                    Julia.jl_call2(delete_, REFS, jlA);
                    Julia.jl_call2(delete_, REFS, jlx);
                }
            }


            Julia.jl_atexit_hook(0);

        }


    }
}
