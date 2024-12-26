using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Filijala
{
    internal class Filijala
    {
        static void Main(string[] args)
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                IPAddress selectedAddress = null;

                foreach (var address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        selectedAddress = address;
                        break;
                    }
                }

                if (selectedAddress == null)
                {
                    Console.WriteLine("IPv4 adresa nije pronađena.");
                    return;
                }

                // Kreiranje UDP soketa za primanje inicijalizacijskih podataka
                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Any, 55555);
                udpSocket.Bind(udpEndPoint);
                Console.WriteLine("Filijala sada sluša na UDP portu 55555.");

                // Primanje inicijalizacijskih podataka
                byte[] buffer = new byte[1024];
                EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int receivedBytes = udpSocket.ReceiveFrom(buffer, ref serverEndPoint);
                string response = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                Console.WriteLine($"Primljen odgovor od servera: {response}");

                // TCP soket za prihvat klijenata
                TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Filijala sada sluša na TCP portu 8888.");

                while (true)
                {
                    // Prihvatanje klijenta
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Klijent se povezao.");

                    // Obrađivanje klijentovih zahteva
                    NetworkStream stream = tcpClient.GetStream();
                    byte[] requestBuffer = new byte[1024];
                    int bytesRead = stream.Read(requestBuffer, 0, requestBuffer.Length);
                    string clientMessage = Encoding.UTF8.GetString(requestBuffer, 0, bytesRead);
                    Console.WriteLine($"Primljena poruka od klijenta: {clientMessage}");

                    // Odgovaranje klijentu
                    string responseMessage = "Poruka uspešno primljena.";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    tcpClient.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }
    }
}
