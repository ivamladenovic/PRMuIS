using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {
            try
            {
                UdpClient udpServer = new UdpClient(8888); 
                Console.WriteLine("Centralni server pokrenut na portu 8888 (UDP).");

                while (true)
                {
                    IPEndPoint remoteEndPoint = null;
                    byte[] receivedBytes = udpServer.Receive(ref remoteEndPoint);
                    string request = Encoding.UTF8.GetString(receivedBytes);

                    Console.WriteLine($"Primljen zahtev od filijale: {request}");

                    if (request.StartsWith("INIT"))
                    {
                        string maxBudzet = "1000000"; 
                        string response = maxBudzet; 
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        udpServer.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                        Console.WriteLine($"Odgovor poslat filijali: {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška na serveru: {ex.Message}");
            }
        }
    }
}
