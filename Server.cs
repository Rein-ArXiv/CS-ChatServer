using System.Net;
using System.Net.Sockets;
using System.Text;

class Server {
    private static Socket serverSocket;
    private static bool isRunning = true;

    // Client Socket과 NickName을 저장하는 Dicktionary
    private static Dictionary<Socket, string> clients = new Dictionary<Socket, string>();

    static void Main()
    {
        // 1. Socket 생성 (TCP)
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 2. Socket Bind (IP, Port 설정)
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 59765);
        serverSocket.Bind(endPoint);

        // 3. Connection 대기
        serverSocket.Listen(10);

        Console.WriteLine("Server 시작");

        // 4. Client connection을 비동기로 처리할 thread 시작
        Thread clientHandlerThread = new Thread(HandleClients);
        clientHandlerThread.Start();


    }

    static void HandleClients()
    {
        while (isRunning)
        {
            try
            {
                // 5. Client connection 수락
                Socket clientSocket = serverSocket.Accept();
                Console.WriteLine("Client가 연결되었습니다.");

                // 6. Client를 처리하는 Thread 시작
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(clientSocket);
            }
            catch (SocketException)
            {
                if (!isRunning) {
                    Console.WriteLine("Server 종료");
                    break;
                }
            }
        }
    }

    static void HandleClient(object obj)
    {
        Socket clientSocket = (Socket) obj;
        byte[] buffer = new byte[1024];

        // 7. Client 닉네임 받기
        clientSocket.Receive(buffer);
        string nickName = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        clients.Add(clientSocket, nickName);
        Console.WriteLine($"{nickName}이(가) 연결되었습니다.");

        Array.Clear(buffer, 0, buffer.Length);

        while (isRunning){
            try
            {
                // 8. Client로부터 데이터 수신
                int bytesRead = clientSocket.Receive(buffer);
                if (bytesRead == 0) break;      // client가 연결을 종료한 경우

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"메시지 ({nickName}): {message}");

                // 9. 귓속말 처리 ('/w 대상 닉네임 메시지' 형식)
                if (message.StartsWith("/w")){
                    string[] splitMessage = message.Split(' ', 3);

                    if (splitMessage.Length >= 3){
                        string targetNickname = splitMessage[1];
                        string whisperMessage = splitMessage[2];
                        SendWhisper(clientSocket, targetNickname, whisperMessage);
                    }
                    else {
                        SendToClient(clientSocket, "잘못된 귓속말 형식입니다. '/w 대상닉네임 메세지' 형식으로 보내세요.");
                    }
                }

                else{
                    BroadcastMessage($"{nickName}: {message}", clientSocket);
                }
                Array.Clear(buffer, 0, buffer.Length);
            }
            catch (Exception) {
                Console.WriteLine($"{nickName}의 연결이 끊겼습니다.");
                break;
            }
        }
    }

    static void BroadcastMessage(string message, Socket excludeClient = null)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var client in clients.Keys){
            if (client != excludeClient){
                client.Send(data);
            }
        }
    }

    static void SendWhisper(Socket sender, string targetNickname, string message)
    {
        foreach (var client in clients) {
            if (client.Value == targetNickname){
                byte[] data = Encoding.UTF8.GetBytes($"[귓속말] {clients[sender]}: {message}");
                client.Key.Send(data);
                SendToClient(sender, $"[귓속말 전송] {targetNickname}: {message}");
                return;
            }
        }
        SendToClient(sender, $"{targetNickname} 닉네임을 가진 사용자를 찾을 수 없습니다.");
    }
    static void SendToClient(Socket client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        client.Send(data);
    }
}
