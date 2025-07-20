using Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static List<Transakcija> transakcije = new List<Transakcija>();
        private static readonly string sifra = "tajna123";
        static void Main(string[] args)
        {
            try
            {
                // Slanje UDP poruke filijali pre pokretanja TCP listener-a
                PosaljiStartFilijali("127.0.0.1", 7777);

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
                    string encryptedRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string request;
                    try
                    {
                        request = Enkriptor.Decrypt(encryptedRequest,sifra);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška prilikom dešifrovanja zahteva: {ex.Message}");
                        client.Close();
                        continue;
                    }

                    string response = ObradiZahtev(request);
                    if (!response.EndsWith("<END>"))
                    {
                        response += "<END>";
                    }


                    string encryptedResponse = Enkriptor.Encrypt(response, sifra);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedResponse);

                    stream.Write(responseBytes, 0, responseBytes.Length);
                    stream.Flush();
                    System.Threading.Thread.Sleep(50);

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška na serveru: {ex.Message}");
            }
        }

        private static void PosaljiStartFilijali(string filijalaIp, int udpPort)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                byte[] poruka = Encoding.UTF8.GetBytes("START");
                udpClient.Send(poruka, poruka.Length, filijalaIp, udpPort);
                Console.WriteLine($"Poslata UDP poruka 'START' filijali {filijalaIp}:{udpPort}");
            }
        }

        private static string ObradiZahtev(string request)
        {
            if (request.StartsWith("INIT"))
                return PosaljiteMaksimalniBudzet();
            else if (request.StartsWith("REGISTRACIJA"))
                return Registracija(request);
            else if (request.StartsWith("PRIJAVA"))
                return Prijava(request);
            else if (request.StartsWith("STANJE"))
                return PregledStanja(request);
            else if (request.StartsWith("TRANSAKCIJA"))
                return Transakcija(request);
            else if (request.StartsWith("TRANSFER"))
                return Transfer(request);
            else if (request.StartsWith("ISTORIJA"))
                return VratiIstorijuTransakcija(request);
            else
                return "Greška: Nepoznata akcija.";
        }


private static string Transfer(string request)
        {
            // FORMAT: TRANSFER|lozinka_posiljaoca|lozinka_primaoca|iznos
            string[] delovi = request.Split('|');
            if (delovi.Length != 4)
            {
                return "Greška: Nevalidni podaci za transfer.";
            }

            string lozinkaPosiljaoca = delovi[1];
            string lozinkaPrimaoca = delovi[2];
            if (!double.TryParse(delovi[3], out double iznos))
            {
                return "Greška: Uneti iznos nije validan broj.";
            }

            if (iznos <= 0)
            {
                return "Greška: Iznos za transfer mora biti veći od nule.";
            }

            Korisnik posiljalac = korisnici.Find(k => k.Lozinka == lozinkaPosiljaoca);
            Korisnik primalac = korisnici.Find(k => k.Lozinka == lozinkaPrimaoca);

            if (posiljalac == null)
                return "Greška: Pogrešna lozinka pošiljaoca.";

            if (primalac == null)
                return "Greška: Pogrešna lozinka primaoca.";

            if (posiljalac.StanjeNaRačunu < iznos)
                return "Greška: Pošiljalac nema dovoljno sredstava.";

            if (posiljalac.LimitZaIsplatu < iznos)
                return "Greška: Pošiljalac je prekoračio limit za isplatu.";

            if (maxBudzet < iznos)
                return $"Greška: Nema dovoljno sredstava u ukupnom budžetu filijale ({maxBudzet} dinara).";

            // Izvršavanje transfera
            posiljalac.StanjeNaRačunu -= iznos;
            primalac.StanjeNaRačunu += iznos;
            maxBudzet -= iznos;

            // Evidencija transfera sa imenima
            Transakcija t = new Transakcija(
                Guid.NewGuid().ToString(),
                "TRANSFER",
                iznos,
                DateTime.Now,
                posiljalac.Ime + " " + posiljalac.Prezime,
                primalac.Ime + " " + primalac.Prezime
            );

            transakcije.Add(t);
            Console.WriteLine($"Evidentiran transfer: {t.OdKoga} > {t.KaKome}, {t.Iznos} din");

            return $"Transfer uspešan! Novo stanje pošiljaoca: {posiljalac.StanjeNaRačunu:F2} dinara, primaoca: {primalac.StanjeNaRačunu:F2} dinara.";
        }

        private static string VratiIstorijuTransakcija(string request)
        {
            // Format: ISTORIJA|lozinka
            string[] delovi = request.Split('|');
            if (delovi.Length != 2)
                return "Greška: Nevalidan zahtev za istoriju.<END>";

            string lozinka = delovi[1];
            Korisnik korisnik = korisnici.Find(k => k.Lozinka == lozinka);

            if (korisnik == null)
                return "Greška: Korisnik nije pronađen.<END>";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Transakcije za korisnika {korisnik.Ime} {korisnik.Prezime}:");

            string punoIme = korisnik.Ime + " " + korisnik.Prezime;

            foreach (var t in transakcije)
            {
                if (t.TipTransakcije == "TRANSFER" && t.OdKoga == korisnik.Ime + " " + korisnik.Prezime)
                {
                    sb.AppendLine($"[TRANSFER-POSLAT] {t.OdKoga} > {t.KaKome}, Iznos: {t.Iznos} din, Datum: {t.Datum}");
                }
                else if (t.TipTransakcije == "TRANSFER" && t.KaKome == korisnik.Ime + " " + korisnik.Prezime)
                {
                    sb.AppendLine($"[TRANSFER-PRIMLJEN] {t.OdKoga} > {t.KaKome}, Iznos: {t.Iznos} din, Datum: {t.Datum}");
                }
                else if (t.VlasnikLozinka == korisnik.Lozinka)
                {
                    if (t.TipTransakcije == "UPLATA")
                    {
                        sb.AppendLine($"[UPLATA]  +{t.Iznos} din, Datum: {t.Datum}");
                    }
                    else if (t.TipTransakcije == "ISPLATA")
                    {
                        sb.AppendLine($"[ISPLATA] -{t.Iznos} din, Datum: {t.Datum}");
                    }
                }
            }

            return sb.ToString();
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

                Transakcija transakcija = new Transakcija(
                    Guid.NewGuid().ToString(),
                    tip,
                    iznos,
                    DateTime.Now,
                    vlasnikLozinka: korisnik.Lozinka
                );
                transakcije.Add(transakcija);
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
