/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 10/01/2015
 * Heure: 11:58
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SharpGL;
using System;
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    /// <summary>
    /// Description of _3d.
    /// </summary>
    public abstract class TroisD : Fond
    {
        protected int _largeur, _hauteur;
        protected float _zCamera;
        protected float _tailleCubeX, _tailleCubeY, _tailleCubeZ;
        public const float MAX_COORD = Int32.MaxValue;
        public const float MIN_COORD = Int32.MinValue;
        protected static float FOV;

        protected TroisD(OpenGL gl, float vpX, float vpY, float vpZ, float zCam) : base(gl)
        {
            _tailleCubeX = vpX;
            _tailleCubeY = vpY;
            _tailleCubeZ = vpZ;
            _zCamera = zCam;
            FOV = getConfiguration().getParametre("FOV", 60.0f, (a) => { FOV = (float)Convert.ToDouble(a); });
        }


        protected static void NormalQuad(OpenGL gl, Vecteur3D v1, Vecteur3D v2, Vecteur3D v3)
        {
            //Vecteur3D[] v = { v1, v2, v3 };

            Vecteur3D d1 = v2 - v1;// new Vecteur3D(v[1].x - v[0].x, v[1].y - v[0].y, v[1].z - v[0].z) ;
            Vecteur3D d2 = v3 - v1;// new Vecteur3D(v[2].x - v[0].x, v[2].y - v[0].y, v[2].z - v[0].z) ;
            Vecteur3D crossProduct = new Vecteur3D(d1.y * d2.z - d1.z * d2.y, d1.z * d2.x - d1.x * d2.z, d1.x * d2.y - d1.y * d2.x);

            crossProduct.Normalize();
            crossProduct.Normal(gl);
        }


        protected static Vecteur3D NormaleTriangle(Vecteur3D P1, Vecteur3D P2, Vecteur3D P3)
        {
            Vecteur3D v = new Vecteur3D();
            v.x = (P2.y - P1.y) * (P3.z - P1.z) - (P2.z - P1.z) * (P3.y - P1.y);
            v.y = (P2.z - P1.z) * (P3.x - P1.x) - (P2.x - P1.x) * (P3.z - P1.z);
            v.z = (P2.x - P1.x) * (P3.y - P1.y) - (P2.y - P1.y) * (P3.x - P1.x);
            v.Normalize();
            return v;
        }

        /*
        protected float AngleEntre(Vecteur3D v1, Vecteur3D v2)
        {
            float angle = v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
            return (float)Math.Acos(angle);
        }*/

        /// <summary>
        /// Change le zoom de la camera
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="FOV"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="nearPlane">ATTENTION Bug! ne marche pas si nearplane = 0</param>
        /// <param name="farPlane"></param>
        protected static void changeZoom(OpenGL gl, int width, int height, float nearPlane, float farPlane)
        {
            gl.Viewport(0, 0, width, height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(FOV, (float)width / height, nearPlane, farPlane);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        public void dessineAxe(OpenGL gl, float taille)
        {
            gl.Begin(OpenGL.GL_LINES);
            { // X
                gl.Color(1.0f, 0.0f, 0.0f, 0.75f);
                gl.Vertex(0, 0, 0); gl.Vertex(taille, 0, 0);
                gl.Vertex(taille, 0, 0); gl.Vertex(taille * 0.8f, taille * 0.2f, 0);
                gl.Vertex(taille, 0, 0); gl.Vertex(taille * 0.8f, taille * -0.2f, 0);

                gl.Vertex(taille * 1.1, taille * 0.05f, taille * 0.05f); gl.Vertex(taille * 1.1, taille * -0.05f, taille * -0.05f);
                gl.Vertex(taille * 1.1, taille * 0.05f, taille * -0.05f); gl.Vertex(taille * 1.1, taille * -0.05f, taille * 0.05f);
            }
            { // Y : vert
                gl.Color(0.0f, 1.0f, 0.0f, 0.75f);
                gl.Vertex(0, 0, 0); gl.Vertex(0, taille, 0);
                gl.Vertex(0, taille, 0); gl.Vertex(taille * 0.2f, taille * 0.8f, 0);
                gl.Vertex(0, taille, 0); gl.Vertex(taille * -0.2f, taille * 0.8f, 0);
            }
            { // Z
                gl.Color(0.0f, 0.0f, 1.0f, 0.75f);
                gl.Vertex(0, 0, 0); gl.Vertex(0, 0, taille);
                gl.Vertex(0, 0, taille); gl.Vertex(taille * 0.2f, 0, taille * 0.8f);
                gl.Vertex(0, 0, taille); gl.Vertex(taille * -0.2f, 0, taille * 0.8f);

                gl.Vertex(taille * 0.05f, taille * 0.05f, taille * 1.1); gl.Vertex(taille * -0.05f, taille * -0.05f, taille * 1.1f);
                gl.Vertex(taille * -0.05f, taille * 0.05f, taille * 1.1); gl.Vertex(taille * 0.05f, taille * 0.05f, taille * 1.1f);
                gl.Vertex(taille * -0.05f, taille * -0.05f, taille * 1.1); gl.Vertex(taille * 0.05f, taille * -0.05f, taille * 1.1f);

            }
            gl.End();
        }

    }


}
