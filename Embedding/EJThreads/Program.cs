using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace EJThreads
{
    class Program
    {
        // The `Julia` class imply p/invokes libjulia.
        static void Main(string[] args)
        {
            var t1 = new Thread(RunJuliaThread);
            t1.Start(); // start a new thread
            t1.Join(); // wait for t1 to finish

            Julia.jl_eval_string("nothing");
        }


        static void RunJuliaThread()
        {
            Julia.jl_init__threading();
            IntPtr ans = Julia.jl_eval_string("1.3 + 2");
            Debug.Assert(ans != IntPtr.Zero); // all good here
            Console.WriteLine(Julia.jl_unbox_float64(ans));
        }

        static void DoWork()
        {
            Julia.jl_eval_string("nothing"); // > System.AccessViolationException

        }
    }
}
