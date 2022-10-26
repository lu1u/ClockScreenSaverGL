/*
 * Vecteur 2D (x,y)
 */
using SharpGL;
using System;
using System.Runtime.CompilerServices;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of Vecteur2D.
    /// </summary>
    public class Vecteur2D
    {
        public float x, y;
        public static readonly Vecteur2D ZERO = new Vecteur2D(0, 0);
        public static readonly Vecteur2D UN = new Vecteur2D(1, 1);
        public static readonly Vecteur2D Y = new Vecteur2D(0, 1);
        public static readonly Vecteur2D MOINS_Y = new Vecteur2D(0, -1);
        public static readonly Vecteur2D X = new Vecteur2D(1, 0);
        public static readonly Vecteur2D MOINS_X = new Vecteur2D(-1, 0);

        public Vecteur2D()
        {
            x = 0;
            y = 0;
        }

        public Vecteur2D(float X, float Y)
        {
            x = X;
            y = Y;
        }

        public Vecteur2D(Vecteur2D v)
        {
            x = v.x;
            y = v.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Longueur()
        {
            return (float)Math.Sqrt((x * x) + (y * y));
        }

        public void set(float X, float Y)
        {
            x = X;
            y = Y;
        }

        public void Normalize()
        {
            float n = Longueur();
            x /= n;
            y /= n;
        }

        public float[] tabf
        {
            get
            {
                float[] f = new float[2];
                f[0] = x;
                f[1] = y;
                return f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Vertex(OpenGL gl) => gl.Vertex(x, y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur2D operator *(float f, Vecteur2D v)     //produit par un réel
        { return new Vecteur2D(v.x * f, v.y * f); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vecteur2D operator *(Vecteur2D v, float f)     //le prod par un float est commutatif !!!
        { return new Vecteur2D(v.x * f, v.y * f); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vecteur2D operator /(Vecteur2D v, float f) => (v * (1 / f));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float operator *(Vecteur2D v, Vecteur2D w)     //produit scalaire
        { return v.prodscal(w); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur2D operator +(Vecteur2D v, Vecteur2D w) => new Vecteur2D(v.x + w.x, v.y + w.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur2D operator -(Vecteur2D v, Vecteur2D w)     //différence vectorielle
        { return new Vecteur2D(v.x - w.x, v.y - w.y); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur2D operator -(Vecteur2D v)     //negatif
        { return new Vecteur2D(-v.x, -v.y); }


        /**
         * Limit the magnitude of this vector 
         * @param max the maximum Length to limit this vector 
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Limiter(float max)
        {
            if (Longueur() > max)
            {
                Normalize();
                multiplier_par(max);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void multiplier_par(float a) { x = a * x; y = a * y; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void diviser_par(float a) { x = x / a; y = y / a; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public float prodscal(Vecteur2D v) { return (x * v.x + y * v.y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void additionner(float a) { x = a + x; y = a + y; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void additionner(float a, float b, float c) { x = a + x; y = b + y; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void additionner(Vecteur2D a) { x = x + a.x; y = y + a.y; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void soustraire(Vecteur2D a) { x = x - a.x; y = y - a.y; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] static float DEG_TO_RAD(float a) { return a * (float)Math.PI / 360.0f; }
        /**
         * Calculate the angle of rotation for this vector
         * @return the angle of rotation 
         */
        public float Heading()
        {
            return -(float)Math.Atan2(-y, x);
        }


        ///////////////////////////////////////////////////////////////////////////////
        // Rotaton du vecteur autour de l'axe des Z
        // ENTREES:	Angle en degres
        ///////////////////////////////////////////////////////////////////////////////
        public void Rotate(float AngleDegres)
        {
            double Angle = DEG_TO_RAD(AngleDegres);
            x = (float)((Math.Cos(Angle) * x) - (Math.Sin(Angle) * y));
            y = (float)((Math.Sin(Angle) * x) + (Math.Cos(Angle) * y));
        }



        /**
         * Calculate the Euclidean distance between two points (considering a point as a vector object) 
         * @param v another vector 
         * @return the Euclidean distance between 
         */
        public float Distance(Vecteur2D v)
        {
            float dx = x - v.x;
            float dy = y - v.y;
            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }
    }
}
