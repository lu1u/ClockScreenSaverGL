using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    internal static class GLLines
    {
        public const double ANGLE_DROIT = Math.PI * 0.5;


        public static void DessineLigne(OpenGL gl, PointF p1, PointF p2, float LargeurLignes)
        {
            PointF[] p = GetMultilinePolygon(p1, p2, LargeurLignes);
            using (new GLBegin(gl, OpenGL.GL_QUAD_STRIP))
            {
                for (int i = 0; i < p.Length / 4; i++)
                {
                    int POINT = i * 4;
                    gl.Vertex(p[POINT + 3].X, p[POINT + 3].Y);
                    gl.Vertex(p[POINT + 0].X, p[POINT + 0].Y);
                    gl.Vertex(p[POINT + 2].X, p[POINT + 2].Y);
                    gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);
                }
            }
        }


        public static void DessinePolyLine(OpenGL gl, PointF[] points, float LargeurLignes)
        {
            PointF[] p = GetMultilinePolygon(points, LargeurLignes);
            using (new GLBegin(gl, OpenGL.GL_QUAD_STRIP))
            {
                for (int i = 0; i < p.Length / 4; i++)
                {
                    int POINT = i * 4;
                    gl.Vertex(p[POINT + 3].X, p[POINT + 3].Y);
                    gl.Vertex(p[POINT + 0].X, p[POINT + 0].Y);
                    gl.Vertex(p[POINT + 2].X, p[POINT + 2].Y);
                    gl.Vertex(p[POINT + 1].X, p[POINT + 1].Y);

                }
            }
        }

        /// <summary>
        /// Calcule un ensemble de points pour tracer une multiline (de plusieurs segments) epaisse
        /// Si la ligne contient N points, la "ligne epaisse" contient (N-1) * 4 points
        /// 
        /// (0), (1), (2)... les points de la ligne
        /// 
        ///     p0------------p1   
        ///     |             |    
        ///    (0) segment 1 (1)   
        ///     |             |    
        ///     p3------------p2   
        ///     
        /// </summary>
        /// <param name="multiline"></param>
        /// <param name="largeurLigne"></param>
        /// <returns></returns>
        public static PointF[] GetMultilinePolygon(PointF p1, PointF p2, float LargeurLignes)
        {
            PointF[] p = new PointF[4];
            CalculePerpendiculaires(p1, p2, LargeurLignes, out p[0], out p[1], out p[2], out p[3]);
            return p;
        }

        /// <summary>
        /// Calcule un ensemble de points pour tracer une multiline (de plusieurs segments) epaisse
        /// Si la ligne contient N points, la "ligne epaisse" contient (N-1) * 4 points
        /// 
        /// (0), (1), (2)... les points de la ligne
        /// 
        ///     p0------------p1     p4------------p5     p8------------p9
        ///     |             |      |             |      |             |
        ///    (0) segment 1 (1)    (1) segment 2 (2)    (2) segment 3 (3)
        ///     |             |      |             |      |             |
        ///     p3------------p2     p7------------p6     p11-----------p10
        ///     
        /// </summary>
        /// <param name="multiline"></param>
        /// <param name="largeurLigne"></param>
        /// <returns></returns>
        public static PointF[] GetMultilinePolygon(PointF[] points, float LargeurLignes)
        {
            PointF[] p = new PointF[(points.Length - 1) * 4];

            // Les perpendiculaires aux lignes
            for (int i = 0; i < points.Length - 1; i++)
            {
                int POINT = i * 4;
                CalculePerpendiculaires(points[i], points[i + 1], LargeurLignes, out p[POINT + 0], out p[POINT + 1], out p[POINT + 2], out p[POINT + 3]);
            }

            return p;
        }

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
        public static void CalculePerpendiculaires(PointF p1, PointF p2, float largeur, out PointF per1, out PointF per2, out PointF per3, out PointF per4)
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

