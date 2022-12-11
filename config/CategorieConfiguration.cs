﻿///
/// Categorie de configuration ( = un fichier de conf, = un objet affiche)
///

using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace ClockScreenSaverGL.Config
{
    public class CategorieConfiguration : IDisposable
    {
        #region Public Fields

        public const string EXTENSION_CONF = ".conf";

        #endregion Public Fields

        #region Private Fields

        private const string DEBUT_COMMENTAIRE = "#";
        public string _fileName, _nom;
        private bool _propre = true;
        private SortedDictionary<string, Parametre> _valeurs = new SortedDictionary<string, Parametre>();

        private string keyCourante;

        #endregion Private Fields



        #region Public Constructors

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="nom">Nom de la categorie</param>
        public CategorieConfiguration(string nom)
        {
            _nom = nom;
            _fileName = getFileName(nom);
            LireFichier(_fileName);
        }

        #endregion Public Constructors

        #region Private Destructors

        ~CategorieConfiguration()
        {
            Dispose();
        }

        #endregion Private Destructors


        #region Public Methods

        /// <summary>
        /// Obtient le nom de fichier pour cette categorie
        /// </summary>
        /// <param name="nom"></param>
        /// <returns></returns>
        public static string getFileName(string nom)
        {
            return Path.Combine(Configuration.getRepertoire(), nom + EXTENSION_CONF);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
                flush();
        }
        public virtual void Dispose()
        {
            flush();
        }

        public string[] getLignesParametres()
        {
            string[] res = new string[_valeurs.Count];
            int i = 0;
            foreach (Parametre p in _valeurs.Values.OrderBy(p => p._nom))
            {
                char couleur;
                if (keyCourante == null)
                    keyCourante = p._nom;

                if (p.modifiable)
                    couleur = p._nom.Equals(keyCourante) ? 'Y' : 'G';
                else
                    couleur = p._nom.Equals(keyCourante) ? 'W' : 'r';

                res[i] = couleur + p._nom + " = " + p.valueToString();
                i++;
            }
            return res;
        }

#if SHARPGL
        /// <summary>
        /// Ajoute les parametre dans la console texte
        /// </summary>
        /// <param name="gl"></param>
        public void fillConsole(OpenGL gl)
        {
            DisplayedObjects.Console c = DisplayedObjects.Console.getInstance(gl);
            c.AddLigne(Color.LightGreen, "");
            c.AddLigne(Color.LightGreen, _nom);
            c.AddLigne(Color.LightGreen, "");
            c.AddLigne(Color.LightGreen, "8/2 : changer le parametre courant");
            c.AddLigne(Color.LightGreen, "4/6 : modifier la valeur du parametre courant");
            c.AddLigne(Color.LightGreen, "Les valeurs en gris nécessitent de redémarrer le fond (touche R)");
            c.AddLigne(Color.LightGreen, "");

            foreach (Parametre p in _valeurs.Values.OrderBy(p => p._nom))
                if (p._type != Parametre.TYPE_PARAMETRE.T_STRING)
                {
                    if (p.modifiable)
                        c.AddLigne(p._nom.Equals(keyCourante) ? Color.Yellow : Color.Green, p._nom + " = " + p.valueToString());
                    else
                        c.AddLigne(p._nom.Equals(keyCourante) ? Color.White : Color.Gray, p._nom + " = " + p.valueToString());
                }
        }
#endif
        /// <summary>
        /// S'assurer que les modifications sur la categorie sont bien ecrites dans le fichier
        /// </summary>
        public void flush()
        {
            if (OnDoitEcrire())
            {
                using (TextWriter tw = new StreamWriter(_fileName))
                {
                    tw.WriteLine("# ----------------------------------------");
                    tw.WriteLine("# " + _nom);
                    tw.WriteLine("# Fichier de configuration ");
                    tw.WriteLine("# (c) Lucien Pilloni 2014");
                    tw.WriteLine("# ----------------------------------------");
                    tw.WriteLine("");
                    foreach (Parametre p in _valeurs.Values.OrderBy(p => p._nom))
                        if (p._utilisé)
                            p.ecritDansFichier(tw);

                    tw.Close();
                }
                _propre = true;
            }
        }

        private bool OnDoitEcrire()
        {
            if (!_propre)
                return true;

            foreach (Parametre p in _valeurs.Values)
                if (!p._utilisé)
                    return true;

            return false;
        }

        /// <summary>
        /// Pression sur une touche, retourner true si l'objet a traite, false = fin de l'economiseur
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <returns>true si touche utilisee</returns>
        public virtual bool KeyDown(Keys k)
        {
            // Tableau des clefs
            List<string> clefs = new List<string>();
            foreach (Parametre p in _valeurs.Values.OrderBy(p => p._nom)/*.Where(p => p._modifiable)*/)
                clefs.Add(p._nom);

            if (clefs.Count == 0)
                return k == Keys.NumPad2 || k == Keys.NumPad8 || k == Keys.NumPad4 || k == Keys.NumPad6;

            if (keyCourante == null)
                keyCourante = clefs[0];

            switch (k)
            {
                case Keys.NumPad2:
                    {
                        int indice = clefs.IndexOf(keyCourante);
                        indice++;
                        if (indice >= clefs.Count)
                            indice = 0;

                        keyCourante = clefs[indice];
                        break;
                    }

                case Keys.NumPad8:
                    {
                        int indice = clefs.IndexOf(keyCourante);
                        indice--;
                        if (indice < 0)
                            indice = clefs.Count - 1;

                        keyCourante = clefs[indice];
                        break;
                    }

                case Keys.NumPad4:
                    {
                        Parametre p;
                        if (_valeurs.TryGetValue(keyCourante, out p))
                        {
                            p.Diminue();
                            _propre = false;
                            //_parametreChange?.Invoke(keyCourante);
                        }
                        break;
                    }

                case Keys.NumPad6:
                    {
                        Parametre p;
                        if (_valeurs.TryGetValue(keyCourante, out p))
                        {
                            p.Augmente();
                            _propre = false;
                            //_parametreChange?.Invoke(keyCourante);
                        }
                        break;
                    }

                case Keys.NumPad5:
                    {
                        Parametre p;
                        if (_valeurs.TryGetValue(keyCourante, out p))
                        {
                            p.Defaut();
                            _propre = false;
                            //_parametreChange?.Invoke(keyCourante);
                        }
                        break;
                    }
                case Keys.NumPad0:
                    {
                        Parametre p;
                        if (_valeurs.TryGetValue(keyCourante, out p))
                        {
                            p.Nulle();
                            _propre = false;
                            //_parametreChange?.Invoke(keyCourante);
                        }
                        break;
                    }

                case Keys.Subtract:
                    {
                        Parametre p;
                        if (_valeurs.TryGetValue(keyCourante, out p))
                        {
                            p.Negatif();
                            _propre = false;
                            //_parametreChange?.Invoke(keyCourante);
                        }

                        break;
                    }
                default:
                    return false;
            }

            return true;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Lit les parametres dans le fichier de la categorie (un par ligne)
        /// </summary>
        /// <param name="filename"></param>
        private void LireFichier(string filename)
        {
            try
            {
                // La remplir a partir du contenu du fichier
                using (StreamReader file = new System.IO.StreamReader(filename))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                        if ((line.Length > 0) && (!line.StartsWith(DEBUT_COMMENTAIRE))) // Commentaire
                        {
                            try
                            {
                                Parametre parametre = new Parametre(line);
                                _valeurs.Add(parametre._nom, parametre);
                            }
                            catch (Exception e)
                            {
                                Log.instance.error("Exception dans CategorieConfiguration.LireFichier " + filename);
                                Log.instance.error(line);
                                Log.instance.error(e.Message);
                            }
                        }
                }
            }
            catch (FileNotFoundException)
            {
                // C'est normal: pas encore de fichier de configuration (premier lancement)
            }
            catch (DirectoryNotFoundException)
            {
                // C'est normal: par encore de fichier de configuration (premier lancement)
            }
            catch (Exception)
            {
                // Erreur inconnue
                throw;
            }
        }

        #endregion Private Methods

        #region setParametre

        public void setParametre(string nom, bool valeur)
        {
            setParametre(nom, Parametre.TYPE_PARAMETRE.T_BOOL, valeur);
        }

        public void setParametre(string nom, float valeur)
        {
            setParametre(nom, Parametre.TYPE_PARAMETRE.T_FLOAT, valeur);
        }

        public void setParametre(string nom, int valeur)
        {
            setParametre(nom, Parametre.TYPE_PARAMETRE.T_INT, valeur);
        }

        /// <summary>
        /// Changement d'un parametre
        /// </summary>
        /// <param name="valueName"></param>
        /// <param name="type"></param>
        /// <param name="defaut"></param>
        private void setParametre(string valueName, Parametre.TYPE_PARAMETRE type, object defaut)
        {
            if (_valeurs.ContainsKey(valueName))
                _valeurs.Remove(valueName);

            Parametre parametre = new Parametre(valueName, type, defaut);
            parametre._utilisé = true;
            _valeurs.Add(valueName, parametre);

            _propre = false;
        }

        #endregion setParametre

        #region getParametre

        public int getParametre(string name, int defaut, Action<object> actionModif = null)
        {
            return (int)(getParametre(name, Parametre.TYPE_PARAMETRE.T_INT, defaut, actionModif));
        }

        public bool getParametre(string name, bool defaut, Action<object> actionModif = null)
        {
            return (bool)(getParametre(name, Parametre.TYPE_PARAMETRE.T_BOOL, defaut, actionModif));
        }

        public float getParametre(string name, float defaut, Action<object> actionModif = null)
        {
            return (float)(getParametre(name, Parametre.TYPE_PARAMETRE.T_FLOAT, defaut, actionModif));
        }

        public double getParametre(string name, double defaut, Action<object> actionModif = null)
        {
            return (double)(getParametre(name, Parametre.TYPE_PARAMETRE.T_DOUBLE, defaut, actionModif));
        }

        public string getParametre(string name, string defaut, Action<object> actionModif = null)
        {
            return (string)(getParametre(name, Parametre.TYPE_PARAMETRE.T_STRING, defaut, actionModif));
        }

        public byte getParametre(string name, byte defaut, Action<object> actionModif = null)
        {
            return (byte)(getParametre(name, Parametre.TYPE_PARAMETRE.T_BYTE, defaut, actionModif));
        }

        /// <summary>
        /// Obtention d'un parametre, s'il n'existe pas dans la categorie, on l'ajoute
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="type"></param>
        /// <param name="defaut"></param>
        /// <param name="modifiable"></param>
        /// <returns></returns>
        private object getParametre(string nom, Parametre.TYPE_PARAMETRE type, object defaut, Action<object> actionModif = null)
        {
            Parametre p;
            _valeurs.TryGetValue(nom, out p);
            if ((p != null) && (p is Parametre))
            {
                if (p._type != type)
                    return defaut;
                p._defaut = defaut;
                p._action = actionModif;
                p._utilisé = true;
                return p._value;
            }

            p = new Parametre(nom, type, defaut, actionModif);
            p._utilisé = true;                      // Le parametre etait manquant dans le fichier, il faut l'ajouter a la prochaine ecriture
            _valeurs.Add(nom, p);
            _propre = false;

            return defaut;
        }

        #endregion getParametre
    }
}
