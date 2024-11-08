using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using GLfloat = System.Single;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public partial class Grenoble : Fond //MateriauGlobal
    {
        #region Parametres
        private const string CAT = "Grenoble";
        private CategorieConfiguration c;
        private string NOM_FICHIER_HEIGHTMAP;
        public float LARGEUR_CARTE = 20;
        public float HAUTEUR_CARTE = 20;
        private float VITESSE_ROTATION = 2;
        private float RAPPORT_ALTITUDE = 0.005f;
        private float HAUTEUR_VUE = 8;
        private float FOG_DENSITY;
        private bool HAUTEUR_VUE_RELATIVE;
        private float RAPPORT_DISTANCE_CAMERA;
        private float CABINET_X, CABINET_Z;
        private float PONDERATION_CAMERA;
        #endregion

        private float _angleVue;
        private TextureAsynchrone _textureTerrrain;
        private HeightmapAsynchrone _heightMap;
        private static readonly GLfloat[] fogcolor = { 0.11f, 0.11f, 0.11f, 1 };
        private float _altitudePrecedente = 0;

        public Grenoble(OpenGL gl) : base(gl)
        {

        }

        protected override void Init(OpenGL gl)
        {
            base.Init(gl);
            GetConfiguration();
            _heightMap = new HeightmapAsynchrone(gl, NOM_FICHIER_HEIGHTMAP, RAPPORT_ALTITUDE, LARGEUR_CARTE, HAUTEUR_CARTE, false);
            _heightMap.Init();
            _textureTerrrain = new TextureAsynchrone(gl, Path.Combine(Configuration.GetDataDirectory(CAT), "Grenoble.png"));
            _textureTerrrain.Init();
            _angleVue = FloatRandom(0, 360);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] private float CalculX(int x) => x * LARGEUR_CARTE / _heightMap.Largeur - LARGEUR_CARTE / 2.0f;
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] private float CalculZ(int z) => (z - (_heightMap.Hauteur * 0.5f)) * HAUTEUR_CARTE / _heightMap.Hauteur;

        /// <summary>
        /// Chargement de la configuration
        /// </summary>
        /// <returns></returns>
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NOM_FICHIER_HEIGHTMAP = Path.Combine(Configuration.GetDataDirectory(CAT), "heightmap.png");// c.GetParametre("HeightMap", Path.Combine(Configuration.GetDataDirectory(CAT), "heightmap.raw"));
                CABINET_X = c.GetParametre("Cabinet X", -1.284778f, (a) => { CABINET_X = (float)Convert.ToDouble(a); });
                CABINET_Z = c.GetParametre("Cabinet Z", 3.209182f, (a) => { CABINET_Z = (float)Convert.ToDouble(a); });
                VITESSE_ROTATION = c.GetParametre("Vitesse Rotation", 2.0f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                HAUTEUR_VUE = c.GetParametre("Hauteur vue", 32.0f, (a) => { HAUTEUR_VUE = (float)Convert.ToDouble(a); });
                FOG_DENSITY = c.GetParametre("Densité brouillard", 0.1f, (a) => { FOG_DENSITY = (float)Convert.ToDouble(a); });
                HAUTEUR_VUE_RELATIVE = c.GetParametre("Hauteur vue relative", true, (a) => { HAUTEUR_VUE_RELATIVE = Convert.ToBoolean(a); });
                RAPPORT_ALTITUDE = c.GetParametre("Rapport Altitude", 0.007f);
                RAPPORT_DISTANCE_CAMERA = c.GetParametre("Rapport distance camera", 0.41f, (a) => { RAPPORT_DISTANCE_CAMERA = (float)Convert.ToDouble(a); });
                PONDERATION_CAMERA = c.GetParametre("Pondération caméra", 0.5f, (a) => { PONDERATION_CAMERA = (float)Convert.ToDouble(a); });
                _altitudePrecedente = HAUTEUR_VUE;
            }
            return c;
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(c.R / 256.0f, c.G / 256.0f, c.B / 256.0f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            return true;
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
            if (_heightMap.Pret)
            {
                gl.PushAttrib(SharpGL.Enumerations.AttributeMask.All);
                gl.LoadIdentity();

                float cameraX = (float)(LARGEUR_CARTE * 0.5 * Math.Sin(_angleVue)) * RAPPORT_DISTANCE_CAMERA;
                float cameraZ = (float)(LARGEUR_CARTE * 0.5 * Math.Cos(_angleVue)) * RAPPORT_DISTANCE_CAMERA;
                float cameraY = GetHauteurVue();

                //gl.LookAt(0, HAUTEUR_VUE, -LARGEUR_CARTE / 6.0f, 0, 0, 0, 0, 1, 0);
                //gl.Rotate(0, _angleVue, 0);
                //
                gl.LookAt(cameraX, cameraY, cameraZ, 0, 0, 0, 0, 1, 0);

                gl.LineWidth(1);
                gl.Disable(OpenGL.GL_ALPHA_TEST);
                gl.Disable(OpenGL.GL_BLEND);
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Enable(OpenGL.GL_CULL_FACE);
                gl.CullFace(OpenGL.GL_BACK);
                gl.ShadeModel(OpenGL.GL_SMOOTH);

                //if (LUMIERE)
                //{
                //    LIGHTPOS[0] = -calculX(0) / 2.0f;
                //    LIGHTPOS[1] = HAUTEUR_LUMIERE;
                //    LIGHTPOS[2] = calculZ(0) / 2.0f;
                //    LIGHTPOS[3] = 1.0f;
                //    setGlobalMaterial(gl, couleur);
                //}
                gl.Color(1.0f, 1.0f, 1.0f);
                
                Brouillard(gl, couleur);

                if (_textureTerrrain.Pret)
                {
                    gl.Enable(OpenGL.GL_TEXTURE_2D);
                    _textureTerrrain.Texture.Bind(gl);
                }
                else
                    gl.Disable(OpenGL.GL_TEXTURE_2D);


                _heightMap.callList(gl);
                PointeCabinet(gl);
                gl.PopAttrib();
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Retourne l'altitude de la camera:
        /// - soit hauteur fixe
        /// - soit hauteur relative a l'altitude du point de vue
        /// </summary>
        /// <returns></returns>
        private float GetHauteurVue()
        {

            float res;
            if (HAUTEUR_VUE_RELATIVE)
            {
                int x = (int)(_heightMap.Largeur * 0.5f * Math.Sin(_angleVue) * RAPPORT_DISTANCE_CAMERA + _heightMap.Largeur * 0.5f);
                int z = (int)(_heightMap.Hauteur * 0.5f * Math.Cos(_angleVue) * RAPPORT_DISTANCE_CAMERA + _heightMap.Hauteur * 0.5f);

                res = _heightMap.getAltitude(x, z) * RAPPORT_ALTITUDE + HAUTEUR_VUE;
            }
            else
            {
                res = HAUTEUR_VUE;
            }

            res = _altitudePrecedente + ((res - _altitudePrecedente) * PONDERATION_CAMERA);
            _altitudePrecedente = res;
            return res;
        }

        /// <summary>
        /// Pointe la position du cabinet
        /// </summary>
        /// <param name="gl"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void PointeCabinet(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.LineWidth(6);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            using (new GLBegin(gl, OpenGL.GL_LINES))
            {
                gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
                gl.Vertex(CABINET_X, 0, CABINET_Z);
                gl.Color(1.0f, 1.0f, 1.0f, 0.0f);
                gl.Vertex(CABINET_X, RAPPORT_ALTITUDE * 256.0f, CABINET_Z);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Parametrer le brouillard
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="col"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void Brouillard(OpenGL gl, Color couleur)
        {
            // Brouillard
            gl.Enable(OpenGL.GL_FOG);
            fogcolor[0] = couleur.R / 256.0f;
            fogcolor[1] = couleur.G / 256.0f;
            fogcolor[2] = couleur.B / 256.0f;

            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_EXP2);
            gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, FOG_DENSITY);
            gl.Fog(OpenGL.GL_FOG_START, CalculX(_heightMap.Largeur) * 0.95f);
            gl.Fog(OpenGL.GL_FOG_END, CalculX(_heightMap.Largeur));
        }



        /// <summary>
        /// Deplacement
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            // Angle de vue de la camera
            _angleVue += VITESSE_ROTATION * maintenant.intervalleDepuisDerniereFrame;
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }


    }

}
