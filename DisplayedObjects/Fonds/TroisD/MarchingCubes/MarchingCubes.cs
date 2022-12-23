using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;
using GLfloat = System.Single;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.MarchingCubes
{
    public partial class MarchingCubes : MateriauGlobal
    {
        private const string CAT = "MarchingCubes";
        protected CategorieConfiguration c;
        private float SEUIL;
        private static int TAILLE_X;
        private static int TAILLE_Y;
        private static int TAILLE_Z;
        private float LINE_WIDTH;
        private float FOG_DENSITY;
        private float VITESSE_ROTATION;
        private float FOG_END;
        private float EPAISSEUR_LIGNES;
        private int OCTAVES;
        private float[,,] _cubes;
        private float angle = 0;
        private uint liste;
        private static bool changement = false;
        private static readonly GLfloat[] fogcolor = { 0.11f, 0.11f, 0.11f, 1 };
        private int nbVertex;
        public MarchingCubes(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            _cubes = new float[TAILLE_X, TAILLE_Y, TAILLE_Z];

            PerlinNoise p = new PerlinNoise(TAILLE_X);
            for (int x = 0; x < TAILLE_X; x++)
                for (int y = 0; y < TAILLE_Y; y++)
                    for (int z = 0; z < TAILLE_Z; z++)
                        _cubes[x, y, z] = (float)p.OctavePerlin(x / (float)TAILLE_X, y / (float)TAILLE_Y, z / (float)TAILLE_Z, OCTAVES, 2.0f);

            genList(gl);
            LIGHTPOS[0] = -1.0f;
            LIGHTPOS[1] = 1.0f;
            LIGHTPOS[2] = -1.0f;

        }


        private void genList(OpenGL gl)
        {
            liste = gl.GenLists(1);
            gl.NewList(liste, OpenGL.GL_COMPILE);
            nbVertex = Generate(gl, _cubes, SEUIL, TAILLE_X, TAILLE_Y, TAILLE_Z);
            gl.EndList();
            changement = false;
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                SEUIL = c.GetParametre("Seuil", 0.5f, (a) => { SEUIL = (float)Convert.ToDouble(a); changement = true; });
                TAILLE_X = c.GetParametre("Taille X", 40);
                TAILLE_Y = c.GetParametre("Taille Y", 40);
                TAILLE_Z = c.GetParametre("Taille Z", 40);
                OCTAVES = c.GetParametre("Octeves", 4);
                FOG_DENSITY = c.GetParametre("Fog Density", 0.5f, (a) => { FOG_DENSITY = (float)Convert.ToDouble(a); });
                LINE_WIDTH = c.GetParametre("Largeur Lignes", 1.0f, (a) => { LINE_WIDTH = (float)Convert.ToDouble(a); });
                VITESSE_ROTATION = c.GetParametre("Vitesse Rotation", 0.5f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                FOG_END = c.GetParametre("Fog End", 0.5f, (a) => { FOG_END = (float)Convert.ToDouble(a); });
                EPAISSEUR_LIGNES = c.GetParametre("Epaisseur lignes", 1.5f, (a) => { EPAISSEUR_LIGNES = (float)Convert.ToDouble(a); });
            }
            return c;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Effacer l'ecran
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="couleur"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public override bool ClearBackGround(OpenGL gl, Color couleur)
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
            if (changement)
            {
                gl.DeleteLists(liste, 1);
                genList(gl);
            }

            float[] color = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1 };

            gl.LoadIdentity();
            //gl.Enable(OpenGL.GL_DEPTH);
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.LineWidth(EPAISSEUR_LIGNES);

            gl.LookAt(0.1f, 0.1f, 0.1f, 0, 0, 0, 0, 1, 0);
            gl.Rotate(angle, angle, angle);
            changeZoom(gl, tailleEcran.Width, tailleEcran.Height, 0.001f, 10f);
            setGlobalMaterial(gl, color[0], color[1], color[2]);
            brouillard(gl, color);
            gl.Color(color[0], color[1], color[2], color[3]);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.CullFace(OpenGL.GL_BACK);

            using (new PolygonMode(gl, OpenGL.GL_LINE, LINE_WIDTH))
            {
                gl.CallList(liste);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Parametrer le brouillard
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="col"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void brouillard(OpenGL gl, float[] col)
        {
            // Brouillard
            gl.Enable(OpenGL.GL_FOG);
            fogcolor[0] = 0;//ol[0] * RATIO_FOG;
            fogcolor[1] = 0;//ol[1] * RATIO_FOG;
            fogcolor[2] = 0;//col[2] * RATIO_FOG;

            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            gl.Fog(OpenGL.GL_FOG_COLOR, fogcolor);
            gl.Fog(OpenGL.GL_FOG_DENSITY, FOG_DENSITY);
            gl.Fog(OpenGL.GL_FOG_START, 0);
            gl.Fog(OpenGL.GL_FOG_END, 2.0f * FOG_END);

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

            angle += maintenant.intervalleDepuisDerniereFrame * VITESSE_ROTATION;
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

        public override String DumpRender()
        {
            return base.DumpRender() + ", " + nbVertex + " vertexes";
        }
    }
}