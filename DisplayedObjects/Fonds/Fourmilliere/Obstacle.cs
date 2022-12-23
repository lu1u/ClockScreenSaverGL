using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Fourmilliere
{
    internal class Obstacle
    {
        private readonly float _x;
        private readonly float _y;
        private readonly float _taille;
        private readonly float _nuance;

        public Obstacle(float x, float y, float taille, float nuance)
        {
            _x = x;
            _y = y;
            _taille = taille;
            _nuance = nuance;
        }

        /// <summary>
        /// Affiche l'obstacle
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="couleur"></param>
        internal void Affiche(OpenGL gl, CouleurGlobale couleur, int details)
        {
            Color c = CouleurGlobale.Light(couleur.GetColorWithHueChange(_nuance), 0.4f);
            gl.Color(c.R, c.G, c.B, (byte)128);
            DisplayedObject.DessineCercle(gl, _x, _y, _taille, details);
        }

        /// <summary>
        /// Modifier le cap pour eviter la collision avec cet obstacle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cap"></param>
        /// <param name="v"></param>
        internal bool ModifieCap(float x, float y, float dx, float dy, float cap, ref float cumulVirage, float vitesse, float variationCap)
        {
            // Position de la fourmi au prochain pas
            float xProchain = x + dx;
            float yProchain = y + dy;

            // Distance (au carré, pour eviter de calculer des racines carrées) avec l'obstacle
            float distanceCarreProchain = ((_x - xProchain) * (_x - xProchain)) + ((_y - yProchain) * (_y - yProchain));
            if (distanceCarreProchain > _taille * _taille)
                // Pas besoin d'eviter cet obstacle
                return false;

            // Obstacle en approche!
            // Tourner à droite ou à gauche?
            float tourne = variationCap * (_taille / (float)Math.Sqrt(distanceCarreProchain)); // Plus on est proche, plus il faut tourner vite

            // Gauche
            float dxGauche = x + (vitesse * (float)Math.Cos(cap + tourne));
            float dyGauche = y + (vitesse * (float)Math.Sin(cap + tourne));
            float distanceCarreGauche = (_x - dxGauche) * (_x - dxGauche) + (_y - dyGauche) * (_y - dyGauche);

            // Droite
            float dxDroite = x + (vitesse * (float)Math.Cos(cap - tourne));
            float dyDroite = y + (vitesse * (float)Math.Sin(cap - tourne));
            float distanceCarreDroite = (_x - dxDroite) * (_x - dxDroite) + (_y - dyDroite) * (_y - dyDroite);

            if ((distanceCarreGauche < (_taille * _taille)) && (distanceCarreDroite < (_taille * _taille)))
            {
                // Demi tour d'urgence!
                cumulVirage = DisplayedObject.PI;
                return true;
            }

            if (distanceCarreGauche > distanceCarreDroite)
                cumulVirage += tourne;
            else
                cumulVirage -= tourne;

            return false;
        }
    }
}
