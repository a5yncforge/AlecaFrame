# AlecaFrame Source Code - How to build

## Requirements

- Visual Studio
  - .NET Desktop Development
- .NET Framework 4.8 SDK
- .NET 10 Runtime

## Build Intructions

- Run the `build.vbs` script to automatically build the libraries and create a valid Overwolf app that you can import.
  - After running the script you can simply "Load Unpacked" from `overwolf://packages`. Make sure you have Overwolf Developer mode (https://dev.overwolf.com/ow-native/getting-started/project-roadmap)
- If you only want to build the libraries then you can build the solution directly from Visual Studio.

- If you want to keep an original version of AlecaFrame as well as this version you must deactivate the original app. You also want to edit the `manifest.json` `meta.name` property to something else. Then you can activate/deactivate whatever version u want.
