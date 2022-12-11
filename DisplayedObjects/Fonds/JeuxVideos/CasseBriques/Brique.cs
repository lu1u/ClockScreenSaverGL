using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Brique
    {
        public Brique(float x, float y, float largeur, float hauteur, float changeCouleur)
        {
            X = x;
            Y = y;
            Largeur = largeur;
            Hauteur = hauteur;
            Visible = true;
            ChangeCouleur = changeCouleur;
        }

        public float ChangeCouleur { get; set; }
        public bool Visible { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Largeur { get; set; }
        public float Hauteur { get; set; }
        public float Droite { get => X + Largeur; }
        public float Gauche { get => X - Largeur; }
        public float Haut { get => Y - Hauteur; }
        public float Bas { get => Y + Hauteur; }

        public Color Couleur { get; set; }
    }
}
