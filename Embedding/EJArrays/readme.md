# Work with arrays and memory management

## Create an array
### C code in the documentation
We see in the Julia [documentation](https://docs.julialang.org/en/v1/manual/embedding/#Working-with-Arrays) that a 1D array is created as follows:
```c
jl_value_t* array_type = jl_apply_array_type((jl_value_t*)jl_float64_type, 1);
jl_array_t* x          = jl_alloc_array_1d(array_type, 10);
```
Clearly, we need to specify the data type (like `jl_float64_type`) and the size of the array to create a `array_type` for subsequent consumption.

The exported C interface of `jl_apply_array_type` in [julia.h](https://github1s.com/JuliaLang/julia/blob/HEAD/src/julia.h#L1532) is 
```c
JL_DLLEXPORT jl_value_t *jl_apply_array_type(jl_value_t *type, size_t dim);
```

Note that the above `jl_float64_type` is an *extern* variable of type `jl_datatype_t*` exported in the DLL:
```c
extern JL_DLLIMPORT jl_datatype_t *jl_float64_type 
```
That is, `jl_float64_type` is like a singleton variable initialized by the Julia runtime and exposed to us in the DLL. 

Unlike a function, we cannot `DllImport` an extern variable in C#. 

### Access an exported variable from a DLL

A good tutorial is posted in [Accessing Exported Data From a DLL in Managed Code.](https://limbioliong.wordpress.com/2011/11/11/accessing-exported-data-from-a-dll-in-managed-code/). We brief the procedures below, which involve three Win32 functions in order.

- [LoadLibraryA](https://docs.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-loadlibrarya)
  Loads the specified module into the address space of the calling process.
- [GetProcAddress](https://docs.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-getprocaddress) Retrieves the address of an exported function or **variable** from the specified dynamic-link library (DLL).
- [FreeLibrary](https://docs.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-freelibrary) Frees the loaded dynamic-link library (DLL) module

The above three system calls provided by *kernel32.dll* are P/Invoked in [Win32.cs](./Win32.cs).

As mentioned in the [introductory tutorial](../EJStarter), we can confirm the existence of the `jl_float64_type` symbol by inspecting *libjulia.dll* with [Dependencies](https://github.com/lucasg/Dependencies). 

Combining all, we get the minimal example below that obtains the exported `jl_float64_type` variable:
```csharp
static void Main(string[] _)
{
    // always initialize Julia first
    Julia.jl_init__threading();

    IntPtr juliaDLL = Win32.LoadLibraryA("libjulia.dll");
    Debug.Assert(juliaDLL != IntPtr.Zero);

    //`pJlFloat64` is a pointer to the pointer variable `jl_float64_type`
    IntPtr pJlFloat64 = Win32.GetProcAddress(juliaDLL, "jl_float64_type");
    // dereference `pJlFloat64` to obtain the pointer variable
    // Use `PtrToStructure` in general, but better to use the dedicated `ReadIntPtr`
    //IntPtr jlFloat64 = Marshal.PtrToStructure<IntPtr>(pJlFloat64);
    IntPtr jlFloat64 = Marshal.ReadIntPtr(pJlFloat64);

    Win32.FreeLibrary(juliaDLL);
    Julia.jl_atexit_hook(0);
}
```

## Working with arrays
We will complete the following list of tasks similar to the [documentation](https://docs.julialang.org/en/v1/manual/embedding/#Working-with-Arrays)
- Allocate a 1D array on the Julia side (i.e., in managed memory by Julia): `jl_apply_array_type`, `jl_alloc_array_1d`
- Fill the array with a constant 3.14: `jl_call2`

```csharp
// NOT memory safe in this part
// allocate an array managed by Julia
IntPtr arrayType = Julia.jl_apply_array_type(jlFloat64, 1);
IntPtr jlVector = Julia.jl_alloc_array_1d(arrayType, 10);  // an array of length 10
// fill the array in Julia
IntPtr fill_ = Julia.jl_eval_string("fill!");
Julia.jl_call2(fill_, jlVector, Julia.jl_box_float64(3.14));
```
- Access the array in C# and print its content: use `jl_array_data` in C but, unfortunately, the `jl_array_data` is just a macro and we have to `DllImport` its internally used functions. [The macro](https://github1s.com/JuliaLang/julia/blob/HEAD/src/julia.h#L944) is defined in *julia.h* by
    ```c
    #define jl_array_data(a)  ((void*)((jl_array_t*)(a))->data)
    ```
    That is, the data is located by the `data` member in the struct `jl_array_t`. The above `jlVector` in our C# code is effectively same as `jl_array_t*` in C. However,  there is nothing like `IntPtr -> data` in C#. To access a `struct`'s member, a common approach is to [marshal the struct](https://docs.microsoft.com/en-us/dotnet/framework/interop/marshaling-classes-structures-and-unions) in C# with the same memory layout as that in C. 
    
    One struct marshaling example specific to the `jl_array_t` struct can be found [here](https://github.com/WangyuHello/JuliaSharp/blob/71c88aa4ce86f6322abfefb649c0756aa475dabb/JuliaSharp/JuliaArray.cs#L141). In this example, we take a shortcut: only fetch the `data` member by offsetting the pointer to the `jl_array_t`. Since `data` is the first member ([source](https://github1s.com/JuliaLang/julia/blob/HEAD/src/julia.h#L196)), 
    ```c
    JL_EXTENSION typedef struct {
        JL_DATA_TYPE
        void *data;
        ...
    }
    ```
    the pointer to `jl_array_t` (i.e., `jlVector` above) also points to `data` (which itself is a pointer variable). We thus have
    ```csharp
    // get the pointer to the data member of the `jl_array_t`
    IntPtr jlVectorData = Marshal.ReadIntPtr(jlVector);
    ```
    After that, we can use `Marshal.Copy` to read the data from `jlVectorData` into a C# array.


## Accessing returned arrays
The approach is similar as above. The returned array is in a form `jl_array_t*`, i.e., the pointer to the array is returned. We then get the `data` field (also a pointer) in that `struct`. To read the array data, we can copy the data into a normal C# array as we have done above, or read the element one by one without copy.

```csharp
Console.WriteLine("Add 1 to the array");
IntPtr dotAdd = Julia.jl_eval_string(".+");
IntPtr jlVector1 = Julia.jl_call2(dotAdd, jlVector, Julia.jl_box_float64(1.0)); // return an array
IntPtr jlVector1Data = Marshal.ReadIntPtr(jlVector1);
for (int i = 0; i < 5; i++)
{
    IntPtr p = IntPtr.Add(jlVector1Data, sizeof(double) * i);
    Console.WriteLine(Marshal.PtrToStructure<double>(p));
}
```
The above `for` loop can also be replaced by `unsafe` code to deal with the pointer (of type `double*`) directly.


    
    
    
    


