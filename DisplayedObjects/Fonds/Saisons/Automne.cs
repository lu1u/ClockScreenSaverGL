/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 30/12/2014
 * Heure: 23:08
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Saisons
{
    /// <summary>
    /// Description of Neige.
    /// </summary>
    public sealed class Automne : TroisD
    {
        #region Parametres
        public const string CAT = "Automne.OpenGL";
        private CategorieConfiguration c;

        private float VITESSE_ROTATION;
        private float PERIODE_ROTATION;
        private float VITESSE_Y;
        private float VITESSE_DELTA_VENT;
        private float MAX_VENT;
        private int NB_FEUILLES;
        private float TAILLE_FEUILLE;
        private float DIEDRE_FEUILLE;
        private float NB_FACES_FEUILLES;
        private float CHANGE_COULEUR;
        private float FOG_DENSITY;
        #endregion

        private sealed class Feuille
        {
            public float x, y, z;
            public float vx, vy, vz;
            public float ax, ay, az;
            public float diedre;
            public int type;
            public double changeCouleur;
        }

        private readonly Feuille[] _feuilles;

        private float _xWind = 0;
        private float _xRotation;
        private readonly DateTime debut = DateTime.Now;
        private const float VIEWPORT_X = 1f;
        private const float VIEWPORT_Y = 1f;
        private const float VIEWPORT_Z = 1f;
        private const int NB_TYPES_FEUILLES = 5;
        private readonly Texture _texture;

        public Automne(OpenGL gl) : base(gl, VIEWPORT_X, VIEWPORT_Y, VIEWPORT_Z, 100)
        {
            GetConfiguration();
            _xRotation = _tailleCubeX * 0.75f;

            _feuilles = new Feuille[NB_FEUILLES];
            for (int i = 0; i < NB_FEUILLES; i++)
            {
                NouvelleFeuille(ref _feuilles[i]);
                _feuilles[i].y = FloatRandom(-_tailleCubeY * 16, _tailleCubeY * 16);
            }

            _texture = new Texture();
            _texture.Create(gl, c.GetParametre("texture feuilles", Config.Configuration.GetImagePath("automne.png")));
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);

                VITESSE_ROTATION = c.GetParametre("VitesseRotation", 0.2f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                PERIODE_ROTATION = c.GetParametre("PeriodeRotation", 20.0f, (a) => { PERIODE_ROTATION = (float)Convert.ToDouble(a); });
                VITESSE_Y = c.GetParametre("VitesseChute", 8.0f, (a) => { VITESSE_Y = (float)Convert.ToDouble(a); });
                VITESSE_DELTA_VENT = c.GetParametre("VitesseDeltaVent", 1f, (a) => { VITESSE_DELTA_VENT = (float)Convert.ToDouble(a); });
                MAX_VENT = c.GetParametre("MaxVent", 3f, (a) => { MAX_VENT = (float)Convert.ToDouble(a); });
                NB_FEUILLES = c.GetParametre("NbFeuilles", 10);
                TAILLE_FEUILLE = c.GetParametre("TailleFeuilles", 5.0f, (a) => { TAILLE_FEUILLE = (float)Convert.ToDouble(a); });
                DIEDRE_FEUILLE = c.GetParametre("DiedreFeuilles", 0.25f, (a) => { DIEDRE_FEUILLE = (float)Convert.ToDouble(a); });
                NB_FACES_FEUILLES = c.GetParametre("Nb Faces", 3, (a) => { NB_FACES_FEUILLES = (float)Convert.ToDouble(a); });
                CHANGE_COULEUR = c.GetParametre("Change couleur", 0.2f, a => { CHANGE_COULEUR = (float)Convert.ToDouble(a); });
                FOG_DENSITY = c.GetParametre("Densité brouillard", 0.01f, a => { FOG_DENSITY = (float)Convert.ToDouble(a); });
            }
            return c;
        }
        private void NouvelleFeuille(ref Feuille f)
        {
            if (f == null)
                f = new Feuille();
            f.x = FloatRandom(-_tailleCubeX * 50, _tailleCubeX * 50);
            f.z = FloatRandom(-_tailleCubeZ * 2, _zCamera);
            f.y = VIEWPORT_Y * f.z;

            f.vx = FloatRandom(-0.1f, 0.1f);
            f.vy = FloatRandom(VITESSE_Y * 0.75f, VITESSE_Y * 1.5f);
            f.vz = FloatRandom(-0.1f, 0.1f);

            f.ax = FloatRandom(0, 360);
            f.ay = FloatRandom(0, 360);
            f.az = FloatRandom(0, 360);
            f.type = random.Next(0, NB_TYPES_FEUILLES);
            f.diedre = FloatRandom(DIEDRE_FEUILLE * 0.5f, DIEDRE_FEUILLE * 2.0f) * TAILLE_FEUILLE;
            f.changeCouleur = FloatRandom(-1.0f, 1.0f);
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            GLfloat[] fogcolor = { c.R / 2048.0f, c.G / 2048.0f, c.B / 2048.0f, 0.5f };

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.ClearColor(fogcolor[0], fogcolor[1], fogcolor[2], fogcolor[3]);
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
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);


            gl.Enable(OpenGL.GL_FOG);
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_EXP);
            GLfloat[] fogcolor = { couleur.R / 2048.0f, couleur.G / 2048.0f, couleur.B / 2048.0f, 0.5f }; gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, FOG_DENSITY);
            gl.Fog(OpenGL.GL_FOG_START, _tailleCubeZ);
            gl.Fog(OpenGL.GL_FOG_END, _tailleCubeZ * 2);

            gl.LoadIdentity();
            gl.Translate(0, 0, -_zCamera);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            Color cG = Color.FromArgb(couleur.R, couleur.G, couleur.B, 255);
            _texture.Bind(gl);
            foreach (Feuille o in _feuilles)
            {
                float largeurTxtr = 1.0f / NB_TYPES_FEUILLES;
                gl.PushMatrix();
                gl.Translate(o.x, o.y, o.z);
                gl.Rotate(o.ax, o.ay, o.az);
                SetColorWithHueChange(gl, cG, o.changeCouleur * CHANGE_COULEUR);

                using (new GLBegin(gl, OpenGL.GL_QUAD_STRIP))
                {
                    for (int i = 0; i <= NB_FACES_FEUILLES; i++)
                    {
                        float f = i / NB_FACES_FEUILLES;
                        float d = o.diedre * (float)Math.Cos(f * Math.PI * 2.0);

                        gl.TexCoord((o.type + f) * largeurTxtr, 0.0f); gl.Vertex(f * TAILLE_FEUILLE, d, -TAILLE_FEUILLE / 2);
                        gl.TexCoord((o.type + f) * largeurTxtr, 1.0f); gl.Vertex(f * TAILLE_FEUILLE, d, TAILLE_FEUILLE / 2);
                    }
                }
                gl.PopMatrix();
            }

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PopMatrix();
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Deplacement de tous les objets: feuilles, camera...
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
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
            float px, pz;
            //bool trier = false;
            // Deplace les flocons
            for (int i = 0; i < NB_FEUILLES; i++)
            {
                if (_feuilles[i].y < -VIEWPORT_Y * 40)
                {
                    NouvelleFeuille(ref _feuilles[i]);
                    //    trier = true;
                }
                else
                {
                    // Deplacement
                    _feuilles[i].x += ((_feuilles[i].vx + _xWind) * maintenant.intervalleDepuisDerniereFrame);
                    _feuilles[i].y -= (_feuilles[i].vy * maintenant.intervalleDepuisDerniereFrame);
                    _feuilles[i].z += (_feuilles[i].vz * maintenant.intervalleDepuisDerniereFrame);

                    // Variation de vitesse
                    Varie(ref _feuilles[i].vx, -1, 1, 10, maintenant.intervalleDepuisDerniereFrame);
                    Varie(ref _feuilles[i].vz, -1, 1, 10, maintenant.intervalleDepuisDerniereFrame);
                    // Rotation due a la position de la camera
                    px = (CosTheta * (_feuilles[i].x - _xRotation)) - (SinTheta * _feuilles[i].z) + _xRotation;
                    pz = (SinTheta * (_feuilles[i].x - _xRotation)) + (CosTheta * _feuilles[i].z);

                    _feuilles[i].x = px;
                    _feuilles[i].z = pz;
                    _feuilles[i].ax += vitesseRot;
                    _feuilles[i].ay += vitesseRot;
                    _feuilles[i].az += vitesseRot;
                }
            }

            Varie(ref _xWind, -MAX_VENT, MAX_VENT, VITESSE_DELTA_VENT, maintenant.intervalleDepuisDerniereFrame);
            Varie(ref _xRotation, -_tailleCubeX / 2, _tailleCubeX / 2, 10, maintenant.intervalleDepuisDerniereFrame);

            //if (trier)
            Array.Sort(_feuilles, delegate (Feuille O1, Feuille O2)
            {
                if (O1.z > O2.z) return 1;
                if (O1.z < O2.z) return -1;
                return 0;
            });
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }
    }
}
