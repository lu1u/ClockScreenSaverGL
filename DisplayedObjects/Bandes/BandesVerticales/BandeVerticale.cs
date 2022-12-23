/*
 * Created by SharpDevelop.
 * User: lucien
 * Date: 22/06/2014
 * Time: 20:15
 * 
 * To change this template use Tools  Options  Coding  Edit Standard Headers.
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Bandes.BandeVerticale
{
    /// <summary>
    /// Description of BandeVerticale.
    /// </summary>
    public abstract class BandeVerticale : Bande
    {
        public const string CAT = "BandeVerticale";
        protected CategorieConfiguration c;
        private readonly OpenGLFonte _glFonte;

        protected BandeVerticale(OpenGL gl, int valMax, int intervalle, float largeurcase, float origineY, float Px, int largeur)
            : base(gl, valMax, intervalle, largeurcase, origineY, largeur)
        {
            GetConfiguration();

            _glFonte = new OpenGLFonte(gl, "0123456789", _hauteurFonte, FontFamily.GenericSansSerif, FontStyle.Regular);
            _trajectoire = new TrajectoireDiagonale(Px, _origine, c.GetParametre("VY", 20f), 0);
            _taillebande = new SizeF(_hauteurFonte * 2, largeur);
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                _hauteurFonte = c.GetParametre("TailleFonte", 30);
            }
            return c;
        }
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
            GetValue(maintenant, out float valeur, out float decalage);

            float Decalage = _origine - (decalage * _largeurCase);
            float Y = Decalage;
            float X = _trajectoire._Px;

            int val = (int)valeur;

            // Reculer jusqu'à la droite de l'écran
            while (Y > 0)
            {
                Y -= _largeurCase;
                val--;
            }

            // Revenir jusqu'a la gauche de l'ecran
            while (val < 0)
                val += _valeurMax;

            // Trace les chiffres et marques
            while (Y < tailleEcran.Height)
            {
                gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(X, Y);
                gl.Vertex(val % _intervalleTexte == 0 ? X + _hauteurFonte : X + _hauteurFonte / 2.0f, Y);
                gl.End();

                if (val % _intervalleTexte == 0)
                    _glFonte.drawOpenGL(gl, val.ToString(), X, Y, couleur);
                Y += _largeurCase;
                val++;
                while (val >= _valeurMax)
                    val -= _valeurMax;
            }


            gl.Begin(OpenGL.GL_LINES);
            // Deux lignes verticales pour les bords de la bande
            gl.Vertex(X, 0);
            gl.Vertex(X, tailleEcran.Height);

            gl.Vertex(X + _hauteurFonte * 2.0f, 0);
            gl.Vertex(X + _hauteurFonte * 2.0f, tailleEcran.Height);


            // Repere pour la valeur
            gl.Vertex(X - 4, _origine);
            gl.Vertex(X + 4 + _hauteurFonte * 2, _origine);

            gl.End();

        }

    }
}