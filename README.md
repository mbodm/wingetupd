# wingetupd
A tiny commandline tool, using [_WinGet_](https://docs.microsoft.com/de-de/windows/package-manager/winget), to update a specific set of packages on a Windows machine.

![wingetupd.exe](screenshot.png)

### What it is
It´s a simple and tiny tool named _wingetupd.exe_. The tool works on top of the popular Windows-App [_WinGet_](https://docs.microsoft.com/de-de/windows/package-manager/winget). The tool uses _WinGet_, to update a specific bunch of packages, on a Windows machine.

When using _WinGet_ to install and update Windows software, `wingetupd.exe` just wanna make your life a tiny bit easier, by updating all your software (or better said: a specific set of software) within a single call.

For more information about _WinGet_ itself, take a look at: https://docs.microsoft.com/de-de/windows/package-manager/winget

### How it works
- When started, `wingetupd.exe` first searches for a so-called "package-file". The package-file is simply a file named _packages.txt_, located in the same folder as the `wingetupd.exe`. The package-file contains a list of _WinGet_ package-ID´s (__not__ package-names, this is important).
- So, when `wingetupd.exe` is started and it founds a package-file, it just checks for each package-ID, listed in the package-file, if the package exists, if the package is installed and if the package has an update. If so, it updates the package. `wingetupd.exe` does all of this, by using _WinGet_ internally.
- This means: All you have to do, is to edit the package-file and insert the _WinGet_-Package-ID´s of your installed Windows applications there. When `wingetupd.exe` is called, it will try to update all that packages (aka "your Windows applications").
- The tool is specifically __not__ designed to install packages (that are not actually installed on your machine). It´s sole purpose is just to update your installed applications. Means: Before you can update some of your applications with this tool, you have to install them "by hand" or by using _WinGet_. In short: The tool can not (and want not) install any software. It´s just there for updating your already existing software.

### Requirements
There are not any special requirements. It´s just a typical commandline _.exe_ file. Just download the newest release, from the [_Releases_](https://github.com/MBODM/wingetupd/releases) page. Unzip and run it. All the releases are compiled for x64, assuming you are using some 64-bit Windows (and that's quite likely).

### Notes
- When `wingetupd.exe` starts, it creates a log file named "_wingetupd.log_" in the same folder.
- This means: The folder needs the rights for writing files in it. A location like _C:\ProgramFiles_ not has this rights.
- The log file contains all the _WinGet_ calls and their output, so you can exactly see how _WinGet_ was used.
- The tool has no parameters and is not using any parameters at all.
- To find out the package-ID´s (you wanna use inside the package-file) of your installed applications, you can use `winget search`.
- At the time of writing, the ID _Zoom.Zoom_ seems to not match the corresponding installed _Zoom_ package.
- The tool is written in C#, using .NET 6 and built with _Visual Studio 2022_.
- If you wanna compile the source by your own, you just need _Visual Studio 2022 Community_. Nothing else.
- The release-binaries are compiled as _self-contained_ .NET 6 .exe files, with _x64 Windows_ as target.
- Self-contained: That´s the reason why the binariy-size is 15 MB and why you don´t need any .NET Framework as requirement.
- The .csproj source file contains a MSBUILD task, to create a release zip file, when publishing with VS2022.
- The tool just exists, because i am lazy and made my life a bit easier, by writing this tool.

#### Have fun.
