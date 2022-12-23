/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 26/06/2014
 * Heure: 09:58
 * 
 * Affiche un objet texte contenant la date du jour
 * Derive de Texte, se contente de fournir la date sous forme de texte
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Textes
{
    /// <summary>
    /// Description of Date.
    /// </summary>
    public class DateTexte : Texte
    {
        private const string CAT = "DateTexte";
        protected CategorieConfiguration c;
        

        public DateTexte(OpenGL gl, int Px, int Py, int tailleFonte) : base(gl)
        {
            GetConfiguration();
            _alpha = c.GetParametre("Alpha", (byte)160);
            _fonte = CreerFonte(tailleFonte);
            _trajectoire = new TrajectoireDiagonale(Px, Py, c.GetParametre("VX", -17), 0);
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
            texte = maintenant.temps.ToLongDateString(); 
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                return g.MeasureString(texte, _fonte);
        }


        public override void DateChangee(OpenGL gl, Temps maintenant)
        {
            string date = maintenant.temps.ToLongDateString();
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                _taille = g.MeasureString(date, _fonte);
        }

    }
}
