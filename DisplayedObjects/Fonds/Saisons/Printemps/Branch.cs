
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps
{
    public class Branch
    {
        readonly public static float LARGEUR_INITIALE = 2;

        public Branch Parent { get; private set; }
        public Vecteur3D GrowDirection { get; set; }
        public Vecteur3D OriginalGrowDirection { get; set; }
        public int GrowCount { get; set; }
        public Vecteur3D Position { get; private set; }
        public float Size { get; set; }

        public Branch(Branch parent, Vecteur3D position, Vecteur3D growDirection)
        {
            Parent = parent;
            Position = position;
            GrowDirection = growDirection;
            OriginalGrowDirection = growDirection;
            Size = LARGEUR_INITIALE;
        }

        public void Reset()
        {
            GrowCount = 0;
            GrowDirection = OriginalGrowDirection;
        }

        public void Draw(Graphics g, float dx, float dy)
        {
            if (Parent != null)
                using (Pen p = new Pen(Color.FromArgb(255, 0, 0, 0), Size))
                    g.DrawLine(p, Position.X + dx, Position.Y + dy, Parent.Position.X + dx, Parent.Position.Y + dy);
        }

        /// <summary>
        /// Dessine la branche en utilisant OpenGL
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Draw(OpenGL gl, float dx, float dy)
        {
            if (Parent != null)
            {
                PointF p1 = new PointF(Position.X + dx, Position.Y + dy);
                PointF p2 = new PointF(Parent.Position.X + dx, Parent.Position.Y + dy);

                PointF per1, per2, per3, per4;

                calculePerpendiculaires(p1, p2, Size,  out per1, out per2, out per3, out per4);
                gl.Vertex(per1.X, per1.Y);
                gl.Vertex(per2.X, per2.Y);
                gl.Vertex(per3.X, per3.Y);
                gl.Vertex(per4.X, per4.Y);

                // gl.LineWidth(Size);
                // gl.Begin(OpenGL.GL_LINES);
                // gl.Vertex(Position.X+dx , Position.Y+dy);
                // gl.Vertex(Parent.Position.X + dx, Parent.Position.Y+ dy);
                // gl.End();
            }
        }
        const double ANGLE_DROIT = Math.PI * 0.5;
        /// <summary>
        /// Calcule les points perpendiculaires autour d'un segment pour tracer un segment epais sous forme d'un rectangle
        /// 
        ///     per1--------per2
        ///     |           | 
        ///     P1          P2 
        ///     |           | 
        ///     per4--------per3
        ///     
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="largeur"></param>
        /// <param name="per1"></param>
        /// <param name="per2"></param>
        /// <param name="per3"></param>
        /// <param name="per4"></param>
        public static void calculePerpendiculaires(PointF p1, PointF p2, float largeur, out PointF per1, out PointF per2, out PointF per3, out PointF per4)
        {
            double angleAlpha;
            if (p1.X != p2.X)
            {
                angleAlpha = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X));
            }
            else
            {
                if (p1.Y > p2.Y)
                    angleAlpha = -ANGLE_DROIT;
                else
                    if (p1.Y < p2.Y)
                    angleAlpha = ANGLE_DROIT;
                else
                {
                    // Les deux points sont confondus, on ne peut pas calculer de perpendiculaire
                    per1 = new PointF();
                    per2 = new PointF();
                    per3 = new PointF();
                    per4 = new PointF();
                    return;
                }
            }

            angleAlpha += ANGLE_DROIT;

            float fX = largeur * (float)Math.Cos(angleAlpha);
            float fY = largeur * (float)Math.Sin(angleAlpha);

            per1 = new PointF(p1.X - fX, p1.Y - fY);
            per2 = new PointF(p2.X - fX, p2.Y - fY);
            per3 = new PointF(p2.X + fX, p2.Y + fY);
            per4 = new PointF(p1.X + fX, p1.Y + fY);
        }
    }
}
