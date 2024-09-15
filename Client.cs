using System.Net;
using System.Net.Sockets;
using System.Text;

class Client {
    private static Socket clientSocket;
    private static string nickName;
    static void Main()
    {
        // 1. 소켓 생성 (TCP 소켓)
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // 2. 서버에 연결
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 59765));
            Console.WriteLine("서버에 연결되었습니다.");

            // 3. 닉네임 설정
            Console.Write("닉네임을 입력하세요: ");
            nickName = Console.ReadLine();
            byte[] nicknameData = Encoding.UTF8.GetBytes(nickName);
            clientSocket.Send(nicknameData);


            // 4. 메시지 전송 및 수신 루프
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (true){
                Console.Write("> ");
                string message = Console.ReadLine();

                if (message == "exit") break;

                // 5. 서버에 메시지 전송
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(data);
            }

        }
        catch (SocketException e){
            Console.WriteLine($"소켓 예외 발생: {e.Message}");
        }
        catch (Exception e){
            Console.WriteLine($"오류 발생: {e.Message}");
        }
        finally {
            // 7. 소켓 닫기
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
    }

    static void ReceiveMessages()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (true) {
                int bytesRead = clientSocket.Receive(buffer);
                if (bytesRead > 0) {
                    string serverMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"{serverMessage}");
                    Console.Write("> ");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"메세지 수신 중 오류: {e.Message}");
        }
    }
}
