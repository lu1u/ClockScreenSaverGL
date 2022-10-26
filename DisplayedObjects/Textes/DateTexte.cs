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
        const string CAT = "DateTexte";
        protected CategorieConfiguration c;
        private string _date; // Sera initialise dans OnDateChange


        public DateTexte(OpenGL gl, int Px, int Py, int tailleFonte) : base(gl)
        {
            getConfiguration();
            _alpha = c.getParametre("Alpha", (byte)160);
            _fonte = CreerFonte(tailleFonte);
            _trajectoire = new TrajectoireDiagonale(Px, Py, c.getParametre("VX", -17), 0);
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
            }
            return c;
        }
        protected override SizeF getTexte(Temps maintenant, out string texte)
        {
            _date = maintenant.temps.ToLongDateString();
            texte = _date;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                return g.MeasureString(_date, _fonte);
        }


        public override void DateChangee(OpenGL gl, Temps maintenant)
        {
            _date = maintenant.temps.ToLongDateString();
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                _taille = g.MeasureString(_date, _fonte);
        }

    }
}
