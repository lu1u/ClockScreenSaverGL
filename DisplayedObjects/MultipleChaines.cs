using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class MultiplesChaines : Fond

    {
        #region Parametres
        private const string CAT = "MultiChaines";
        protected CategorieConfiguration c;
        private float ANGLE_ECRANS;
        private int NB_ECRANS_LARGEUR;
        private int NB_ECRANS_HAUTEUR;
        private float MARGE_ECRAN;
        private float HAUTEUR_ECRAN;
        private float LARGEUR_ECRAN;
        private float RAYON_RONDE;
        private int NB_CHAINES;
        private int LARGEUR_TEXTURE;
        private int HAUTEUR_TEXTURE;
        private float FOV;
        private float VITESSE_PANORAMIQUE;
        #endregion

        private readonly int[,] Chaines;
        private readonly Fond[] _objets;
        private float angle = 0;
        private readonly TimerIsole timerNouvelleChaine = new TimerIsole(10000);
        private readonly TimerIsole timerEcranChangeChaine = new TimerIsole(1000);
        private readonly uint[] textures;
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_ECRANS_LARGEUR = c.GetParametre("Nb ecrans largeur", 6);
                NB_ECRANS_HAUTEUR = c.GetParametre("Nb ecrans Hauteur", 3);
                ANGLE_ECRANS = c.GetParametre("Angle ecrans", 0.5f);
                MARGE_ECRAN = c.GetParametre("Marge ecrans", 0.3f);
                HAUTEUR_ECRAN = c.GetParametre("Hauteur ecrans", 0.75f);
                LARGEUR_ECRAN = c.GetParametre("Largeur ecrans", 0.9f);
                RAYON_RONDE = c.GetParametre("Rayon courbe", 4.0f);
                NB_CHAINES = c.GetParametre("Nb chaines", 4);
                LARGEUR_TEXTURE = c.GetParametre("Largeur texture", 256);
                HAUTEUR_TEXTURE = c.GetParametre("Hauteur texture", 256);
                FOV = c.GetParametre("FOV", 75.0f);
                VITESSE_PANORAMIQUE = c.GetParametre("Vitesse panoramique", 0.5f);
            }
            return c;
        }
        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public MultiplesChaines(OpenGL gl) : base(gl)
        {
            GetConfiguration();

            Chaines = new int[NB_ECRANS_LARGEUR, NB_ECRANS_HAUTEUR];
            for (int i = 0; i < NB_ECRANS_LARGEUR; i++)
                for (int j = 0; j < NB_ECRANS_HAUTEUR; j++)
                    Chaines[i, j] = random.Next(NB_CHAINES);

            _objets = new Fond[NB_CHAINES];
            textures = new uint[NB_CHAINES];


        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="gl"></param>
        /// <returns></returns>
        protected override void Init(OpenGL gl)
        {
            for (int i = 0; i < NB_CHAINES; i++)
            {
                _objets[i] = DisplayedObjectFactory.CreerFondAleatoire(gl, random);
                _objets[i].Initialisation(gl);
                textures[i] = CreateEmptyTexture(LARGEUR_TEXTURE, HAUTEUR_TEXTURE);
            }
        }
        ///////////////////////////////////////////////////////////////////////
        public override void Dispose()
        {
            for (int i = 0; i < NB_CHAINES; i++)
                DeleteEmptyTexture(textures[i]);

            foreach (DisplayedObject o in _objets)
                o.Dispose();
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Initialisation d'un objet a afficher dans une des teles
        /// </summary>
        /// <param name="gl"></param>
        /// <returns></returns>


        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Affichage OpenGL
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
            if (timerNouvelleChaine.Ecoule())
            {
                int noChaine = random.Next(NB_CHAINES);
                _objets[noChaine].Dispose();
                _objets[noChaine] = DisplayedObjectFactory.CreerFondAleatoire(gl, random);
                _objets[noChaine].Initialisation(gl);
            }

            RenderToTextures(gl, maintenant, tailleEcran, couleur);
            gl.ClearColor(couleur.R / 1024.0f, couleur.G / 1024.0f, couleur.B / 1024.0f, 1);                // Set The Clear Color To Medium Blue
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);      // Clear The Screen And Depth Buffer
            //
            gl.Enable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_DEPTH_TEST);

            gl.MatrixMode(OpenGL.GL_PROJECTION);                        // Select The Projection Matrix
            gl.LoadIdentity();                                   // Reset The Projection Matrix

            // Calculate The Aspect Ratio Of The Window
            gl.Perspective(FOV, tailleEcran.Width / (float)tailleEcran.Height, 1.0f, RAYON_RONDE * 2);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);                         // Select The Modelview Matrix
            gl.LoadIdentity();                                  // Reset The Modelview Matri

            gl.LookAt(0, 0f, -RAYON_RONDE * 0.3f, 0, 0, 0, 0, 1, 0);
            gl.Rotate(0, (float)Math.Sin(angle) * 6.0f, 0);

            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1 };
            gl.Color(col);

            float ANGLE = ANGLE_ECRANS / NB_ECRANS_LARGEUR;
            gl.Rotate(0, -(ANGLE_ECRANS * 1.2f) / 2.0f, 0);
            for (int x = 0; x < NB_ECRANS_LARGEUR; x++)
            {
                gl.Rotate(0, ANGLE, 0);
                gl.PushMatrix();
                gl.Translate(0, -(HAUTEUR_ECRAN + MARGE_ECRAN) * NB_ECRANS_HAUTEUR * 0.8f, 0);
                for (int y = 0; y < NB_ECRANS_HAUTEUR; y++)
                {
                    // Front Face
                    gl.Disable(OpenGL.GL_TEXTURE_2D);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Normal(0.0f, 0.0f, 1.0f);                  // Normal Pointing Towards Viewer
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(-LARGEUR_ECRAN * 1.05f, -HAUTEUR_ECRAN * 1.05f, RAYON_RONDE * 1.01f);  // Point 1 (Front)
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(LARGEUR_ECRAN * 1.05f, -HAUTEUR_ECRAN * 1.05f, RAYON_RONDE * 1.01f);  // Point 2 (Front)
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(LARGEUR_ECRAN * 1.05f, HAUTEUR_ECRAN * 1.05f, RAYON_RONDE * 1.01f);  // Point 3 (Front)
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(-LARGEUR_ECRAN * 1.05f, HAUTEUR_ECRAN * 1.05f, RAYON_RONDE * 1.01f);  // Point 4 (Front)
                    gl.End();

                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[Chaines[x, y]]);
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Normal(0.0f, 0.0f, 1.0f);                  // Normal Pointing Towards Viewer
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(-LARGEUR_ECRAN, -HAUTEUR_ECRAN, RAYON_RONDE);  // Point 1 (Front)
                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(LARGEUR_ECRAN, -HAUTEUR_ECRAN, RAYON_RONDE);  // Point 2 (Front)
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(LARGEUR_ECRAN, HAUTEUR_ECRAN, RAYON_RONDE);  // Point 3 (Front)
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(-LARGEUR_ECRAN, HAUTEUR_ECRAN, RAYON_RONDE);  // Point 4 (Front)
                    gl.End();

                    gl.Translate(0, (HAUTEUR_ECRAN + MARGE_ECRAN) * 2.0f, 0);
                }

                gl.PopMatrix();
            }

            Console console = Console.GetInstance(gl);
            foreach (DisplayedObject o in _objets)
                console.AddLigne(Color.Green, o.GetType().Name);

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }


        public void RenderToTextures(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
            Rectangle r = new Rectangle(0, 0, LARGEUR_TEXTURE, HAUTEUR_TEXTURE);
            gl.Viewport(0, 0, r.Width, r.Height);                    // Set Our Viewport (Match Texture Size)

            for (int i = 0; i < NB_CHAINES; i++)
            {
                gl.PushAttrib(OpenGL.GL_ENABLE_BIT | OpenGL.GL_CURRENT_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_FOG_BIT | OpenGL.GL_COLOR_BUFFER_BIT);

                _objets[i].ClearBackGround(gl, couleur);
                _objets[i].AfficheOpenGL(gl, maintenant, r, couleur);

                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);
                gl.CopyTexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGB16, 0, 0, LARGEUR_TEXTURE, HAUTEUR_TEXTURE, 0);

                gl.PopAttrib();
            }

            gl.Viewport(0, 0, tailleEcran.Width, tailleEcran.Height);
        }


        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            base.Deplace(maintenant, tailleEcran);
            angle += maintenant.intervalleDepuisDerniereFrame * VITESSE_PANORAMIQUE;
            foreach (DisplayedObject o in _objets)
                o.Deplace(maintenant, tailleEcran);

            if (timerEcranChangeChaine.Ecoule())
            {
                int x = random.Next(NB_ECRANS_LARGEUR);
                int y = random.Next(NB_ECRANS_HAUTEUR);

                Chaines[x, y] = (Chaines[x, y] + 1) % NB_CHAINES;
            }
        }
    }
}
