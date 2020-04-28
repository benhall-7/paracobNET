# paracobNET

Param (.prc / .stprm / .stdat) class library for Smash Ultimate, written in C#, and the central tool for other projects in this repository. Can open, manipulate, save, and create param files. Dependent on .NET Standard 2.0

**Note**: for a faster, stand-alone alternative of ParamXML, see: https://github.com/ultimate-research/prc-rs/ 

## Prerequisites to run

- Download appropriate application from [Releases](https://github.com/BenHall-7/paracobNET/releases)

- ParamLabels.csv from [param-labels repo](https://github.com/ultimate-research/param-labels)

- [.NET Core 3 runtime](https://dotnet.microsoft.com/download/dotnet-core/3.0)
  - use `.NET Core Installer`
  - for prcEditor, also use `.NET Core Desktop Installer`

# prcEditor

GUI for editing param files using a TreeView -> DataGrid format (Windows only)

To load labels, place the label file in the same directory as the application. To open a selected param from the TreeView, press the "Enter" keyboard key

# ParamXML

Command line program to convert params to XML format and back. For help information use the `-h` argument.

# prcScript

Interfaces param editing methods through a lua context. Script your edits!

For help information use the `-h` argument. For for the API available in scripts, use  `-a`. See [the Wiki](https://github.com/BenHall-7/paracobNET/wiki/prcScript) for an example script.
