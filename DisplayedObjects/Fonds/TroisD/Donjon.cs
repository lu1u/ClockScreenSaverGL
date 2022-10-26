using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Primitives;
using SharpGL.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    class Donjon : MateriauGlobal
    {
        #region Parametres
        const String CAT = "Donjon";
        CategorieConfiguration c;
        #endregion
        float rotate = 0;

        uint _genLists;
        uint _listeCouloirDroit;
        public Donjon(OpenGL gl) : base(gl)
        {
            _genLists = gl.GenLists(1);
            _listeCouloirDroit = creerCouloirDroit(gl);
        }

        public override void Dispose()
        {
            base.Dispose();
            _gl.DeleteLists(_genLists, 1);
        }

        /***
         * Creer une CallList OpenGL pour dessiner un couloir droit
         * @return: calllist id
         */
        private uint creerCouloirDroit(OpenGL gl)
        {
            uint res = _genLists;
            gl.NewList(res, OpenGL.GL_COMPILE);
            gl.Begin(OpenGL.GL_QUADS);

            // Sol
            gl.Normal(0, 1, 0);
            gl.Vertex(-1, 0, 1);
            gl.Vertex(1, 0, 1);
            gl.Vertex(1, 0, -1);
            gl.Vertex(-1, 0, -1);

            // Mur a droite
            gl.Normal(1, 0, 0);
            gl.Vertex(1, 0, 1);
            gl.Vertex(1, 1, 1);
            gl.Vertex(1, 1, -1);
            gl.Vertex(1, 0, -1);

            // Mur a gauche
            gl.Normal(-1, 0, 0); 
            gl.Vertex(-1, 0, -1);
            gl.Vertex(-1, 1, -1);
            gl.Vertex(-1, 1, 1);
            gl.Vertex(-1, 0, 1);
            gl.End();
            
            // Plafond
            const int NB_FACES = 16;
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i <= NB_FACES; i++)
            {
                double angle = (PI * i / NB_FACES) - PI_SUR_DEUX;
                double sin = Math.Sin(angle);
                double cos = Math.Cos(angle);

                gl.Normal(sin, cos, 0);

                sin *= 1;
                cos *= 1;

                gl.Vertex(sin, 1 + cos, -1);
                gl.Vertex( sin, 1 + cos, 1);
            }
            gl.End();
            
            gl.EndList();

            return res;
        }

        private float Max(float[] extent)
        {
            float res = extent[0];
            for (int i = 1; i < extent.Length; i++)
                if (res < extent[i])
                    res = extent[i];

            return res;
        }


        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                //NB_ESCALIERS = c.getParametre("NbEscaliers", 500);
                //RATIO_COULEUR_MIN = c.getParametre("Ratio Couleur Min", 0.95f, (a) => { RATIO_COULEUR_MIN = (float)Convert.ToDouble(a); });
                //RATIO_COULEUR_MAX = c.getParametre("Ratio Couleur Max", 1.05f, (a) => { RATIO_COULEUR_MAX = (float)Convert.ToDouble(a); });
                //MIN_TAILLE_X = c.getParametre("Min tailleX", 0.3f);
                //MAX_TAILLE_X = c.getParametre("Max tailleX", 0.8f);
                //MIN_TAILLE_Y = c.getParametre("Min tailleY", 0.1f);
                //MAX_TAILLE_Y = c.getParametre("Max tailleY", 0.2f);
                //MIN_TAILLE_Z = c.getParametre("Min tailleZ", 0.2f);
                //MAX_TAILLE_Z = c.getParametre("Max tailleZ", 0.5f);
                //VITESSE_ROTATION = c.getParametre("Vitesse Rotation", 10.0f, (a) => { VITESSE_ROTATION = (float)Convert.ToDouble(a); });
                //SEUIL_DECALAGE = c.getParametre("Seuil Decalage", 0.01f, (a) => { SEUIL_DECALAGE = (float)Convert.ToDouble(a); });
                //ACCELERATION_DECALAGE = c.getParametre("Acceleration Decalage", 0.9f, (a) => { ACCELERATION_DECALAGE = (float)Convert.ToDouble(a); });
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


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Affichage
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_FOG);
            //  Clear and load the identity.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            //  View from a bit away the y axis and a few units above the ground.
            gl.LookAt(-1, 1, -5, 0, 0, 0, 0, 1, 0);


            setGlobalMaterial(gl, couleur);

            //gl.PushMatrix();
            //  Rotate the objects every cycle.
            gl.Rotate(0.0f, rotate, 0.0f);
            gl.CallList(_listeCouloirDroit);
            //gl.PopMatrix();


            //  Rotate a bit more each cycle.
            rotate += 1f;
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }
    }


}
