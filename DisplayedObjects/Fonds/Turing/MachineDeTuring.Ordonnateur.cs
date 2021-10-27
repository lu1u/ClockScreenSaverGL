using ClockScreenSaverGL.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Turing
{
    partial class MachineDeTuring
    {
        public enum VALEUR { NULL, ZERO, UN };
        public enum DEPLACEMENT { GAUCHE, DROITE, RIEN }
        public class Instruction
        {
            public VALEUR _valeurAEcrire;               // Valeur a ecrire sur le ruban
            public DEPLACEMENT _decaleRuban;            // Deplacement de la tete de lecture
            public int _etatSuivant;                    // Etat suivant
            public Instruction(VALEUR ecrire, DEPLACEMENT decale, int suiv)
            {
                _valeurAEcrire = ecrire;
                _decaleRuban = decale;
                _etatSuivant = suiv;
            }
        }

        public class Etat
        {
            public Instruction[] _instructions = new Instruction[NB_INSTRUCTIONS];
            public string _commentaire = "" ;
        }

        private List<string> _fichiers;
        static readonly int NB_INSTRUCTIONS = 3;
        static readonly int INDICE_INSTRUCTION_NULL = 0;
        static readonly int INDICE_INSTRUCTION_ZERO = 1;
        static readonly int INDICE_INSTRUCTION_UN = 2;

        public static readonly int NB_SYMBOLES = 18;
        public static readonly int SYMBOLE_VIDE = 0;
        public static readonly int SYMBOLE_ZERO = 1;
        public static readonly int SYMBOLE_UN = 2;
        public static readonly int SYMBOLE_DEUX = 3;
        public static readonly int SYMBOLE_TROIS = 4;
        public static readonly int SYMBOLE_QUATRE = 5;
        public static readonly int SYMBOLE_CINQ = 6;
        public static readonly int SYMBOLE_SIX = 7;
        public static readonly int SYMBOLE_SEPT = 8;
        public static readonly int SYMBOLE_HUIT = 9;
        public static readonly int SYMBOLE_NEUF = 10;
        public static readonly int SYMBOLE_GAUCHE = 11;
        public static readonly int SYMBOLE_DROITE = 12;
        public static readonly int SYMBOLE_LECTURE = 13;
        public static readonly int SYMBOLE_ECRITURE = 14;
        public static readonly int SYMBOLE_DEPLACEMENT = 15;
        public static readonly int SYMBOLE_ETAT = 16;

        List<Etat> _etats = new List<Etat>();
        char[] ruban;
        int _indiceRuban;
        int _instructionCible;
        public enum ETAPE_TURING { DEBUT, LECTURE, RECHERCHE_INSTRUCTION, ECRITURE, DEPLACEMENT, CHANGE_ETAT };
        ETAPE_TURING _etatOrdonnateur;
        VALEUR _valeurAEcrire;
        string _nom = "";
        string _description = "";
        int _instructionActive = 0;
        int _etatActif = 0;


        private void InitOrdonnateur()
        {
            _etatOrdonnateur = ETAPE_TURING.DEBUT;

            _instructionActive = -1;
            LitFichiers();
            ProchainFichier();
        }

        /// <summary>
        /// Lit les fichiers dans le repertoire des programmes turing
        /// </summary>
        private void LitFichiers()
        {
            _fichiers = new List<string>();
            string repertoire = Configuration.getDataDirectory("Turing");
            string[] filePaths = Directory.GetFiles(repertoire);
            foreach (string filename in filePaths)
                _fichiers.Add(filename);
        }


        /// <summary>
        /// Lit le prochain fichier molecule dans le repertoire
        /// </summary>
        private void ProchainFichier()
        {
            FICHIER_EN_COURS++;
            if (FICHIER_EN_COURS >= _fichiers.Count)
                FICHIER_EN_COURS = 0;

            c.setParametre(PARAM_FICHIER_EN_COURS, FICHIER_EN_COURS);
            c.flush();

            LireFichierProgramme(_fichiers[FICHIER_EN_COURS]);
        }

        /// <summary>
		/// Lecture du fichier programme et creation de l'objet _etats
		/// </summary>
		/// <param name="v">Nom du fichier a lire</param>
		private void LireFichierProgramme(string fichier)
        {
            StreamReader file = new StreamReader(fichier);
            string l;
            List<string> rubans = new List<string>();
            _etats = new List<Etat>();
            _nom = "";
            _description = "";

            while ((l = file.ReadLine()) != null)
            {
                string line = l.ToUpper();

                if (line.StartsWith("NOM"))
                    _nom = getValeur(l);
                else if (line.StartsWith("RUBAN"))
                    rubans.Add(getValeur(line));
                else
                if (line.StartsWith("DESCRIPTION"))
                    _description = getValeur(l);
                else if (line.StartsWith("ETAT"))
                {
                    Etat etat = getEtat(getValeur(l));
                    _etats.Add(etat);
                }
            }
            file.Close();
            _etatActif = 0;

            if (rubans.Count == 0)
            {
                string rub = "";
                for (int i = 0; i < 20; i++)
                {
                    switch (r.Next(3))
                    {
                        case 0: rub += ' '; break;
                        case 1: rub += '0'; break;
                        default: rub += '1'; break;
                    }
                }
                rubans.Add(rub);
            }

            ruban = rubans[r.Next(rubans.Count)].ToCharArray();
            VerifieRuban(ruban);
            _indiceRuban = ruban.Length / 2;
        }

        /// <summary>
        /// Verifier que le ruban ne contient que des caracteres valides
        /// </summary>
        /// <param name="ruban"></param>
        private void VerifieRuban(char[] ruban)
        {
            for (int i = 0; i < ruban.Length; i++)
                switch (ruban[i])
                {
                    case ' ': break;
                    case '0': break;
                    case '1': break;
                    default: ruban[i] = ' '; break;
                }
        }

        private Etat getEtat(string v)
        {
            Etat etat = new Etat();
            
            string[] e = v.Split('|');
            if (e.Length >= 3)
            {
                etat._instructions[0] = getInstruction(e[0]);
                etat._instructions[1] = getInstruction(e[1]);
                etat._instructions[2] = getInstruction(e[2]);

                if (e.Length >= 4)
                    etat._commentaire = e[3];
            }
            else
                Log.instance.error("Turing: " + v + ": instruction incorrecte");
            return etat;
        }

        private Instruction getInstruction(string v)
        {
            VALEUR valeurEcrite = VALEUR.NULL;
            DEPLACEMENT deplacement = DEPLACEMENT.RIEN;
            int etatSuivant = 0;
            if (v != null)
            {
                int longueur = v.Length;
                if (longueur > 0) valeurEcrite = getValeur(v[0]);
                if (longueur > 1) deplacement = getDeplacement(v[1]);
                if (longueur > 2) etatSuivant = Int16.Parse(v.Substring(2));
            }

            return new Instruction(valeurEcrite, deplacement, etatSuivant);
        }

        private static DEPLACEMENT getDeplacement(char v)
        {
            switch (v)
            {
                case '<': return DEPLACEMENT.GAUCHE;
                case '>': return DEPLACEMENT.DROITE;
                default: return DEPLACEMENT.RIEN;
            }
        }

        private static VALEUR getValeur(char c)
        {
            switch (c)
            {
                case '0': return VALEUR.ZERO;
                case '1': return VALEUR.UN;
                default: return VALEUR.NULL;
            }
        }

        private string getValeur(string line)
        {
            int indice = line.IndexOf("=");
            return line.Substring(indice + 1);
        }



        private void EtapeSuivante()
        {
            switch (_etatOrdonnateur)
            {
                case ETAPE_TURING.DEBUT: EtapeDebut(); break;
                case ETAPE_TURING.LECTURE: EtapeLecture(); break;
                case ETAPE_TURING.RECHERCHE_INSTRUCTION: EtapeRechercheInstruction(); break;
                case ETAPE_TURING.ECRITURE: EtapeEcriture(); break;
                case ETAPE_TURING.DEPLACEMENT: EtapeDeplacement(); break;
                case ETAPE_TURING.CHANGE_ETAT: EtapeChangeFiche(); break;
            }
        }


        /// <summary>
        /// Lecture de la valeur sous la tete
        /// </summary>
        private void EtapeDebut()
        {
            _tete.TeteInitAnimationLecture(DUREE_ANIMATION);
            _animation = _tete.TeteAnimationLecture;
            _etatOrdonnateur = ETAPE_TURING.LECTURE;
        }

        /// <summary>
        /// Fin de l'etape lecture, passer a l'etape recherche de l'instruction correspondante
        /// </summary>
        private void EtapeLecture()
        {
            switch (ruban[_indiceRuban])
            {
                case ' ':
                    _instructionCible = INDICE_INSTRUCTION_NULL;
                    break;
                case '0':
                    _instructionCible = INDICE_INSTRUCTION_ZERO;
                    break;
                default:
                    _instructionCible = INDICE_INSTRUCTION_UN;
                    break;
            }

            _etatOrdonnateur = ETAPE_TURING.RECHERCHE_INSTRUCTION;
            _programme.InitAnimationRechercheInstruction(_instructionCible, DUREE_ANIMATION);
            _animation = _programme.AnimationRechercheInstruction;
        }

        /// <summary>
        /// Recherche de l'instruction correspondant a la valeur lue
        /// </summary>
        private void EtapeRechercheInstruction()
        {
            // Fin de l'étape de lecture
            _instructionActive = _instructionCible;
            _valeurAEcrire = _etats[_etatActif]._instructions[_instructionActive]._valeurAEcrire;
            if (toChar(_valeurAEcrire) == ruban[_indiceRuban])
            {
                // On passe directement a l'etape deplacement
                _etatOrdonnateur = ETAPE_TURING.DEPLACEMENT;
                _ruban.InitDeplacement(_etats[_etatActif]._instructions[_instructionActive]._decaleRuban, DUREE_ANIMATION);
                _animation = _ruban.AnimationDeplacement;
            }
            else
            {
                // On passe a l'etape d'ecriture
                _etatOrdonnateur = ETAPE_TURING.ECRITURE;
                _tete.TeteInitAnimationEcriture(DUREE_ANIMATION);
                _animation = _tete.TeteAnimationEcriture;
            }
        }


        /// <summary>
        /// Ecriture de la valeur programmee sous la tete
        /// </summary>
        private void EtapeEcriture()
        {
            _etatOrdonnateur = ETAPE_TURING.DEPLACEMENT;
            ruban[_indiceRuban] = toChar(_valeurAEcrire);
            _ruban.InitDeplacement(_etats[_etatActif]._instructions[_instructionActive]._decaleRuban, DUREE_ANIMATION);
            _animation = _ruban.AnimationDeplacement;
        }


        private void EtapeDeplacement()
        {
            // Fin de la phase d'ecriture
            switch (_etats[_etatActif]._instructions[_instructionActive]._decaleRuban)
            {
                case DEPLACEMENT.DROITE: DecaleRubanGauche(); break;
                case DEPLACEMENT.GAUCHE: DecaleRubanDroite(); break;
                default: break;
            }

            if (_etats[_etatActif]._instructions[_instructionActive]._etatSuivant == _etatActif)
            {
                /// Pas de changement d'etat
                _etatOrdonnateur = ETAPE_TURING.DEBUT;
            }
            else
            {
                // Changement d'etat du programme
                _etatOrdonnateur = ETAPE_TURING.CHANGE_ETAT;
            }

            _programme.InitAnimationChangeEtat(DUREE_ANIMATION);
            _animation = _programme.AnimationChangeEtat;
        }

        private void DecaleRubanGauche()
        {
            char r = ruban[0];
            for (int i = 1; i < ruban.Length; i++)
                ruban[i - 1] = ruban[i];

            ruban[ruban.Length - 1] = r;
        }

        private void DecaleRubanDroite()
        {
            char r = ruban[ruban.Length - 1];
            for (int i = ruban.Length - 1; i > 0; i--)
                ruban[i] = ruban[i - 1];

            ruban[0] = r;
        }

        private char toChar(VALEUR v)
        {
            switch (v)
            {
                case VALEUR.UN: return '1';
                case VALEUR.ZERO: return '0';
                default: return ' ';
            }
        }


        /// <summary>
        /// Etape: changer de fiches
        /// </summary>
        private void EtapeChangeFiche()
        {
            _etatOrdonnateur = ETAPE_TURING.DEBUT;
            int e = _etats[_etatActif]._instructions[_instructionActive]._etatSuivant;
            if (e != _etatActif)
                if (e >= 0 && e < _etats.Count)
                {
                    _etatActif = e;
                    _programme.Init(_gl, c, _nom, _description, _etatActif, _etats.Count, _etats[_etatActif]._commentaire); ;
                }
        }
    }
}
