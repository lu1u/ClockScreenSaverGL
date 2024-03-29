﻿using ClockScreenSaverGL.DisplayedObjects.Fonds.Utils;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class Vaisseau
    {
        public static float POUSSEE = 0.1f;
        public static float VITESSE_ROT = 0.5f;
        public static float VITESSE_TIR = 0.5f;
        public static float VIE_TIR = 3;
        public static float VROT_MAX = 3f;
        public static float FREINAGE_INERTIEL = 0.97f;
        public static int NB_MAX_TIRS = 5;
        private readonly float _taille;
        private readonly List<Tir> _tirs = new List<Tir>();
        private float _x, _y;
        private float _rotationRadians;
        private float _dx, _dy;
        private float _vrot;
        private float _poussee = 0;


        public class Tir
        {
            public float x, y, dx, dy, vie;
        }

        public List<Tir> Tirs
        {
            get => _tirs;
        }
        public float X { get => _x; }
        public float Y { get => _y; }
        public float AngleDegres { get => _rotationRadians * DisplayedObject.RADIAN_TO_DEG; }

        public Vaisseau(float taille, float x, float y, float vrot, float dx, float dy)
        {
            _taille = taille;
            _x = x;
            _y = y;
            _rotationRadians = (float)Math.PI * 0.25f;
            _vrot = vrot;
            _dx = dx;
            _dy = dy;

        }


        public void Affiche(OpenGL gl, float LargeurLignes)
        {
            // Tirs
            using (new GLBegin(gl, OpenGL.GL_POINTS))
                foreach (Tir t in _tirs)
                    gl.Vertex(t.x, t.y);

            // Vaisseau, tourné et déplacé
            gl.PushMatrix();
            gl.Translate(_x, _y, 0);
            gl.Rotate(0, 0, _rotationRadians * DisplayedObject.RADIAN_TO_DEG);

            {
                PointF p1 = new PointF(0 * _taille, -1 * _taille);
                PointF p2 = new PointF(0.6f * _taille, 0.75f * _taille);
                PointF p3 = new PointF(0 * _taille, 0.25f * _taille);
                PointF p4 = new PointF(-0.6f * _taille, 0.75f * _taille);

                GLLines.DessineLigne(gl, p1, p2, LargeurLignes);
                GLLines.DessineLigne(gl, p2, p3, LargeurLignes);
                GLLines.DessineLigne(gl, p3, p4, LargeurLignes);
                GLLines.DessineLigne(gl, p4, p1, LargeurLignes);
            }


            if (_poussee > 0)
            {
                PointF p1 = new PointF(0.25f * _taille, 0.5f * _taille);
                PointF p2 = new PointF(0.00f * _taille, 1.5f * _taille);
                PointF p3 = new PointF(-0.25f * _taille, 0.5f * _taille);
                GLLines.DessineLigne(gl, p2, p1, LargeurLignes);
                GLLines.DessineLigne(gl, p2, p3, LargeurLignes);

            }

            gl.PopMatrix();
        }

        /// <summary>
        /// Deplacement du vaisseau
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="MIN_VIEWPORT_X"></param>
        /// <param name="MAX_VIEWPORT_X"></param>
        /// <param name="MIN_VIEWPORT_Y"></param>
        /// <param name="MAX_VIEWPORT_Y"></param>
        internal void Deplace(Temps maintenant, float MIN_VIEWPORT_X, float MAX_VIEWPORT_X, float MIN_VIEWPORT_Y, float MAX_VIEWPORT_Y)
        {
            // Vaisseau
            _rotationRadians += maintenant.intervalleDepuisDerniereFrame * _vrot;
            _x += _dx * maintenant.intervalleDepuisDerniereFrame;
            _y += _dy * maintenant.intervalleDepuisDerniereFrame;
            _poussee -= 0.25f;
            _dx *= FREINAGE_INERTIEL;
            _dy *= FREINAGE_INERTIEL;
            _vrot *= FREINAGE_INERTIEL;

            MathUtils.ContraintTore(ref _x, MIN_VIEWPORT_X, MAX_VIEWPORT_X);
            MathUtils.ContraintTore(ref _y, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);

            // Tirs
            int i = 0;
            while (i < _tirs.Count)
            {
                Tir t = _tirs[i];
                if (t.vie > 0)
                {
                    t.x += t.dx * maintenant.intervalleDepuisDerniereFrame;
                    t.y += t.dy * maintenant.intervalleDepuisDerniereFrame;
                    t.vie -= maintenant.intervalleDepuisDerniereFrame;
                    MathUtils.ContraintTore(ref t.x, MIN_VIEWPORT_X, MAX_VIEWPORT_X);
                    MathUtils.ContraintTore(ref t.y, MIN_VIEWPORT_Y, MAX_VIEWPORT_Y);
                    i++;
                }
                else
                    _tirs.RemoveAt(i);
            }
        }



        /// <summary>
        /// Le vaisseau allume son reacteur et accelere vers l'avant
        /// </summary>
        internal void Poussee()
        {
            float pX = (float)Math.Sin(_rotationRadians) * POUSSEE;
            float pY = (float)Math.Cos(_rotationRadians) * POUSSEE;

            _dx += pX;
            _dy -= pY;
            _poussee = 1.0f;
        }

        internal void TournerAGauche()
        {
            _vrot += VITESSE_ROT;
            if (_vrot > VROT_MAX)
                _vrot = VROT_MAX;
        }

        internal void TournerADroite()
        {
            _vrot -= VITESSE_ROT;
            if (_vrot < -VROT_MAX)
                _vrot = -VROT_MAX;
        }

        internal void Tirer()
        {
            if (_tirs.Count >= NB_MAX_TIRS)
                return;

            Tir t = new Tir
            {
                dx = _dx + VITESSE_TIR * (float)Math.Sin(_rotationRadians),
                dy = _dy - VITESSE_TIR * (float)Math.Cos(_rotationRadians)
            };
            t.x = _x + t.dx * _taille;
            t.y = _y + t.dy * _taille;
            t.vie = VIE_TIR;
            _tirs.Add(t);
        }
    }

}
