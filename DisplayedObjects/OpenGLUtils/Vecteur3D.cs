/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 17/01/2015
 * Heure: 13:04
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SharpGL;
using System;
using System.Runtime.CompilerServices;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of Vecteur3D.
    /// </summary>
    public class Vecteur3D
    {
        public float x, y, z;
        public static readonly Vecteur3D ZERO = new Vecteur3D(0, 0, 0);
        public static readonly Vecteur3D UN = new Vecteur3D(1, 1, 1);
        public static readonly Vecteur3D Z = new Vecteur3D(0, 0, 1);
        public static readonly Vecteur3D MOINS_Z = new Vecteur3D(0, 0, -1);
        public static readonly Vecteur3D Y = new Vecteur3D(0, 1, 0);
        public static readonly Vecteur3D MOINS_Y = new Vecteur3D(0, -1, 0);
        public static readonly Vecteur3D X = new Vecteur3D(1, 0, 0);
        public static readonly Vecteur3D MOINS_X = new Vecteur3D(-1, 0, 0);

        public Vecteur3D()
        {
        }
        public Vecteur3D(float X, float Y, float Z = 0)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public Vecteur3D(Vecteur3D v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public float Longueur()
        {
            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        public void Set(float X, float Y, float Z = 0)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public void Normalize()
        {
            float n = Longueur();
            x /= n;
            y /= n;
            z /= n;
        }

        public float[] Tabf
        {
            get
            {
                float[] f = new float[3];
                f[0] = x;
                f[1] = y;
                f[2] = z;
                return f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Vertex(OpenGL gl) => gl.Vertex(x, y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normal(OpenGL gl) => gl.Normal(x, y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur3D operator *(float f, Vecteur3D v)     //produit par un réel
        { return new Vecteur3D(v.x * f, v.y * f, v.z * f); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vecteur3D operator *(Vecteur3D v, float f)     //le prod par un float est commutatif !!!
        { return new Vecteur3D(v.x * f, v.y * f, v.z * f); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vecteur3D operator /(Vecteur3D v, float f) => (v * (1 / f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public float operator *(Vecteur3D v, Vecteur3D w)     //produit scalaire
        { return v.Prodscal(w); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur3D operator +(Vecteur3D v, Vecteur3D w) => new Vecteur3D(v.x + w.x, v.y + w.y, v.z + w.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur3D operator -(Vecteur3D v, Vecteur3D w)     //différence vectorielle
        { return new Vecteur3D(v.x - w.x, v.y - w.y, v.z - w.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur3D operator -(Vecteur3D v)     //negatif
        { return new Vecteur3D(-v.x, -v.y, -v.z); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public Vecteur3D operator ^(Vecteur3D v, Vecteur3D w)     //produit vectoriel
        {
            Vecteur3D z = new Vecteur3D(
                v.y * w.z - w.y * v.z,
                v.z * w.x - w.z * v.x,
                v.x * w.y - w.x * v.y
            );
            return z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vecteur3D Cross(Vecteur3D w)     //produit vectoriel
        {
            Vecteur3D z = new Vecteur3D((this.y * w.z) - (w.y * this.z),
                (this.z * w.x) - (w.z * x),
                (x * w.y) - (w.x * y)
            );
            return z;
        }

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
                Multiplier_par(max);
            }
        }

        public void Multiplier_par(float a) { x = a * x; y = a * y; z = a * z; }
        public void Diviser_par(float a) { x /= a; y /= a; z /= a; }
        public float Prodscal(Vecteur3D v) { return (x * v.x + y * v.y + z * v.z); }
        public void Additionner(float a) { x = a + x; y = a + y; z = a + z; }
        public void Additionner(float a, float b, float c) { x = a + x; y = b + y; z = c + z; }
        public void Additionner(Vecteur3D a) { x += a.x; y += a.y; z += a.z; }

        public void Soustraire(Vecteur3D a) { x -= a.x; y -= a.y; z -= a.z; }

        private static float DEG_TO_RAD(float a) { return a * (float)Math.PI / 360.0f; }
        public void RotateX(float AngleDegres)
        {
            double Angle = DEG_TO_RAD(AngleDegres);
            y = (float)((Math.Cos(Angle) * y) - (Math.Sin(Angle) * z));
            z = (float)((Math.Sin(Angle) * y) + (Math.Cos(Angle) * z));
        }

        /**
         * Calculate the angle of rotation for this vector (only 2D vectors) 
         * @return the angle of rotation 
         */
        public float Heading2D()
        {
            return -(float)Math.Atan2(-y, x);
        }
        ///////////////////////////////////////////////////////////////////////////////
        // Rotaton du vecteur autour de l'axe des Y
        // ENTREES:	Angle en degres
        ///////////////////////////////////////////////////////////////////////////////
        public void RotateY(float AngleDegres)
        {
            double Angle = DEG_TO_RAD(AngleDegres);
            x = (float)((Math.Cos(Angle) * x) + (Math.Sin(Angle) * z));
            z = (float)(-(Math.Sin(Angle) * x) + (Math.Cos(Angle) * z));
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Rotaton du vecteur autour de l'axe des Z
        // ENTREES:	Angle en degres
        ///////////////////////////////////////////////////////////////////////////////
        public void RotateZ(float AngleDegres)
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
        public float Distance(Vecteur3D v)
        {
            float dx = x - v.x;
            float dy = y - v.y;
            float dz = z - v.z;
            return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        public static Vecteur3D Normale(Vecteur3D P1, Vecteur3D P2, Vecteur3D P3)
        {
            Vecteur3D v = new Vecteur3D
            {
                x = (P2.y - P1.y) * (P3.z - P1.z) - (P2.z - P1.z) * (P3.y - P1.y),
                y = (P2.z - P1.z) * (P3.x - P1.x) - (P2.x - P1.x) * (P3.z - P1.z),
                z = (P2.x - P1.x) * (P3.y - P1.y) - (P2.y - P1.y) * (P3.x - P1.x)
            };
            v.Normalize();
            return v;
        }
    }
}
