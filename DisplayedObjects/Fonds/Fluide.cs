using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Utils;
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
    internal class Fluide : Fond
    {
        #region Parametres
        private const string CAT = "Fluide";
        private CategorieConfiguration c;
        private float REPETITION_TEXTURE;
        private int NB_LIGNES_A_LA_FOIS;
        private int LARGEUR, HAUTEUR;
        private int LARGEUR_FILTRE, HAUTEUR_FILTRE;
        private float LARGEUR_CLOCHE;
        private float CENTRE_CLOCHE;
        private float LOOK_AT_X;
        private float LOOK_AT_Y;
        private float LOOK_AT_Z;
        private float VITESSE_ANGLE;
        private float VITESSE_PROGRESSION_CLOCHE;
        private float MIN_PROGRESSION_CLOCHE;
        #endregion

        private float _valeursTotalesFiltre;
        private float[,] _cellules, _cellulesCalcul;
        private float[,] _filtre;
        private int _ligneEnCours = 0;
        private Texture _textureBitmap;
        private bool _changerTexture;
        private float _angle;

        public Fluide(OpenGL gl) : base(gl)
        {

        }
        protected override void Init(OpenGL gl)
        {
            base.Init(gl);
            GetConfiguration();

            // Filtre
            ChargeFiltre();

            // Tableau des particules
            _cellules = new float[LARGEUR, HAUTEUR];
            _cellulesCalcul = new float[LARGEUR, HAUTEUR];
            for (int i = 0; i < LARGEUR; i++)
                for (int j = 0; j < HAUTEUR; j++)
                {
                    _cellules[i, j] = FloatRandom(0.0f, 1.0f);
                    _cellulesCalcul[i, j] = _cellules[i, j];
                }

            _changerTexture = false;
            _ligneEnCours = HAUTEUR;
        }

        /// <summary>
        /// Creer un tableau pour le filtre, tableau a 2 dimensions
        /// valeurs de 0 à 1
        /// </summary>
        private void ChargeFiltre()
        {
            _filtre = new float[LARGEUR_FILTRE, HAUTEUR_FILTRE];
            _valeursTotalesFiltre = 0;

            int centreX = (LARGEUR_FILTRE + 1) / 2;
            int centreY = (HAUTEUR_FILTRE + 1) / 2;
            float distanceMax = Math.Min(LARGEUR_FILTRE, HAUTEUR) / ((float)Math.PI);

            for (int i = 0; i < LARGEUR_FILTRE; i++)
                for (int j = 0; j < HAUTEUR_FILTRE; j++)
                {
                    float distance = MathUtils.Distance(centreX, centreY, i, j) / distanceMax;
                    //if (distance > distanceMax)
                    //    _filtre[i, j] = 0;
                    //else
                    _filtre[i, j] = 1.0f - (float)Math.Cos(distance);
                    //distance = Math.Abs(distance - 1.0f);
                    //_filtre[i, j] = 1.0f - distance;
                    //
                    //if (_filtre[i, j] < 0)
                    //    _filtre[i, j] = 0;
                    //else
                    //if (_filtre[i, j] > 1.0f)
                    //    _filtre[i, j] = 1.0f;
                    _valeursTotalesFiltre += _filtre[i, j];
                }
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
                LARGEUR_FILTRE = c.GetParametre("Largeur filtre", 20);
                HAUTEUR_FILTRE = c.GetParametre("Hauteur filtre", 20);
                NB_LIGNES_A_LA_FOIS = c.GetParametre("Nb Lignes a la fois", 4, (a) => { NB_LIGNES_A_LA_FOIS = Convert.ToInt32(a); });
                CENTRE_CLOCHE = c.GetParametre("Centre cloche croissance", 0.5f, (a) => { CENTRE_CLOCHE = (float)Convert.ToDouble(a); });
                LARGEUR_CLOCHE = c.GetParametre("Largeur cloche croissance", 10f, (a) => { LARGEUR_CLOCHE = (float)Convert.ToDouble(a); });
                REPETITION_TEXTURE = c.GetParametre("Répétition texture", 4f, (a) => { REPETITION_TEXTURE = (float)Convert.ToDouble(a); });
                VITESSE_PROGRESSION_CLOCHE = c.GetParametre("Vitesse progression cloche", 1f, (a) => { VITESSE_PROGRESSION_CLOCHE = (float)Convert.ToDouble(a); });
                MIN_PROGRESSION_CLOCHE = c.GetParametre("Minimum progression cloche", 0.2f, (a) => { MIN_PROGRESSION_CLOCHE = (float)Convert.ToDouble(a); });
                VITESSE_ANGLE = c.GetParametre("Vitesse Angle", 2.0f, (a) => { VITESSE_ANGLE = (float)Convert.ToDouble(a); });

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
                    // Fin d'une passe de calcul sur toutes les cellules
                    CENTRE_CLOCHE += VITESSE_PROGRESSION_CLOCHE;
                    if (CENTRE_CLOCHE > (1.0f - MIN_PROGRESSION_CLOCHE))
                    {
                        CENTRE_CLOCHE = 1.0f - MIN_PROGRESSION_CLOCHE;
                        VITESSE_PROGRESSION_CLOCHE = -Math.Abs(VITESSE_PROGRESSION_CLOCHE);
                    }
                    if (CENTRE_CLOCHE < MIN_PROGRESSION_CLOCHE)
                    {
                        CENTRE_CLOCHE = MIN_PROGRESSION_CLOCHE;
                        VITESSE_PROGRESSION_CLOCHE = Math.Abs(VITESSE_PROGRESSION_CLOCHE);
                    }

                    for (int i = 0; i < LARGEUR; i++)
                        for (int j = 0; j < HAUTEUR; j++)
                            _cellules[i, j] = _cellulesCalcul[i, j];

                    _changerTexture = true;
                    _ligneEnCours = 0;
                }

                // Calcul, une ligne a la fois
                for (int i = 0; i < LARGEUR; i++)
                {
                    float score = CalculeScore(i, _ligneEnCours);
                    _cellulesCalcul[i, _ligneEnCours] = EvolueCase(_cellules[i, _ligneEnCours], score, maintenant.intervalleDepuisDerniereFrame);
                }
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        /// <summary>
        /// Calcule le nouvel état de la particule
        /// </summary>
        /// <param name="particule"></param>
        /// <param name="score"></param>
        /// <param name="intervalleFrame"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float EvolueCase(float particule, float score, float intervalleFrame)
        {
            float tauxCroissance = MathUtils.CourbeEnCloche(score, LARGEUR_CLOCHE, CENTRE_CLOCHE);
            particule += tauxCroissance * intervalleFrame;

            if (particule < 0.0f)
                particule = 0.0f;
            else
                if (particule > 1.0f)
                particule = 1.0f;

            return particule;
        }


        // Calcule le "score" d'une case: on prend toutes les cases adjacentes (sur la taille du filtre)
        // et on fait une moyenne ponderee
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float CalculeScore(int i, int j)
        {
            int debutX = i - LARGEUR_FILTRE / 2;
            int debutY = j - HAUTEUR_FILTRE / 2;
            float score = 0;

            // Moyenne ponderee des cases autour de la case actuelle, en fonction du filtre
            for (int x = 0; x < LARGEUR_FILTRE; x++)
                for (int y = 0; y < HAUTEUR_FILTRE; y++)
                {
                    score += _cellules[LimiteX(x + debutX), LimiteY(y + debutY)] * _filtre[x, y];
                }

            // Normaliser la moyenne
            score /= _valeursTotalesFiltre;
            return score;
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

            for (int y = 0; y < bmpd.Height; y++)
            {
                int* pixels = (int*)(bmpd.Scan0 + (y * bmpd.Stride));

                for (int x = 0; x < largeur; x++)
                {
                    //Color Couleur = GetColorWithHueChange(col, _particules[x, y]);
                    //*pixels++ = (Couleur.R << 16) | ((Couleur.G) << 8) | Couleur.B;
                    Color c = Couleur.GetColorWithHueChange(col, _cellules[x, y]);
                    //int couleur = (int)(_cellules[x, y] * 255.0f);
                    *pixels++ = (c.R << 16) | ((c.G) << 8) | c.B;
                    //*pixels++ = (couleur << 16) | ((couleur) << 8) | couleur;
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
