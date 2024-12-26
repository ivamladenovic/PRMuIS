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

                // Kreiranje UDP soketa za komunikaciju sa filijalama
                Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint serverEndPoint = new IPEndPoint(selectedAddress, 55555);

                // Inicijalizacija filijale sa početnim resursima
                string initMessage = "Max budget: 100000, Max TCP connections: 5";
                byte[] buffer = Encoding.UTF8.GetBytes(initMessage);

                // Slanje inicijalizacije filijalama
                udpSocket.SendTo(buffer, serverEndPoint);
                Console.WriteLine("Server poslao inicijalizaciju filijalama.");

                // Zatvaranje UDP soketa
                udpSocket.Close();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket greška: {ex.Message}");
            }

            // Održavanje servera otvorenim dok korisnik ne pritisne enter
            Console.WriteLine("Server je završio sa radom. Pritisnite Enter da biste zatvorili.");
            Console.ReadLine();
        }
    }
}
