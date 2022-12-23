using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Utils
{
    public class MathUtils
    {
        /// <summary>
        /// Retourne la distance entre deux points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance( float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)));
        }


        /// <summary>
        /// Calcule l'angle en degres entre deux points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleDegres(float x1, float y1, float x2, float y2)
        {
            Vector vector1 = new Vector(x1, y1);
            Vector vector2 = new Vector(x2, y2);
            
            return (float)Vector.AngleBetween(vector1, vector2);
        }


        /// <summary>
        /// Contraint une valeur entre deux bornes, s'il depasse d'un coté
        /// on le fait reapparaitre de l'autr (comme dans le tore de Pacman)
        /// </summary>
        /// <param name="v"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ContraintTore(ref float v, float min, float max)
        {
            if (v < min)
                v += (max - min);
            else
                if (v > max)
                v -= (max - min);
        }
    }
}
