# wingetupd
A tiny commandline tool, using _WinGet_, to update a specific set of packages on a Windows machine.

![wingetupd.exe](screenshot-tool.png)

### What it is
It´s a simple tiny tool named _wingetupd.exe_. The tool works on top of the popular Windows-Tool _WinGet_, by using _WinGet_ to update a specific bunch of packages, on a Windows machine.

When using _WinGet_ to install and update Windows software, _wingetupd.exe_ just wanna make your life a tiny bit easier, by updating all your software (or better said: a specific set of software) within a single click.

For more information about _WinGet_ itself, take a look at: https://docs.microsoft.com/de-de/windows/package-manager/winget

### How it works
- When started, _wingetupd.exe_ first searches for a so-called package-file. The package-file is simply a file named _packages.txt_, located in the same folder as the _wingetupd.exe_. The package-file contains a list of _WinGet_ package-ID´s (__not__ package-names, this is important).
- So, when _wingetupd.exe_ is started and it founds a package-file, it just checks for each package-ID, listed in the package-file, if the package exists, if the package is installed and if the package has an update. If so, it updates the package. _wingetupd.exe_ does all of this, by using _WinGet_.
- This means: All you have to do, is to edit the package-file and insert the ID´s of your installed _WinGet_ packages there. When starting _wingetupd.exe_ it will try to update all that packages.
- The tool is specifically not designed to install packages. It´s sole purpose is just to update packages. This means, before you can update your packages with this tool, you have to install them "by hand" or by using _WinGet_. In short: The tool can not and want not install any software. It´s just there to let you update your already existing software.

### Requirements
- There are not really any special requirements. It´s just a typical commandline _.exe_ file. Just download the newest release, from the _Releases_ page (https://github.com/MBODM/wingetupd/releases). All the releases are compiled for x64, assuming you have some 64-bit Windows version (which is rather likely).

### Additional notes
- The tool is written in C# using .NET 6 and Visual Studio 2022.
- The binaries in the releases are compiled as self-contained .NET 6 .exe files.
- That´s the reason why it´s size is 15 MB and why you don´t need any .NET Framework as requirement.
- When _wingetupd.exe_ starts it creates a log file (_wingetupd.log_) in the same folder.
- The log file contains all the _WinGet_ calls and their output, so you can exactly see what _WinGet_ does.
- The tool has no paramters and is not using any parameters at all.
- The tool just exists, because i am lazy and made my life a bit easier, by writing this tool.

#### Have fun.
