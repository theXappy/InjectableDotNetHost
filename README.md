# InjectableDotNetHost
This probject provides a way to host the .NET Core/5/6/7+ runtime in a native (C/C++/...) target app.  
It is done by injecting a native (C++) dll into the target, so the focus of this project is closed source targets.  
If you have the source code for the target, you should call the nethost APIs directly like here:  
https://github.com/dotnet/samples/tree/main/core/hosting  

## Thanks
Most of the code in the repo was directly copied from [Rutherther/NosSmooth.Local](https://github.com/Rutherther/NosSmooth.Local)  
Some adjustments were done to also target x64 processes.
