/***
 * Representation d'une ligne de prevision meteo
 * A changer en fonction du site qu'on utilise
 * */

using ClockScreenSaverGL.Config;
using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ClockScreenSaverGL.DisplayedObjects.Meteo
{
    public class LignePrevisionMeteo : IDisposable
    {
        const int NB_HEURES_PREVI = 4;
         private Image _bmp;
        private string _date;
        private string _temperature;
        private string _texte;
        private string _vent;
        private string _pluie;
		internal static int TAILLE_ICONE_METEO;

		public LignePrevisionMeteo(string icone, string date, string temperature, string texte, string vent, string pluie)
        {
           _texte = texte;

            try
            {
                string fichierIcone = @"Meteo\" + icone + ".png";
                string chemin = Config.Configuration.getImagePath(fichierIcone, true);

                if ( chemin == null)
                {
                    Log.instance.error($"icone météo inconnue \"{icone}\", chemin \"{fichierIcone}\", texte \"{texte}\"");
                    _bmp = Image.FromFile(Config.Configuration.getImagePath(@"Meteo\inconnu.png"));
                    //_texte = "{" + icone + "}" + _texte;
                }
                else
                    _bmp = Image.FromFile(chemin);
            }
            catch (Exception e)
            {
                Log.instance.error(e.Message);
                Log.instance.error(e.StackTrace);
                Log.instance.error($"impossible de charger l'icône météo \"{icone}\", texte \"{texte}\"");
                _bmp = Image.FromFile(Configuration.getImagePath(@"Meteo\inconnu.png"));
                //_texte = "{" + icone + "}" + _texte;
            }
            _date = date;
            _temperature = temperature;
            _vent = vent;
            _pluie = pluie;
        }


        public void Dispose()
        {
            _bmp?.Dispose();
        }

        /// <summary>
        /// Conversion degres Kelvin => Celsius
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /*private static float KelvinToCelsius(float v)
        {
            return v - 273.15f;
        }
*/

        public float affiche(Graphics g, Font fTitre, Font fSousTitre, float Y)
        {
            if (_bmp != null)
                g.DrawImage(_bmp, 0, Y, TAILLE_ICONE_METEO, TAILLE_ICONE_METEO);
            
            float H = 0;
            // Date
            SizeF size = g.MeasureString(_date, fTitre);
            g.DrawString(_date, fTitre, Brushes.White, TAILLE_ICONE_METEO, Y);
            H += size.Height;

            // Temperature
            g.DrawString(_temperature, fTitre, Brushes.White, TAILLE_ICONE_METEO, Y + H);
            size = g.MeasureString(_temperature, fSousTitre);
            H += size.Height;

            // Vent
            g.DrawString(_vent, fTitre, Brushes.White, TAILLE_ICONE_METEO, Y + H);
            size = g.MeasureString(_vent, fSousTitre);
            H += size.Height;

            // Texte
            g.DrawString(_texte, fTitre, Brushes.White, TAILLE_ICONE_METEO, Y + H);
            size = g.MeasureString(_temperature, fSousTitre);
            H += size.Height;

            return H;
        }
    }
}
