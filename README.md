# Console Chat 🚀  

Console Chat is a lightweight real-time chat application built using .NET Minimal API and WebSockets. It consists of two main components:  
- **ConsoleChat.Server**: The backend server that manages chat rooms and WebSocket connections.  
- **ConsoleChat.Client**: The console-based client that connects to the server for real-time messaging.  

---  

## Features ✨  
- Real-time messaging using WebSockets.  
- Room-based chat system where users can join and chat in different rooms.  
- Lightweight and fast.

---  

## Architecture Overview 🏛️  
- **ConsoleChat.Server**: Manages WebSocket connections, room creation, and message broadcasting.  
- **ConsoleChat.Client**: Console application that allows users to join rooms and chat in real-time.  

---  

## Prerequisites 🔧  
- Download and install [.NET 9.0 SDK](https://dotnet.microsoft.com/download)  
- Azure App Service (for deployment)  
- Visual Studio or VS Code  

---  

## Getting Started (running locally) 🚀  

### Clone the Repository  
```bash
git clone https://github.com/slalithprasad/ConsoleChat.git
cd ConsoleChat
```

### Setting Up the Server  
1. Navigate to the server project directory:  
    ```bash
    cd ConsoleChat.Server
    ```

2. Restore dependencies:  
    ```bash
    dotnet restore
    ```

3. Run the server locally:  
    ```bash
    dotnet run
    ```

4. The server will be available at `https://localhost:7028` or `http://localhost:5106`.  

### Setting Up the Client  
1. Navigate to the client project directory:  
    ```bash
    cd ConsoleChat.Client
    ```

2. set environment variable on local machine:
    ```csharp
    string baseUrl = Environment.GetEnvironmentVariable("CONSOLECHAT_SERVERURI")!;
    ```
    - **Name**: `CONSOLECHAT_SERVERURI`  
    - **Value**: `http://localhost:5106/`   

3. Restore dependencies:  
    ```bash
    dotnet restore
    ```

4. Run the client:  
    ```bash
    dotnet run
    ```

## Deployment on Azure ☁️  

### 1. **Publish the Server**  
- Right-click on the `ConsoleChat.Server` project in Visual Studio.  
- Select **Publish**.  
- Choose **Azure App Service**.  
- Use the downloaded deployment profile or configure manually.  
- Complete the deployment steps. 

### 2. **Environment variables on local machine**  
After deploying the server:  
- Under **Environment Variables**, add a new variable:  
    - **Name**: `CONSOLECHAT_SERVERURI`  
    - **Value**: `https://<your-azure-app-name>.azurewebsites.net/`   

### 3. **Run the client**  
- Run the client, create the room, share the room id with your friend and start messaging. 

## Example Chat

### Create Room

![Create Room](/images/create_room.png)

### Join Room

![Join Room](/images/join_room.png)

### Chat

![Chat](/images/chat.png)

---

Happy Chatting! 🗨️🚀