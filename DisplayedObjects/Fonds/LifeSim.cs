﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Utils;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Text;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class LifeSimulation : Fond
    {
        public const float MIN_VIEWPORT_X = -1.0f;
        public const float MIN_VIEWPORT_Y = -1.0f;
        public const float MAX_VIEWPORT_X = 1.0f;
        public const float MAX_VIEWPORT_Y = 1.0f;
        public const float LARGEUR_VIEWPORT = MAX_VIEWPORT_X - MIN_VIEWPORT_X;
        public const float HAUTEUR_VIEWPORT = MAX_VIEWPORT_Y - MIN_VIEWPORT_X;

        #region Configuration
        private const string CAT = "LifeSimulation";
        private CategorieConfiguration c;
        private bool REGLES_SYMETRIQUES = false;
        private float TAILLE_PARTICULE;
        private int NB_PARTICULES;
        private int NB_COULEURS;
        private float DISTANCE_MAX;
        private float FLUIDITE;
        private float COEFF_DISTANCE;
        private bool ADDITIF;
        private float ATTRACTION_MAX;
        private float ATTRACTION_MIN;
        #endregion

        sealed private class Particule
        {
            public int couleur;
            public float ax, ay;
            public float x, y, vx, vy;
        };

        private struct CouleurParticule
        {
            public byte R, G, B;
        }
        private Particule[] _particules;                    // Tableau des particules
        private float[,] _regles;                           // Tableau des regles d'attraction entre types de particules
        private CouleurParticule[] _couleursAffichees;                 // Tableau des couleurs visibles par type de particules
        private readonly Texture _texture = new Texture();


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                REGLES_SYMETRIQUES = c.GetParametre("Règles symétriques", false);
                TAILLE_PARTICULE = c.GetParametre("Taille particules", 0.01f, (a) => { TAILLE_PARTICULE = (float)Convert.ToDouble(a); });
                NB_PARTICULES = c.GetParametre("Nb particules", 4000);
                NB_COULEURS = c.GetParametre("Nb couleurs", 4);
                DISTANCE_MAX = c.GetParametre("Distance max", 0.12f, (a) => { DISTANCE_MAX = (float)Convert.ToDouble(a); });
                COEFF_DISTANCE = c.GetParametre("Coeff Distance", 1f, (a) => { COEFF_DISTANCE = (float)Convert.ToDouble(a); });
                FLUIDITE = c.GetParametre("Fluidité", 0.001f, (a) => { FLUIDITE = (float)Convert.ToDouble(a); });
                ADDITIF = c.GetParametre("Additif", false, (a) => { ADDITIF = Convert.ToBoolean(a); });
                ATTRACTION_MAX = c.GetParametre("Attraction max", 0.75f);
                ATTRACTION_MIN = c.GetParametre("Attraction min", 0.02f);
            }
            return c;
        }
        public LifeSimulation(OpenGL gl) : base(gl)
        {
        }

        protected override void Init(OpenGL gl)
        {
            string nomImage = c.GetParametre("Etoile", Configuration.GetImagePath("particuleTexture.png"));
            _texture.Create(gl, nomImage);

            _regles = new float[NB_COULEURS, NB_COULEURS];
            _particules = new Particule[NB_PARTICULES];
            _couleursAffichees = new CouleurParticule[NB_COULEURS];

            InitRegles();
            InitParticules();
        }

        /// <summary>
        /// Initialisation des regles d'attraction entre types de particules
        /// </summary>
        private void InitRegles()
        {
            for (int i = 0; i < NB_COULEURS; i++)
                for (int j = 0; j < NB_COULEURS; j++)
                    _regles[i, j] = FloatRandom(ATTRACTION_MIN, ATTRACTION_MAX) * SigneRandom();

            if (REGLES_SYMETRIQUES)
                for (int i = 0; i < NB_COULEURS; i++)
                    for (int j = i + 1; j < NB_COULEURS; j++)
                        _regles[i, j] = -_regles[j, i];
        }

        /// <summary>
        /// Initialisation du tableau des particules
        /// </summary>
        private void InitParticules()
        {
            for (int i = 0; i < NB_PARTICULES; i++)
            {
                Particule p = new Particule
                {
                    x = FloatRandom(MIN_VIEWPORT_X, MAX_VIEWPORT_X),
                    y = FloatRandom(MIN_VIEWPORT_Y, MAX_VIEWPORT_Y),
                    vx = 0,
                    vy = 0,
                    couleur = i % NB_COULEURS
                };

                _particules[i] = p;
            }
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.LoadIdentity();

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_FOG);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, ADDITIF ? OpenGL.GL_ONE : OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _texture.Bind(gl);
            RemplitCouleurs(couleur);

            using (new Viewport2D(gl, MIN_VIEWPORT_X, MIN_VIEWPORT_Y, MAX_VIEWPORT_X, MAX_VIEWPORT_Y))
            {
                gl.Begin(OpenGL.GL_QUADS);

                foreach (Particule p in _particules)
                {
                    CouleurParticule col = _couleursAffichees[p.couleur];
                    gl.Color(col.R, col.G, col.B, (byte)255);

                    gl.TexCoord(0.0f, 0.0f); gl.Vertex(p.x - TAILLE_PARTICULE, p.y - TAILLE_PARTICULE, 0);
                    gl.TexCoord(0.0f, 1.0f); gl.Vertex(p.x - TAILLE_PARTICULE, p.y + TAILLE_PARTICULE, 0);
                    gl.TexCoord(1.0f, 1.0f); gl.Vertex(p.x + TAILLE_PARTICULE, p.y + TAILLE_PARTICULE, 0);
                    gl.TexCoord(1.0f, 0.0f); gl.Vertex(p.x + TAILLE_PARTICULE, p.y - TAILLE_PARTICULE, 0);
                }

                gl.End();
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        /// <summary>
        /// Calcule le tableau des couleurs pour chaque type de particule, en fonction de la couleur globale courante
        /// </summary>
        /// <param name="couleur"></param>
        private void RemplitCouleurs(Color couleur)
        {
            CouleurGlobale clr = new CouleurGlobale(couleur);

            for (int i = 0; i < NB_COULEURS; i++)
            {
                Color col = clr.GetColorWithHueChange(i / (float)NB_COULEURS);
                _couleursAffichees[i].R = col.R;
                _couleursAffichees[i].G = col.G;
                _couleursAffichees[i].B = col.B;
            }
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            float DISTANCE_CARRE = DISTANCE_MAX * DISTANCE_MAX;

            for (int i = 0; i < NB_PARTICULES; i++)
            {
                Particule a = _particules[i];

                // Calculer l'acceleration entre cette particule et toutes les autres suffisament proches
                for (int j = i + 1; j < NB_PARTICULES; j++)
                {
                    Particule b = _particules[j];
                    float dx = a.x - b.x;
                    float dy = a.y - b.y;

                    float distance = (dx * dx) + (dy * dy);     // Distance au carré !!

                    // Controler la distance des particules
                    if (distance > 0 && distance < DISTANCE_CARRE)
                    {
                        // On ne calcule la racine carree que si la distance est courte, pour gagner du temps
                        distance = (float)Math.Sqrt(distance * COEFF_DISTANCE);

                        // Acceleration b => a
                        float F = _regles[a.couleur, b.couleur] / distance;
                        a.ax += F * dx;
                        a.ay += F * dy;

                        // Acceleration a => b
                        F = _regles[b.couleur, a.couleur] / distance;
                        b.ax -= F * dx;
                        b.ay -= F * dy;
                    }
                }

                // Appliquer l'acceleration
                a.vx += a.ax * FLUIDITE;
                a.vy += a.ay * FLUIDITE;
                // Appliquer le mouvement
                a.x += a.vx * maintenant.intervalleDepuisDerniereFrame;
                a.y += a.vy * maintenant.intervalleDepuisDerniereFrame;
                MathUtils.ContraintTore(ref a.x, MIN_VIEWPORT_X, MAX_VIEWPORT_X);
                MathUtils.ContraintTore(ref a.y, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);

                // Frein
                a.vx *= FLUIDITE;
                a.vy *= FLUIDITE;

                // Reinitialisation de l'acceleration pour la fois suivante
                a.ax = 0;
                a.ay = 0;
            }

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }
        public override void FillConsole(OpenGL gl)
        {
            base.FillConsole(gl);
            Console console = Console.GetInstance(gl);
            for (int i = 0; i < NB_COULEURS; i++)
            {
                StringBuilder s = new StringBuilder();
                for (int j = 0; j < NB_COULEURS; j++)
                {
                    s.Append(string.Format("{0,-8:0.0000}", _regles[i, j]) + "  ");
                }

                console.AddLigne(Color.AliceBlue, s.ToString());
            }
        }
    }



}
