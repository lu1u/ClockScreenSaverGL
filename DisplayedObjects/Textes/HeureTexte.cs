/*
 * Created by SharpDevelop.
 * User: lucien
 * Date: 21/06/2014
 * Time: 23:03
 * 
 * To change this template use Tools  Options  Coding  Edit Standard Headers.
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;
namespace ClockScreenSaverGL.DisplayedObjects.Textes
{


    /// <summary>
    /// Description of HeureTexte.
    /// </summary>
    public class HeureTexte : Texte
    {
        private const string CAT = "HeureTexte";
        protected CategorieConfiguration c;
        private int HAUTEUR_FONTE;
        private OpenGLFonte _glFonte;

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                HAUTEUR_FONTE = c.getParametre("TailleFonte", 80);
            }
            return c;
        }
        public HeureTexte(OpenGL gl, int Px, int Py, int tailleFonte) : base(gl)
        {
            getConfiguration();
            HAUTEUR_FONTE = tailleFonte;
            _alpha = c.getParametre("Alpha", (byte)160);
            _glFonte = new OpenGLFonte(gl, "0123456789 :/-", tailleFonte, FontFamily.GenericMonospace, FontStyle.Bold);
            _fonte = CreerFonte(tailleFonte);
            _trajectoire = new TrajectoireDiagonale(Px, Py, -c.getParametre("VX", 15), 0);
        }

        protected override Font CreerFonte(int tailleFonte)
        {
            return new Font(FontFamily.GenericSansSerif, tailleFonte, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            base.Deplace(maintenant, tailleEcran);
            //tailleEcran = new Rectangle(tailleEcran.Left, tailleEcran.Top, tailleEcran.Width, tailleEcran.Height - (int)_taille.Height);
        }

        protected override SizeF getTexte(Temps maintenant, out string texte)
        {
            texte = maintenant.heure + ":"
                + maintenant.minute.ToString("D2") + ":"
                + maintenant.seconde.ToString("D2") + ":"
                + maintenant.milliemesDeSecondes.ToString("D3");
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                return g.MeasureString(texte, _fonte);
        }

        protected override bool TexteChange() { return false; }

        /// <summary>
        /// Appelee quand la date change: mettre la date a jour
        /// </summary>
        /// <param name="g"></param>
        /// <param name="maintenant"></param>
        public override void DateChangee(OpenGL gl, Temps maintenant)
        {

        }

        protected override void drawOpenGL(OpenGL gl, Rectangle tailleEcran, Color couleur, Temps maintenant)
        {
            string texte = maintenant.heure + ":"
                + maintenant.minute.ToString("D2") + ":"
                + maintenant.seconde.ToString("D2") + ":"
                + maintenant.milliemesDeSecondes.ToString("D3");
            _glFonte.drawOpenGL(gl, texte, _trajectoire._Px, _trajectoire._Py, couleur);
        }
    }
}
