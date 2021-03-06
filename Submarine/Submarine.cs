﻿using OOP.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Submarine
{
    public enum TSubmarineTravelWay { automotive = 1, towed, alloyed, drifting }

    [Serializable]
    public class Submarine : ITransportPlugin
    {
        public string Name { get; set; }

        public string Manufacturer { get; set; }

        public TSubmarineTravelWay TravelWay { get; set; }

        public bool IsAtomic { get; set; }

        public Submarine()
        {
            Name = "Submarine";
        }

        public Submarine(string Manufacturer, TSubmarineTravelWay TravelWay, bool IsAtomic)
        {
            this.Manufacturer = Manufacturer;
            this.TravelWay = TravelWay;
            this.IsAtomic = IsAtomic;
            Name = "Submarine";
        }


        public string PrintInfo()
        {
            return $"A" + ((IsAtomic) ? " atomic " : " ") + $"{TravelWay} submarine '{Manufacturer}'.";
        }

        public void AskInfo(Object[] args)
        {
            Manufacturer = (string)args[0];
            TravelWay = (TSubmarineTravelWay)args[1];
            IsAtomic = (bool)args[2];
        }

        public Object[] GetInfo()
        {
            Object[] obj = new Object[3];
            obj[0] = Manufacturer;
            obj[1] = TravelWay;
            obj[2] = IsAtomic;
            return obj;
        }
    }
}
