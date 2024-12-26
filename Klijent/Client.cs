using System;
using System.Net.Sockets;
using System.Text;

namespace Klijent
{
    internal class Client
    {
        static void Main(string[] args)
        {
            try
            {
                string serverAddress = "127.0.0.1"; // IP adresa filijale
                int port = 8888; // TCP port filijale
                TcpClient tcpClient = new TcpClient(serverAddress, port);
                NetworkStream stream = tcpClient.GetStream();

                // Slanje zahteva za registraciju
                string message = "Registracija klijenta: Korisnik - testuser";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(messageBytes, 0, messageBytes.Length);

                // Čitanje odgovora od filijale
                byte[] responseBuffer = new byte[1024];
                int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                string responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                Console.WriteLine($"Odgovor od filijale: {responseMessage}");

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}
