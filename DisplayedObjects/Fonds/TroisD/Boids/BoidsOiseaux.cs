﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Boids
{
    internal class BoidsOiseaux : Boids
    {
        private const String CAT = "Boids Oiseaux";
        private CategorieConfiguration c;
        private static int NB;
        private static float DIMINUE_VITESSE_V;
        private static float DIMINUE_ACCELERATION_V;
        private readonly Texture _texture = new Texture();


        public BoidsOiseaux(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            _texture.Create(gl, c.GetParametre("Oiseau", Configuration.GetImagePath("oiseau.png")));
            NB_BOIDS = NB;
        }

        public override void Dispose()
        {
            base.Dispose();
            _texture?.Destroy(_gl);
        }


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);

                NB = c.GetParametre("Nb", 200);
                MAX_SPEED = c.GetParametre("Max Speed", 3.5f, (a) => { MAX_SPEED = (float)Convert.ToDouble(a); });
                MAX_FORCE = c.GetParametre("Max force", 0.01f, (a) => { MAX_FORCE = (float)Convert.ToDouble(a); });
                TAILLE = c.GetParametre("Taille", 0.33f, (a) => { TAILLE = (float)Convert.ToDouble(a); });
                DISTANCE_VOISINS = c.GetParametre("Distance voisins", 3.7f, (a) => { DISTANCE_VOISINS = (float)Convert.ToDouble(a); });
                SEPARATION = c.GetParametre("Separation", 1.5f, (a) => { SEPARATION = (float)Convert.ToDouble(a); });
                VITESSE_ANIMATION = c.GetParametre("Vitesse animation", 2.0f, (a) => { VITESSE_ANIMATION = (float)Convert.ToDouble(a); });
                DIMINUE_VITESSE_V = c.GetParametre("Diminution vitesse verticale", 0.9999f, (a) => { DIMINUE_VITESSE_V = (float)Convert.ToDouble(a); });
                DIMINUE_ACCELERATION_V = c.GetParametre("Diminution acceleration verticale", 0.9999f, (a) => { DIMINUE_ACCELERATION_V = (float)Convert.ToDouble(a); });
            }
            return c;
        }

        /// <summary>
        /// Dessine une des images animees d'un boid
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="noImage">0..1</param>
        protected override void DessineBoid(OpenGL gl, float noImage)
        {
            double angle = noImage * 2.0 * Math.PI;

            gl.Begin(OpenGL.GL_QUAD_STRIP);
            gl.TexCoord(0, 0); gl.Vertex(TAILLE, TAILLE * Math.Sin(angle), -TAILLE);
            gl.TexCoord(0, 1.0f); gl.Vertex(-TAILLE, TAILLE * Math.Sin(angle), -TAILLE);

            gl.TexCoord(0.4f, 0.0f); gl.Vertex(+TAILLE, -TAILLE * 0.2 * Math.Sin(angle), -TAILLE * 0.2f);
            gl.TexCoord(0.4f, 1.0f); gl.Vertex(-TAILLE, -TAILLE * 0.2 * Math.Sin(angle), -TAILLE * 0.2f);

            gl.TexCoord(0.6f, 0.0f); gl.Vertex(+TAILLE, TAILLE * 0.2 * Math.Sin(angle), TAILLE * 0.2f);
            gl.TexCoord(0.6f, 1.0f); gl.Vertex(-TAILLE, TAILLE * 0.2 * Math.Sin(angle), TAILLE * 0.2f);

            gl.TexCoord(1.0f, 0.0f); gl.Vertex(TAILLE, TAILLE * Math.Sin(angle), TAILLE);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(-TAILLE, TAILLE * Math.Sin(angle), TAILLE);
            gl.End();
        }

        protected override void InitOpenGL(OpenGL gl, Temps maintenant, Color couleur)
        {
            couleur = OpenGLColor.GetCouleurOpaqueAvecAlpha(couleur, 64);
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1 };

            gl.Disable(OpenGL.GL_COLOR_MATERIAL);

            gl.Enable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_DEPTH_TEST);

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            gl.Enable(OpenGL.GL_FOG);
            gl.Fog(OpenGL.GL_FOG_MODE, OpenGL.GL_LINEAR);
            gl.Fog(OpenGL.GL_FOG_COLOR, col);
            gl.Fog(OpenGL.GL_FOG_START, MAX_Z * 0.5f);
            gl.Fog(OpenGL.GL_FOG_END, MAX_Z * 2.0f);

            gl.Color(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _texture.Bind(gl);

            _boids.Sort(delegate (Boid O1, Boid O2)
            {
                if (O1._Position.z < O2._Position.z) return 1;
                if (O1._Position.z > O2._Position.z) return -1;
                return 0;
            });
        }

        /// <summary>
        /// Initialisation du tableau de boids
        /// </summary>
        /// <param name="_boids"></param>
        protected override void InitBoids(List<Boid> _boids)
        {
            for (int i = 0; i < NB_BOIDS; i++)
                _boids.Add(new BoidOiseau(FloatRandom(-MAX_X, MAX_X), FloatRandom(-MAX_Y, MAX_Y), FloatRandom(-MAX_Z, MAX_Z)));
        }
        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            c = OpenGLColor.GetCouleurOpaqueAvecAlpha(c, 128);
            gl.ClearColor(c.R / 256.0f, c.G / 256.0f, c.B / 256.0f, 1);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        protected override Boid NewBoid()
        {
            return new BoidOiseau(FloatRandom(-MAX_X, MAX_X), FloatRandom(-MAX_Y, MAX_Y), FloatRandom(-MAX_Z, MAX_Z));
        }

        protected class BoidOiseau : Boid
        {
            public BoidOiseau(float x, float y, float z) : base(x, y, z)
            {
            }

            public override void Dessine(OpenGL gl)
            {
                double angle = _image * 2.0 * Math.PI;

                gl.Begin(OpenGL.GL_QUAD_STRIP);
                gl.TexCoord(0, 0); gl.Vertex(TAILLE, TAILLE * Math.Sin(angle), -TAILLE);
                gl.TexCoord(0, 1.0f); gl.Vertex(-TAILLE, TAILLE * Math.Sin(angle), -TAILLE);

                gl.TexCoord(0.5f, 0.0f); gl.Vertex(+TAILLE, -TAILLE * 0.1 * Math.Sin(angle), -TAILLE * 0.1);
                gl.TexCoord(0.5f, 1.0f); gl.Vertex(-TAILLE, -TAILLE * 0.1 * Math.Sin(angle), -TAILLE * 0.1);

                gl.TexCoord(0.6f, 0.0f); gl.Vertex(+TAILLE, TAILLE * 0.1 * Math.Sin(angle), TAILLE * 0.1);
                gl.TexCoord(0.6f, 1.0f); gl.Vertex(-TAILLE, TAILLE * 0.1 * Math.Sin(angle), TAILLE * 0.1);

                gl.TexCoord(1.0f, 0.0f); gl.Vertex(TAILLE, TAILLE * Math.Sin(angle), TAILLE);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(-TAILLE, TAILLE * Math.Sin(angle), TAILLE);
                gl.End();
            }

            public override void Update(Temps maintenant)
            {
                _Acceleration.y *= DIMINUE_ACCELERATION_V;
                _Vitesse.y *= DIMINUE_VITESSE_V;
                _couleur = Color.Black;
                base.Update(maintenant);
            }
        }
    }
}
