using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps
{
    public class Vecteur3D
    {
        public static readonly Vecteur3D Zero = new Vecteur3D(0, 0, 0);
        public float X;
        public float Y;
        public float Z;
        public Vecteur3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public void Normalize()
        {
            float l = Length();
            X /= l;
            Y /= l;
            Z /= l;
        }

        public static Vecteur3D operator +(Vecteur3D v1, Vecteur3D v2)
        {
            return new Vecteur3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vecteur3D operator -(Vecteur3D v1, Vecteur3D v2)
        {
            return new Vecteur3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z + -v2.Z);
        }

        public static Vecteur3D operator *(Vecteur3D v1, float m)
        {
            return new Vecteur3D(v1.X * m, v1.Y * m, v1.Z * m);
        }

        public static float operator *(Vecteur3D v1, Vecteur3D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static Vecteur3D operator /(Vecteur3D v1, float m)
        {
            return new Vecteur3D(v1.X / m, v1.Y / m, v1.Z / m);
        }

        public static float Distance(Vecteur3D v1, Vecteur3D v2)
        {
            return (float)Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2 * +Math.Pow(v1.Z - v2.Z, 2)));
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public PointF Point()
        {
            return new PointF(X, Y);
        }
    }
}
