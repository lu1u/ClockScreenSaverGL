// 

using ClockScreenSaverGL.Config;
///
/// http://www.meteofrance.com/previsions-meteo-france/crolles/38920
/// 
using System;
using System.Collections.Generic;
using System.IO;

namespace ClockScreenSaverGL.DisplayedObjects.Meteo
{
    internal class MeteoInfo 
    {
        private const string CAT = "meteo";
        protected CategorieConfiguration c;
        private string CHEMIN_FICHIER;
        public int NB_LIGNES_INFO_MAX;

        #region MEMBRES_PUBLICS
        public List<LignePrevisionMeteo> _lignes = new List<LignePrevisionMeteo>();
        public string _title;
        public string _url;
        #endregion MEMBRES_PUBLICS

        private static readonly Dictionary<string, string> _liensIcones = new Dictionary<string, string>();

        public MeteoInfo()
        {
            GetConfiguration();

            _title = "Crolles";

            LitCorrespondancesMeteo();
            ChargeDonnees();
        }

        private void GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                CHEMIN_FICHIER = c.GetParametre("chemin fichier", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), "clockscreensaver meteo.txt"));
                NB_LIGNES_INFO_MAX = c.GetParametre("nb lignes info", 5);
            }
        }

        /// <summary>
        /// Lecture de la table de correspondance qui fait le lien entre les nom utilises sur le site meteo
        /// et les icones utilisees par le programme
        /// </summary>
        private void LitCorrespondancesMeteo()
        {
            _liensIcones.Clear();
            string fichierSources = Path.Combine(Configuration.GetDataDirectory(), "icones meteo.txt");
            StreamReader file = new StreamReader(fichierSources);
            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = line.Trim();
                if (!line.StartsWith("#")) // Commentaire a ignorer ?
                {
                    string[] tokens = line.Split('>');
                    if (tokens?.Length == 2)
                        _liensIcones.Add(tokens[0], tokens[1]);
                }
            }

            file.Close();
        }

        /// <summary>
        /// Retrouve le nom de l'image a utiliser par ce programme en fonction de l'information
        /// trouvee sur la page du site meteo
        /// </summary>
        /// <param name="imageSurLeSite"></param>
        /// <returns></returns>
        public static string GetIcone(string imageSurLeSite)
        {
            if (_liensIcones.TryGetValue(imageSurLeSite, out string valeur))
                return valeur;

            return imageSurLeSite;
        }


        /// <summary>
        /// Lecture des previsions meteo en multithreading
        /// </summary>
        public void ChargeDonnees()
        {
            _lignes.Clear();

            try
            {
                string[] lignes = File.ReadAllLines(CHEMIN_FICHIER);
                if (lignes == null)
                    return;

                foreach (string ligne in lignes)
                {
                    if (ligne.StartsWith("URL"))
                        _url = DeuxiemePartie(ligne);
                    else
                        if (ligne.StartsWith("TITRE"))
                        _title = DeuxiemePartie(ligne);
                    else
                        if (ligne.StartsWith("JOUR"))
                    {
                        if (_lignes.Count < NB_LIGNES_INFO_MAX)
                            _lignes.Add(CreateLigne(DeuxiemePartie(ligne)));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error("Meteo Info: impossible de charger les données");
                Log.Instance.Error(e.Message);
            }
        }

        private LignePrevisionMeteo CreateLigne(string v)
        {
            string[] tokens = v.Split('|');
            string icone = "";
            string date = "";
            string tempmin = "";
            string tempmax = "";
            string texte = "";
            string vent = "";
            string pluie = "";

            foreach (string token in tokens)
            {
                if (token.StartsWith("DATE"))
                    date = DeuxiemePartie(token);
                else
                    if (token.StartsWith("PREVISION"))
                    texte = DeuxiemePartie(token);
                else
                    if (token.StartsWith("PIC"))
                    icone = GetIcone(DeuxiemePartie(token));
                else
                    if (token.StartsWith("TEMPMIN"))
                    tempmin = DeuxiemePartie(token);
                else
                    if (token.StartsWith("TEMPMAX"))
                    tempmax = DeuxiemePartie(token);
            }

            return new LignePrevisionMeteo(icone, date, $"Min:{tempmin}, max:{tempmax}", texte, vent, pluie);
        }

        private string DeuxiemePartie(string ligne)
        {
            int index = ligne.IndexOf('=');
            if (index == -1)
                return "";

            return ligne.Substring(index + 1);
        }
    }
}
