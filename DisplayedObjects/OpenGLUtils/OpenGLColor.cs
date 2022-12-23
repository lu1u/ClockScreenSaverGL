using SharpGL;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    internal static class OpenGLColor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Couleur(OpenGL gl, Color couleur, float v = 1.0f)
        {
            gl.Color(v * couleur.R / 256.0f, v * couleur.G / 256.0f, v * couleur.B / 256.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ColorWithLuminance(OpenGL gl, Color couleur, float v)
        {
            Color cG = new CouleurGlobale(couleur).GetColorWithValueChange(v);
            gl.Color(cG.R / 256.0f, cG.G / 256.0f, cG.B / 256.0f, cG.A / 256.0f);
        }

        /// <summary>
        /// Retourne une couleur correspondant a la teinte donnee avec la transparence donnee
        /// </summary>
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color GetCouleurAvecAlpha(Color color, byte alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color GetCouleurOpaqueAvecAlpha(Color color, byte alpha)
        {
            float a = alpha / 255.0f;
            if (a < 0) a = 0;
            if (a > 1.0f) a = 1.0f;

            return Color.FromArgb(255, (byte)(color.R * a), (byte)(color.G * a), (byte)(color.B * a));
        }
    }
}
