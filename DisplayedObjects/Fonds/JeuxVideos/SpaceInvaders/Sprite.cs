using SharpGL;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class Sprite
    {
        public int image;
        public float changeCouleur;

        public Sprite(float X, float Y, int Image, float ChangeCouleur = 0.0f)
        {
            this.X = X;
            this.Y = Y;
            image = Image;
            changeCouleur = ChangeCouleur;
        }

        public object Couleur { get; private set; }
        public float X { get; set; }
        public float Y { get; set; }

        public void Affiche(OpenGL gl, int nbImagesLargeur, int nbImagesHauteur, int phase, float LARGEUR_CASE, float HAUTEUR_CASE, Color couleur)
        {
            Fond.SetColorWithLuminanceChange(gl, couleur, changeCouleur);

            float imageG = image / (float)nbImagesLargeur;
            float imageD = (image + 1) / (float)nbImagesLargeur;
            float imageH = phase / (float)nbImagesHauteur;
            float imageB = (phase + 1) / (float)nbImagesHauteur;

            gl.TexCoord(imageG, imageB); gl.Vertex(X, Y + HAUTEUR_CASE);
            gl.TexCoord(imageD, imageB); gl.Vertex(X + LARGEUR_CASE, Y + HAUTEUR_CASE);
            gl.TexCoord(imageD, imageH); gl.Vertex(X + LARGEUR_CASE, Y);
            gl.TexCoord(imageG, imageH); gl.Vertex(X, Y);
        }
    }
}
