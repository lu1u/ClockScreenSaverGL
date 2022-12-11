using System.Collections.Generic;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Fourmilliere
{
    internal class Monde
    {
        public GrilleMarqueur _pheromonesRechercheNourriture;
        public GrilleMarqueur _grilleRapporteNourriture;
        public List<Obstacle> _listeObstacles = new List<Obstacle>();

        public float X_NID;
        public float Y_NID;
        public float X_NOURRITURE;
        public float Y_NOURRITURE;
        public float TAILLE_NOURRITURE;
        public float TAILLE_NID;
        public float DISTANCE_PERCEPTION;
        public float VALEUR_RENFORCE;
        public float VARIATION_CAP;
        public float VARIATION_CAP_OBSTACLE;

        public int DELAI_MARQUEUR { get; internal set; }

        public Monde(int taille)
        {
            _pheromonesRechercheNourriture = new GrilleMarqueur(taille, taille);
            _grilleRapporteNourriture = new GrilleMarqueur(taille, taille);
        }
    }
}
