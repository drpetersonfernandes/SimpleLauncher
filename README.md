**Simple Launcher**
===============

A simple emulator launcher for Windows.

![Screenshot (2)](screenshot%20(2).jpg)

This program search for any ZIP, 7Z or ISO file in the current directory.
Then it display the list of files in a grid with cover image (on top) and the filename (at botton).
The cover images need to have the same filename as the file to be launch. All the images need to be placed inside the images folder. The images need to be in png format. It is recommended that the image has the width less than 200 pixels and the height of 200 pixels. If you use a higger resolution image it will consume more memory. If the main program don't find an image with the same filename inside the images folder it will load default.png.

On the top of the program there is a combo box that allow the user to select the emulator to use.
All the emulator settings and parameters are store in the file parameters.txt, located in the current directory.

The format of the parameters.txt file is the following:


<pre>

Id: 1
ProgramName: ProgramName
ProgramLocation: ProgramLocation
Parameters: Parameters

Id: 2
ProgramName: Retroarch
ProgramLocation: G:\Emulators\Retroarch\retroarch.exe
Parameters: -L "G:\Emulators\Retroarch\cores\picodrive_libretro.dll" -c "G:\Emulators\Retroarch\Config.cfg" -f
</pre>



You can add as many emulator as you want. The ProgramName is the name that will be display in the combo box. The ProgramLocation is the path to the emulator executable. The Parameters is the parameters that will be pass to the emulator executable. Please follow the format provided in this exemple.

When the user click on the selected grid, the program with launch the selected emulator + parameters + file to be launch.

This is a windows only program. It was tested on Windows 11.

There are some fix that need to be done and more error checking to be implemented. The program is not perfect. But it work.

*Written in C# using<br>
Microsoft Visual Studio Community 2022 Version 17.8.0 Preview 1.0<br>
Windows Presentation Foundation (WPF) Framework<br>
Microsoft .NET Framework Version 4.8.09032*
