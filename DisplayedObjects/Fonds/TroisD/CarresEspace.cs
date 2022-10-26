using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of Neige.
    /// </summary>
    public class CarresEspace : TroisD
    {
        #region Parametres

        public const string CAT = "CarresEspace";
        private CategorieConfiguration c;
        private int NB_PAVES;
        private float TAILLE_CARRE;
        private float PERIODE_ROTATION;
        private float VITESSE_ROTATION;
        private float VITESSE;
        private float CHANGE_COULEUR;
        #endregion Parametres

        private const int VIEWPORT_X = 60;
        private const int VIEWPORT_Y = 60;
        private const float VIEWPORT_Z = 20.0f;
        private static readonly GLfloat[] fogcolor = { 0, 0, 0, 1 };
        private class Carre
        {
            public float x, y, z;
            public double changeCouleur;
        }

        private Carre[] _Carres;
        private DateTime _dernierDeplacement = DateTime.Now;
        private DateTime _debutAnimation = DateTime.Now;

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NB_PAVES = c.getParametre("Nb", 200);
                TAILLE_CARRE = c.getParametre("Taille", 5.0f, (a) => { TAILLE_CARRE = (float)Convert.ToDouble(a); });
                CHANGE_COULEUR = c.getParametre("Change couleur", 0.2f, a => { CHANGE_COULEUR = (float)Convert.ToDouble(a); });
                PERIODE_ROTATION = c.getParametre("PeriodeRotation", 10.0f, (a) => { PERIODE_ROTATION = (float)Convert.ToDouble(a); });
                VITESSE_ROTATION = c.getParametre("VitesseRotation", 50f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                VITESSE = c.getParametre("Vitesse", 8f, (a) => { VITESSE = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public CarresEspace(OpenGL gl)
            : base(gl, VIEWPORT_X, VIEWPORT_Y, VIEWPORT_Z, 100)
        {
            getConfiguration();
            _Carres = new Carre[NB_PAVES];
            // Initialiser les carres
            for (int i = 0; i < NB_PAVES; i++)
                NouveauCarre(ref _Carres[i]);

            TrierTableau();
        }

        /// <summary>
        /// Creation d'un carre tout au fond
        /// </summary>
        /// <param name="f"></param>
        private void NouveauCarre(ref Carre f)
        {
            if (f == null)
            {
                f = new Carre();
                f.z = -VIEWPORT_Z + TAILLE_CARRE * r.Next(0, (int)(_zCamera + VIEWPORT_Z) / (int)TAILLE_CARRE);
            }
            else
                while (f.z > -VIEWPORT_Z)
                    f.z -= VIEWPORT_Z;

            //f.aSupprimer = false;
            f.x = GetXCoord();
            f.y = GetYCoord();
            f.changeCouleur = FloatRandom(-1.0f, 1.0f);
        }

        private int GetYCoord()
        {
            int c;
            do
            {
                c = r.Next(-VIEWPORT_Y / 5, VIEWPORT_Y / 5) * 5;
            }
            while (c == 0);
            return c;
        }

        private int GetXCoord()
        {
            int c = r.Next(-VIEWPORT_X, VIEWPORT_X);
            return (int)((int)(c / TAILLE_CARRE) * TAILLE_CARRE);
        }

        /// <summary>
        /// Efface le fond d'ecran
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="c"></param>
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
            float depuisdebut = (float)(_debutAnimation.Subtract(_dernierDeplacement).TotalMilliseconds / 1000.0);
            float vitesseCamera = (float)Math.Sin(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;

            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.Enable(OpenGL.GL_FOG);
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, 0.2f);
            gl.Fog(OpenGL.GL_FOG_START, VIEWPORT_Z);
            gl.Fog(OpenGL.GL_FOG_END, _zCamera);

            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, 0.001f, VIEWPORT_Z * 10);

            gl.Translate(0, 0, -_zCamera);
            gl.Rotate(0, 0, vitesseCamera + 90);
            
            Color cG = Color.FromArgb(couleur.R, couleur.G, couleur.B, 255);
            gl.Begin(OpenGL.GL_QUADS);
            foreach (Carre c in _Carres)
            {
                setColorWithHueChange(gl, cG, c.changeCouleur * CHANGE_COULEUR);
                
                gl.Vertex(c.x, c.y, c.z);
                gl.Vertex(c.x, c.y, c.z + TAILLE_CARRE);
                gl.Vertex(c.x + TAILLE_CARRE,c.y, c.z + TAILLE_CARRE);
                gl.Vertex(c.x + TAILLE_CARRE,c.y, c.z);
            }
            gl.End();
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Deplacement de tous les objets: carres, camera...
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            float depuisdebut = (float)(_debutAnimation.Subtract(_dernierDeplacement).TotalMilliseconds / 1000.0);
            float deltaZ = VITESSE * maintenant.intervalleDepuisDerniereFrame;

            // Deplace les carres
            bool trier = false;
            for (int i = 0; i < NB_PAVES; i++)
            {
                if (_Carres[i].z > (_zCamera + TAILLE_CARRE))
                {
                    // Nouveau carre tout au fond
                    NouveauCarre(ref _Carres[i]);
                    trier = true;               // Il faudra trier le tableau
                }
                else
                {
                    _Carres[i].z += deltaZ;
                }
            }

            if (trier)
                TrierTableau();

            _dernierDeplacement = maintenant.temps;
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }

        private void TrierTableau()
        {
            Array.Sort(_Carres, delegate (Carre O1, Carre O2)
            {
                if (DistanceCarre(O1) > DistanceCarre(O2)) return -1;
                if (DistanceCarre(O1) < DistanceCarre(O2)) return 1;
                return 0;
            });
        }

        /// <summary>
        /// Calcule la distance au carre du point à la camera
        /// on n'a pas besoin de la racine carre, donc on ne perd pas de temps
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        private double DistanceCarre(Carre C)
        {
            return (C.x * C.x) + (C.y * C.y) + ((C.z - _zCamera) * (C.z - _zCamera));
        }

#if TRACER

        public override String DumpRender()
        {
            return base.DumpRender() + " NbCarres:" + NB_PAVES;
        }

#endif
    }
}
