﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Config ;

namespace Extraire_Donnees_Meteo
{
	internal class Actualites
    {
        public static readonly char[] TRIM_CARACTERES = { ' ', '\n', '\r' };

        public const string CAT = "Actualites";
        public const string EXTENSION_ACTUALITES = "txt";
        public const string EXTENSION_IMAGES = "jpg";
        static protected Config.CategorieConfiguration c = Configuration.getCategorie(CAT);
        private static readonly int NB_JOURS_MAX_INFO = c.getParametre("Actu Nb jours info max", 4);
        private static readonly string CHEMIN_REPERTOIRE_ACTUS = c.getParametre("repertoire actualites", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), "actualites"));
        private static readonly string FILTRE_EXCLUSION_TITRE = c.getParametre("exclure titre", "zzzzzzzz");
        private static readonly string FILTRE_EXCLUSION_DESC = c.getParametre("exclure desc", "zzzzzzzzz");
        public static readonly int HAUTEUR_BANDEAU = c.getParametre("Hauteur bandeau", 150);
        public static readonly int MIN_LIGNES = c.getParametre("Nb lignes min", 50);
        public static readonly int MAX_LIGNES = c.getParametre("Nb lignes max", 100);
        public static readonly int MAX_LIGNES_PAR_SOURCE = c.getParametre("Nb lignes max par source", 10);
        private static readonly char[] SEPARATEURS = { '|' };
        private static readonly string RFC822 = "ddd, dd MMM yyyy HH:mm:ss zzz";

        private List<string> _sourcesActualite;
        private Regex regExTitre, regExDesc;

        public Actualites()
        {
            regExTitre = new Regex(FILTRE_EXCLUSION_TITRE);
            regExDesc = new Regex(FILTRE_EXCLUSION_TITRE);
            LireFichierSources();
        }

        /// <summary>
        /// Ouvrir le repertoire des actualites dans l'explorer
        /// </summary>
        internal void ouvreRepertoireActualites()
        {
            ouvreRepertoire(Path.Combine(Configuration.getDataDirectory(), "Actualites"));
        }

        /// <summary>
        /// Ouvrir le repertoire de configuration dans l'explorer
        /// </summary>
        internal void ouvreRepertoireConfig()
        {
            ouvreRepertoire(Configuration.getRepertoire());
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Ouvrir dans l'explorer un repertoire dont on donne le chemin
        /// </summary>
        /// <param name="repertoire"></param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ouvreRepertoire(string repertoire)
        {
            try
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = repertoire;
                Process process = Process.Start(StartInformation);
            }
            catch (Exception)
            {
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Lecture de la liste des sources RSS d'actualite dans le fichier {install}/{donnees}/actualites.txt
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void LireFichierSources()
        {
            _sourcesActualite = new List<string>();
            string fichierSources = Path.Combine(Configuration.getDataDirectory(), "actualites.txt");

            try
            {
                // Lire le fichier des sources d'actualite
                StreamReader file = new StreamReader(fichierSources);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!line.StartsWith("#"))  // Ligne mise en commentaire
                    {
                        _sourcesActualite.Add(line);
                    }
                }

                file.Close();
            }
            catch (Exception)
            {
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Extraire les actualites a partir des flux RSS pour creer les fichiers dans le repertoire d'actualites
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void extraireActualites()
        {
            WebRequest.DefaultCachePolicy = new RequestCachePolicy(RequestCacheLevel.Revalidate);

            foreach (String sourceALire in _sourcesActualite)
            {
                LitRSS(sourceALire);
            }

            supprimerViellesActualites();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Supprimer les actualites qui sont trop vieilles
        /// </summary>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void supprimerViellesActualites()
        {
            try
            {
                DateTime limite = DateTime.Now.AddDays(-NB_JOURS_MAX_INFO);                     // Date limite pour considerer qu'une actualite est trop viellle

                string repertoire = Path.Combine(Configuration.getDataDirectory(), "Actualites");
                string[] fichiers = Directory.GetFiles(repertoire, $"*.{EXTENSION_ACTUALITES}");
                foreach (string fichier in fichiers)
                {
                    DateTime date = ExtraitDate(fichier);                                       // Utiliser la date donnee dans le flux RSS, pas la date de creation du fichier
                    if (date < limite)
                    {
                        File.Delete(fichier);
                        string image = Path.ChangeExtension(fichier, EXTENSION_IMAGES);
                        if (File.Exists(image))
                        {
                            File.Delete(image);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Retrouve la date de creation du fichier codee dans la premiere partie du nom:
        /// {date.Year}_{date.Month}_{date.Day}
        /// </summary>
        /// <param name="fichier"></param>
        /// <returns></returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DateTime ExtraitDate(string fichier)
        {
            // Retrouver le nom du fichier
            String s = Path.GetFileNameWithoutExtension(fichier);

            // Extraire la premiere partie contenant la date
            String date = s.Substring(0, s.IndexOf(' ') + 1);

            if (date?.Length > 0)
            {
                string[] morceaux = date.Split('_');
                if (morceaux?.Length > 2)
                {
                    int annee = Int32.Parse(morceaux[0]);
                    int mois = Int32.Parse(morceaux[1]);
                    int jour = Int32.Parse(morceaux[2]);

                    return new DateTime(annee, mois, jour);
                }
            }

            return DateTime.Now;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Lit un flux RSS et ajoute les objets LigneActu
        /// </summary>
        /// <param name="source">URL du flux RSS</param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void LitRSS(string source)
        {
            string[] tokens = source.Split(SEPARATEURS);
            if (tokens == null || tokens.Length < 2)
            {
                return;
            }

            string sourceACharger = tokens[0];
            string urlACharger = tokens[1];
            if (sourceACharger == null || urlACharger == null)
            {
                return;
            }

            try
            {
                XmlDocument RSSXml = new XmlDocument();
                RSSXml.Load(urlACharger);

                XmlNodeList RSSNodeList = RSSXml.SelectNodes("rss/channel/item");

                StringBuilder sb = new StringBuilder();
                int nbLignesPourCetteSource = 0;
                foreach (XmlNode RSSNode in RSSNodeList)
                {
                    XmlNode RSSSubNode;
                    RSSSubNode = RSSNode.SelectSingleNode("pubDate");

                    if (recent(RSSSubNode))
                    {
                        // L'article n'est pas trop vieux pour etre affiche
                        DateTime date = RSSDateToDateTime(RSSSubNode.InnerText);
                        RSSSubNode = RSSNode.SelectSingleNode("title");
                        string title = RSSSubNode != null ? nettoieXML(RSSSubNode.InnerText) : "";
                        RSSSubNode = RSSNode.SelectSingleNode("description");
                        string desc = RSSSubNode != null ? nettoieXML(RSSSubNode.InnerText) : "";

                        if (!existeDeja(sourceACharger, title, date, desc) && okFiltre(title, desc))
                        {

                            Image image = chargeBitmap(RSSNode);
                            ajoute(sourceACharger, title, date, desc, image);

                            nbLignesPourCetteSource++;
                            if (nbLignesPourCetteSource > Actualites.MAX_LIGNES_PAR_SOURCE)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //_lignes.Add(new LigneActu(url_a_charger, "Impossible de charger les informations", DateTime.Now, e.Message, null));
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Retourne true si la news ne doit pas etre exclue en fonction du filtre actuel
        /// </summary>
        /// <param name="title"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool okFiltre(string title, string desc)
        {
            if (regExTitre.Match(title).Success)
            {
                return false;
            }

            if (regExDesc.Match(desc).Success)
            {
                return false;
            }

            return true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Ajoute l'actualite a la liste
        /// </summary>
        /// <param name="source_a_charger"></param>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <param name="desc"></param>
        /// <param name="image"></param>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ajoute(string source_a_charger, string title, DateTime date, string desc, Image image)
        {
            try
            {
                string nom = getNomFichierActualite(source_a_charger, title, date, desc);
                string fichier = $"{nom}.{EXTENSION_ACTUALITES}";

                // Un fichier texte contenant les infos
                using (StreamWriter file = new StreamWriter(fichier))
                {
                    file.WriteLine(source_a_charger);
                    file.WriteLine(date.ToLongDateString());
                    file.WriteLine(date.ToLongTimeString());
                    file.WriteLine(title);
                    file.WriteLine(desc);
                    file.Close();
                }
                File.SetCreationTime(fichier, date);

                // Un fichier image
                if (image != null)
                {
                    fichier = $"{nom}.{EXTENSION_IMAGES}";
                    image.Save(fichier, ImageFormat.Jpeg);
                    File.SetCreationTime(fichier, date);
                }
            }
            catch (Exception)
            {
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Calcule un nom de fichier (avec chemin) pour une actualite
        /// </summary>
        /// <param name="source_a_charger"></param>
        /// <param name="title"></param>
        /// <param name="date"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static string getNomFichierActualite(string source_a_charger, string title, DateTime date, string desc)
        {
            String repertoire = Path.Combine(Configuration.getDataDirectory(), "Actualites");
            if (!Directory.Exists(repertoire))
            {
                Directory.CreateDirectory(repertoire);
            }

            String nom = Path.Combine(repertoire, SanitizeFileName($"{date.Year}_{date.Month}_{date.Day} {date.ToShortTimeString()} {source_a_charger} {title}"));
            FieldInfo maxPathField = typeof(Path).GetField("MaxPath", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
            int MaxPathLength = (int)maxPathField.GetValue(null);
            if (nom.Length > MaxPathLength)
            {
                nom = nom.Substring(0, MaxPathLength);
            }

            return nom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="replacementChar"></param>
        /// <returns></returns>
        public static string SanitizeFileName(string fileName, char replacementChar = '_')
        {
            var blackList = new HashSet<char>(System.IO.Path.GetInvalidFileNameChars());
            var output = fileName.ToCharArray();
            for (int i = 0, ln = output.Length; i < ln; i++)
            {
                if (blackList.Contains(output[i]))
                {
                    output[i] = replacementChar;
                }
            }

            return new String(output);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Charge une bitmap a partir d'un element d'actualite RSS, si possible
        /// </summary>
        /// <param name="innerText"></param>
        /// <returns></returns>
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private Image chargeBitmap(XmlNode Node)
        {
            try
            {
                XmlNode n = Node.SelectSingleNode("enclosure ");
                if (n == null)
                {
                    return null;
                }

                string URL = n.Attributes["url"].InnerText;
                if (URL == null)
                {
                    return null;
                }

                if (URL.Length == 0)
                {
                    return null;
                }

                var request = WebRequest.Create(URL);
                var response = request.GetResponse();
                var stream = response.GetResponseStream();
                return Bitmap.FromStream(stream);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Determine si une actualite existe deja 
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private bool existeDeja(string source_a_charger, string title, DateTime date, string desc)
        {
            String chemin = getNomFichierActualite(source_a_charger, title, date, desc);
            return File.Exists($"{chemin}.txt");
        }

        /// <summary>
        /// Retourne true si l'article est suffisament recent pour etre pris en compte
        /// </summary>
        /// <param name="RSSSubNode"></param>
        /// <returns></returns>
        private static bool recent(XmlNode RSSSubNode)
        {
            if (RSSSubNode == null)
            {
                return true; // Pas de date
            }

            DateTime rss = RSSDateToDateTime(RSSSubNode.InnerText);
            if (rss == null)
            {
                return true; // Date non interpretable
            }

            return DateTime.Now.Subtract(rss).Days < Actualites.NB_JOURS_MAX_INFO;
        }
        /// <summary>
        /// Converti une date RSS en date C#
        /// </summary>
        /// <param name="RSSDate"></param>
        /// <returns></returns>
        private static DateTime RSSDateToDateTime(string RSSDate)
        {
            try
            {
                return DateTime.ParseExact(RSSDate, RFC822, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch (Exception)
            {

                return DateTime.MinValue;
            }
        }

        public static string nettoieXML(string s)
        {
            if (s == null)
            {
                return "null";
            }

            s = WebUtility.HtmlDecode(s);
            s = Regex.Replace(s, @"<[^>]*>", String.Empty)
                .Replace("&nbsp;", " ")
                .Replace("&exclamation;", "!")
            .Replace("&quot;", "\"")
            .Replace("&percent;", "%")
            .Replace("&amp;", "&")
            .Replace("&add;", "+")
            .Replace("&lt;", "<")
            .Replace("&equal;", "=")
            .Replace("&gt;", ">")
            .Replace("&iexcl;", "¡")
            .Replace("&cent;", "¢")
            .Replace("&pound;", "£")
            .Replace("&curren;", "¤")
            .Replace("&yen;", "¥")
            .Replace("&brvbar;", "¦")
            .Replace("&copy;", "©")
            .Replace("&reg;", "®")
            .Replace("&frac14;", "¼")
            .Replace("&frac12;", "½")
            .Replace("&frac34;", "¾")
            .Replace("&euro;", "€")
            .Replace("&#39;", "'")
            .Replace("&#8364;", "€")
            .Replace("a&#768;", "à")
            .Replace("a&#770;", "â")
            .Replace("&#160;", " ")
            .Trim(TRIM_CARACTERES);

            if (s.IndexOf("nbsp") != -1)
            {
                Log.getInstance().warning("Reste un nbsp: " + s);
            }

            return s;
        }
    }

}


