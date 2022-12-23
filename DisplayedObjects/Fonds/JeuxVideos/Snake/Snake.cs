using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Diagnostics;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class Snake : Fond
    {
        #region Configuration
        private const string CAT = "Snake";
        private CategorieConfiguration c;
        private int NB_LIGNES, NB_COLONNES, NB_POMMES;
        private int DELAI_TIMER_JEUX;
        #endregion

        private const float MIN_VIEWPORT_X = 0;
        private const float MIN_VIEWPORT_Y = 0;
        private const float MAX_VIEWPORT_X = 1.0f;
        private const float MAX_VIEWPORT_Y = 1.0f;
        private float LARGEUR_CASE, HAUTEUR_CASE;
        private const int NB_IMAGES_TEXTURE = 6;
        private int NB_STEP = 50;

        private enum CASE { VIDE = 0, POMME = 1, NORD = 2, SUD = 3, EST = 4, OUEST = 5 };

        sealed private class Case
        {
            public Case(CASE c)
            {
                _case = c;
                changeCouleur = 0;
            }
            public CASE _case;
            public float changeCouleur;
        }

        private enum MODE_JEU { MODE_NORMAL, PERDU }

        private MODE_JEU _mode;
        private readonly Case[,] _cases;
        private int _xTete, _yTete, _xQueue, _yQueue;
        private CASE _directionDeplacement;
        private TimerIsole _timerJeu;
        private readonly TextureAsynchrone _texture;
        private bool _clignotePerdu;
        private int _compteurPerdu;
        private int _longueurSnake;
        private float _stepCouleur;

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_LIGNES = c.GetParametre("Nb lignes", 30);
                NB_COLONNES = c.GetParametre("Nb Colonnes", 40);
                NB_POMMES = c.GetParametre("Nb Pommes", 10);
                LARGEUR_CASE = (MAX_VIEWPORT_X - MIN_VIEWPORT_X) / NB_COLONNES;
                HAUTEUR_CASE = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y) / NB_LIGNES;
                DELAI_TIMER_JEUX = c.GetParametre("Timer jeux", 300, a => { DELAI_TIMER_JEUX = Convert.ToInt32(a); _timerJeu = new TimerIsole(DELAI_TIMER_JEUX); });
                NB_STEP = c.GetParametre("Nb Etapes couleur", 100, a => { NB_STEP = Convert.ToInt32(a); });
            }

            return c;
        }

        public Snake(OpenGL gl) : base(gl)
        {
            c = GetConfiguration();
            _texture = new TextureAsynchrone(gl, Configuration.GetImagePath("snake2.png"));
            _texture.Init();
            _cases = new Case[NB_LIGNES, NB_COLONNES];
            _timerJeu = new TimerIsole(DELAI_TIMER_JEUX);
            InitJeu();
        }

        /// <summary>
        /// Remise a zero du jeu
        /// </summary>
        private void InitJeu()
        {
            _mode = MODE_JEU.MODE_NORMAL;
            // Vider le plateau de jeu
            for (int ligne = 0; ligne < NB_LIGNES; ligne++)
                for (int colonne = 0; colonne < NB_COLONNES; colonne++)
                    _cases[ligne, colonne] = new Case(CASE.VIDE);

            // Placer quelques pommes
            for (int i = 0; i < NB_POMMES; i++)
                _cases[random.Next(NB_LIGNES), random.Next(NB_COLONNES)] = new Case(CASE.POMME);

            // Depart du serpent: milieu, longueur: 5, la queue vers la gauche
            _longueurSnake = 5;
            for (int i = 0; i < _longueurSnake; i++)
            {
                _cases[NB_LIGNES / 2, NB_COLONNES / 2 + i]._case = CASE.EST;
            }

            _xTete = NB_COLONNES / 2 + _longueurSnake;
            _yTete = NB_LIGNES / 2;
            _xQueue = NB_COLONNES / 2;
            _yQueue = NB_LIGNES / 2;
            _directionDeplacement = CASE.EST;
            _stepCouleur = 0;
        }

        /// <summary>
        /// Dessine l'ensemble du jeu
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            using (new Viewport2D(gl, MIN_VIEWPORT_X, MAX_VIEWPORT_Y, MAX_VIEWPORT_X, MIN_VIEWPORT_Y))
            {
                if (_texture.Pret)
                {
                    gl.Disable(OpenGL.GL_LIGHTING);
                    gl.Disable(OpenGL.GL_DEPTH);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.Enable(OpenGL.GL_BLEND);
                    gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                    _texture.Texture.Bind(gl);
                    gl.Begin(OpenGL.GL_QUADS);

                    for (int ligne = 0; ligne < NB_LIGNES; ligne++)
                        for (int colonne = 0; colonne < NB_COLONNES; colonne++)
                        {
                            if (_cases[ligne, colonne]._case != CASE.VIDE)
                            {
                                Color Couleur;

                                if (_mode == MODE_JEU.PERDU && _clignotePerdu)
                                    Couleur = Color.White;
                                else
                                    Couleur = GetColorWithHueChange(couleur, _cases[ligne, colonne].changeCouleur);

                                gl.Color(Couleur.R / 256.0f, Couleur.G / 256.0f, Couleur.B / 256.0f, 1.0f);

                                float image = (float)_cases[ligne, colonne]._case;

                                float imageG = (float)image / NB_IMAGES_TEXTURE;
                                float imageD = (float)(image + 1) / NB_IMAGES_TEXTURE;

                                gl.TexCoord(imageG, 1.0f); gl.Vertex(colonne * LARGEUR_CASE, (ligne + 1.0f) * HAUTEUR_CASE);
                                gl.TexCoord(imageD, 1.0f); gl.Vertex((colonne + 1.0f) * LARGEUR_CASE, (ligne + 1.0f) * HAUTEUR_CASE);
                                gl.TexCoord(imageD, 0.0f); gl.Vertex((colonne + 1.0f) * LARGEUR_CASE, ligne * HAUTEUR_CASE);
                                gl.TexCoord(imageG, 0.0f); gl.Vertex(colonne * LARGEUR_CASE, ligne * HAUTEUR_CASE);
                            }
                        }

                    gl.End();
                }

                gl.End();
            }

            OpenGLColor.Couleur(gl, couleur);
            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Deplacement, rythmé par _timerJeu pour que ca ne soit pas trop rapide
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            if (_timerJeu.Ecoule())
                switch (_mode)
                {
                    case MODE_JEU.MODE_NORMAL: ModeNormal(); break;
                    case MODE_JEU.PERDU: ModePerdu(); break;

                }
        }

        /// <summary>
        /// Mode normal du jeu: deplacer le Snake
        /// </summary>
        private void ModeNormal()
        {
            switch (AvanceTete())
            {
                case CASE.VIDE:
                    AvanceQueue();
                    break;

                case CASE.POMME:
                    PommeMangee();
                    break;

                default:
                    // Ni pomme, ni vide: le corps du Snake
                    Perdu();
                    break;
            }
        }

        private void ModePerdu()
        {
            _clignotePerdu = !_clignotePerdu;
            _compteurPerdu--;
            if (_compteurPerdu <= 0)
            {
                // Repasser en mode normal
                InitJeu();
            }
        }

        private void AvanceQueue()
        {
            int TempX = _xQueue;
            int TempY = _yQueue;
            switch (_cases[_yQueue, _xQueue]._case)
            {
                case CASE.NORD:
                    if (_yQueue > 0)
                        _yQueue--;
                    break;
                case CASE.SUD:
                    if (_yQueue < NB_LIGNES - 1)
                        _yQueue++;
                    break;

                case CASE.OUEST:
                    if (_xQueue > 0)
                        _xQueue--;
                    break;

                case CASE.EST:
                    if (_xQueue < NB_COLONNES - 1)
                        _xQueue++;
                    break;
            }

            // Vider la case ou etait precedemment la queue
            _cases[TempY, TempX]._case = CASE.VIDE;
        }

        /// <summary>
        /// Avance la tete, en evitant de sortir du jeu, en cherchant une pomme
        /// </summary>
        /// <returns>La case qui etait sous la tete</returns>
        private CASE AvanceTete()
        {
            // Decider de la nouvelle direction
            (int dx, int dy) = GetDeplacement(_directionDeplacement);

            if (ChercheDevant())
            {
                // Rien a changer, on continue dans la meme direction
            }
            else
                if (ChercheADroite())
                _directionDeplacement = TourneADroite(_directionDeplacement);
            else
                if (ChercheAGauche())
                _directionDeplacement = TourneAGauche(_directionDeplacement);

            if ((dx != 0) && (_xTete <= 0 || _xTete >= NB_COLONNES - 1) || ((dy != 0) && (_yTete <= 0 || _yTete >= NB_LIGNES - 1)))
            {
                _directionDeplacement = TourneADroiteOuAGauche(_directionDeplacement, _xTete, _yTete);
                if (_xTete < 0) _xTete = 0;
                if (_xTete > NB_COLONNES - 1) _xTete = NB_COLONNES - 1;
                if (_yTete < 0) _yTete = 0;
                if (_yTete > NB_LIGNES - 1) _yTete = NB_LIGNES - 1;
            }

            (dx, dy) = GetDeplacement(_directionDeplacement);

            // Memoriser la nouvelle direction dans la case actuelle
            CASE ret = _cases[_yTete, _xTete]._case;
            _cases[_yTete, _xTete]._case = _directionDeplacement;
            _stepCouleur++;
            if (_stepCouleur > NB_STEP)
                _stepCouleur = 0;
            _cases[_yTete, _xTete].changeCouleur = NB_STEP / (_stepCouleur + 1.0f);

            // Avancer
            _xTete += dx;
            _yTete += dy;

            // Retourner le contenu initial de la case
            return ret;
        }


        /// <summary>
        /// Evaluer le deplacement apres avoir tourné a gauche
        /// </summary>
        /// <returns></returns>
        private bool ChercheAGauche()
        {
            CASE direction = TourneAGauche(_directionDeplacement);
            (int dx, int dy) = GetDeplacement(direction);
            return Cherche(_xTete, _yTete, dx, dy);
        }

        private bool ChercheADroite()
        {
            CASE direction = TourneADroite(_directionDeplacement);
            (int dx, int dy) = GetDeplacement(direction);
            return Cherche(_xTete, _yTete, dx, dy);
        }

        /// <summary>
        /// Evaluer le deplacement en continuant tout droit
        /// </summary>
        /// <returns></returns>
        private bool ChercheDevant()
        {
            (int dx, int dy) = GetDeplacement(_directionDeplacement);
            return Cherche(_xTete, _yTete, dx, dy);
        }
        private bool Cherche(int x, int y, int dx, int dy)
        {
            while (true)
            {
                x += dx;
                y += dy;
                if ((x <= 0) || (x >= NB_COLONNES - 1))
                {
                    //Debug.WriteLine("X hors limites");
                    return false;
                }
                if ((y <= 0) || (y >= NB_LIGNES - 1))
                {
                    //Debug.WriteLine("Y hors limites");
                    return false;
                }
                if (_cases[y, x]._case == CASE.POMME)
                {
                    //Debug.WriteLine("Trouvé pomme!, return true");
                    return true;
                }

                if (_cases[y, x]._case != CASE.VIDE)
                {
                    //Debug.WriteLine("Case non vide");
                    return false;
                }
            }
        }

        /// <summary>
        /// Tourne la direction vers la gauche
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        static private CASE TourneAGauche(CASE direction)
        {
            switch (direction)
            {
                case CASE.NORD: return CASE.OUEST;
                case CASE.SUD: return CASE.EST;
                case CASE.EST: return CASE.NORD;
                default: return CASE.SUD;
            }
        }


        /// <summary>
        /// Tourne la direction vers la droite
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        static private CASE TourneADroite(CASE direction)
        {
            switch (direction)
            {
                case CASE.NORD: return CASE.EST;
                case CASE.SUD: return CASE.OUEST;
                case CASE.EST: return CASE.SUD;
                default: return CASE.NORD;
            }
        }

        private CASE TourneADroiteOuAGauche(CASE direction, int x, int y)
        {
            switch (direction)
            {
                case CASE.EST:
                case CASE.OUEST:
                    // On se dirige vers l'est ou l'ouest: se diriger vers le nord ou le sud
                    {
                        if (y <= 0)
                        {
                            // Deja en haut: se diriger vers le sud
                            direction = CASE.SUD;
                        }
                        else
                            if (y >= NB_COLONNES - 1)
                        {
                            // Deja en bas: se diriger vers le nord
                            direction = CASE.NORD;
                        }
                        else
                        {
                            // Nord ou sud, une fois sur deux
                            direction = Probabilite(0.5f) ? CASE.NORD : CASE.SUD;
                        }
                    }
                    break;

                case CASE.NORD:
                case CASE.SUD:
                    // On se dirige vers le nord ou le sud: se diriger vers l'est ou l'ouest
                    {
                        if (x <= 0)
                        {
                            // Deja a gauche: se diriger vers l'est
                            direction = CASE.EST;
                        }
                        else
                            if (x >= NB_LIGNES - 1)
                        {
                            // Deja à droite: se diriger vers l'ouest
                            direction = CASE.OUEST;

                        }
                        else
                        {
                            // Est ou ouest, une fois sur deux
                            direction = Probabilite(0.5f) ? CASE.EST : CASE.OUEST;
                        }
                    }
                    break;
            }

            return direction;
        }

        private (int, int) GetDeplacement(CASE direction)
        {
            int dx, dy;

            switch (direction)
            {
                case CASE.NORD:
                    dx = 0;
                    dy = -1;
                    break;
                case CASE.SUD:
                    dx = 0;
                    dy = 1;
                    break;
                case CASE.EST:
                    dx = 1;
                    dy = 0;
                    break;
                default:
                    dx = -1;
                    dy = 0;
                    break;
            }

            return (dx, dy);
        }



        
        private void Perdu()
        {
            _mode = MODE_JEU.PERDU;
            _compteurPerdu = 10;
            _clignotePerdu = true;
        }

        private void PommeMangee()
        {
            // Augmenter le score
            _longueurSnake++;

            // Nouvelle pomme
            bool ajouté = false;
            while (!ajouté)
            {
                int x = random.Next(NB_COLONNES);
                int y = random.Next(NB_LIGNES);
                if (_cases[y, x]._case == CASE.VIDE)
                {
                    _cases[y, x]._case = CASE.POMME;
                    _cases[y, x].changeCouleur = FloatRandom(0, 1.0f);
                    ajouté = true;
                }
            }
        }
    }
}
