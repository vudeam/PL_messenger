# Welcome to VectorChat repo! ![Logo](./Client_WPF/Assets/VectorChatLogo.png "VectorChat Logo")

## Quick start guide

### Client
#### Run:
1. [Download](https://github.com/vudeam/PL_messenger/releases "Project releases") the latest Client app build (**portable**, no build needed)
2. Run the `.exe` file **once** and close it. This creates `config.json` file in the same folder it was ran. The file should look something like that:
```
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
#### Build:

### Server
