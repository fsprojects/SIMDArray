namespace System
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[<assembly: AssemblyTitle("SIMDArray")>]
[<assembly: AssemblyDescription("SIMD Enhanced Array Opertaion Extensions")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("John Mott")>]
[<assembly: AssemblyProduct("SIMDArray")>]
[<assembly: AssemblyCopyright("Copyright ©  2016")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[<assembly: ComVisible(false)>]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[<assembly: Guid("aed427e4-ce8c-47fc-8918-2bac5d4d3d35")>]

// Version information for an assembly consists of the following four values:
//
//       Major Version
//       Minor Version
//       Build Number
//       Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyVersion("0.5.0.0")>]
[<assembly: AssemblyFileVersion("0.5.0.0")>]

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.1"
    let [<Literal>] InformationalVersion = "0.0.1"
