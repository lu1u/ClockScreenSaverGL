/*
 * Un objet graphique Texte qui contient un texte fixe de copyright
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Textes
{
    /// <summary>
    /// Un objet graphique Texte qui contient un texte fixe de copyright
    /// </summary>
    public class TexteCopyright : Texte
    {
        private const string CAT = "TexteCopyright";
        protected CategorieConfiguration c;
        private const string _texte = "Lucien Pilloni (c) 2014";
        private SizeF _Taille;
        public TexteCopyright(OpenGL gl, int Px, int Py) : base(gl)
        {
            GetConfiguration();
            _alpha = c.GetParametre("Alpha", (byte)160);
            _fonte = CreerFonte(c.GetParametre("TailleFonte", 80));
            _trajectoire = new TrajectoireDiagonale(Px, Py, c.GetParametre("VX", 4), c.GetParametre("VY", 4));
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                _Taille = g.MeasureString(_texte, _fonte);
        }


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
            }
            return c;
        }
        protected override SizeF getTexte(Temps maintenant, out string texte)
        {
            texte = _texte;
            return _Taille;
        }

        public override void DateChangee(OpenGL gl, Temps maintenant)
        {

        }
    }
}
