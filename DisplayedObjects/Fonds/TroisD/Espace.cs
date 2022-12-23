
using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using GLfloat = System.Single;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Voyage dans les etoiles
    /// </summary>
    public class Espace : TroisD, IDisposable
    {
        #region Parametres
        public const string CAT = "Espace";
        protected CategorieConfiguration c;

        private byte ALPHA;
        private float TAILLE_ETOILE;
        private int NB_ETOILES;
        private float PERIODE_TRANSLATION;
        private float PERIODE_ROTATION;
        private float VITESSE_ROTATION;
        private float VITESSE_TRANSLATION;
        private float VITESSE;
        private float CHANGE_COULEUR;

        #endregion

        private const float VIEWPORT_X = 5f;
        private const float VIEWPORT_Y = 5f;
        private const float VIEWPORT_Z = 5f;
        private static readonly GLfloat[] fogcolor = { 0, 0, 0, 1 };

        private class Etoile
        {
            public float x, y, z;
            public double changeCouleur;
        }
        private readonly Etoile[] _etoiles;
        private DateTime _dernierDeplacement = DateTime.Now;
        private DateTime _debutAnimation = DateTime.Now;
        private Texture _texture = new Texture();

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Espace(OpenGL gl)
            : base(gl, VIEWPORT_X, VIEWPORT_Y, VIEWPORT_Z, 100)
        {
            GetConfiguration();

            _etoiles = new Etoile[NB_ETOILES];
            _texture.Create(gl, c.GetParametre("Etoile", Configuration.GetImagePath("etoile.png")));

            // Initialiser les etoiles
            for (int i = 0; i < NB_ETOILES; i++)
                NouvelleEtoile(ref _etoiles[i]);
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                ALPHA = c.GetParametre("Alpha", (byte)255);
                TAILLE_ETOILE = c.GetParametre("Taille", 0.15f);
                NB_ETOILES = c.GetParametre("NbEtoiles", 2000);
                PERIODE_TRANSLATION = c.GetParametre("PeriodeTranslation", 13.0f);
                PERIODE_ROTATION = c.GetParametre("PeriodeRotation", 10.0f);
                VITESSE_ROTATION = c.GetParametre("VitesseRotation", 50f);
                VITESSE_TRANSLATION = c.GetParametre("VitesseTranslation", 0.2f);
                VITESSE = c.GetParametre("Vitesse", 8f);
                CHANGE_COULEUR = c.GetParametre("Change couleur", 0.2f, a => { CHANGE_COULEUR = (float)Convert.ToDouble(a); });
            }
            return c;
        }
        public override void Dispose()
        {
            base.Dispose();
            _texture.Destroy(_gl);
        }

        private void NouvelleEtoile(ref Etoile f)
        {
            if (f == null)
            {
                f = new Etoile();
                // Au debut, on varie la distance des etoiles
                f.z = FloatRandom(-VIEWPORT_Z, _zCamera);
            }
            else
                f.z = -VIEWPORT_Z;

            f.x = FloatRandom(-VIEWPORT_X * 6, VIEWPORT_X * 6);
            f.y = FloatRandom(-VIEWPORT_Y * 6, VIEWPORT_Y * 6);

            f.changeCouleur = FloatRandom(-1.0f, 1.0f);
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }
        /// <summary>
        /// Affichage des flocons
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

            gl.Enable(OpenGL.GL_FOG);
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, 0.1f);
            gl.Fog(OpenGL.GL_FOG_START, VIEWPORT_Z * 1);
            gl.Fog(OpenGL.GL_FOG_END, _zCamera);

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Translate(0, 0, -_zCamera);

            float depuisdebut = (float)(_debutAnimation.Subtract(_dernierDeplacement).TotalMilliseconds / 1000.0);
            float vitesseCamera = (float)Math.Sin(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;
            gl.Rotate(0, 0, vitesseCamera);

            _texture.Bind(gl);
            gl.Begin(OpenGL.GL_QUADS);

            Color cG = Color.FromArgb(ALPHA, couleur.R, couleur.G, couleur.B);
            foreach (Etoile o in _etoiles)
            {
                SetColorWithHueChange(gl, cG, o.changeCouleur * CHANGE_COULEUR);

                gl.TexCoord(0.0f, 0.0f); gl.Vertex(o.x - TAILLE_ETOILE, o.y - TAILLE_ETOILE, o.z);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(o.x - TAILLE_ETOILE, o.y + TAILLE_ETOILE, o.z);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(o.x + TAILLE_ETOILE, o.y + TAILLE_ETOILE, o.z);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(o.x + TAILLE_ETOILE, o.y - TAILLE_ETOILE, o.z);
            }
            gl.End();
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

            float depuisdebut = (float)(_debutAnimation.Subtract(_dernierDeplacement).TotalMilliseconds / 1000.0);
            float vitesseCamera = (float)Math.Sin(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;
            float deltaZ = VITESSE * maintenant.intervalleDepuisDerniereFrame;
            float deltaWind = (float)Math.Sin(depuisdebut / PERIODE_TRANSLATION) * VITESSE_TRANSLATION * maintenant.intervalleDepuisDerniereFrame;
            // Deplace les etoiles
            bool trier = false;
            foreach (Etoile e in _etoiles)
            {
                if (e.z > _zCamera)
                {
                    e.z -= (_zCamera + VIEWPORT_Z);
                    trier = true;
                }
                else
                {
                    e.z += deltaZ;
                    e.x += deltaWind;
                }
            }

            if (trier)
                // Trier les etoiles de la plus loin a la plus proche (pour OpenGL)
                Array.Sort(_etoiles, delegate (Etoile O1, Etoile O2)
                {
                    if (O1.z > O2.z) return 1;
                    if (O1.z < O2.z) return -1;
                    return 0;
                });
            _dernierDeplacement = maintenant.temps;

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

#if TRACER
        public override String DumpRender()
        {
            return base.DumpRender() + " NbParticules:" + NB_ETOILES;
        }

#endif
    }
}
