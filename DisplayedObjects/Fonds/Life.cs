/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 22/12/2014
 * Heure: 14:32
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Drawing;

using System.Runtime.CompilerServices;
using ClockScreenSaverGL.Config;

using SharpGL;
using SharpGL.SceneGraph.Assets;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    /// <summary>
    /// Description of Life.
    /// </summary>
    public class Life : Fond
    {
        #region Parametres
        public const string CAT = "JeuDeLaVie";
        protected CategorieConfiguration c;

        protected int LARGEUR;
        protected int HAUTEUR;
        protected float COULEUR_NAISSANCE;
        protected float COULEUR_NORMAL;
        protected int SKIP;
        protected float VITESSE_ANGLE;
        protected float LOOK_AT_X;
        protected float LOOK_AT_Y;
        protected float LOOK_AT_Z;
        #endregion
        protected byte[,] cellules;
        protected byte[,] cellulestemp;

        protected const byte MORT = 0;
        protected const byte NORMAL = 1;
        protected const byte NAISSANCE = 2;
        private int _colonneMin, _colonneMax, _largeurCalcul;
        private float _angle = 0;
        protected Texture textureCellule = new Texture();
        public Life(OpenGL gl) : base(gl)
        {
            getConfiguration();
            _largeurCalcul = LARGEUR / SKIP;
            _colonneMin = -_largeurCalcul;
            _colonneMax = _colonneMin + _largeurCalcul;
            cellules = new byte[LARGEUR, HAUTEUR];
            cellulestemp = new byte[LARGEUR, HAUTEUR];
            InitCellules();
            textureCellule.Create(gl, c.getParametre("texture particule", Configuration.getImagePath("particule.png")));
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                LARGEUR = c.getParametre("Largeur", 60);
                HAUTEUR = c.getParametre("Hauteur", 50);
                COULEUR_NAISSANCE = c.getParametre("CouleurNaissance", 0.3f, (a) => { COULEUR_NAISSANCE = (float)Convert.ToDouble(a); });
                COULEUR_NORMAL = c.getParametre("CouleurNormale", 0.4f, (a) => { COULEUR_NORMAL = (float)Convert.ToDouble(a); });
                SKIP = c.getParametre("Skip", 2, (a) => { SKIP = Convert.ToInt32(a); });
                VITESSE_ANGLE = c.getParametre("Vitesse Angle", 2.0f);
                LOOK_AT_X = c.getParametre("LookAtX", 0.1f, (a) => { LOOK_AT_X = (float)Convert.ToDouble(a); });
                LOOK_AT_Y = c.getParametre("LookAtY", 0.02f, (a) => { LOOK_AT_Y = (float)Convert.ToDouble(a); });
                LOOK_AT_Z = c.getParametre("LookAtZ", -0.3f, (a) => { LOOK_AT_Z = (float)Convert.ToDouble(a); });
            }
            return c;
        }


        /// <summary>
        /// Etat initial des cellules
        /// </summary>
        private void InitCellules()
        {
            Random r = new Random();
            for (int x = 0; x < LARGEUR; x++)
                for (int y = 0; y < HAUTEUR; y++)
                    cellules[x, y] = r.Next(11) > 5 ? MORT : NAISSANCE;
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
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
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_BLEND);

            Color Naissance = getCouleurOpaqueAvecAlpha(couleur, Convert.ToByte(COULEUR_NAISSANCE * 255));
            byte[] cNaissance = { Naissance.R, Naissance.G, Naissance.B };
            Color Normal = getCouleurOpaqueAvecAlpha(couleur, Convert.ToByte(COULEUR_NORMAL * 255));
            byte[] cNormal = { Normal.R, Normal.G, Normal.B };

            gl.LookAt(LOOK_AT_X, LOOK_AT_Y, LOOK_AT_Z, 0, -0.1f, 0, 0, -1, 0);
            gl.Scale(1.7f / LARGEUR, 1.7f / HAUTEUR, 1);
            gl.Rotate(0, 0, _angle);
            byte ancienType = MORT;

            textureCellule.Bind(gl);
            gl.Translate(-LARGEUR / 2, -HAUTEUR / 2, 0);
            gl.Begin(OpenGL.GL_QUADS);

            for (int x = 0; x < LARGEUR; x++)
                for (int y = 0; y < HAUTEUR; y++)
                {
                    if (cellules[x, y] != MORT)
                    {
                        if (cellules[x, y] != ancienType)
                        {
                            if (cellules[x, y] == NAISSANCE)
                                gl.Color(cNaissance[0], cNaissance[1], cNaissance[2]);
                            else
                                gl.Color(cNormal[0], cNormal[1], cNormal[2]);
                            ancienType = cellules[x, y];
                        }

                        gl.TexCoord(0.0f, 0.0f); gl.Vertex(x, y + 1);
                        gl.TexCoord(0.0f, 1.0f); gl.Vertex(x, y);
                        gl.TexCoord(1.0f, 1.0f); gl.Vertex(x + 1, y);
                        gl.TexCoord(1.0f, 0.0f); gl.Vertex(x + 1, y + 1);
                    }

                }
            gl.End();

            Console.getInstance(gl).AddLigne(Color.Green, "Largeur " + LARGEUR + "x Hauteur " + HAUTEUR);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Calcul des changements de cellules
        /// On ne calcule a chaque fois qu'une seule partie du tableau (voir parametre Skip)
        /// pour mieux repartir la charge entre les frames
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

            _angle += maintenant.intervalleDepuisDerniereFrame * VITESSE_ANGLE;
            int xMin, xMax;
            DecoupeEnBandes(out xMin, out xMax);

            int NbVoisines;
            int XM1, XP1, YM1, YP1;

            for (int x = xMin; x < xMax; x++)
            {
                XM1 = LimiteTore(x - 1, 0, LARGEUR);
                XP1 = LimiteTore(x + 1, 0, LARGEUR);

                for (int y = 0; y < HAUTEUR; y++)
                {
                    YM1 = LimiteTore(y - 1, 0, HAUTEUR);
                    YP1 = LimiteTore(y + 1, 0, HAUTEUR);

                    NbVoisines = GetNbVoisines(x, y, XM1, XP1, YM1, YP1);
                    switch (NbVoisines)
                    {
                        case 3:
                            cellulestemp[x, y] = (cellules[x, y] == MORT) ? NAISSANCE : NORMAL;
                            break;

                        case 2:
                            cellulestemp[x, y] = cellules[x, y] == MORT ? MORT : NORMAL;
                            break;

                        default:
                            cellulestemp[x, y] = MORT;
                            break;
                    }
                }
            }
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        /// <summary>
        /// Comme on ne calcule pas tout a chaque frame, on partage le calcule
        /// </summary>
        /// <param name="xMin"></param>
        /// <param name="xMax"></param>
        private void DecoupeEnBandes(out int xMin, out int xMax)
        {
            if (_colonneMax < LARGEUR)
            {
                _colonneMin += _largeurCalcul;
                _colonneMax += _largeurCalcul;
                if (_colonneMax > LARGEUR)
                    _colonneMax = LARGEUR;
            }
            else
            {
                _colonneMin = 0;
                _colonneMax = _largeurCalcul;

                // On a tout calcule, echanger les tableaux
                byte[,] t = cellules;
                cellules = cellulestemp;
                cellulestemp = t;
            }

            xMin = _colonneMin;
            xMax = _colonneMax;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNbVoisines(int x, int y, int XM1, int XP1, int YM1, int YP1)
        {
            return Voisine(XM1, YM1) + Voisine(x, YM1) + Voisine(XP1, YM1)
                + Voisine(XM1, y) + Voisine(XP1, y)
                + Voisine(XM1, YP1) + Voisine(x, YP1) + Voisine(XP1, YP1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LimiteTore(int val, int Min, int Max)
        {
            if (val < Min)
                return Max - 1;

            if (val >= Max)
                return Min;

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Voisine(int x, int y)
        {
            if (cellules[x, y] == MORT)
                return 0;
            else
                return 1;
        }

    }
}
