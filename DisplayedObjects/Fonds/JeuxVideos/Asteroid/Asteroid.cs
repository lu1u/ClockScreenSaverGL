using ClockScreenSaverGL.DisplayedObjects.Fonds.Utils;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Runtime.CompilerServices;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Asteroid
    {
        public static float TAILLE_MAX;
        public static int NIVEAU_MAX;
        public static int NB_COINS;
        readonly float _taille;
        readonly float _dx, _dy;
        readonly float _vrot;
        readonly int _niveau;
        readonly float[] _rayons;
        float _x, _y;
        float _rot;

        public int Niveau { get => _niveau; }
        public float Dx { get => _dx; }
        public float Dy { get => _dy; }
        public float VRot { get => _vrot; }
        public float X { get => _x; }
        public float Y { get => _y; }

        public Asteroid(int niveau, float x, float y, float vrot, float dx, float dy, Random r)
        {
            _taille = TAILLE_MAX / (float)(niveau + 1);
            _niveau = niveau;
            _x = x;
            _y = y;
            _rot = 0;
            _vrot = vrot;
            _dx = dx;
            _dy = dy;

            _rayons = new float[NB_COINS];
            for (int i = 0; i < NB_COINS; i++)
                _rayons[i] = _taille * (float)(0.6 * r.NextDouble() + 0.6);
        }


        /// <summary>
        /// Afficher l'asteroid
        /// </summary>
        /// <param name="gl"></param>
        public void Affiche(OpenGL gl)
        {
            using (new GLBegin(gl, OpenGL.GL_LINE_LOOP))
            {
                for (int i = 0; i < NB_COINS; i++)
                    gl.Vertex(GetX(i), GetY(i));
            }
        }
        /// <summary>
        /// Retrouve la coordonnee X du coin donné en parametre
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetX(int i)
        {
            return _x + (Math.Sin((i * Math.PI * 2.0f / NB_COINS) + _rot) * _rayons[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetY(int i)
        {
            return _y + (Math.Cos((i * Math.PI * 2.0f / NB_COINS) + _rot) * _rayons[i]);
        }

        /// <summary>
        /// Deplace l'asteroide: translation et rotation
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="MIN_VIEWPORT_X"></param>
        /// <param name="MAX_VIEWPORT_X"></param>
        /// <param name="MIN_VIEWPORT_Y"></param>
        /// <param name="MAX_VIEWPORT_Y"></param>
        internal void Deplace(Temps maintenant, float MIN_VIEWPORT_X, float MAX_VIEWPORT_X, float MIN_VIEWPORT_Y, float MAX_VIEWPORT_Y)
        {
            _rot += maintenant.intervalleDepuisDerniereFrame * _vrot;
            _x += _dx * maintenant.intervalleDepuisDerniereFrame;
            _y += _dy * maintenant.intervalleDepuisDerniereFrame;
            MathUtils.ContraintTore(ref _x, MIN_VIEWPORT_X, MAX_VIEWPORT_X);
            MathUtils.ContraintTore(ref _y, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);
        }

        /// <summary>
        /// Determine s'il y a collision entre l'asteroide et un tir
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool Collision(float x, float y)
        {
            return MathUtils.Distance(x, y, _x, _y) < _taille;
        }
    }
}
