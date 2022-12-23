using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;
using T_CASE = System.Int16;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class Tetris : Fond
    {
        #region Configuration
        private const string CAT = "Tetris";
        private CategorieConfiguration c;
        private int NB_LIGNES, NB_COLONNES;
        private float LARGEUR_CASE, HAUTEUR_CASE;
        private int DELAI_TIMER_JEUX;
        #endregion

        private const float MIN_VIEWPORT_X = 0;
        private const float MIN_VIEWPORT_Y = 0;
        private const float MAX_VIEWPORT_X = 1.0f;
        private const float MAX_VIEWPORT_Y = 1.0f;
        private const float MARGE_JEU = 0.005f;

        private const float LARGEUR_JEU = (MAX_VIEWPORT_X - MIN_VIEWPORT_X) * 0.4f;
        private const float HAUTEUR_JEU = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y) * 0.95f;
        private const float MIN_JEU_X = (MAX_VIEWPORT_X - MIN_VIEWPORT_X - LARGEUR_JEU) / 2.0f;
        private const float MAX_JEU_X = (MAX_VIEWPORT_X - MIN_VIEWPORT_X + LARGEUR_JEU) / 2.0f;
        private const float MIN_JEU_Y = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y - HAUTEUR_JEU) / 2.0f;
        private const float MAX_JEU_Y = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y + HAUTEUR_JEU) / 2.0f;
        private readonly Plateau _plateau;
        private PieceTetris _piece;
        private TimerIsole _timerJeu;

        private enum MODE_JEU { NORMAL, LIGNES_COMPLETES, PARTIE_PERDUE };

        private MODE_JEU _modeJeu;

        private readonly TextureAsynchrone _textureBrique, _textureFond;
        private int _colonneCible, _rotationCible;
        private int _nbClignotes;
        private bool _clignote;

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_LIGNES = c.GetParametre("Nb Lignes", 30);
                NB_COLONNES = c.GetParametre("Nb Colonnes", 13);
                DELAI_TIMER_JEUX = c.GetParametre("Timer jeux", 300, a => { DELAI_TIMER_JEUX = Convert.ToInt32(a); _timerJeu = new TimerIsole(DELAI_TIMER_JEUX); });
                Plateau.COEFF_LIGNES_VIDES = c.GetParametre("Coeff Lignes vides", 30.0f, a => Plateau.COEFF_LIGNES_VIDES = Convert.ToInt32(a));
                Plateau.COEFF_TROUS = c.GetParametre("Coeff Trous", -100.0f, a => Plateau.COEFF_TROUS = Convert.ToInt32(a));
                Plateau.COEFF_LIGNE_HAUTE = c.GetParametre("Coeff Ligne haute", 20.0f, a => Plateau.COEFF_LIGNE_HAUTE = Convert.ToInt32(a));
                LARGEUR_CASE = LARGEUR_JEU / NB_COLONNES;
                HAUTEUR_CASE = HAUTEUR_JEU / NB_LIGNES;
            }
            return c;
        }

        public Tetris(OpenGL gl) : base(gl)
        {
            c = GetConfiguration();
            _textureBrique = new TextureAsynchrone(gl, Configuration.GetImagePath("brique.png"));
            _textureBrique.Init();
            _textureFond = new TextureAsynchrone(gl, Configuration.GetImagePath("tetris.png"));
            _textureFond.Init();

            _timerJeu = new TimerIsole(DELAI_TIMER_JEUX);
            _plateau = new Plateau(NB_LIGNES, NB_COLONNES);

            NouvellePiece();
        }


        /// <summary>
        /// Effacer le fond de la fenetre
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            if (_textureFond.Pret)
            {
                gl.Color(c.R / 512.0f, c.G / 512.0f, c.B / 512.0f, 1.0f);
                gl.LoadIdentity();
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Disable(OpenGL.GL_BLEND);

                using (new Viewport2D(gl, MIN_VIEWPORT_X, MAX_VIEWPORT_Y, MAX_VIEWPORT_X, MIN_VIEWPORT_Y))
                {
                    gl.Color(c.R / 256.0f, c.G / 256.0f, c.B / 256.0f, 1.0f);
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    _textureFond.Texture.Bind(gl);

                    // Surface de jeu
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(MIN_VIEWPORT_X, MAX_VIEWPORT_Y);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(MAX_VIEWPORT_X, MAX_VIEWPORT_Y);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(MAX_VIEWPORT_X, MIN_VIEWPORT_Y);
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(MIN_VIEWPORT_X, MIN_VIEWPORT_Y);
                    gl.End();
                }
                return true;
            }
            else
            {
                gl.ClearColor(c.R / 512.0f, c.G / 512.0f, c.B / 512.0f, 1.0f);
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            }
            return true;
        }
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_BLEND);

            using (new Viewport2D(gl, MIN_VIEWPORT_X, MAX_VIEWPORT_Y, MAX_VIEWPORT_X, MIN_VIEWPORT_Y))
            {
                gl.Disable(OpenGL.GL_BLEND);

                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1.0f);
                gl.Begin(OpenGL.GL_QUADS);
                gl.Vertex(MIN_JEU_X - MARGE_JEU, MAX_JEU_Y + MARGE_JEU);
                gl.Vertex(MAX_JEU_X + MARGE_JEU, MAX_JEU_Y + MARGE_JEU);
                gl.Vertex(MAX_JEU_X + MARGE_JEU, MIN_JEU_Y - MARGE_JEU);
                gl.Vertex(MIN_JEU_X - MARGE_JEU, MIN_JEU_Y - MARGE_JEU);
                gl.End();

                // Plateau de jeu noir
                gl.Color(0, 0, 0, 1.0f);
                gl.Begin(OpenGL.GL_QUADS);
                gl.Vertex(MIN_JEU_X, MAX_JEU_Y);
                gl.Vertex(MAX_JEU_X, MAX_JEU_Y);
                gl.Vertex(MAX_JEU_X, MIN_JEU_Y);
                gl.Vertex(MIN_JEU_X, MIN_JEU_Y);
                gl.End();

                if (_textureBrique.Pret)
                {
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    _textureBrique.Texture.Bind(gl);

                    // Plateau
                    bool clignoter = _modeJeu == MODE_JEU.LIGNES_COMPLETES && _clignote;
                    _plateau.Affiche(gl, MIN_JEU_X, MIN_JEU_Y, LARGEUR_CASE, HAUTEUR_CASE, couleur, clignoter);

                    // Piece courante
                    _piece?.Affiche(gl, MIN_JEU_X, MIN_JEU_Y, LARGEUR_CASE, HAUTEUR_CASE, couleur);
                }
            }

            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            if (_timerJeu.Ecoule())
            {
                switch (_modeJeu)
                {
                    case MODE_JEU.PARTIE_PERDUE:
                        ModePartiePerdue();
                        break;

                    case MODE_JEU.LIGNES_COMPLETES:
                        ModeLignesCompletes();
                        break;

                    default:
                        ModeJeuNormal();
                        break;
                }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Mode "Partie perdue: reinitialiser le plateau
        /// </summary>
        private void ModePartiePerdue()
        {
            _plateau.Vide();
            SwitchToMode(MODE_JEU.NORMAL);

        }

        private void SwitchToMode(MODE_JEU mode)
        {
            _modeJeu = mode;
            Log.Instance.Verbose("Switch to " + mode);
            switch (mode)
            {
                case MODE_JEU.NORMAL:
                    _timerJeu = new TimerIsole(DELAI_TIMER_JEUX);
                    NouvellePiece();
                    break;

                case MODE_JEU.PARTIE_PERDUE:
                    _timerJeu = new TimerIsole(2000);
                    break;

                case MODE_JEU.LIGNES_COMPLETES:
                    _timerJeu = new TimerIsole(200);

                    _nbClignotes = 10;
                    _clignote = true;
                    break;
            }
        }

        /// <summary>
        /// Mode jeu normal: deplacer la piece
        /// </summary>
        private void ModeJeuNormal()
        {
            // Deplacer la piece vers la colonne cible
            if (_colonneCible > _piece.CaseX)
                _piece.CaseX++;
            else
            if (_colonneCible < _piece.CaseX)
                _piece.CaseX--;

            // Tourner la piece vers la rotation cible
            if (_rotationCible > _piece.Rotation)
                _piece.TourneADroite();
            else
            if (_rotationCible < _piece.Rotation)
                _piece.TourneAGauche();


            if (!_plateau.LigneVide(0))
            {
                Log.Instance.Verbose("Ligne du haut non vide");
                // Il y a des pieces sur la ligne du haut: partie perdue
                SwitchToMode(MODE_JEU.PARTIE_PERDUE);
            }
            else
            if (_plateau.Disponible(_piece, _piece.CaseX, _piece.CaseY + 1))
            {
                Log.Instance.Verbose("Place disponible pour descendre");
                // Descend la piece
                _piece.CaseY++;
            }
            else
            {
                // La piece ne peut plus descendre, la déposer ici
                Log.Instance.Verbose("Place non disponible");

                _plateau.DeposePiece(_piece);
                _piece = null;

                if (_plateau.LignesCompletes())
                    SwitchToMode(MODE_JEU.LIGNES_COMPLETES);
                else
                    NouvellePiece();
            }
        }

        /// <summary>
        /// Gestion du mode "Lignes completes": faire clignoter les lignes qui sont completes avant de les supprimer
        /// </summary>
        private void ModeLignesCompletes()
        {
            _clignote = !_clignote;
            _nbClignotes--;

            if (_nbClignotes <= 0)
            {
                _plateau.VideLignesCompletes();
                NouvellePiece();
                SwitchToMode(MODE_JEU.NORMAL);
            }
        }

        /// <summary>
        /// Creation d'une nouvelle piece de jeu qui tombe
        /// </summary>
        private void NouvellePiece()
        {
            Log.Instance.Verbose("Nouvelle piece");
            _piece = PieceTetris.CreerPiece((T_CASE)random.Next(PieceTetris.NB_PIECES));
            _piece.CaseX = (NB_COLONNES - _piece.NbColonnes) / 2;
            _piece.CaseY = 0;

            _plateau.CalculeCible(_piece, out _colonneCible, out _rotationCible);
        }
    }
}
