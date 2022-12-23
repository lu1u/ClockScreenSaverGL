using System;
using System.IO;
/// <summary>
/// Parametre dans un fichier de configuration, stocke dans une ligne au format:
/// {nom}:{type},{modifiable}={valeur}
/// ou
/// {type} = int, float,double, bool, string, byte
/// {modifiable} = modifiable, non_modifiable
/// </summary>

namespace ClockScreenSaverGL.Config
{
    public class Parametre
    {
        public enum TYPE_PARAMETRE { T_INT, T_FLOAT, T_DOUBLE, T_BOOL, T_STRING, T_BYTE };

        public Action<object> _action;
        public object _defaut;
        public TYPE_PARAMETRE _type;
        public object _value;
        public bool _utilisé = false;
        public string _nom;


        private const string TYPE_BOOL = "bool";
        private const string TYPE_BYTE = "byte";
        private const string TYPE_DOUBLE = "double";
        private const string TYPE_FLOAT = "float";
        private const string TYPE_INT = "int";
        private const string TYPE_STRING = "string";


        #region Public Constructors

        /// <summary>
        /// Construit le parametre en interpretant une ligne du fichier de configuration
        /// </summary>
        /// <param name="ligneFichier"></param>
        /// <exception cref="Exception"></exception>
        public Parametre(string ligneFichier)
        {
            string tmp = ligneFichier;

            // Extraire le nom
            int finMot = ligneFichier.IndexOf(':');
            if (finMot == -1)
                throw new Exception("Impossible de trouver le nom du parametre " + ligneFichier);
            _nom = ligneFichier.Substring(0, finMot);
            ligneFichier = ligneFichier.Substring(finMot + 1);

            // Type
            finMot = ligneFichier.IndexOf(',');
            if (finMot == -1)
                throw new Exception("Impossible de trouver le type du parametre " + tmp);
            _type = StringToType(ligneFichier.Substring(0, finMot).Trim());
            ligneFichier = ligneFichier.Substring(finMot + 1);

            // Modifiable
            finMot = ligneFichier.IndexOf('=');
            if (finMot == -1)
                throw new Exception("Impossible de trouver le code modifiable du parametre " + tmp);
            ligneFichier = ligneFichier.Substring(finMot + 1);

            // Valeur
            _value = ToObject(_type, ligneFichier);
            _defaut = ToObject(_type, ligneFichier);
            _utilisé = false;
        }

        /// <summary>
        /// Construit un parametre en fonction des parametres donnés
        /// </summary>
        /// <param name="nom"></param>
        /// <param name="type"></param>
        /// <param name="valeur"></param>
        /// <param name="action"></param>
        public Parametre(string nom, TYPE_PARAMETRE type, object valeur, Action<object> action = null)
        {
            this._nom = nom;
            _type = type;
            _value = valeur;
            _defaut = valeur;
            _action = action;
        }

        #endregion Public Constructors



        #region Public Properties

        public bool Modifiable

        {
            get { return _action != null; }
        }

        #endregion Public Properties

        #region Public Methods

        static public string ValueToString(TYPE_PARAMETRE type, object value)
        {
            switch (type)
            {
                case TYPE_PARAMETRE.T_INT:
                    return ((int)value).ToString();
                case TYPE_PARAMETRE.T_FLOAT:
                    return ((float)value).ToString();
                case TYPE_PARAMETRE.T_DOUBLE:
                    return ((double)value).ToString();
                case TYPE_PARAMETRE.T_BOOL:
                    return (StringFromBool((bool)value)).ToString();
                case TYPE_PARAMETRE.T_STRING:
                    return ((string)value);
                case TYPE_PARAMETRE.T_BYTE:
                    return ((byte)value).ToString();

                default:
                    return null;
            }
        }

