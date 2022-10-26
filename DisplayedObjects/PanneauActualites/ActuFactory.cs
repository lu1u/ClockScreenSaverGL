////
//// Gestion de la liste des actualites avec chargement des flux RSS en tache de fond
using SharpGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ClockScreenSaverGL.DisplayedObjects.PanneauActualites
{
    class ActuFactory : IDisposable
    {
        public const string EXTENSION_ACTUALITES = "txt";
        public const string EXTENSION_IMAGES = "jpg";

        public List<LigneActu> _lignes = new List<LigneActu>();

        public ActuFactory()
        {

        }

        internal void Init()
        {
            try
            {
                using (var c = new Chronometre("Init Actufactory"))
                {
                    string repertoire = Path.Combine(Config.Configuration.getDataDirectory(), "Actualites");
                    string[] fichiers = Directory.GetFiles(repertoire, $"*.{EXTENSION_ACTUALITES}");
                    if (fichiers != null)
                    {
                        Array.Sort(fichiers, delegate (string O1, string O2)
                        {
                            return -string.Compare(O1, O2);
                        });
                        foreach (string fichier in fichiers)
                        {
                            string image = Path.ChangeExtension(fichier, EXTENSION_IMAGES);
                            LigneActu l = new LigneActu(fichier, image);
                            // Les textures seront creees au moment du premier affichage pour accelecer le démarrage l.CreerTexture(gl, true);
                            _lignes.Add(l);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        public void Dispose()
        {
            foreach (LigneActu l in _lignes)
                l.Dispose();

            _lignes.Clear();
        }
    }
}
