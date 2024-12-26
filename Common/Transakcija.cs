using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Transakcija
    {
        public string TransakcioniId { get; set; }  // ID transakcije
        public string TipTransakcije { get; set; } // Tip transakcije (Uplata, Isplata, Transfer)
        public double Iznos { get; set; }          // Iznos transakcije
        public DateTime Datum { get; set; }        // Datum kada je transakcija obavljena

        // Konstruktor klase
        public Transakcija(string transakcioniId, string tipTransakcije, double iznos, DateTime datum)
        {
            TransakcioniId = transakcioniId;
            TipTransakcije = tipTransakcije;
            Iznos = iznos;
            Datum = datum;
        }

        // Metoda za ispis podataka o transakciji
        public override string ToString()
        {
            return $"Transakcija ID: {TransakcioniId}, Tip: {TipTransakcije}, Iznos: {Iznos}, Datum: {Datum}";
        }
    }
}
