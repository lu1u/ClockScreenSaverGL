using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ClockScreenSaverGL.Config
{
    public class Configuration : IDisposable
    {
        private const string REPERTOIRE_DONNEES = "Donnees";
        private const string REPERTOIRE_IMAGES = "Images";
        private const string IMAGE_DEFAUT = "particule.png";
        public static readonly string NOM_PROGRAMME = "ClockScreenSaverGL";

        // Singleton
        private static Configuration _instance;

        // Dictionnaire des categories de configuration
        private readonly Dictionary<string, CategorieConfiguration> _categories;

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                    return _instance = new Configuration();

                return _instance;
            }
        }

        /// <summary>
        /// Retourne l'objet CategorieConfiguration dont on donne le nom
        /// </summary>
        /// <param name="nom"></param>
        /// <returns>L'objet CaterogieConfiguration demandé</returns>
        public static CategorieConfiguration GetCategorie(string nom)
        {
            using (var c = new Chronometre("Configuration " + nom))
            {
                Configuration conf = Instance;
                conf._categories.TryGetValue(nom, out CategorieConfiguration categorie);
                if (categorie != null)
                    return categorie;

                // Est-ce qu'un fichier existe?
                try
                {
                    string rep = GetRepertoire();
                    string filePath = Path.Combine(rep, nom + CategorieConfiguration.EXTENSION_CONF);
                    if (File.Exists(filePath))
                    {
                        CategorieConfiguration cat = new CategorieConfiguration(nom);
                        conf._categories.Add(nom, cat);
                        return cat;
                    }
                }
                catch (Exception)
                {
                    // Fichier inexistant ou non lisible
                }
                // La categorie n'existe pas encore
                categorie = new CategorieConfiguration(nom);
                conf._categories.Add(nom, categorie);
                return categorie;
            }
        }

        private Configuration()
        {
            _categories = new Dictionary<string, CategorieConfiguration>();
            //LireCategories();
        }


        ~Configuration()
        {
            Flush();
        }

        /// <summary>
        /// Reecriture de tous les fichiers de configuration qui en ont besoin
        /// </summary>
        public void Flush()
        {
            foreach (CategorieConfiguration cat in _categories.Values)
                cat.Flush();
        }

        /// <summary>
        /// Calcule un repertoire pour stocker les fichiers de conf en utilisant le nom du programme et sa version
        /// </summary>
        /// <returns></returns>
        public static string GetRepertoire()
        {
            string res = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString(),
            NOM_PROGRAMME, //System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,	
            "Version 3");

            if (!Directory.Exists(res))
                Directory.CreateDirectory(res);

            return res;
        }

        /// <summary>
        /// Retrouve le chemin du repertoire de stockage des donnees du programme,
        /// DIFFERENT du repertoire de la configuration
        /// </summary>
        /// <returns></returns>
        public static string GetDataDirectory() => Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName, REPERTOIRE_DONNEES);
        public static string GetDataDirectory(string sousrepertoire) => Path.Combine(GetDataDirectory(), sousrepertoire);
        public static string GetImagesDirectory() => Path.Combine(GetDataDirectory(), REPERTOIRE_IMAGES);


        /// <summary>
        /// Retourne le chemin vers l'une des images dans le repertoire des images
        /// </summary>
        /// <param name="imgName">Nom de fichier de l'image</param>
        /// <param name="nullIfNotExist">Si true: on obtient un nom de fichier null si le chemin n'existe pas,
        /// si false: on obtient le chemin d'une image par defaut</param>
        /// <returns></returns>
        public static string GetImagePath(string imgName, bool nullIfNotExist = false)
        {
            string res = Path.Combine(GetImagesDirectory(), imgName);
            if (File.Exists(res))
                return res;

            Log.Instance.Warning("Image inexistante: " + imgName);

            if (nullIfNotExist)
                return null;
            else
                return Path.Combine(GetImagesDirectory(), IMAGE_DEFAUT);
        }

        public void Dispose()
        {
            Flush();
        }
    }
}
