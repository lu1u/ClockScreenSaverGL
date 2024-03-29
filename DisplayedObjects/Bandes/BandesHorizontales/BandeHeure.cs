﻿/*
 * Created by SharpDevelop.
 * User: lucien
 * Date: 22/06/2014
 * Time: 16:45
 * 
 * To change this template use Tools  Options  Coding  Edit Standard Headers.
 */
using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects.Bandes.BandeHorizontale
{
    /// <summary>
    /// Description of BandeHeure.
    /// </summary>
    public sealed class BandeHeure : BandeHorizontale
    {
        public BandeHeure(OpenGL gl, float LargeurSeconde, float OrigineX, float Py, int largeur)
            : base(gl, 24, 1, LargeurSeconde, OrigineX, Py, largeur)
        {
            _alpha = c.GetParametre("AlphaHeure", (byte)40);
        }

        protected override void GetValue(Temps maintenant, out float value, out float decalage)
        {
            decalage = (maintenant.minute + (maintenant.seconde + (maintenant.milliemesDeSecondes / 1000.0f)) / 60.0f) / 60.0f;
            value = maintenant.heure;
        }
    }
}
