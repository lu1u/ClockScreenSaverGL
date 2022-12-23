using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Fourmilliere
{
    internal class GrilleMarqueur
    {
        private readonly float[,] _marqueurs;
        private readonly int _LARGEUR, _HAUTEUR;
        public GrilleMarqueur(int l, int h)
        {
            _LARGEUR = l;
            _HAUTEUR = h;
            _marqueurs = new float[_LARGEUR, _HAUTEUR];

            //Random r = new Random();
            //for (int x = 0; x < _LARGEUR; x++)
            //    for (int y = 0; y < _HAUTEUR; y++)
            //    {
            //        _marqueurs[x, y] = (float)r.NextDouble();
            //    }
        }


        public void Affiche(OpenGL gl, float maxPheromone, Color couleur)
        {
            if (maxPheromone <= 0)
                return;

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            for (int x = 0; x < _LARGEUR; x++)
                for (int y = 0; y < _HAUTEUR; y++)
                    if (_marqueurs[x, y] > 0.01f)
                    {
                        int alpha = Math.Min(255, (int)(_marqueurs[x, y] / maxPheromone * 255));
                        Color c = OpenGLColor.GetCouleurOpaqueAvecAlpha(couleur, Convert.ToByte(alpha));
                        gl.Color(Convert.ToByte(c.R), Convert.ToByte(c.G), Convert.ToByte(c.B));

                        gl.Begin(OpenGL.GL_QUADS);
                        gl.Vertex((float)x / _LARGEUR, (float)y / _HAUTEUR);
                        gl.Vertex((float)x / _LARGEUR, (float)(y + 1) / _HAUTEUR);
                        gl.Vertex((float)(x + 1) / _LARGEUR, (float)(y + 1) / _HAUTEUR);
                        gl.Vertex((float)(x + 1) / _LARGEUR, (float)y / _HAUTEUR);
                        gl.End();
                    }
        }

        /// <summary>
        /// Ajoute un peu de phéromone aux coordonnees donnees
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void PoseMarqueur(float x, float y, float valeur)
        {
            int X = (int)Math.Round(x * _LARGEUR);
            int Y = (int)Math.Round(y * _HAUTEUR);
            X = Contraint(X, 0, _LARGEUR - 1);
            Y = Contraint(Y, 0, _HAUTEUR - 1);

            _marqueurs[X, Y] += valeur;
            if (_marqueurs[X, Y] > 1.0f)
                _marqueurs[X, Y] = 1.0f;
        }

        internal void Renforce(float x, float y, float valeur) => PoseMarqueur(x, y, valeur);

        /// <summary>
        /// Diminue la valeur de tous les marqueurs
        /// </summary>
        /// <param name="v"></param>
        /// <returns>Valeur du marqueur le plus fort</returns>
        internal float Evapore(float v)
        {
            float max = float.MinValue;

            for (int x = 0; x < _LARGEUR; x++)
                for (int y = 0; y < _HAUTEUR; y++)
                {
                    if (_marqueurs[x, y] > v)
                        _marqueurs[x, y] -= v;
                    else
                        _marqueurs[x, y] = 0;

                    if (_marqueurs[x, y] > max)
                        max = _marqueurs[x, y];
                }

            return max;
        }


        /// <summary>
        /// Chercher la position du marqueur le plus fort dans la zone de perception
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="distancePerception"></param>
        /// <returns></returns>
        /// <param name="xMarqueur"></param><param name="yMarqueur"></param>
        internal bool chercheMarqueurMax(float x, float y, float distancePerception, out float xMarqueur, out float yMarqueur)
        {
            int minX = Contraint((int)((x - distancePerception) * _LARGEUR), 0, _LARGEUR);
            int maxX = Contraint((int)((x + distancePerception) * _LARGEUR), 0, _LARGEUR);
            int minY = Contraint((int)((y - distancePerception) * _HAUTEUR), 0, _HAUTEUR);
            int maxY = Contraint((int)((y + distancePerception) * _HAUTEUR), 0, _HAUTEUR);

            xMarqueur = 0;
            yMarqueur = 0;
            float poidsMax = 0; // Valeur min des cases, pas Float.MinValue!!
            bool trouve = false;

            for (int X = minX; X < maxX; X++)
                for (int Y = minY; Y < maxY; Y++)
                    if (_marqueurs[X, Y] > poidsMax)
                    {
                        trouve = true;
                        poidsMax = _marqueurs[X, Y];
                        xMarqueur = X;
                        yMarqueur = Y;
                    }

            // Convertir les indices en coordonnees 0..1
            xMarqueur = xMarqueur / _LARGEUR;
            yMarqueur = yMarqueur / _HAUTEUR;

            return trouve;
        }
        internal bool chercheMarqueurMin(float x, float y, float distancePerception, out float xMarqueur, out float yMarqueur)
        {
            int minX = Contraint((int)((x - distancePerception) * _LARGEUR), 0, _LARGEUR);
            int maxX = Contraint((int)((x + distancePerception) * _LARGEUR), 0, _LARGEUR);
            int minY = Contraint((int)((y - distancePerception) * _HAUTEUR), 0, _HAUTEUR);
            int maxY = Contraint((int)((y + distancePerception) * _HAUTEUR), 0, _HAUTEUR);
            xMarqueur = 0;
            yMarqueur = 0;
            float poidsMin = float.MaxValue;

            bool trouve = false;
            for (int X = minX; X < maxX; X++)
                for (int Y = minY; Y < maxY; Y++)
                {
                    float poids = _marqueurs[X, Y];
                    if (poids > 0 && poids < poidsMin)
                    {
                        trouve = true;
                        poidsMin = _marqueurs[X, Y];
                        xMarqueur = X;
                        yMarqueur = Y;
                    }
                }
            // Convertir les indices en coordonnees 0..1
            xMarqueur = xMarqueur / _LARGEUR;
            yMarqueur = yMarqueur / _HAUTEUR;

            return trouve;
        }
        public static int Contraint(int v, int min, int max)
        {
            if (v < min)
                return min;
            if (v > max)
                return max;
            return v;
        }
    }
}
