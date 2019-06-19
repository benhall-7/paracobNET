# paracobNET

Param (.prc) class library for Smash Ultimate, and the central tool for other projects in this repository. Can open, manipulate, save, and create param files. Dependent on .NET Standard 2.0

# ParamXML

Command line program to convert params to XML format and back. To run requires the .NET Core version 2.1 runtime (https://dotnet.microsoft.com/download/dotnet-core/2.1). To build requires the SDK of the same version.

### how to use:

With `dotnet` added to Path, you can run the program using a command such as `dotnet ParamXML.dll [args]`.

Use `-h` or `-help` to see the help text, containing the required/optional args.

Example: `dotnet ParamXML.dll -d fighter_param.prc -l ParamLabels.csv`

# prcEditor

WPF Graphical User Interface for editing param files using a TreeView -> DataGrid format (Windows only). To build requires .NET Framework 4.6.1

To load labels, place the label file (named "ParamLabels.csv") in the same directory as the application. To open a selected struct or list from the TreeView, press the "Enter" key. Additional features and documentation thereof are WIP.
