using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of Neige.
    /// </summary>
    public class Nuages2 : TroisD, IDisposable
    {
        #region Parametres
        public const string CAT = "Nuages2";
        protected CategorieConfiguration c;
        private float ALPHA;
        private int NB_NUAGES;
        private float TAILLE_NUAGE;
        private float ROULIS_MAX;
        private float VITESSE_ROULIS;
        private float VITESSE;
        private float VITESSE_LATERALE;
        private float COLOR_RATIO;
        private float HAUTEUR_VUE;
        #endregion
        private const float VIEWPORT_X = 12f;
        private const float VIEWPORT_Y = 5f;
        private const float VIEWPORT_Z = 5f;

        private class Nuage
        {
            public float x, y, z;
            public float tailleX, tailleY;
            public int texture;
        }
        private Nuage[] _nuages;
        private const int NB_TEXTURES = 6;  // Nombre de nuages differents dans la texture
        private readonly Texture _texture;
        private float angle = 0;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Nuages2(OpenGL gl) : base(gl, VIEWPORT_X, VIEWPORT_Y, VIEWPORT_Z, 100)
        {
            c = GetConfiguration();
            _texture = new Texture();
            _texture.Create(gl, c.GetParametre("Nuages", Configuration.GetImagePath("nuages.png")));
        }
        protected override void Init(OpenGL gl)
        {
            _nuages = null;
            c = GetConfiguration();
            Nuage[] nuages = new Nuage[NB_NUAGES]; // Fonction asynchrone, on garde _nuages a null tant que tout n'est pas initialisé

            // Initialiser les nuages
            for (int i = 0; i < NB_NUAGES; i++)
            {
                NouveauNuage(ref nuages[i]);
                // Au debut, on varie la distance des nuages
                nuages[i].z = FloatRandom(-VIEWPORT_Z, _zCamera);
            }

            Array.Sort(nuages, delegate (Nuage O1, Nuage O2)
            {
                if (O1.z > O2.z) return 1;
                if (O1.z < O2.z) return -1;
                return 0;
            });

            _nuages = nuages;
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                ALPHA = c.GetParametre("Alpha", (byte)1.0f);
                HAUTEUR_VUE = c.GetParametre("Hauteur vue", 5.0f, a => { HAUTEUR_VUE = (float)Convert.ToDouble(a); });
                NB_NUAGES = c.GetParametre("Nb Nuages", 200);
                TAILLE_NUAGE = c.GetParametre("Taille", 9);
                ROULIS_MAX = c.GetParametre("Roulis max", 3.0f, a => { ROULIS_MAX = (float)Convert.ToDouble(a); });
                VITESSE_ROULIS = c.GetParametre("Vitesse roulis", 0.1f, a => { VITESSE_ROULIS = (float)Convert.ToDouble(a); });
                VITESSE = c.GetParametre("Vitesse", 2.0f, a => { VITESSE = (float)Convert.ToDouble(a); });
                VITESSE_LATERALE = c.GetParametre("Vitesse laterale", 10.0f, a => { VITESSE_LATERALE = (float)Convert.ToDouble(a); });
                COLOR_RATIO = c.GetParametre("Ratio couleur", 150.0f, a => { COLOR_RATIO = (float)Convert.ToDouble(a); });
            }
            return c;
        }


        private void NouveauNuage(ref Nuage f)
        {
            if (f == null)
                f = new Nuage();

            f.x = VIEWPORT_X * FloatRandom(-6, 6);
            f.z = -VIEWPORT_Z;
            f.y = FloatRandom(-VIEWPORT_Y * 3, -1.5f);
            f.tailleX = TAILLE_NUAGE * FloatRandom(0.6f, 1.4f);
            f.tailleY = TAILLE_NUAGE * FloatRandom(0.6f, 1.4f);
            f.texture = random.Next(NB_TEXTURES);
        }

        public override bool ClearBackGround(OpenGL gl, Color couleur)
        {
            gl.ClearColor(couleur.R / 512.0f, couleur.G / 512.0f, couleur.B / 512.0f, 1);
            gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_COLOR_MATERIAL);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            using (new Viewport2D(gl, -1.0f, -1.0f, 1.0f, 1.0f))
            {
                gl.Begin(OpenGL.GL_QUAD_STRIP);
                {
                    gl.Color(1.0f, 1.0f, 1.0f); gl.Vertex(-1f, -1f); gl.Vertex(1f, -1f);
                    gl.Color(couleur.R / 512.0f, couleur.G / 512.0f, couleur.B / 512.0f, 1); gl.Vertex(-1f, 0.4f); gl.Vertex(1f, 0.64f);
                    gl.Color(0f, 0f, 0f); gl.Vertex(-1f, 1f); gl.Vertex(1f, 1f);
                }
                gl.End();
            }
            return true;
        }


        /// <summary>
        /// Affichage des nuages
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_DEPTH_TEST);

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.LookAt(0, HAUTEUR_VUE, _zCamera, VITESSE_LATERALE * (float)Math.Sin(angle), 0, 0, 0, 1, 0);
            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, _zCamera, VIEWPORT_Z);

            gl.Rotate(0, 0, (float)(Math.Sin(angle) * ROULIS_MAX));

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _texture.Bind(gl);

            if (_nuages != null)
            {
                gl.Begin(OpenGL.GL_QUADS);
                foreach (Nuage o in _nuages)
                {
                    gl.Color(couleur.R / COLOR_RATIO, couleur.G / COLOR_RATIO, couleur.B / COLOR_RATIO, (float)(ALPHA / 255.0f) - ((VIEWPORT_Z - o.z) / VIEWPORT_Z));

                    float xG = (1.0f / NB_TEXTURES) * o.texture;
                    float xD = (1.0f / NB_TEXTURES) * (o.texture + 1);
                    gl.TexCoord(xG, 1.0f); gl.Vertex(o.x - o.tailleX, o.y - o.tailleY, o.z);
                    gl.TexCoord(xG, 0.0f); gl.Vertex(o.x - o.tailleX, o.y + o.tailleY, o.z);
                    gl.TexCoord(xD, 0.0f); gl.Vertex(o.x + o.tailleX, o.y + o.tailleY, o.z);
                    gl.TexCoord(xD, 1.0f); gl.Vertex(o.x + o.tailleX, o.y - o.tailleY, o.z);
                }
                gl.End();
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }



        /// <summary>
        /// Deplacement de tous les objets: flocons, camera...
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

            angle += VITESSE_ROULIS * maintenant.intervalleDepuisDerniereFrame;
            if (_nuages != null)
            {
                float deltaZ = VITESSE * maintenant.intervalleDepuisDerniereFrame;
                // Deplace les nuages
                bool trier = false;
                for (int i = 0; i < NB_NUAGES; i++)
                {
                    if (_nuages[i].z > _zCamera)
                    {
                        NouveauNuage(ref _nuages[i]);
                        trier = true;
                    }
                    else
                    {
                        _nuages[i].z += deltaZ;
                    }
                }

                if (trier)
                    Array.Sort(_nuages, delegate (Nuage O1, Nuage O2)
                    {
                        if (O1.z > O2.z) return 1;
                        if (O1.z < O2.z) return -1;
                        return 0;
                    });
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

#if TRACER
        public override string DumpRender()
        {
            return base.DumpRender() + " Nb nuages:" + NB_NUAGES;
        }

#endif
    }
}
