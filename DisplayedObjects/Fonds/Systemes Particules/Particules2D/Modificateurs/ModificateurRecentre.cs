namespace ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs
{
    internal class ModificateurRecentre : Modificateur
    {
        private readonly float _vitesse;
        public ModificateurRecentre(float vitesse)
        {
            _vitesse = vitesse;
        }

        public override void Applique(SystemeParticules2D s, Temps maintenant)
        {
            float decalage = _vitesse * maintenant.intervalleDepuisDerniereFrame;
            int indice = 0;
            float tailleMax = -1;
            for (int i = 0; i < s._nbParticules; i++)
                if (s._particules[i].active)
                    if (s._particules[i].taille > tailleMax)
                    {
                        indice = i;
                        tailleMax = s._particules[i].taille;
                    }


            // Decaler toutes les particules pour attirer la plus grosse au centre
            float decalageX = s._particules[indice].x * decalage;
            float decalageY = s._particules[indice].y * decalage;
            float decalageVX = s._particules[indice].vx * decalage;
            float decalageVY = s._particules[indice].vy * decalage;

            for (int i = 0; i < s._nbParticules; i++)
                if (s._particules[i].active)
                {
                    s._particules[i].x -= decalageX;
                    s._particules[i].y -= decalageY;
                    s._particules[i].vx -= decalageVX;
                    s._particules[i].vy -= decalageVY;
                }
        }
    }
}
