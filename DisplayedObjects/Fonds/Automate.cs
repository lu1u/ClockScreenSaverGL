using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

// Inspiré de "Science Etonnante" : Lenia
namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Automate : Fond
    {
        #region Parametres
        private const string CAT = "Automate";
        private CategorieConfiguration c;
        private float REPETITION_TEXTURE;
        private int NB_LIGNES_A_LA_FOIS;
        private int LARGEUR, HAUTEUR;
        private int SEUIL_ETAT;
        private float LOOK_AT_X;
        private float LOOK_AT_Y;
        private float LOOK_AT_Z;
        private float VITESSE_ANGLE;
        private int NB_ETATS;
        #endregion

        private int[,] _cellules, _cellulesCalcul;
        private int _ligneEnCours = 0;
        private Texture _textureBitmap;
        private bool _changerTexture;
        private float _angle;

        public Automate(OpenGL gl) : base(gl)
        {

        }
        protected override void Init(OpenGL gl)
        {
            base.Init(gl);
            GetConfiguration();

            // Tableau des particules
            _cellules = new int[LARGEUR, HAUTEUR];
            _cellulesCalcul = new int[LARGEUR, HAUTEUR];
            for (int i = 0; i < LARGEUR; i++)
                for (int j = 0; j < HAUTEUR; j++)
                {
                    _cellules[i, j] = random.Next(0, NB_ETATS);
                    _cellulesCalcul[i, j] = _cellules[i, j];
                }

            _changerTexture = false;
            _ligneEnCours = HAUTEUR;
        }



        /// <summary>
        /// Chargement de la configuration
        /// </summary>
        /// <returns></returns>
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                LARGEUR = c.GetParametre("Largeur", 100);
                HAUTEUR = c.GetParametre("Hauteur", 100);
                SEUIL_ETAT = c.GetParametre("Seuil état", 1, (a) => { SEUIL_ETAT = Convert.ToInt32(a); });

                NB_LIGNES_A_LA_FOIS = c.GetParametre("Nb Lignes a la fois", 4, (a) => { NB_LIGNES_A_LA_FOIS = Convert.ToInt32(a); });
                REPETITION_TEXTURE = c.GetParametre("Répétition texture", 4f, (a) => { REPETITION_TEXTURE = (float)Convert.ToDouble(a); });
                VITESSE_ANGLE = c.GetParametre("Vitesse Angle", 2.0f, (a) => { VITESSE_ANGLE = (float)Convert.ToDouble(a); });
                NB_ETATS = c.GetParametre("Nb Etats", 16);
                LOOK_AT_X = c.GetParametre("LookAtX", 0.1f, (a) => { LOOK_AT_X = (float)Convert.ToDouble(a); });
                LOOK_AT_Y = c.GetParametre("LookAtY", 0.02f, (a) => { LOOK_AT_Y = (float)Convert.ToDouble(a); });
                LOOK_AT_Z = c.GetParametre("LookAtZ", -0.3f, (a) => { LOOK_AT_Z = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /// <summary>
        /// Affichage
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
            gl.PushAttrib(SharpGL.Enumerations.AttributeMask.All);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_MIRRORED_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_MIRRORED_REPEAT);

            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);
            if (_changerTexture)
            {
                CreerNouvelleTexture(gl, couleur);
                _changerTexture = false;
            }

            if (_textureBitmap != null)
            {
                gl.LookAt(LOOK_AT_X, LOOK_AT_Y, LOOK_AT_Z, 0, -0.1f, 0, 0, -1, 0);
                gl.Rotate(0, 0, _angle);

                _textureBitmap.Bind(gl);
                using (new GLBegin(gl, OpenGL.GL_QUADS))
                {
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(-1, 1f);
                    gl.TexCoord(0.0f, REPETITION_TEXTURE); gl.Vertex(-1, -1);
                    gl.TexCoord(REPETITION_TEXTURE, REPETITION_TEXTURE); gl.Vertex(1, -1);
                    gl.TexCoord(REPETITION_TEXTURE, 0.0f); gl.Vertex(1, 1);
                }
            }

            gl.PopAttrib();
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Creation de la nouvelle texture OpenGL pour representer les cellules
        /// (appelé chaque fois qu'on a fait le calcul sur tout le tableau)
        /// </summary>
        /// <param name="gl"></param>
        private void CreerNouvelleTexture(OpenGL gl, Color col)
        {
            if (_textureBitmap != null)
            {
                uint[] textures = { _textureBitmap.TextureName };
                gl.DeleteTextures(1, textures);
                _textureBitmap.Destroy(gl);
            }

            Bitmap bmp = UpdateFrame(col);
            _textureBitmap = new Texture();
            _textureBitmap.Create(gl, bmp);
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            _angle += maintenant.intervalleDepuisDerniereFrame * VITESSE_ANGLE;

            for (int x = 0; x < NB_LIGNES_A_LA_FOIS; x++)
            {
                if (_ligneEnCours < HAUTEUR - 1)
                    // Ligne suivante
                    _ligneEnCours++;
                else
                {
                    for (int i = 0; i < LARGEUR; i++)
                        for (int j = 0; j < HAUTEUR; j++)
                            _cellules[i, j] = _cellulesCalcul[i, j];

                    _changerTexture = true;
                    _ligneEnCours = 0;
                }

                // Calcul, une ligne a la fois
                for (int i = 0; i < LARGEUR; i++)
                {
                    int prochainEtat = ProchainEtat(_cellulesCalcul[i, _ligneEnCours]);
                    if (NbVoisines(i, _ligneEnCours, prochainEtat) >= SEUIL_ETAT)
                        _cellulesCalcul[i, _ligneEnCours] = prochainEtat;
                }
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        private int ProchainEtat(int etat)
        {
            if (etat < NB_ETATS - 1)
                return etat + 1;
            else
                return 0;
        }

        /// <summary>
        /// Compte le nombre de voisines de la cellule qui sont dans l'état donné
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="etat"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int NbVoisines(int x, int y, int etat)
        {
            int res = 0;
            // Ligne au dessus
            if (Case(x - 1, y - 1) == etat)
                res++;

            if (Case(x, y - 1) == etat)
                res++;

            if (Case(x + 1, y - 1) == etat)
                res++;

            // Ligne en dessous
            if (Case(x - 1, y + 1) == etat)
                res++;

            if (Case(x, y + 1) == etat)
                res++;

            if (Case(x + 1, y + 1) == etat)
                res++;

            // Meme ligne (on ne regarde pas le centre
            if (Case(x - 1, y) == etat)
                res++;

            if (Case(x + 1, y) == etat)
                res++;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Case(int x, int y)
        {
            return _cellules[LimiteX(x), LimiteY(y)];
        }

        /// <summary>
        /// Rempli les pixels de la bitmap
        /// </summary>
        /// <param name="bmpd"></param>
        protected unsafe Bitmap UpdateFrame(Color col)
        {
            // Montrer le filtre (DEBUG)
            //for (int i = 0; i < LARGEUR_FILTRE; i++)
            //    for (int j = 0; j < HAUTEUR_FILTRE; j++)
            //        _cellules[i+10, j+10] = _filtre[i, j];            

            Bitmap bmp = new Bitmap(LARGEUR, HAUTEUR, PixelFormat.Format32bppRgb);
            BitmapData bmpd = bmp.LockBits(new Rectangle(0, 0, LARGEUR, HAUTEUR), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            double largeur = bmpd.Width;
            float[] palette = new float[NB_ETATS];
            for (int i = 0; i < NB_ETATS; i++)
                palette[i] = i / (float)NB_ETATS;

            for (int y = 0; y < bmpd.Height; y++)
            {
                int* pixels = (int*)(bmpd.Scan0 + (y * bmpd.Stride));

                for (int x = 0; x < largeur; x++)
                {
                    Color c = Couleur.GetColorWithHueChange(col, palette[_cellules[x, y]]);
                    //Color Couleur = GetColorWithHueChange(col, _particules[x, y]);
                    //*pixels++ = (Couleur.R << 16) | ((Couleur.G) << 8) | Couleur.B;
                    //int couleur = (int)(_cellules[x, y] * 255.0f / NB_ETATS);
                    *pixels++ = (c.R << 16) | ((c.G) << 8) | c.B;
                }
            }



            bmp.UnlockBits(bmpd);
            return bmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int LimiteX(int x)
        {
            while (x < 0)
                x += LARGEUR;
            while (x >= LARGEUR)
                x -= LARGEUR;
            return x;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int LimiteY(int y)
        {
            while (y < 0)
                y += HAUTEUR;
            while (y >= HAUTEUR)
                y -= HAUTEUR;
            return y;
        }
    }
}
