﻿/*
 * Created by SharpDevelop.
 * User: lucien
 * Date: 22/06/2014
 * Time: 16:28
 * 
 * To change this template use Tools  Options  Coding  Edit Standard Headers.
 */
using SharpGL;
namespace ClockScreenSaverGL.DisplayedObjects.Bandes.BandeVerticale
{
    /// <summary>
    /// Description of BandeMinute.
    /// </summary>
    public sealed class BandeMinute : BandeVerticale
    {

        public BandeMinute(OpenGL gl, float LargeurSeconde, float OrigineX, float Py, int largeur)
            : base(gl, 60, 5, LargeurSeconde, OrigineX, Py, largeur)
        {
            _alpha = c.GetParametre("AlphaMinute", (byte)60);
        }
        protected override void GetValue(Temps maintenant, out float value, out float decalage)
        {
            decalage = (maintenant.seconde + (maintenant.milliemesDeSecondes / 1000.0f)) / 60.0f;
            value = maintenant.minute;
        }

    }
}
