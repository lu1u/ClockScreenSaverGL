namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Balle
    {
        public enum COLLISION { DROITE, GAUCHE, HAUT, BAS, RIEN };
        public Balle(float x, float y, float vx, float vy, float largeur, float hauteur)
        {
            X = x;
            Y = y;
            Vx = vx;
            Vy = vy;
            Largeur = largeur;
            Hauteur = hauteur;
            Visible = true;
        }

        public bool Visible { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public float Vx { get; set; }
        public float Vy { get; set; }
        public float Largeur { get; set; }
        public float Hauteur { get; set; }
        public float Droite { get => X + Largeur; }
        public float Gauche { get => X - Largeur; }
        public float Haut { get => Y - Hauteur; }
        public float Bas { get => Y + Hauteur; }

        internal COLLISION Collision(Brique brique)
        {
            if (Droite + Vx > brique.Gauche && Gauche + Vx < brique.Droite && Bas + Vy > brique.Haut && Haut + Vy < brique.Bas)
            {
                float distanceX = Vx > 0 ? (Droite + Vx) - brique.Gauche : brique.Droite - (Gauche + Vx);
                float distanceY = Vy > 0 ? (Bas + Vy) - brique.Haut : brique.Bas - (Haut + Vx);
                if (distanceX < distanceY)
                    return Vx > 0 ? COLLISION.GAUCHE : COLLISION.DROITE;
                else
                    return Vy > 0 ? COLLISION.BAS : COLLISION.HAUT;
            }

            return COLLISION.RIEN;
        }
    }
}
