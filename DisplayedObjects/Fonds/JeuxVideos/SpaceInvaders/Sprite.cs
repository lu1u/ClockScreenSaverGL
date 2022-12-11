using SharpGL;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    public class Sprite
    {
        public float x, y;
        public int image;
        public float changeCouleur;

        public Sprite(float X, float Y, int Image, float ChangeCouleur = 0.0f)
        {
            x = X;
            y = Y;
            image = Image;
            changeCouleur = ChangeCouleur;
        }

        public object Couleur { get; private set; }

        public void Affiche(OpenGL gl, int nbImagesLargeur, int nbImagesHauteur, int phase, float LARGEUR_CASE, float HAUTEUR_CASE, Color couleur)
        {
            Color c = Fond.getColorWithLuminanceChange(couleur, changeCouleur);
            gl.Color(c.R / 256.0f, c.G / 256.0f, c.B / 256.0f, 1.0f);

            float imageG = image / (float)nbImagesLargeur;
            float imageD = (image + 1) / (float)nbImagesLargeur;
            float imageH = phase / (float)nbImagesHauteur;
            float imageB = (phase + 1) / (float)nbImagesHauteur;

            gl.TexCoord(imageG, imageB); gl.Vertex(x, y + HAUTEUR_CASE);
            gl.TexCoord(imageD, imageB); gl.Vertex(x + LARGEUR_CASE, y + HAUTEUR_CASE);
            gl.TexCoord(imageD, imageH); gl.Vertex(x + LARGEUR_CASE, y);
            gl.TexCoord(imageG, imageH); gl.Vertex(x, y);
        }
    }
}
