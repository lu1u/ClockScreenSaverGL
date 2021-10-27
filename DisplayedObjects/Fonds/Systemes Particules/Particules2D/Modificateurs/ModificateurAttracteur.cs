using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs
{
    internal class ModificateurAttracteur : Modificateur
    {
        private readonly float _g;
        private Trajectoire _traj;
        private const float SEUIL = 0.000002f;
        private static RectangleF bounds = new RectangleF(SystemeParticules2D.MIN_X, SystemeParticules2D.MIN_Y, SystemeParticules2D.LARGEUR, SystemeParticules2D.HAUTEUR);
        private static SizeF tailleEmetteur = new SizeF(0.1f, 0.1f);
        public ModificateurAttracteur(Trajectoire t, float G)
        {
            _traj = t;
            _g = G;
        }

        public override void Applique(SystemeParticules2D s, Temps maintenant)
        {

            _traj.Avance(bounds, tailleEmetteur, maintenant);
            float dG = _g * maintenant.intervalleDepuisDerniereFrame;
            int NbParticules = s._nbParticules;

            for (int i = 0; i < NbParticules; i++)
            {
                if (s._particules[i].active)
                {
                    // Distance de la particule a l'attracteur
                    float distX = s._particules[i].x - _traj._Px;
                    float distY = s._particules[i].y - _traj._Py;
                    double dist = Math.Sqrt((distX * distX) + (distY * distY));

                    // ================================================== Calcul de la distance
                    float Distance = (float)Math.Sqrt((distX * distX) + (distY * distY));
                    float DistanceCube = Distance * Distance * Distance;

                    if (DistanceCube > SEUIL)
                    {
                        float DistanceCubeDivDelaiImage = DistanceCube / maintenant.intervalleDepuisDerniereFrame;
                        s._particules[i].vx -= dG * (distX / DistanceCubeDivDelaiImage);
                        s._particules[i].vy -= dG * (distY / DistanceCubeDivDelaiImage);
                    }
                    else
                    {
                        s._particules[i].vx = _traj._Vx;
                        s._particules[i].vy = _traj._Vy;
                    }
                }
            }
        }
    }
}
