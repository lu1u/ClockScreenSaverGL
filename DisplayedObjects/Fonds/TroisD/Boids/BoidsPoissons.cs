﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Boids
{
    internal class BoidsPoissons : Boids
    {
        private const String CAT = "Boids Poissons";
        private static CategorieConfiguration c;
        private int NB;
        private float FOG_DENSITY;
        private readonly float HAUTEUR_CORPS;
        private readonly float LONGUEUR_TETE;
        private readonly float LONGUEUR_CORPS;
        private readonly float LONGUEUR_QUEUE;
        private readonly float HAUTEUR_QUEUE;


        public BoidsPoissons(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            HAUTEUR_CORPS = 0.5f * TAILLE;
            LONGUEUR_TETE = 0.75f * TAILLE;
            LONGUEUR_CORPS = 1.25f * TAILLE;
            LONGUEUR_QUEUE = -0.35f * TAILLE;
            HAUTEUR_QUEUE = 0.35f * TAILLE;
            NB_BOIDS = NB;

            LIGHTPOS[0] = 0;
            LIGHTPOS[1] = MAX_Y;
            LIGHTPOS[0] = 0;
        }

        public override bool ClearBackGround(OpenGL gl, Color couleur)
        {
            couleur = OpenGLColor.GetCouleurOpaqueAvecAlpha(couleur, 32);
            gl.ClearColor(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);

                NB = c.GetParametre("Nb", 400);
                FOG_DENSITY = c.GetParametre("Fog density", 0.015f, (a) => { FOG_DENSITY = (float)Convert.ToDouble(a); });
                MAX_SPEED = c.GetParametre("Max Speed", 0.95f, (a) => { MAX_SPEED = (float)Convert.ToDouble(a); });
                MAX_FORCE = c.GetParametre("Max force", 0.011f, (a) => { MAX_FORCE = (float)Convert.ToDouble(a); });
                TAILLE = c.GetParametre("Taille", 0.17f, (a) => { TAILLE = (float)Convert.ToDouble(a); });
                DISTANCE_VOISINS = c.GetParametre("Distance voisins", 25.0f, (a) => { DISTANCE_VOISINS = (float)Convert.ToDouble(a); });
                SEPARATION = c.GetParametre("Separation", 3.7f, (a) => { SEPARATION = (float)Convert.ToDouble(a); });
                VITESSE_ANIMATION = c.GetParametre("Vitesse animation", 0.9f, (a) => { VITESSE_ANIMATION = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /// <summary>
        /// Initialisation d'opengl avant de faire l'affichage
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="maintenant"></param>
        /// <param name="couleur"></param>
        protected override void InitOpenGL(OpenGL gl, Temps maintenant, Color couleur)
        {
            float[] col = { couleur.R / 512.0f, couleur.G / 512.0f, couleur.B / 512.0f, 1 };

            /* gl.Translate(0, 0, -MAX_Z * 1.5f);
             gl.Rotate(_angleCamera, _angleCamera, _angleCamera);
             */
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.Enable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_FOG);
            gl.Fog(OpenGL.GL_FOG_DENSITY, FOG_DENSITY);
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            gl.Fog(OpenGL.GL_FOG_COLOR, col);
            gl.Fog(OpenGL.GL_FOG_START, MAX_Z * 0.5f);
            gl.Fog(OpenGL.GL_FOG_END, MAX_Z * 3.0f);
            setGlobalMaterial(gl, couleur);
            gl.Color(col);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, LIGHTPOS);
        }

        /// <summary>
        /// Preparation de la call list opengl pour dessiner un des boids
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="noImage"></param>
        protected override void DessineBoid(OpenGL gl, float noImage)
        {
            double angle = noImage * 2.0 * Math.PI;

            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            {
                // Corps Droit
                Vecteur3D.MOINS_Z.Normal(gl); gl.Vertex(0, 0, TAILLE * (-0.5f));
                Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, TAILLE * 0.05f, 0);
                Vecteur3D.Y.Normal(gl); gl.Vertex(0, HAUTEUR_CORPS, 0);
                Vecteur3D.MOINS_X.Normal(gl); gl.Vertex(-LONGUEUR_CORPS, 0, 0);
                Vecteur3D.MOINS_Y.Normal(gl); gl.Vertex(0, -HAUTEUR_CORPS, 0);
                Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, TAILLE * -0.05f, 0);
            }
            gl.End();

            gl.Begin(OpenGL.GL_TRIANGLE_FAN);
            {
                // Corps Gauche
                Vecteur3D.Z.Normal(gl); gl.Vertex(0, 0, 4.5f);
                Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, TAILLE * 0.05f, 0);
                Vecteur3D.Y.Normal(gl); gl.Vertex(0, HAUTEUR_CORPS, 0);
                Vecteur3D.MOINS_X.Normal(gl); gl.Vertex(-LONGUEUR_CORPS, 0, 0);
                Vecteur3D.MOINS_Y.Normal(gl); gl.Vertex(0, -HAUTEUR_CORPS, 0);
                Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, TAILLE * -0.05f, 0);
            }
            gl.End();

            gl.Begin(OpenGL.GL_TRIANGLES);
            {
                // Queue
                float z = TAILLE * 0.5f * (float)Math.Sin(angle);
                float x = -LONGUEUR_CORPS + LONGUEUR_QUEUE * (float)Math.Abs(Math.Cos(angle));
                gl.Vertex(-LONGUEUR_CORPS, 0, 0);
                gl.Vertex(x, HAUTEUR_QUEUE, z);
                gl.Vertex(x, -HAUTEUR_QUEUE, z);
            }
            gl.End();
        }

        protected override Boid NewBoid()
        {
            return new BoidPoisson(FloatRandom(-MAX_X, MAX_X), FloatRandom(-MAX_Y, MAX_Y), FloatRandom(-MAX_Z, MAX_Z), TAILLE, LONGUEUR_TETE, HAUTEUR_CORPS, LONGUEUR_CORPS, LONGUEUR_QUEUE, HAUTEUR_QUEUE);
        }

        protected class BoidPoisson : Boid
        {
            private float TAILLE, LONGUEUR_TETE, HAUTEUR_CORPS, LONGUEUR_CORPS, LONGUEUR_QUEUE, HAUTEUR_QUEUE;
            public BoidPoisson(float x, float y, float z, float taille, float longueurTete, float hauteurCorps, float longueurCorps, float longueurQueue, float hauteurQueue) : base(x, y, z)
            {
                TAILLE = taille;
                LONGUEUR_TETE = longueurTete;
                LONGUEUR_CORPS = longueurCorps;
                HAUTEUR_CORPS = hauteurCorps;
                LONGUEUR_QUEUE = longueurQueue;
                HAUTEUR_QUEUE = hauteurQueue;
            }

            public override void Dessine(OpenGL gl)
            {
                double angle = _image * 2.0 * Math.PI;

                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                {
                    // Corps Droit
                    Vecteur3D.MOINS_Z.Normal(gl); gl.Vertex(0, 0, -TAILLE * 0.5f);
                    Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, TAILLE * 0.05f, 0);
                    Vecteur3D.Y.Normal(gl); gl.Vertex(0, HAUTEUR_CORPS, 0);
                    Vecteur3D.MOINS_X.Normal(gl); gl.Vertex(-LONGUEUR_CORPS, 0, 0);
                    Vecteur3D.MOINS_Y.Normal(gl); gl.Vertex(0, -HAUTEUR_CORPS, 0);
                    Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, -TAILLE * 0.05f, 0);
                }
                gl.End();

                gl.Begin(OpenGL.GL_TRIANGLE_FAN);
                {
                    // Corps Gauche
                    Vecteur3D.Z.Normal(gl); gl.Vertex(0, 0, TAILLE * 0.5f);
                    Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, -TAILLE * 0.05f, 0);
                    Vecteur3D.MOINS_Y.Normal(gl); gl.Vertex(0, -HAUTEUR_CORPS, 0);
                    Vecteur3D.MOINS_X.Normal(gl); gl.Vertex(-LONGUEUR_CORPS, 0, 0);
                    Vecteur3D.Y.Normal(gl); gl.Vertex(0, HAUTEUR_CORPS, 0);
                    Vecteur3D.X.Normal(gl); gl.Vertex(LONGUEUR_TETE, TAILLE * 0.05f, 0);
                }
                gl.End();

                gl.Begin(OpenGL.GL_TRIANGLES);
                {
                    // Queue
                    float z = TAILLE * 0.5f * (float)Math.Sin(angle);
                    float x = -LONGUEUR_CORPS + LONGUEUR_QUEUE * (float)Math.Abs(Math.Cos(angle));
                    gl.Vertex(-LONGUEUR_CORPS, 0, 0);
                    gl.Vertex(x, HAUTEUR_QUEUE, z);
                    gl.Vertex(x, -HAUTEUR_QUEUE, z);
                }
                gl.End();
            }
        }
    }
}
