/***
 * Representation d'une ligne de prevision meteo
 * A changer en fonction du site qu'on utilise
 * */

using ClockScreenSaverGL.Config;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Meteo
{
    public class LignePrevisionMeteo : IDisposable
    {
        private const int NB_HEURES_PREVI = 4;
        private Image _bmp;
        private string _date;
        private string _temperature;
        private string _texte;
        private string _vent;
        private string _pluie;

        public LignePrevisionMeteo(string icone, string date, string temperature, string texte, string vent, string pluie)
        {
            _texte = texte;

            try
            {
                string fichierIcone = @"Meteo\" + icone + ".png";
                string chemin = Configuration.getImagePath(fichierIcone, true);

                if (chemin == null)
                {
                    Log.instance.error($"icone météo inconnue \"{icone}\", chemin \"{fichierIcone}\", texte \"{texte}\"");
                    _bmp = Image.FromFile(Configuration.getImagePath(@"Meteo\inconnu.png"));
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

        public float affiche(Graphics g, Font fTitre, Font fSousTitre, float Y, int tailleIconeMeteo)
        {
            if (_bmp != null)
                g.DrawImage(_bmp, 0, Y, tailleIconeMeteo, tailleIconeMeteo);

            SizeF sizeTitre = g.MeasureString(_date, fTitre);
            g.DrawString(_date, fTitre, Brushes.White, tailleIconeMeteo, Y);

            string texte = "";
            if (_temperature?.Length > 0)
                texte += _temperature + "\n";
            if (_texte?.Length > 0)
                texte += _texte + "\n";
            if (_vent?.Length > 0)
                texte += _vent + "\n";
            if (_pluie?.Length > 0)
                texte += _pluie + "\n";

            // Date
            SizeF size = g.MeasureString(texte, fTitre);
            g.DrawString(texte, fSousTitre, Brushes.White, tailleIconeMeteo, Y + sizeTitre.Height);
            return size.Height + sizeTitre.Height;
        }
    }
}
