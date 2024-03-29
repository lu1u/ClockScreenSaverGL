﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extraire_Donnees_Meteo
{
    partial class Form2
    {
        static readonly string URL = c.getParametre("url", @"http://www.meteofrance.com/previsions-meteo-france/saint-pancrasse/38660");
        static readonly string TITRE = c.getParametre("titre", @"Saint Pancrasse (meteofrance.com)");
        static readonly string GET_JOURS = c.getParametre("xpath jours", @"//div[@class='prevision-ville']//div[@class='liste-jours']//ul/li");
        
        static readonly string XPATH_NOMVILLE = c.getParametre("xpath nom ville", "innerText|//header[starts-with(@class,'mod-previsions-header')]//h1");
        static readonly string XPATH_PREVISION = c.getParametre("xpath prevision", "attribut|title|.");
        static readonly string XPATH_DATE = c.getParametre("xpath date", @"innerText|descendant::dt");
        static readonly string XPATH_PIC = c.getParametre("xpath image", @"attribut|class|descendant::dd[starts-with(@class,'pic')]/@class");
        static readonly string XPATH_TEMP_MIN = c.getParametre("xpath temp min", @"innerText|descendant::span[@class='min-temp']");
        static readonly string XPATH_TEMP_MAX = c.getParametre("xpath temp max", @"innerText|descendant::span[@class='max-temp']");
        static readonly string CHEMIN_FICHIER = c.getParametre("chemin fichier", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), "clockscreensaver meteo.txt"));
        
        public const string FILE_HEADER = "Extraire donnees meteo - L.Pilloni";
        public const string FILE_VERSION = "Version 0.2";
        /// <summary>
        /// Charge la page Web et extrait les informations dans un fichier
        /// </summary>
        private void extraitInformationsMeteo()
        {
            try
            {
                HtmlNode doc = Internet.getInstance().loadPage(URL).DocumentNode;
                if (doc == null)
                {
                    notifyIcon.Text = $"Impossible de charger la page {URL}";
                    return;
                }

                using (StreamWriter file = new StreamWriter(CHEMIN_FICHIER))
                {
                    HtmlNodeCollection elements = doc.SelectNodes(GET_JOURS);

                    // URL de la page
                    file.WriteLine("URL=" + URL);

                    // Nom de la ville
                    file.WriteLine("TITRE=" + TITRE);

                    // Previsions par jour
                    foreach (HtmlNode e in elements)
                        file.WriteLine($"JOUR=DATE={getValue(e, XPATH_DATE)}|PREVISION={getValue(e, XPATH_PREVISION)}|PIC={getValue(e, XPATH_PIC)}|TEMPMIN={getValue(e, XPATH_TEMP_MIN)}|TEMPMAX={getValue(e, XPATH_TEMP_MAX)}");

                    file.Close();
                }

            }
            catch (Exception)
            {

                notifyIcon.Text = "Erreur au chargement de la page";
            }
        }

        private static string GetDate()
        {
            DateTime n = DateTime.Now;

            return $"{n.ToShortDateString()} {n.ToShortTimeString()}";
        }

        static string getValue(HtmlNode e, string xpathIndicator)
        {
            string res = "";
            try
            {
                string[] tokens = xpathIndicator.Split('|');
                if (tokens?.Length > 1)
                {
                    switch (tokens[0])
                    {
                        case "attribut":
                            string attribut = tokens[1];
                            string xpath = tokens[2];
                            res = e.SelectSingleNode(xpath).GetAttributeValue(attribut, "");
                            break;

                        case "innerText":
                            res = e.SelectSingleNode(tokens[1]).InnerText;
                            break;
                    }
                }
                else
                    res = e.SelectSingleNode(xpathIndicator).InnerText;
            }
            catch (Exception)
            {

            }

            return res.Trim();
        }

    }
}
