﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Transakcija
    {
        public string TransakcioniId { get; set; }  
        public string TipTransakcije { get; set; } 
        public double Iznos { get; set; }          
        public DateTime Datum { get; set; }        

        public Transakcija(string transakcioniId, string tipTransakcije, double iznos, DateTime datum)
        {
            TransakcioniId = transakcioniId;
            TipTransakcije = tipTransakcije;
            Iznos = iznos;
            Datum = datum;
        }

        public override string ToString()
        {
            return $"Transakcija ID: {TransakcioniId}, Tip: {TipTransakcije}, Iznos: {Iznos}, Datum: {Datum}";
        }
    }
}
