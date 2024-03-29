﻿/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 14/12/2014
 * Heure: 23:18
 * 
 * Stockage optimise de l'heure pour tout calculer une seule fois
 */
using System;

namespace ClockScreenSaverGL
{
    /// <summary>
    /// Description of Temps.
    /// </summary>
    public class Temps
    {
        private const float SEUIL_INTERVAL = 50;

        public DateTime temps;
        public int annee, mois, jourDuMois, jourDeLAnnee;
        public int heure, minute, seconde, milliemesDeSecondes;
        public float intervalleDepuisDerniereFrame;
        public DateTime tempsDerniereFrame;
        public double totalMilliSecondes;
        public static readonly DateTime BASEDATE = new DateTime(1970, 1, 1);

        public Temps(DateTime t, DateTime derniere)
        {
            temps = t;
            annee = t.Year;
            mois = t.Month;
            jourDuMois = t.Day;
            jourDeLAnnee = t.DayOfYear;
            heure = t.Hour;
            minute = t.Minute;
            seconde = t.Second;
            milliemesDeSecondes = t.Millisecond;
            totalMilliSecondes = t.Subtract(BASEDATE).TotalMilliseconds;
            tempsDerniereFrame = derniere;
            intervalleDepuisDerniereFrame = (float)(t.Subtract(derniere).TotalMilliseconds / 1000.0);
            if (intervalleDepuisDerniereFrame > SEUIL_INTERVAL)
            {
                intervalleDepuisDerniereFrame = SEUIL_INTERVAL;
                tempsDerniereFrame = derniere.AddSeconds(SEUIL_INTERVAL * -1000.0);
            }
        }

        public static double Maintenant()
        {
            return DateTime.Now.Subtract(BASEDATE).TotalMilliseconds;
        }
    }
}
