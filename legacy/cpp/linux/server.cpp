#include <iostream>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>

int main()
{
    // 서버 소켓 생성
    int server_fd = socket(AF_INET, SOCK_STREAM, 0);

    if (server_fd == -1)
    {
        std::cerr << "소켓 생성 실패" << std::endl;
        return -1;
    }

    // 소켓에 IP 주소, 포트 번호 할당
    sockaddr_in address;
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = INADDR_ANY;
    address.sin_port = htons(8080);

    if (bind(server_fd, (struct sockaddr *)&address, sizeof(address)) < 0)
    {
        std::cerr << "Bind Failed" << std::endl;
        return -1;
    }

    // 연결 요청 대기 상태 (listen) 전환
    if (listen(server_fd, 3) < 0)
    {
        std::cerr << "Listen failure" << std::endl;
    }
    std::cout << "Server: Waiting for client" << std::endl;

    // 클라이언트의 연결 요청 수락
    int client_socket;
    sockaddr_in client_address;
    int addrlen = sizeof(client_address);

    client_socket = accept(server_fd, (struct sockaddr *)&client_address, (socklen_t*)&addrlen);
    if (client_socket < 0)
    {
        std::cerr << "Failed connect" << std::endl;
        return -1;
    }

    std::cout << "Server: Client connected!" << std::endl;

    close(client_socket);
    close(server_fd);
    return 0;
}