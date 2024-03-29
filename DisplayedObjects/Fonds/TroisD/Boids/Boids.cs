﻿using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Boids
{
    internal abstract class Boids : MateriauGlobal
    {
        #region Parametres

        public const float MAX_X = 10;
        public const float MAX_Y = 10;
        public const float MAX_Z = 10;
        public static float TAILLE, MAX_SPEED, MAX_FORCE, DISTANCE_VOISINS, SEPARATION, VITESSE_ANIMATION;
        protected int NB_BOIDS;
        #endregion
        protected List<Boid> _boids;
        protected float _angleCamera = 0;
        //uint _genLists = 0;
        protected Boids(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            _boids = new List<Boid>();
            InitBoids(_boids);
        }

        protected abstract Boid NewBoid();
        /// <summary>
        /// Initialisation du tableau de boids
        /// </summary>
        /// <param name="_boids"></param>
        protected virtual void InitBoids(List<Boid> _boids)
        {
            // for (int i = 0; i < NB_BOIDS; i++)
            //     _boids[i] = new Boid(r.Next(-MAX_X, MAX_X), r.Next(-MAX_Y, MAX_Y), r.Next(-MAX_Z, MAX_Z));
        }

        protected abstract void DessineBoid(OpenGL gl, float noImage);

        public override bool ClearBackGround(OpenGL gl, Color c)
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
            if (_boids.Count < NB_BOIDS)
            {
                Boid b = NewBoid();
                b._couleur = couleur;
                _boids.Add(b);
            }

            gl.LookAt(-0, -0, -MAX_Z * 1.5f, 0, 0, 0, 0, 1, 0);

            InitOpenGL(gl, maintenant, couleur);

            FrustumCulling frustum = new FrustumCulling(gl);
            foreach (var b in _boids.Where(b => frustum.isVisible(b._Position, TAILLE)))
            {
                float theta = b._Vitesse.Heading2D();
                theta = (float)(theta / Math.PI * 180.0);// - 90.0f;
                gl.Color(b._couleur.R / 256.0f, b._couleur.G / 256.0f, b._couleur.B / 256.0f);
                gl.PushMatrix();
                gl.Translate(b._Position.x, b._Position.y, b._Position.z);
                gl.Rotate(0, 0, theta);
                b.Dessine(gl);// gl.CallList(_genLists + (uint)b._image);

                gl.PopMatrix();
            }



#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        protected abstract void InitOpenGL(OpenGL gl, Temps maintenant, Color couleur);

        /// <summary>
        /// Deplacement des boids
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            _angleCamera += maintenant.intervalleDepuisDerniereFrame * 1.0f;

            foreach (Boid b in _boids)
                b.Flock(_boids);

            float dImage = maintenant.intervalleDepuisDerniereFrame * VITESSE_ANIMATION;
            foreach (Boid b in _boids)
            {
                b.Update(maintenant);
                b._image += dImage * b._vitesseAnimation;
            }
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }


        /// <summary>
        /// Classe des "boids": objets se deplacant en bancs
        /// </summary>
        protected abstract class Boid
        {
            private const float TWO_PI = (float)Math.PI * 2.0f;
            public Vecteur3D _Position;
            public Vecteur3D _Vitesse;
            public Vecteur3D _Acceleration;
            public float _image;
            public float _vitesseAnimation;
            public Color _couleur;
            protected Boid(float x, float y, float z)
            {
                _Acceleration = new Vecteur3D(0, 0);
                _image = FloatRandom(0, (float)(Math.PI * 2.0));
                _vitesseAnimation = FloatRandom(0.7f, 1.3f);
                float angle = FloatRandom(0, TWO_PI);
                float vitesse = FloatRandom(-MAX_SPEED, MAX_SPEED);
                _Vitesse = new Vecteur3D((float)Math.Cos(angle) * vitesse, (float)Math.Sin(angle) * vitesse);
                _Position = new Vecteur3D(x, y, z);
            }

            // We accumulate a new acceleration each time based on three rules
            public void Flock(List<Boid> boids)
            {
                Flocking(boids, out Vecteur3D separation, out Vecteur3D alignement, out Vecteur3D cohesion);

                // Arbitrarily weight these forces
                separation.Multiplier_par(1.5f);
                //ali.multiplier_par(1.0f);
                //coh.multiplier_par(1.0f);
                // additionner the force vectors to acceleration
                _Acceleration.Additionner(separation);
                _Acceleration.Additionner(alignement);
                _Acceleration.Additionner(cohesion);
            }

            private void Flocking(List<Boid> boids, out Vecteur3D sep, out Vecteur3D ali, out Vecteur3D coh)
            {
                sep = new Vecteur3D();
                ali = new Vecteur3D();
                coh = new Vecteur3D();
                int countSep = 0;
                int countAlign = 0;
                int countCoh = 0;

                foreach (Boid other in boids)
                    if (other != this)
                    {
                        float d = _Position.Distance(other._Position);

                        if (d < SEPARATION)
                        {
                            // Separation
                            Vecteur3D diff = _Position - other._Position;
                            diff.Normalize();
                            diff.Diviser_par(d);        // Weight by distance
                            sep.Additionner(diff);
                            countSep++;            // Keep track of how many
                        }

                        if (d < DISTANCE_VOISINS)
                        {
                            // Alignement
                            ali.Additionner(other._Vitesse);
                            countAlign++;

                            // Cohesion
                            coh.Additionner(other._Position); // additionner location
                            countCoh++;
                        }
                    }

                // Separation
                if (countSep > 0)
                {
                    sep.Diviser_par(countSep);
                    // As long as the vector is greater than 0
                    if (sep.Longueur() > 0)
                    {
                        // Implement Reynolds: Steering = Desired - Velocity
                        sep.Normalize();
                        sep.Multiplier_par(MAX_SPEED);
                        sep.Soustraire(_Vitesse);
                        sep.Limiter(MAX_FORCE);
                    }
                }

                // Alignement
                if (countAlign > 0)
                {
                    ali.Diviser_par(countAlign);
                    ali.Normalize();
                    ali.Multiplier_par(MAX_SPEED);
                    ali.Soustraire(_Vitesse);
                    ali.Limiter(MAX_FORCE);

                }

                // Cohesion
                if (countCoh > 0)
                {
                    coh.Diviser_par(countCoh);
                    coh = Seek(coh);
                }
            }

            // Method to update location
            public virtual void Update(Temps maintenant)
            {
                // Update velocity
                _Vitesse.Additionner(_Acceleration);
                // Limiter speed
                _Vitesse.Limiter(MAX_SPEED);
                _Position.Additionner(_Vitesse * maintenant.intervalleDepuisDerniereFrame);
                // Reset accelertion to 0 each cycle
                _Acceleration.Set(0, 0, 0);

                Restreint();
            }

            protected virtual void Restreint()
            {
                Restreint(ref _Position.x, MAX_X);
                Restreint(ref _Position.y, MAX_Y);
                Restreint(ref _Position.z, MAX_Z);
            }

            private void Restreint(ref float v, float max)
            {
                max *= 1.5f;
                while (v > max)
                    v = -max;

                while (v < -max)
                    v = max;
            }

            // A method that calculates and applies a steering force towards a target
            // STEER = DESIRED MINUS VELOCITY
            private Vecteur3D Seek(Vecteur3D target)
            {
                Vecteur3D desired = target - _Position;  // A vector pointing from the location to the target
                                                         // Scale to maximum speed
                desired.Normalize();
                desired.Multiplier_par(MAX_SPEED);

                // Steering = Desired minus Velocity
                Vecteur3D steer = desired - _Vitesse;
                steer.Limiter(MAX_FORCE);  // Limiter to maximum steering force
                return steer;
            }

            public abstract void Dessine(OpenGL gl);

        }
    }
}
