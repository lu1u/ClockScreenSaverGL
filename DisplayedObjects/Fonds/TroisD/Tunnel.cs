/*
 * Un tunnel infini et mouvant
 * 
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of Tunnel.
    /// </summary>
    public class Tunnel : MateriauGlobal
    {
        private const string CAT = "Tunnel.OpenGL";
        private CategorieConfiguration c;
        private int TAILLE_ANNEAU_MIN;
        private int TAILLE_ANNEAU_MAX;
        private int NB_ANNEAUX;
        private float VITESSE_ANNEAU;
        private float PERIODE_ROTATION;
        private float VITESSE_ROTATION;
        private float RATIO_DEPLACEMENT;
        private float RAYON_ANNEAU;
        private GLfloat PERIODE_DEP_X;
        private GLfloat PERIODE_DEP_Y;
        private float CHANGE_COULEUR;


        private int BANDES_PLEINES;
        private int TAILLE_ANNEAU;
        private float _CentreAnneauX;
        private float _CentreAnneauY;

        private DateTime _DernierDeplacement = DateTime.Now;
        private DateTime debut = DateTime.Now;

        private class Anneau
        {
            public Vecteur3D pos ;
            public double changeCouleur;
        }
        private readonly Anneau[,] _anneaux;
        private const float VIEWPORT_X = 2f;
        private const float VIEWPORT_Y = 2f;
        private const float VIEWPORT_Z = 4f;

        /// <summary>
        /// Constructeur: initialiser les anneaux
        /// </summary>
        /// <param name="gl"></param>
        public Tunnel(OpenGL gl)
            : base(gl)
        {
            getConfiguration();
            _tailleCubeX = VIEWPORT_X;
            _tailleCubeY = VIEWPORT_Y;
            _tailleCubeZ = VIEWPORT_Z;
            _zCamera = VIEWPORT_Y / 2;
            TAILLE_ANNEAU = r.Next(TAILLE_ANNEAU_MIN, TAILLE_ANNEAU_MAX + 1);
            if (r.Next(0, 3) > 0)
                BANDES_PLEINES = TAILLE_ANNEAU + 1;
            else
                BANDES_PLEINES = r.Next(1, BANDES_PLEINES + 1);

            _anneaux = new Anneau[NB_ANNEAUX, TAILLE_ANNEAU];

            _CentreAnneauX = 0;
            _CentreAnneauY = 0;
            for (int x = 0; x < NB_ANNEAUX; x++)
            {
                PlaceAnneau(x);
                float CosTheta = (float)Math.Cos(0.1);
                float SinTheta = (float)Math.Sin(0.1);
                float px, py;

                for (int i = 0; i < x; i++)
                    for (int j = 0; j < TAILLE_ANNEAU; j++)
                    {
                        // Tourner autour de l'axe Z
                        px = (CosTheta * (_anneaux[i, j].pos.x)) - (SinTheta * _anneaux[i, j].pos.y);
                        py = (SinTheta * (_anneaux[i, j].pos.x)) + (CosTheta * _anneaux[i, j].pos.y);

                        _anneaux[i, j].pos.x = px;
                        _anneaux[i, j].pos.y = py;
                        _anneaux[i, j].changeCouleur = FloatRandom(-1.0f, 1.0f);
                    }
            }
        }


        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                TAILLE_ANNEAU_MIN = c.getParametre("NbFacettesMin", 4);
                TAILLE_ANNEAU_MAX = c.getParametre("NbFacettesMax", 16);
                NB_ANNEAUX = c.getParametre("Nombre", 200);
                VITESSE_ANNEAU = c.getParametre("Vitesse", 2f);
                PERIODE_ROTATION = c.getParametre("PeriodeRotation", 10.0f);
                VITESSE_ROTATION = c.getParametre("VitesseRotation", 0.2f);
                BANDES_PLEINES = c.getParametre("CouleursPleines", 16 + 1);
                RATIO_DEPLACEMENT = c.getParametre("DeplacementTunnel", 0.5f);
                RAYON_ANNEAU = RATIO_DEPLACEMENT * 5f;
                PERIODE_DEP_X = c.getParametre("PeriodeDEcalageX", 5f);
                PERIODE_DEP_Y = c.getParametre("PeriodeDEcalageY", 7f);
                CHANGE_COULEUR = c.getParametre("Change couleur", 0.2f, a => { CHANGE_COULEUR = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /// <summary>
        /// Placer un anneau
        /// </summary>
        /// <param name="i"></param>
        private void PlaceAnneau(int i)
        {
            float profondeur = _tailleCubeZ * 50f;
            float ecart = profondeur / NB_ANNEAUX;
            float zAnneau = _tailleCubeZ - (i * ecart);

            for (int j = 0; j < TAILLE_ANNEAU; j++)
            {
                double angle = (Math.PI * 2.0 * j) / TAILLE_ANNEAU;
                _anneaux[i, j] = new Anneau() { pos = new Vecteur3D( _CentreAnneauX + (float)(RAYON_ANNEAU * Math.Cos(angle)), _CentreAnneauY + (float)(RAYON_ANNEAU * Math.Sin(angle)), zAnneau) };
                _anneaux[i, j].changeCouleur = FloatRandom(-1.0f, 1.0f);
            }
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
            float depuisdebut = (float)(debut.Subtract(_DernierDeplacement).TotalMilliseconds / 1000.0);
            float rotation = (float)Math.Cos(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;
            //float[] col = { couleur.R / 512.0f, couleur.G / 512.0f, couleur.B / 512.0f, 1.0f };

            gl.ClearColor(0, 0, 0, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_FOG);
            /*
            // Lumiere
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, LIGHT_POS);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, SPECULAR_LIGHT);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, AMBIENT_LIGHT);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, DIFFUSE_LIGHT);
            gl.ShadeModel(OpenGL.GL_FLAT);


            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, COL_AMBIENT);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_EMISSION, COL_EMISSION);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, COL_SPECULAR);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, COL_DIFFUSE);
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, SHININESS);
            */
            setGlobalMaterial(gl, couleur);
            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, 0.001f, _tailleCubeZ * 20);

            gl.Rotate(0, 0, rotation);
            // gl.Color(col);

            // Tracer les anneaux
            Color cG = Color.FromArgb(255, couleur.R, couleur.G, couleur.B);
            gl.Begin(OpenGL.GL_QUADS);
            for (int i = 0; i < NB_ANNEAUX - 1; i++)
            {
                {
                    int iPlusUn = i < (NB_ANNEAUX - 1) ? i + 1 : 0;

                    for (int j = 0; j < TAILLE_ANNEAU; j++)
                    {
                        setColorWithHueChange(gl, cG, _anneaux[i, j].changeCouleur * CHANGE_COULEUR);
                        if ((j + 1) % BANDES_PLEINES != 0)
                        {
                            int jPlusUn = j < (TAILLE_ANNEAU - 1) ? j + 1 : 0;

                            NormaleTriangle(_anneaux[iPlusUn, j].pos, _anneaux[i, j].pos, _anneaux[i, jPlusUn].pos).Normal(gl);
                            _anneaux[i, j].pos.Vertex(gl);
                            _anneaux[i, jPlusUn].pos.Vertex(gl);
                            _anneaux[iPlusUn, jPlusUn].pos.Vertex(gl);
                            _anneaux[iPlusUn, j].pos.Vertex(gl);
                        }
                    }
                }

            }
            gl.End();
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }


        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            float depuisdebut = (float)(debut.Subtract(maintenant.temps).TotalMilliseconds / 1000.0);
            float vitesseCamera = (float)Math.Sin(depuisdebut / PERIODE_ROTATION) * VITESSE_ROTATION;
            float vitesseRot = maintenant.intervalleDepuisDerniereFrame * 100;

            float CosTheta = (float)Math.Cos(vitesseCamera * maintenant.intervalleDepuisDerniereFrame);
            float SinTheta = (float)Math.Sin(vitesseCamera * maintenant.intervalleDepuisDerniereFrame);
            float px, py;

            float dZ = (VITESSE_ANNEAU * maintenant.intervalleDepuisDerniereFrame);

            for (int i = 0; i < NB_ANNEAUX; i++)
                for (int j = 0; j < TAILLE_ANNEAU; j++)
                {
                    _anneaux[i, j].pos.z += dZ;

                    // Tourner autour de l'axe Z
                    px = (CosTheta * (_anneaux[i, j].pos.x)) - (SinTheta * _anneaux[i, j].pos.y);
                    py = (SinTheta * (_anneaux[i, j].pos.x)) + (CosTheta * _anneaux[i, j].pos.y);

                    _anneaux[i, j].pos.x = px;
                    _anneaux[i, j].pos.y = py;
                }

            if (_anneaux[2, 0].pos.z > 0)
            {
                for (int i = 0; i < NB_ANNEAUX - 1; i++)
                    for (int j = 0; j < TAILLE_ANNEAU; j++)
                        _anneaux[i, j] = _anneaux[i + 1, j];

                _CentreAnneauX = (RAYON_ANNEAU * RATIO_DEPLACEMENT) * (float)Math.Sin(depuisdebut / PERIODE_DEP_X);
                _CentreAnneauY = (RAYON_ANNEAU * RATIO_DEPLACEMENT) * (float)Math.Cos(depuisdebut / PERIODE_DEP_Y);

                PlaceAnneau(NB_ANNEAUX - 1);
            }

            _DernierDeplacement = maintenant.temps;
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }
    }
}
