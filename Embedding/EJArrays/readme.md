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