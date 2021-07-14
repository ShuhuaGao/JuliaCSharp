using System;
using System.Runtime.InteropServices;

namespace EJArrays
{
    class Program
    {
        static void Main(string[] _)
        {
            Julia.jl_init__threading();
            IntPtr juliaDLL = NativeWin.LoadLibrary("libjulia.dll");
            if (juliaDLL != IntPtr.Zero)
            {
                Console.WriteLine("DLL is loaded");
                IntPtr p_jl_float64_type = NativeWin.GetProcAddress(juliaDLL, "jl_float64_type");
                if (p_jl_float64_type != IntPtr.Zero)
                {
                    Console.WriteLine(IntPtr.Size);
                    unsafe
                    {
                        byte* p = (byte*)p_jl_float64_type.ToPointer();
                        Console.WriteLine(*p);
                        p++;
                        Console.WriteLine(*p);
                    }

                    // extern JL_DLLIMPORT jl_datatype_t *jl_float64_type
                    var jl_float64_type = Marshal.PtrToStructure<UIntPtr>(p_jl_float64_type);
                    Console.WriteLine(jl_float64_type);
                }

            }
        }
    }
}
