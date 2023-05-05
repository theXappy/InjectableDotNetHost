# InjectableDotNetHost
This probject aims to provide a way to host a .NET Core in a native (C/C++/etc) target app.  
It is done by injecting a native (C++) dll into the target, so the focus of this project is closed source targets.  
If you have the source code for the target, you should call the nethost APIs directly like here:  
https://github.com/dotnet/samples/tree/main/core/hosting  

## Thanks
Most of the code in the repo was directly copied from Rutherther/NosSmooth.Local  
Some adjustments were done to also target x64 processes.