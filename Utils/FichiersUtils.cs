using ClockScreenSaverGL.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClockScreenSaverGL.Utils
{
    internal class FichiersUtils
    {
        private const char COMMENTAIRE = '#';

        /// <summary>
        /// Lit un fichier texte et retourne les lignes sous forme d'une liste de strings
        /// les ligbes commencant par * sont ignorées (commentaires)
        /// </summary>
        /// <param name="nomfichier"></param>
        /// <returns></returns>
        public static List<string> LitFichierChaines(string nomfichier)
        {
            List<string> lignes = new List<string>();
            nomfichier = Path.Combine(Config.Configuration.GetDataDirectory(), nomfichier);
            try
            {
                StreamReader file = new StreamReader(nomfichier, Encoding.UTF8);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length>0)
                        if (line[0] != COMMENTAIRE)
                            lignes.Add(line);
                }

                file.Close();
            }
            catch (Exception e)
            {
                Log.Instance.Error("LitFichierChaines " + nomfichier);
                Log.Instance.Error(e.Message);
            }

            return lignes;
        }
    }
}
