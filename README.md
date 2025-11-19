# paracobNET

Param (.prc / .stprm / .stdat) class library for Smash Ultimate, written in C#, and the central tool for other projects in this repository. Can open, manipulate, save, and create param files. Utilizes .NET 10.

**Note**: for a faster, Rust-based alternative to ParamXML, see: https://github.com/ultimate-research/prc-rs/

## How to run

- Download appropriate application from [Releases](https://github.com/BenHall-7/paracobNET/releases)

- ParamXML is a CLI tool. Refer to the ParamXML section for more detailed instructions.

# prcEditor

Avalonia GUI for editing param files using a TreeView -> DataGrid format (Now cross-platform!).

# ParamXML

Command line program to convert params to XML format and back. To interact with the program directly, open a command line in the folder where the application file is located, and type command arguments there. For instance, using `ParamXML -h` shows you the command documentation.

# prcScript

Deprecated scripting platform for Lua. For a faster, more feature-complete alternative, see [pyprc](https://github.com/benhall-7/pyprc).

For help information use the `-h` argument. For for the API available in scripts, use `-a`. See [the Wiki](https://github.com/BenHall-7/paracobNET/wiki/prcScript) for an example script.
