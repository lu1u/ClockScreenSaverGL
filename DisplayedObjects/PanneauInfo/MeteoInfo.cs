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
    internal class MeteoInfo : IDisposable
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

        private static Dictionary<string, string> _liensIcones = new Dictionary<string, string>();

        public MeteoInfo()
        {
            getConfiguration();

            _title = "Crolles";

            LitCorrespondancesMeteo();
            ChargeDonnees();
        }

        private CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                CHEMIN_FICHIER = c.getParametre("chemin fichier", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(), "clockscreensaver meteo.txt"));
                NB_LIGNES_INFO_MAX = c.getParametre("nb lignes info", 5);
            }
            return c;
        }

        /// <summary>
        /// Lecture de la table de correspondance qui fait le lien entre les nom utilises sur le site meteo
        /// et les icones utilisees par le programme
        /// </summary>
        private void LitCorrespondancesMeteo()
        {
            _liensIcones.Clear();
            string fichierSources = Path.Combine(Configuration.getDataDirectory(), "icones meteo.txt");
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
        public static string getIcone(string imageSurLeSite)
        {
            string valeur;
            if (_liensIcones.TryGetValue(imageSurLeSite, out valeur))
                return valeur;

            return imageSurLeSite;
        }

        public void Dispose()
        {
            if (_lignes != null)
                foreach (LignePrevisionMeteo l in _lignes)
                    l.Dispose();
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
                        _url = deuxiemePartie(ligne);
                    else
                        if (ligne.StartsWith("TITRE"))
                        _title = deuxiemePartie(ligne);
                    else
                        if (ligne.StartsWith("JOUR"))
                    {
                        if (_lignes.Count < NB_LIGNES_INFO_MAX)
                            _lignes.Add(CreateLigne(deuxiemePartie(ligne)));
                    }
                }
            }
            catch (Exception e)
            {
                Log.instance.error("Meteo Info: impossible de charger les données");
                Log.instance.error(e.Message);
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
                    date = deuxiemePartie(token);
                else
                    if (token.StartsWith("PREVISION"))
                    texte = deuxiemePartie(token);
                else
                    if (token.StartsWith("PIC"))
                    icone = getIcone(deuxiemePartie(token));
                else
                    if (token.StartsWith("TEMPMIN"))
                    tempmin = deuxiemePartie(token);
                else
                    if (token.StartsWith("TEMPMAX"))
                    tempmax = deuxiemePartie(token);
            }

            return new LignePrevisionMeteo(icone, date, $"Min:{tempmin}, max:{tempmax}", texte, vent, pluie);
        }

        private string deuxiemePartie(string ligne)
        {
            int index = ligne.IndexOf('=');
            if (index == -1)
                return "";

            return ligne.Substring(index + 1);
        }
    }
}
