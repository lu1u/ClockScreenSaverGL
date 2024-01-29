using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Pong : Fond
    {
        #region Configuration
        private const string CAT = "Pong";
        private CategorieConfiguration c;
        private float COEFF_ACCELERATION_RAQUETTE, VITESSE_BALLE_X, VITESSE_BALLE_Y;
        private float CHANGE_LUMINANCE_TERRAIN;
        #endregion

        private const float MIN_VIEWPORT_X = -1;
        private const float MIN_VIEWPORT_Y = -1;
        private const float MAX_VIEWPORT_X = 1.0f;
        private const float MAX_VIEWPORT_Y = 1.0f;
        private const float TAILLE_PIXEL = 0.03f;
        private const float TAILLE_BALLE = TAILLE_PIXEL * 0.5f;
        private const float TAILLE_RAQUETTE = TAILLE_PIXEL * 10.0f;

        sealed private class Balle
        {
            public float x, y, dx, dy;
        }
        sealed private class Raquette
        {
            public float x, y;
        }

        private readonly Balle _balle;
        private readonly Raquette _raquetteGauche;
        private readonly Raquette _raquetteDroite;

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                COEFF_ACCELERATION_RAQUETTE = c.GetParametre("Coeff accélération raquette", 10.0f, a => COEFF_ACCELERATION_RAQUETTE = (float)Convert.ToDouble(a));
                VITESSE_BALLE_X = c.GetParametre("Vitesse balle X", 0.57f, a => _balle.dy = (float)Convert.ToDouble(a));
                VITESSE_BALLE_Y = c.GetParametre("Vitesse balle Y", 0.42f, a => _balle.dy = (float)Convert.ToDouble(a));
                CHANGE_LUMINANCE_TERRAIN = c.GetParametre("Luminance terrain", -0.2f, a => CHANGE_LUMINANCE_TERRAIN = (float)Convert.ToDouble(a));
            }
            return c;
        }

        public Pong(OpenGL gl) : base(gl)
        {
            c = GetConfiguration();
            _balle = new Balle
            {
                x = 0,
                y = 0,
                dx = VITESSE_BALLE_X,
                dy = VITESSE_BALLE_Y
            };

            _raquetteDroite = new Raquette
            {
                x = MAX_VIEWPORT_X - TAILLE_PIXEL,
                y = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y) / 2.0f
            };
            _raquetteGauche = new Raquette
            {
                x = MIN_VIEWPORT_X,
                y = (MAX_VIEWPORT_Y - MIN_VIEWPORT_Y) / 2.0f
            };

        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            gl.LoadIdentity();
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_BLEND);

            using (new Viewport2D(gl, MIN_VIEWPORT_X, MAX_VIEWPORT_Y, MAX_VIEWPORT_X, MIN_VIEWPORT_Y))
            {
                gl.Disable(OpenGL.GL_BLEND);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                SetColorWithLuminanceChange(gl, couleur, CHANGE_LUMINANCE_TERRAIN);
                gl.Begin(OpenGL.GL_QUADS);

                // Bande haute
                gl.Vertex(MIN_VIEWPORT_X, MIN_VIEWPORT_Y);
                gl.Vertex(MAX_VIEWPORT_X, MIN_VIEWPORT_Y);
                gl.Vertex(MAX_VIEWPORT_X, MIN_VIEWPORT_Y + TAILLE_PIXEL);
                gl.Vertex(MIN_VIEWPORT_X, MIN_VIEWPORT_Y + TAILLE_PIXEL);

                // Bande basse
                gl.Vertex(MIN_VIEWPORT_X, MAX_VIEWPORT_Y);
                gl.Vertex(MAX_VIEWPORT_X, MAX_VIEWPORT_Y);
                gl.Vertex(MAX_VIEWPORT_X, MAX_VIEWPORT_Y - TAILLE_PIXEL);
                gl.Vertex(MIN_VIEWPORT_X, MAX_VIEWPORT_Y - TAILLE_PIXEL);

                // Bande du milieu
                float y = MIN_VIEWPORT_Y + TAILLE_PIXEL * 1.5f;
                while (y < MAX_VIEWPORT_Y - TAILLE_PIXEL)
                {
                    gl.Vertex(-TAILLE_PIXEL / 2.0f, y);
                    gl.Vertex(+TAILLE_PIXEL / 2.0f, y);
                    gl.Vertex(+TAILLE_PIXEL / 2.0f, y + TAILLE_PIXEL * 2);
                    gl.Vertex(-TAILLE_PIXEL / 2.0f, y + TAILLE_PIXEL * 2);

                    y += TAILLE_PIXEL * 6.0f;
                }

                // Raquettes
                gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1.0f);
                gl.Vertex(_raquetteDroite.x + TAILLE_PIXEL, _raquetteDroite.y + TAILLE_RAQUETTE * 0.5f);
                gl.Vertex(_raquetteDroite.x, _raquetteDroite.y + TAILLE_RAQUETTE * 0.5f);
                gl.Vertex(_raquetteDroite.x, _raquetteDroite.y - TAILLE_RAQUETTE * 0.5f);
                gl.Vertex(_raquetteDroite.x + TAILLE_PIXEL, _raquetteDroite.y - TAILLE_RAQUETTE * 0.5f);

                gl.Vertex(_raquetteGauche.x - TAILLE_PIXEL, _raquetteGauche.y + TAILLE_RAQUETTE * 0.5f);
                gl.Vertex(_raquetteGauche.x + TAILLE_PIXEL, _raquetteGauche.y + TAILLE_RAQUETTE * 0.5f);
                gl.Vertex(_raquetteGauche.x + TAILLE_PIXEL, _raquetteGauche.y - TAILLE_RAQUETTE * 0.5f);
                gl.Vertex(_raquetteGauche.x - TAILLE_PIXEL, _raquetteGauche.y - TAILLE_RAQUETTE * 0.5f);


                // Balle
                gl.Vertex(_balle.x - TAILLE_BALLE, _balle.y + TAILLE_BALLE);
                gl.Vertex(_balle.x + TAILLE_BALLE, _balle.y + TAILLE_BALLE);
                gl.Vertex(_balle.x + TAILLE_BALLE, _balle.y - TAILLE_BALLE);
                gl.Vertex(_balle.x - TAILLE_BALLE, _balle.y - TAILLE_BALLE);
                gl.End();
            }

            LookArcade(gl, couleur);
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            DeplaceBalle(maintenant);

            if (_balle.dx > 0)
            {
                AccelereRaquetteVersBalle(_raquetteDroite, _balle, maintenant.intervalleDepuisDerniereFrame);
                Recentre(_raquetteGauche);
            }
            else
            {
                AccelereRaquetteVersBalle(_raquetteGauche, _balle, maintenant.intervalleDepuisDerniereFrame);
                Recentre(_raquetteDroite);
            }


            ContraintRaquette(_raquetteDroite);
            ContraintRaquette(_raquetteGauche);
        }

        private void Recentre(Raquette raquette)
        {
            raquette.y *= 0.9f;
        }

        /// <summary>
        /// Deplacement de la balle
        /// </summary>
        /// <param name="maintenant"></param>
        private void DeplaceBalle(Temps maintenant)
        {
            float X = _balle.x + (_balle.dx * maintenant.intervalleDepuisDerniereFrame);
            float Y = _balle.y + (_balle.dy * maintenant.intervalleDepuisDerniereFrame);

            if (X < MIN_VIEWPORT_X)
            {
                // Bord gauche
                _balle.dx = SignePlus(_balle.dx);
                _balle.x = MIN_VIEWPORT_X;
            }
            else
            if (X > MAX_VIEWPORT_X)
            {
                // Bord droit
                _balle.dx = SigneMoins(_balle.dx);
                _balle.x = MAX_VIEWPORT_X;
            }


            if (Y < MIN_VIEWPORT_Y)
            {
                // Haut
                _balle.dy = SignePlus(_balle.dy);
                _balle.y = MIN_VIEWPORT_Y;
            }
            else
                if (Y > MAX_VIEWPORT_Y)
            {
                // Bas
                _balle.dy = SigneMoins(_balle.dy);
                _balle.y = MAX_VIEWPORT_Y;
            }

            _balle.x = _balle.x + (_balle.dx * maintenant.intervalleDepuisDerniereFrame);
            _balle.y = _balle.y + (_balle.dy * maintenant.intervalleDepuisDerniereFrame);
        }


        /// <summary>
        /// S'assure que la raquette reste dans les limites du jeu
        /// </summary>
        /// <param name="raquette"></param>
        private void ContraintRaquette(Raquette raquette)
        {
            if (raquette.y - TAILLE_RAQUETTE * 0.5f < MIN_VIEWPORT_Y)
            {
                raquette.y = MIN_VIEWPORT_Y + TAILLE_RAQUETTE * 0.5f;
            }
            else
                if (raquette.y + TAILLE_RAQUETTE * 0.5f > MAX_VIEWPORT_Y)
            {
                raquette.y = MAX_VIEWPORT_Y - TAILLE_RAQUETTE * 0.5f;
            }
        }

        /// <summary>
        /// Accelerer la raquette en direction de la balle
        /// </summary>
        /// <param name="raquetteDroite"></param>
        private void AccelereRaquetteVersBalle(Raquette raquette, Balle balle, float intervalle)
        {
            raquette.y += (balle.y - raquette.y) * COEFF_ACCELERATION_RAQUETTE * intervalle;
        }
    }
}
