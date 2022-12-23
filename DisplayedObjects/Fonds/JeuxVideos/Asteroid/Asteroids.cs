using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class Asteroids : Fond
    {
        #region Configuration
        private const string CAT = "Asteroid";
        private CategorieConfiguration c;
        int NB_ASTEROID_DEPART;
        float ROTATION_ASTEROID;
        float LARGEUR_LIGNE;
        float TAILLE_POINT;
        private int NB_MAX_PARTICULE;
        private int NB_PARTICULE_EXPLOSION;
        private float PROBA_POUSSEE;
        private float PROBA_GAUCHE;
        private float PROBA_DROITE;
        private float PROBA_TIR;

        #endregion
        private readonly float MIN_VIEWPORT_X = -1.3f;
        private readonly float MIN_VIEWPORT_Y = -1;
        private readonly float MAX_VIEWPORT_X = 1.3f;
        private readonly float MAX_VIEWPORT_Y = 1.0f;
        List<Asteroid> _asteroids;
        TimerIsole _timerEcran;
        Vaisseau _vaisseau;
        private readonly TextureAsynchrone _textureStart;
        private readonly TextureAsynchrone _textureComplete;

        enum MODE_JEU { START, NORMAL, COMPLETE };
        MODE_JEU _modeJeu;

        sealed private class Particule
        {
            public float x, y, vx, vy, alpha;
        }

        List<Particule> _particules;

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_ASTEROID_DEPART = c.GetParametre("Nb astéroides départ", 4);
                LARGEUR_LIGNE = c.GetParametre("Largeur lignes", 1.0f, a => LARGEUR_LIGNE = (float)Convert.ToDouble(a));
                ROTATION_ASTEROID = c.GetParametre("Rotation astéroide", 1.0f);
                PROBA_DROITE = c.GetParametre("Proba droite", 0.1f, a => PROBA_DROITE = (float)Convert.ToDouble(a));
                PROBA_GAUCHE = c.GetParametre("Proba gauche", 0.1f, a => PROBA_GAUCHE = (float)Convert.ToDouble(a));
                PROBA_POUSSEE = c.GetParametre("Proba poussée", 0.1f, a => PROBA_DROITE = (float)Convert.ToDouble(a));
                PROBA_TIR = c.GetParametre("Proba tir", 0.1f, a => PROBA_TIR = (float)Convert.ToDouble(a));
                TAILLE_POINT = c.GetParametre("Taille points", 2.0f, a => TAILLE_POINT = (float)Convert.ToDouble(a));
                NB_MAX_PARTICULE = c.GetParametre("Nb max particules explosion", 10000);
                NB_PARTICULE_EXPLOSION = c.GetParametre("NB particules explosion", 10, a => NB_PARTICULE_EXPLOSION = Convert.ToInt32(a));
                Vaisseau.FREINAGE_INERTIEL = c.GetParametre("Freinage intertiel", 0.97f, a => Vaisseau.FREINAGE_INERTIEL = (float)Convert.ToDouble(a));
                Vaisseau.VITESSE_TIR = c.GetParametre("Vitesse tir", 0.5f, a => Vaisseau.VITESSE_TIR = (float)Convert.ToDouble(a));
                Vaisseau.VIE_TIR = c.GetParametre("Durée vie tir", 3.0f, a => Vaisseau.VIE_TIR = (float)Convert.ToDouble(a));
                Vaisseau.NB_MAX_TIRS = c.GetParametre("Nb max tirs", 5, a => Vaisseau.NB_MAX_TIRS = Convert.ToInt32(a));
                Asteroid.NIVEAU_MAX = c.GetParametre("Niveau max", 3);
                Asteroid.TAILLE_MAX = c.GetParametre("Taille asteroides", 0.15f, a => Asteroid.TAILLE_MAX = (float)Convert.ToDouble(a));
                Asteroid.NB_COINS = c.GetParametre("Nb coins asteroides", 12);
            }
            return c;
        }

        public Asteroids(OpenGL gl) : base(gl)
        {
            c = GetConfiguration();
            float largeurEcran = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            float hauteurEcran = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            float ratio = largeurEcran / hauteurEcran;
            if (ratio > 1)
            {
                MAX_VIEWPORT_X = MAX_VIEWPORT_Y * ratio;
                MIN_VIEWPORT_X = MIN_VIEWPORT_Y * ratio;
            }
            else
            {
                MAX_VIEWPORT_Y /= ratio;
                MIN_VIEWPORT_Y /= ratio;
            }

            _textureStart = new TextureAsynchrone(gl, Configuration.GetImagePath("Asteroids\\start.png"));
            _textureComplete = new TextureAsynchrone(gl, Configuration.GetImagePath("Asteroids\\complete.png"));
            _textureStart.Init();
            _textureComplete.Init();
        }

        protected override void Init(OpenGL gl)
        {
            _asteroids = new List<Asteroid>();

            for (int i = 0; i < NB_ASTEROID_DEPART; i++)
                _asteroids.Add(new Asteroid(0,
                                           FloatRandom(MIN_VIEWPORT_X, MAX_VIEWPORT_X), FloatRandom(MIN_VIEWPORT_Y, MAX_VIEWPORT_Y),
                                            FloatRandom(ROTATION_ASTEROID * 0.5f, ROTATION_ASTEROID) * SigneRandom(),
                                            FloatRandom(0.01f, 0.2f) * SigneRandom(), FloatRandom(0.01f, 0.25f) * SigneRandom(), random));

            _vaisseau = new Vaisseau(0.05f, 0, 0, 0, 0, 0);
            _particules = new List<Particule>();
            SwitchToModeStart();
        }

        private void SwitchToModeStart()
        {
            _modeJeu = MODE_JEU.START;
            _timerEcran = new TimerIsole(5000);
        }

        private void SwitchToModeNormal()
        {
            _modeJeu = MODE_JEU.NORMAL;
            _timerEcran = null;
        }

        private void SwitchToModeComplete()
        {
            Init(_gl);
            _modeJeu = MODE_JEU.COMPLETE;
            _timerEcran = new TimerIsole(5000);
        }



        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.LineWidth(LARGEUR_LIGNE);
            gl.PointSize(TAILLE_POINT);

            using (new Viewport2D(gl, MIN_VIEWPORT_X, MAX_VIEWPORT_Y, MAX_VIEWPORT_X, MIN_VIEWPORT_Y))
            {
                switch (_modeJeu)
                {
                    case MODE_JEU.START:
                        AfficheStart(gl, couleur);
                        break;
                    case MODE_JEU.NORMAL:
                        AfficheNormal(gl, couleur);
                        break;
                    case MODE_JEU.COMPLETE:
                        AfficheComplete(gl, couleur);
                        break;

                }
            }

            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }


        private void AfficheStart(OpenGL gl, Color couleur)
        {
            // Asteroides
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_BLEND);
            OpenGLColor.Couleur(gl, couleur, 0.5f);
            foreach (Asteroid a in _asteroids)
                a?.Affiche(gl);

            if (_textureStart.Pret)
            {
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                OpenGLColor.Couleur(gl, couleur);
                _textureStart.Texture.Bind(gl);

                // Surface de jeu
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(MIN_VIEWPORT_X, MAX_VIEWPORT_Y);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(MAX_VIEWPORT_X, MAX_VIEWPORT_Y);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(MAX_VIEWPORT_X, MIN_VIEWPORT_Y);
                gl.TexCoord(0.0f, 0.0f); gl.Vertex(MIN_VIEWPORT_X, MIN_VIEWPORT_Y);
                gl.End();
            }
        }

        private void AfficheComplete(OpenGL gl, Color couleur)
        {
            if (_textureComplete.Pret)
            {
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.Enable(OpenGL.GL_BLEND);
                OpenGLColor.Couleur(gl, couleur, 0.5f);
                _textureComplete.Texture.Bind(gl);

                // Surface de jeu
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(MIN_VIEWPORT_X, MAX_VIEWPORT_Y);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(MAX_VIEWPORT_X, MAX_VIEWPORT_Y);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(MAX_VIEWPORT_X, MIN_VIEWPORT_Y);
                gl.TexCoord(0.0f, 0.0f); gl.Vertex(MIN_VIEWPORT_X, MIN_VIEWPORT_Y);
                gl.End();
            }
        }

        private void AfficheNormal(OpenGL gl, Color couleur)
        {
            // Particules
            gl.Enable(OpenGL.GL_BLEND);
            using (new GLBegin(gl, OpenGL.GL_POINTS))
                foreach (Particule p in _particules)
                {
                    gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, p.alpha);
                    gl.Vertex(p.x, p.y);
                }

            // Asteroides
            gl.Disable(OpenGL.GL_BLEND);
            OpenGLColor.ColorWithLuminance(gl, couleur, -0.1f);
            gl.LineWidth(LARGEUR_LIGNE);
            foreach (Asteroid a in _asteroids)
                a?.Affiche(gl);

            // Vaisseau et ses tirs
            OpenGLColor.ColorWithLuminance(gl, couleur, 0.2f);
            _vaisseau?.Affiche(gl);
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            switch (_modeJeu)
            {
                case MODE_JEU.START:
                    DeplaceStart(maintenant);
                    break;
                case MODE_JEU.NORMAL:
                    DeplaceNormal(maintenant);
                    break;
                case MODE_JEU.COMPLETE:
                    DeplaceComplete();
                    break;

            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        private void DeplaceStart(Temps maintenant)
        {
            // Deplacer les asteroides
            foreach (Asteroid a in _asteroids)
                a?.Deplace(maintenant, MIN_VIEWPORT_X, MAX_VIEWPORT_X, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);

            if (_timerEcran.Ecoule())
                SwitchToModeNormal();
        }

        private void DeplaceComplete()
        {
            if (_timerEcran.Ecoule())
                SwitchToModeNormal();
        }

        /// <summary>
        /// Deplacements en mode normal
        /// </summary>
        /// <param name="maintenant"></param>
        private void DeplaceNormal(Temps maintenant)
        {
            if (_asteroids.Count == 0)
            {
                // Partie gagnee!!
                SwitchToModeComplete();
                return;
            }

            // Collisions Tir-Asteroides
            CollisionsTirAsteroides();

            // Deplacer les asteroides
            foreach (Asteroid a in _asteroids)
                a?.Deplace(maintenant, MIN_VIEWPORT_X, MAX_VIEWPORT_X, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);

            // Deplacer le vaisseau
            _vaisseau?.Deplace(maintenant, MIN_VIEWPORT_X, MAX_VIEWPORT_X, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);

            ActionVaisseau();
            DeplaceParticules(maintenant);
        }

        private void ActionVaisseau()
        {
            if (Probabilite(PROBA_POUSSEE))
                _vaisseau.Poussee();

            if (Probabilite(PROBA_GAUCHE))
                _vaisseau.TournerAGauche();

            if (Probabilite(PROBA_DROITE))
                _vaisseau.TournerADroite();

            if (Probabilite(PROBA_TIR))
                _vaisseau.Tirer();
            // Action du "joueur"
            //switch (r.Next(PROBA_MOUVEMENT))
            //{
            //    case 0:
            //        _vaisseau.Poussee();
            //        break;
            //    case 1:
            //        _vaisseau.TournerAGauche();
            //        break;
            //    case 2:
            //        _vaisseau.TournerADroite();
            //        break;
            //    case 3:
            //        _vaisseau.Tirer();
            //        break;
            //}
        }


        /// <summary>
        /// Deplace les particules des explosions
        /// </summary>
        /// <param name="maintenant"></param>
        private void DeplaceParticules(Temps maintenant)
        {
            // Deplacer les particules
            int i = 0;
            while (i < _particules.Count)
            {
                Particule p = _particules[i];
                if ((p.alpha > 0.001f) && (p.x > MIN_VIEWPORT_X) && (p.x < MAX_VIEWPORT_X) && (p.y > MIN_VIEWPORT_Y) && (p.y < MAX_VIEWPORT_Y))
                {
                    p.alpha -= (0.6f * maintenant.intervalleDepuisDerniereFrame);
                    p.x += p.vx * maintenant.intervalleDepuisDerniereFrame;
                    p.y += p.vy * maintenant.intervalleDepuisDerniereFrame;
                    i++;
                }
                else
                    // Duree de vie epuisee ou particule sortie de l'ecran, supprimer cette particule
                    _particules.RemoveAt(i);
            }
        }

        /// <summary>
        /// Gere les collisions des tirs avec les asteroides
        /// </summary>
        private void CollisionsTirAsteroides()
        {
            List<Vaisseau.Tir> _tirs = _vaisseau.Tirs;
            int indiceTir = 0;
            // Parcourir tous les tirs
            while (indiceTir < _tirs.Count)
            {
                Vaisseau.Tir tir = _tirs[indiceTir];

                // Comparer la position du tir avec celle de tous les asteroides
                int indiceAsteroide = 0;
                bool collision = false;
                while (indiceAsteroide < _asteroids.Count && !collision)
                {
                    Asteroid asteroide = _asteroids[indiceAsteroide];
                    if (asteroide.Collision(tir.x, tir.y))
                    {
                        collision = true;
                        AjouteParticules(asteroide.X, asteroide.Y, asteroide.Dx, asteroide.Dy);
                        if (asteroide.Niveau < Asteroid.NIVEAU_MAX)
                        {
                            // Si l'asteroide n'est pas deja petit, placer deux asteroides plus petits au meme endroit
                            _asteroids.Add(new Asteroid(asteroide.Niveau + 1, asteroide.X, asteroide.Y, asteroide.VRot, -asteroide.Dx, asteroide.Dy, random));
                            _asteroids.Add(new Asteroid(asteroide.Niveau + 1, asteroide.X, asteroide.Y, -asteroide.VRot, asteroide.Dx, -asteroide.Dy, random));
                        }
                        _asteroids.RemoveAt(indiceAsteroide);       // Le .add precedent ajoute a la fin de la liste, donc pas d'interference avec indiceAsteroide
                    }
                    else
                        indiceAsteroide++;
                }

                if (collision)
                    _tirs.RemoveAt(indiceTir);
                else
                    indiceTir++;
            }
        }

        /// <summary>
        /// Ajoute des particules pour une explosion
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        private void AjouteParticules(float x, float y, float dx, float dy)
        {
            if (_particules.Count > NB_MAX_PARTICULE)
                _particules.RemoveRange(0, NB_PARTICULE_EXPLOSION * 2);

            for (int i = 0; i < NB_PARTICULE_EXPLOSION; i++)
            {
                float angle = FloatRandom(0, DEUX_PI);
                float vitesse = FloatRandom(0.001f, 0.2f);
                Particule p = new Particule
                {
                    x = x,
                    y = y,
                    vx = dx + vitesse * (float)Math.Cos(angle),
                    vy = dy + vitesse * (float)Math.Sin(angle)
                };
                _particules.Add(p);
            }
        }
    }
}
