using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs
{
    internal class ModificateurAttracteurMutuelle : Modificateur
    {
        private readonly float _g;
        private readonly float _multDist;
        private const float SEUIL = 0.5f;
        public ModificateurAttracteurMutuelle(float G, float MultDist)
        {
            _g = G;
            _multDist = MultDist;
        }

        public override void Applique(SystemeParticules2D s, Temps maintenant)
        {
            int NbParticules = s._nbParticules;

            for (int i = 0; i < NbParticules; i++)
            {
                if (s._particules[i].active)
                {
                    for (int j = i + 1; j < NbParticules; j++)
                    {
                        if (s._particules[j].active)
                        {
                            // Distance de la particule a l'attracteur
                            float distX = (s._particules[i].x - s._particules[j].x) * _multDist;
                            float distY = (s._particules[i].y - s._particules[j].y) * _multDist;
                            double dist = Math.Sqrt((distX * distX) + (distY * distY));
                            if (dist > (s._particules[i].taille + s._particules[j].taille) * SEUIL * _multDist)
                            {
                                // ================================================== Calcul de la distance
                                float Distance = (float)Math.Sqrt((distX * distX) + (distY * distY));
                                float DistanceCube = Distance * Distance * Distance;

                                if (DistanceCube != 0)
                                {
                                    float DistanceCubeDivDelaiImage = _g * DistanceCube / maintenant.intervalleDepuisDerniereFrame;
                                    s._particules[i].vx -= s._particules[j].taille * (distX / DistanceCubeDivDelaiImage);
                                    s._particules[j].vx += s._particules[i].taille * (distX / DistanceCubeDivDelaiImage);

                                    s._particules[i].vy -= s._particules[j].taille * (distY / DistanceCubeDivDelaiImage);
                                    s._particules[j].vy += s._particules[i].taille * (distY / DistanceCubeDivDelaiImage);
                                }

                                //    // Attraction proportionnelle a la distance au carre
                                //    float DistanceCarre = (float)(dist * dist);
                                //
                                //
                                //s._particules[i].vx -= s._particules[j].taille * (dG * (distX / DistanceCarre));
                                //s._particules[i].vy -= s._particules[j].taille * (dG * (distY / DistanceCarre));
                                //
                                //s._particules[j].vx += s._particules[i].taille * (dG * (distX / DistanceCarre));
                                //s._particules[j].vy += s._particules[i].taille * (dG * (distY / DistanceCarre));
                            }
                            else
                            {
                                // Particule rentrent en contact -> fusionnent
                                float masseTotale = s._particules[i].taille + s._particules[j].taille;
                                s._particules[i].vx = ((s._particules[i].vx * s._particules[i].taille) + (s._particules[j].vx * s._particules[j].taille)) / masseTotale;
                                s._particules[i].vy = ((s._particules[i].vy * s._particules[i].taille) + (s._particules[j].vy * s._particules[j].taille)) / masseTotale;
                                s._particules[i].taille = (float)Math.Sqrt((s._particules[i].taille * s._particules[i].taille) + (s._particules[j].taille * s._particules[j].taille));
                                s._particules[j].active = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
