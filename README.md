# Welcome to VectorChat repo! ![Logo](./Client_WPF/Assets/VectorChatLogo.png "VectorChat Logo")

## Quick start guide

### Client

#### Run
1. [Download](https://github.com/vudeam/PL_messenger/releases "Project releases") the latest Client app build (**portable**, no installation needed)
2. Run the `.exe` file **once** and close it. This creates `config.json` file in the same folder where it was ran. The file should look something like that:
```json
{
	"messageRequestTime": 200,
	"mainWindowHeight": 540.0,
	"mainWindowWidth": 960.0,
	"serverAddress": "http://localhost:8080",
	"enableFileAuth": false,
	"login": "",
	"password": ""
}
```
3. Now you need to specify URL and port (`:8080` in the defalt file) for the server. Notice that you do *not* need the terminating `/` at the end of URL
4. After that you may configure some Client defaults like window dimensions at startup and so on
5. Enjoy!

#### Build
The project is available for building. It was tested for build in [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/ru/ "Official website")
> Build requires workloads for C# WPF applications (.NET Framework and .NET Core). You also need to make sure you have installed [dependencies](https://github.com/vudeam/PL_messenger#libraries-and-frameworks-used-in-this-project)

### Server

#### Run
1. [Download](https://github.com/vudeam/PL_messenger/releases "Project releases") the latest Server app build (**portable**, no installation needed)
2. Running the `.exe` file creates `config.json` file in the same folder where the program was ran. The file should look something like that:
```json
{
	"Port": "8080",
	"EnableFileLogging": false,
	"EnableVerboseConsole": false,
	"StoredMessagesLimit": 50
}
```
3. Change (if necessary) some settings like default port (`8080` in the file) or enable extended (verbose) console and file logging
> Note: you may override the server starting port by calling the executable file with command line argument: i.e. `.\Server_ASPNET.exe 8080`. This will ignore `Port` parameter from `config.json`
4. Enjoy! (**but do not forget to pass your server IP address and port to the cliet apps**)

#### Build
The project is available for building. It was tested for build in [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/ru/ "Official website")
> Build requires workloads for C# .NET Core applications. You also need to make sure you have installed [dependencies](https://github.com/vudeam/PL_messenger#libraries-and-frameworks-used-in-this-project)

## Libraries and frameworks used in this project
* [Jdenticon](https://jdenticon.com/ "Website") — a .NET library for generating highly recognizable identicons ([GitHub repo](https://github.com/dmester/jdenticon-net "Repository"))
* [Newtonsoft.Json](https://www.newtonsoft.com/json "Website") — Popular high-performance JSON framework for .NET ([GitHub repo](https://github.com/JamesNK/Newtonsoft.Json "Repository"))
* [Terminal.Gui](https://migueldeicaza.github.io/gui.cs/ "Website") — Terminal GUI toolkit for .NET ([GitHub repo](https://migueldeicaza.github.io/gui.cs/))