        public string ValueToString()
        {
            return ValueToString(_type, _value);
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Augmente la valeur du parametre, methode dependant du type de parametre
        /// </summary>
        internal void Augmente()
        {
            switch (_type)
            {
                case TYPE_PARAMETRE.T_INT:
                    _value = ((int)_value) + 1;
                    break;

                case TYPE_PARAMETRE.T_FLOAT:
                    _value = ((float)_value) * 1.01f;
                    break;
                case TYPE_PARAMETRE.T_DOUBLE:
                    _value = ((double)_value) * 1.01;
                    break;

                case TYPE_PARAMETRE.T_BOOL:
                    _value = !(bool)_value;
                    break;

                case TYPE_PARAMETRE.T_STRING:
                    break;

                case TYPE_PARAMETRE.T_BYTE:
                    _value = (byte)(((byte)_value + 1) % 256);
                    break;
            }

            _action?.Invoke(_value);
        }

        /// <summary>
        /// Remet la valeur par defaut
        /// </summary>
        internal void Defaut()
        {
            _value = _defaut;
            _action?.Invoke(_value);
        }

        /// <summary>
        /// Diminuela valeur du parametre, methode dependant du type de parametre
        /// </summary>
        internal void Diminue()
        {
            switch (_type)
            {
                case TYPE_PARAMETRE.T_INT:
                    _value = ((int)_value) - 1;
                    break;

                case TYPE_PARAMETRE.T_FLOAT:
                    _value = ((float)_value) / 1.01f;
                    break;
                case TYPE_PARAMETRE.T_DOUBLE:
                    _value = ((double)_value) / 1.01;
                    break;

                case TYPE_PARAMETRE.T_BOOL:
                    _value = !(bool)_value;
                    break;

                case TYPE_PARAMETRE.T_STRING:
                    break;

                case TYPE_PARAMETRE.T_BYTE:
                    _value = (byte)_value > 0 ? (byte)((byte)(_value) - 1) : (byte)255;
                    break;
            }

            _action?.Invoke(_value);
        }

        /// <summary>
        /// Ligne de fichier de configuration:
        /// {nom}:{type},modifiable|non modifiable={valeur}|"{valeur}"
        /// </summary>
        /// <param name="tw"></param>
        internal void EcritDansFichier(TextWriter tw)
        {
            tw.WriteLine(_nom + ":" + TypeToString(_type) + "," + ModifiableToString(Modifiable) + "=" + ValueToString(_type, _value));
        }

        /// <summary>
        /// Inverse la valeur du parametre
        /// </summary>
        internal void Negatif()
        {
            switch (_type)
            {
                case TYPE_PARAMETRE.T_INT:
                    _value = -(int)_value;
                    break;

                case TYPE_PARAMETRE.T_FLOAT:
                    _value = -(float)_value;
                    break;
                case TYPE_PARAMETRE.T_DOUBLE:
                    _value = -(double)_value;
                    break;

                case TYPE_PARAMETRE.T_BOOL:
                    _value = false;
                    break;

                case TYPE_PARAMETRE.T_STRING:
                    _value = "";
                    break;

                case TYPE_PARAMETRE.T_BYTE:
                    _value = (byte)0;
                    break;
            }

            _action?.Invoke(_value);
        }

        /// <summary>
        /// Met la valeur du parametre a 0
        /// </summary>
        internal void Nulle()
        {
            switch (_type)
            {
                case TYPE_PARAMETRE.T_INT:
                    _value = 0;
                    break;

                case TYPE_PARAMETRE.T_FLOAT:
                    _value = 0.0f;
                    break;
                case TYPE_PARAMETRE.T_DOUBLE:
                    _value = 0.0;
                    break;

                case TYPE_PARAMETRE.T_BOOL:
                    _value = !(bool)_value;
                    break;

                case TYPE_PARAMETRE.T_STRING:
                    _value = "";
                    break;

                case TYPE_PARAMETRE.T_BYTE:
                    _value = (byte)(256 - (byte)_value);
                    break;
            }

            _action?.Invoke(_value);
        }

        #endregion Internal Methods

        #region Private Methods

        private static bool BoolFromString(string s)
        {
            if (s == null)
                return false;

            string S = s.ToLower();
            if (S.Equals("true") || S.Equals("vrai"))
                return true;

            return false;
        }

        static private byte ByteFromString(string valeur)
        {
            return (byte)(IntFromString(valeur) % 256);
        }

        static private double DoubleFromString(string valeur)
        {
            try
            {
                return Double.Parse(valeur);
            }
            catch (Exception)
            {
                try
                {
                    return Int64.Parse(valeur);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        static private float FloatFromString(string valeur)
        {
            return (float)DoubleFromString(valeur);
        }

        static private int IntFromString(string valeur)
        {
            try
            {
                return (int)long.Parse(valeur);
            }
            catch (Exception)
            {
                try
                {
                    return (int)Math.Round(double.Parse(valeur));
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        static private string ModifiableToString(bool modifiable)
        {
            return modifiable ? "modifiable" : "non_modifiable";
        }

        private static string StringFromBool(bool b)
        {
            return b ? "vrai" : "false";
        }

        static private String StringFromString(string valeur)
        {
            if (valeur.StartsWith("\""))
                valeur = valeur.Substring(1);

            if (valeur.EndsWith("\""))
                valeur = valeur.Substring(0, valeur.Length - 1);

            return valeur;
        }

        /// <summary>
        /// Retourne un Object representant la valeur du parametre
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mot"></param>
        /// <returns></returns>
        private static object ToObject(TYPE_PARAMETRE type, string mot)
        {
            switch (type)
            {
                case TYPE_PARAMETRE.T_INT: return IntFromString(mot);
                case TYPE_PARAMETRE.T_FLOAT: return FloatFromString(mot);
                case TYPE_PARAMETRE.T_DOUBLE: return DoubleFromString(mot);
                case TYPE_PARAMETRE.T_BOOL: return BoolFromString(mot);
                case TYPE_PARAMETRE.T_STRING: return StringFromString(mot);
                case TYPE_PARAMETRE.T_BYTE: return ByteFromString(mot);
                default:
                    return StringFromString(mot);
            }
        }
        /// <summary>
        /// Retourne la chaine de caractere pour identifier un type dans le fichier de configuration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static private string TypeToString(TYPE_PARAMETRE type)
        {
            switch (type)
            {
                case TYPE_PARAMETRE.T_INT:
                    return TYPE_INT;
                case TYPE_PARAMETRE.T_FLOAT:
                    return TYPE_FLOAT;
                case TYPE_PARAMETRE.T_DOUBLE:
                    return TYPE_DOUBLE;
                case TYPE_PARAMETRE.T_BOOL:
                    return TYPE_BOOL;
                case TYPE_PARAMETRE.T_STRING:
                    return TYPE_STRING;
                case TYPE_PARAMETRE.T_BYTE:
                    return TYPE_BYTE;

                default:
                    return null;
            }
        }


        private TYPE_PARAMETRE StringToType(string type)
        {
            if (type.Equals(TYPE_BOOL))
                return TYPE_PARAMETRE.T_BOOL;
            else if (type.Equals(TYPE_DOUBLE))
                return TYPE_PARAMETRE.T_DOUBLE;
            else if (type.Equals(TYPE_FLOAT))
                return TYPE_PARAMETRE.T_FLOAT;
            else if (type.Equals(TYPE_INT))
                return TYPE_PARAMETRE.T_INT;
            else if (type.Equals(TYPE_STRING))
                return TYPE_PARAMETRE.T_STRING;
            else if (type.Equals(TYPE_BYTE))
                return TYPE_PARAMETRE.T_BYTE;

            return TYPE_PARAMETRE.T_STRING;
        }

        #endregion Private Methods
    }
}
