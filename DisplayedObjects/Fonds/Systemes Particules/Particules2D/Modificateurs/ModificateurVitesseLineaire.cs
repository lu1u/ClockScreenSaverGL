﻿namespace ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs
{
    internal class ModificateurVitesseLineaire : Modificateur
    {
        public override void Applique(SystemeParticules2D s, Temps maintenant)
        {
            for (int i = 0; i < s._nbParticules; i++)
                if (s._particules[i].active)
                {
                    s._particules[i].x += s._particules[i].vx * maintenant.intervalleDepuisDerniereFrame;
                    s._particules[i].y += s._particules[i].vy * maintenant.intervalleDepuisDerniereFrame;
                }
        }
    }
}
