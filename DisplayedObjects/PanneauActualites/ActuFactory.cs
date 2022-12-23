////
//// Gestion de la liste des actualites avec chargement des flux RSS en tache de fond
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ClockScreenSaverGL.DisplayedObjects.PanneauActualites
{
    internal class ActuFactory : ObjetAsynchrone
    {
        public const string EXTENSION_ACTUALITES = "txt";
        public const string EXTENSION_IMAGES = "jpg";

        public List<LigneActu> _lignes = new List<LigneActu>();
        private OpenGL _gl;

        public ActuFactory(OpenGL gl) : base(ThreadPriority.BelowNormal)
        {
            _gl = gl;
        }

        protected override void InitAsynchrone()
        {
            try
            {
                using (var c = new Chronometre("Init Actufactory"))
                {
                    string repertoire = Path.Combine(Config.Configuration.GetDataDirectory(), "Actualites");
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
                            LigneActu l = new LigneActu(_gl, fichier, image);
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

        protected override void InitSynchrone()
        {
        }
    }
}
