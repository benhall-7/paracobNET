# paracobNET

Param (.prc) class library for Smash Ultimate, and the central tool for other projects in this repository. Can open, manipulate, save, and create param files. Dependent on .NET Standard 2.0

# ParamXML

Command line program to convert params to XML format and back. To run requires the .NET Core version 2.1 runtime (https://dotnet.microsoft.com/download/dotnet-core/2.1). To build requires the SDK of the same version.

**-usage:**

With `dotnet` added to the Path, you can run the program using a command such as `dotnet ParamXML.dll [args]`.

Use `-h` or `-help` to see the help text, containing the required/optional args.

Example: `dotnet ParamXML.dll -d fighter_param.prc -l ParamLabels.csv`

# Param2Form / ParamCLI

These projects are in progress. If you would like to build them on your own, Param2Form requires .NET Framework 4.6.1 and ParamCLI requires .NET Core 2.1.
