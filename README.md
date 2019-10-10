# paracobNET

Param (.prc / .stprm / .stdat) class library for Smash Ultimate, written in C#, and the central tool for other projects in this repository. Can open, manipulate, save, and create param files. Dependent on .NET Standard 2.0

## Prerequisites to run

- Download appropriate application from [Releases](https://github.com/BenHall-7/paracobNET/releases)

- ParamLabels.csv from [param-labels repo](https://github.com/ultimate-research/param-labels)

- If using non-exe build of `ParamXML` or `prcEditor`: NET Core runtime (version 3)

# prcEditor

GUI for editing param files using a TreeView -> DataGrid format (Windows only)

To load labels, place the label file in the same directory as the application. To open a selected param from the TreeView, press the "Enter" key

# ParamXML

Command line program to convert params to XML format and back. For help information use the `-h` argument.

# prcScript

Interfaces param editing methods through a lua context. Script your edits!

For help information use the `-h` argument. For for the API available in scripts, use  `-a`. See [the Wiki](https://github.com/BenHall-7/paracobNET/wiki/prcScript) for an example script.
