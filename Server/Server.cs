using Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Server
    {
        private static List<Korisnik> korisnici = new List<Korisnik>(); 
        private static Dictionary<string, Korisnik> aktivniKorisnici = new Dictionary<string, Korisnik>(); 
        private static double maxBudzet = 10000; 

        static void Main(string[] args)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888); 
                tcpListener.Start();
                Console.WriteLine("Server pokrenut na portu 8888 (TCP).");

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Filijala se povezala.");

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string response = ObradiZahtev(request);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška na serveru: {ex.Message}");
            }
        }

        private static string ObradiZahtev(string request)
        {
            if (request.StartsWith("INIT"))
            {
                string response = PosaljiteMaksimalniBudzet();
                return response;
            }
            else if (request.StartsWith("REGISTRACIJA"))
            {
                return Registracija(request);
            }
            else if (request.StartsWith("PRIJAVA"))
            {
                return Prijava(request);
            }
            else if (request.StartsWith("STANJE"))
            {
                return PregledStanja(request);
            }
            else if (request.StartsWith("TRANSAKCIJA"))
            {
                return Transakcija(request);
            }
            else
            {
                return "Greška: Nepoznata akcija.";
            }
        }

        private static string Registracija(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 5) 
            {
                string ime = delovi[1];
                string prezime = delovi[2];
                string lozinka = delovi[3];

                if (lozinka.Length < 6)
                {
                    return "Greška: Lozinka mora imati najmanje 6 karaktera.";
                }

                if (!double.TryParse(delovi[4], out double limitZaIsplatu) || limitZaIsplatu <= 0)
                {
                    return "Greška: Nevalidan limit za isplatu.";
                }

                Korisnik noviKorisnik = new Korisnik(Guid.NewGuid().ToString(), ime, prezime, limitZaIsplatu, lozinka);
                korisnici.Add(noviKorisnik);

                return "Registracija uspešna!";
            }
            else
            {
                return "Greška: Nevalidni podaci za registraciju.";
            }
        }
        private static string Prijava(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 3) 
            {
                string ime = delovi[1];
                string lozinka = delovi[2];

                Korisnik korisnik = korisnici.Find(k => k.Ime == ime && k.Lozinka == lozinka);

                if (korisnik != null)
                {
                    aktivniKorisnici[ime] = korisnik;
                    return $"USPEH| Prijava uspešna! Dobrodošli, {korisnik.Ime} {korisnik.Prezime}.";
                }
                else
                {
                    return "Greška: Pogrešno ime ili lozinka.";
                }
            }
            else
            {
                return "Greška: Nevalidni podaci za prijavu.";
            }
        }



        private static string PregledStanja(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 2) // Format: STANJE|LOZINKA
            {
                string lozinka = delovi[1];
                Korisnik korisnik = korisnici.Find(k => k.Lozinka == lozinka);

                if (korisnik != null)
                {
                    return $"Stanje na računu korisnika {korisnik.Ime} {korisnik.Prezime}: {korisnik.StanjeNaRačunu} dinara.";
                }
                else
                {
                    return "Greška: Pogrešna lozinka.";
                }
            }
            else
            {
                return "Greška: Nevalidni podaci za pregled stanja.";
            }
        }

        private static string Transakcija(string request)
        {
            string[] delovi = request.Split('|');
            if (delovi.Length == 4) 
            {
                string tip = delovi[1]; 
                if (!double.TryParse(delovi[2], out double iznos) || iznos <= 0)
                {
                    return "Greška: Nevalidan iznos transakcije.";
                }

                string lozinka = delovi[3];
                Korisnik korisnik = korisnici.Find(k => k.Lozinka == lozinka);

                if (korisnik == null)
                {
                    return "Greška: Pogrešna lozinka.";
                }

                if (tip == "UPLATA")
                {
                    korisnik.StanjeNaRačunu += iznos;
                    maxBudzet += iznos;
                }
                else if (tip == "ISPLATA")
                {
                    if (korisnik.StanjeNaRačunu < iznos)
                    {
                        return "Greška: Nedovoljno sredstava na računu.";
                    }else if(korisnik.LimitZaIsplatu < iznos)
                    {
                        return "Greška: Prekoracili ste limit za isplatu.";
                    }
                    korisnik.StanjeNaRačunu -= iznos;
                    maxBudzet -= iznos;
                    Console.WriteLine($"Maksimalni budžet je sada: {maxBudzet}");

                }
                else
                {
                    return "Greška: Nepoznat tip transakcije.";
                }

                Transakcija transakcija = new Transakcija(Guid.NewGuid().ToString(), tip, iznos, DateTime.Now);
                Console.WriteLine($"Evidentirana transakcija: {transakcija}");
                return $"{tip} uspešna! Novo stanje: {korisnik.StanjeNaRačunu} dinara.";
            }
            else
            {
                return "Greška: Nevalidni podaci za transakciju.";
            }
        }


        private static string PosaljiteMaksimalniBudzet()
        {            
            return $"MAKS_BUDZET|{maxBudzet}";
        }
    }
}
