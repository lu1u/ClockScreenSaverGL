﻿using SharpGL;
using System;


namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    internal class FrustumCulling
    {
        private double[,] g_frustumPlanes = new double[6, 4];

        public FrustumCulling(OpenGL gl)
        {
            float[] p = new float[16];   // projection matrix
            float[] mv = new float[16];  // model-view matrix
            float[] mvp = new float[16]; // model-view-projection matrix
            double t;

            gl.GetFloat(OpenGL.GL_PROJECTION_MATRIX, p);
            gl.GetFloat(OpenGL.GL_MODELVIEW_MATRIX, mv);

            //
            // Concatenate the projection matrix and the model-view matrix to produce 
            // a combined model-view-projection matrix.
            //

            mvp[0] = mv[0] * p[0] + mv[1] * p[4] + mv[2] * p[8] + mv[3] * p[12];
            mvp[1] = mv[0] * p[1] + mv[1] * p[5] + mv[2] * p[9] + mv[3] * p[13];
            mvp[2] = mv[0] * p[2] + mv[1] * p[6] + mv[2] * p[10] + mv[3] * p[14];
            mvp[3] = mv[0] * p[3] + mv[1] * p[7] + mv[2] * p[11] + mv[3] * p[15];

            mvp[4] = mv[4] * p[0] + mv[5] * p[4] + mv[6] * p[8] + mv[7] * p[12];
            mvp[5] = mv[4] * p[1] + mv[5] * p[5] + mv[6] * p[9] + mv[7] * p[13];
            mvp[6] = mv[4] * p[2] + mv[5] * p[6] + mv[6] * p[10] + mv[7] * p[14];
            mvp[7] = mv[4] * p[3] + mv[5] * p[7] + mv[6] * p[11] + mv[7] * p[15];

            mvp[8] = mv[8] * p[0] + mv[9] * p[4] + mv[10] * p[8] + mv[11] * p[12];
            mvp[9] = mv[8] * p[1] + mv[9] * p[5] + mv[10] * p[9] + mv[11] * p[13];
            mvp[10] = mv[8] * p[2] + mv[9] * p[6] + mv[10] * p[10] + mv[11] * p[14];
            mvp[11] = mv[8] * p[3] + mv[9] * p[7] + mv[10] * p[11] + mv[11] * p[15];

            mvp[12] = mv[12] * p[0] + mv[13] * p[4] + mv[14] * p[8] + mv[15] * p[12];
            mvp[13] = mv[12] * p[1] + mv[13] * p[5] + mv[14] * p[9] + mv[15] * p[13];
            mvp[14] = mv[12] * p[2] + mv[13] * p[6] + mv[14] * p[10] + mv[15] * p[14];
            mvp[15] = mv[12] * p[3] + mv[13] * p[7] + mv[14] * p[11] + mv[15] * p[15];

            //
            // Extract the frustum's right clipping plane and normalize it.
            //

            g_frustumPlanes[0, 0] = mvp[3] - mvp[0];
            g_frustumPlanes[0, 1] = mvp[7] - mvp[4];
            g_frustumPlanes[0, 2] = mvp[11] - mvp[8];
            g_frustumPlanes[0, 3] = mvp[15] - mvp[12];

            t = Math.Sqrt(g_frustumPlanes[0, 0] * g_frustumPlanes[0, 0] + g_frustumPlanes[0, 1] * g_frustumPlanes[0, 1] + g_frustumPlanes[0, 2] * g_frustumPlanes[0, 2]);

            g_frustumPlanes[0, 0] /= t;
            g_frustumPlanes[0, 1] /= t;
            g_frustumPlanes[0, 2] /= t;
            g_frustumPlanes[0, 3] /= t;

            //
            // Extract the frustum's left clipping plane and normalize it.
            //

            g_frustumPlanes[1, 0] = mvp[3] + mvp[0];
            g_frustumPlanes[1, 1] = mvp[7] + mvp[4];
            g_frustumPlanes[1, 2] = mvp[11] + mvp[8];
            g_frustumPlanes[1, 3] = mvp[15] + mvp[12];

            t = Math.Sqrt(g_frustumPlanes[1, 0] * g_frustumPlanes[1, 0] + g_frustumPlanes[1, 1] * g_frustumPlanes[1, 1] + g_frustumPlanes[1, 2] * g_frustumPlanes[1, 2]);

            g_frustumPlanes[1, 0] /= t;
            g_frustumPlanes[1, 1] /= t;
            g_frustumPlanes[1, 2] /= t;
            g_frustumPlanes[1, 3] /= t;

            //
            // Extract the frustum's bottom clipping plane and normalize it.
            //

            g_frustumPlanes[2, 0] = mvp[3] + mvp[1];
            g_frustumPlanes[2, 1] = mvp[7] + mvp[5];
            g_frustumPlanes[2, 2] = mvp[11] + mvp[9];
            g_frustumPlanes[2, 3] = mvp[15] + mvp[13];

            t = Math.Sqrt(g_frustumPlanes[2, 0] * g_frustumPlanes[2, 0] + g_frustumPlanes[2, 1] * g_frustumPlanes[2, 1] + g_frustumPlanes[2, 2] * g_frustumPlanes[2, 2]);

            g_frustumPlanes[2, 0] /= t;
            g_frustumPlanes[2, 1] /= t;
            g_frustumPlanes[2, 2] /= t;
            g_frustumPlanes[2, 3] /= t;

            //
            // Extract the frustum's top clipping plane and normalize it.
            //

            g_frustumPlanes[3, 0] = mvp[3] - mvp[1];
            g_frustumPlanes[3, 1] = mvp[7] - mvp[5];
            g_frustumPlanes[3, 2] = mvp[11] - mvp[9];
            g_frustumPlanes[3, 3] = mvp[15] - mvp[13];

            t = Math.Sqrt(g_frustumPlanes[3, 0] * g_frustumPlanes[3, 0] + g_frustumPlanes[3, 1] * g_frustumPlanes[3, 1] + g_frustumPlanes[3, 2] * g_frustumPlanes[3, 2]);

            g_frustumPlanes[3, 0] /= t;
            g_frustumPlanes[3, 1] /= t;
            g_frustumPlanes[3, 2] /= t;
            g_frustumPlanes[3, 3] /= t;

            //
            // Extract the frustum's far clipping plane and normalize it.
            //

            g_frustumPlanes[4, 0] = mvp[3] - mvp[2];
            g_frustumPlanes[4, 1] = mvp[7] - mvp[6];
            g_frustumPlanes[4, 2] = mvp[11] - mvp[10];
            g_frustumPlanes[4, 3] = mvp[15] - mvp[14];

            t = Math.Sqrt(g_frustumPlanes[4, 0] * g_frustumPlanes[4, 0] + g_frustumPlanes[4, 1] * g_frustumPlanes[4, 1] + g_frustumPlanes[4, 2] * g_frustumPlanes[4, 2]);

            g_frustumPlanes[4, 0] /= t;
            g_frustumPlanes[4, 1] /= t;
            g_frustumPlanes[4, 2] /= t;
            g_frustumPlanes[4, 3] /= t;

            //
            // Extract the frustum's near clipping plane and normalize it.
            //

            g_frustumPlanes[5, 0] = mvp[3] + mvp[2];
            g_frustumPlanes[5, 1] = mvp[7] + mvp[6];
            g_frustumPlanes[5, 2] = mvp[11] + mvp[10];
            g_frustumPlanes[5, 3] = mvp[15] + mvp[14];

            t = Math.Sqrt(g_frustumPlanes[5, 0] * g_frustumPlanes[5, 0] + g_frustumPlanes[5, 1] * g_frustumPlanes[5, 1] + g_frustumPlanes[5, 2] * g_frustumPlanes[5, 2]);

            g_frustumPlanes[5, 0] /= t;
            g_frustumPlanes[5, 1] /= t;
            g_frustumPlanes[5, 2] /= t;
            g_frustumPlanes[5, 3] /= t;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Retourne TRUE si l'objet est dans le champs de vision de la camera
        ///////////////////////////////////////////////////////////////////////////////
        public bool isVisible(Vecteur3D Pos, double fRadius)
        {
            for (int i = 0; i < 6; ++i)
            {
                if (g_frustumPlanes[i, 0] * Pos.x +
                    g_frustumPlanes[i, 1] * Pos.y +
                    g_frustumPlanes[i, 2] * Pos.z +
                    g_frustumPlanes[i, 3] <= -fRadius)
                    return false;
            }

            return true;
        }
        ///////////////////////////////////////////////////////////////////////////////
        // Retourne TRUE si l'objet est dans le champs de vision de la camera
        ///////////////////////////////////////////////////////////////////////////////
        public bool isVisible(Vecteur3D P1, Vecteur3D P2, Vecteur3D P3)
        {
            if (isVisible(P1, P2)) return true;
            if (isVisible(P1, P3)) return true;
            if (isVisible(P2, P3)) return true;

            return false;
        }

        ///////////////////////////////////////////////////////////////////////////////
        // Retourne TRUE si l'objet est dans le champs de vision de la camera
        ///////////////////////////////////////////////////////////////////////////////
        public bool isVisible(Vecteur3D P1, Vecteur3D P2)
        {
            double Segment = (P1 - P2).Longueur() / 3.0;
            if (isVisible(P1, Segment)) return true;
            if (isVisible(P2, Segment)) return true;

            Vecteur3D Centre = new Vecteur3D((P1 + P2) / 2.0f);

            if (isVisible(Centre, Segment)) return true;
            return false;
        }


        ///////////////////////////////////////////////////////////////////////////////
        // Retourne TRUE si l'objet est dans le champs de vision de la camera
        ///////////////////////////////////////////////////////////////////////////////
        public bool isVisible(Vecteur3D P1, Vecteur3D P2, Vecteur3D P3, Vecteur3D P4)
        {
            Vecteur3D Centre = new Vecteur3D((P1 + P2 + P3 + P4) / 4.0f);

            if (isVisible(Centre, P1, P2)) return true;
            if (isVisible(Centre, P2, P3)) return true;
            if (isVisible(Centre, P3, P4)) return true;
            if (isVisible(Centre, P4, P1)) return true;
            return false;
        }

        public void isVisibleBas(Vecteur3D p)
        {

        }
    }
}
