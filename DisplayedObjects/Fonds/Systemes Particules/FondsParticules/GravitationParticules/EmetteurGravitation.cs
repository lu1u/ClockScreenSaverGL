﻿using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class EmetteurGravitation : Emetteur2D
    {
        private readonly float _G;
        private readonly float _multDist;
        private readonly TimerIsole _timer;
        private readonly float _taille = 0.01f;

        public EmetteurGravitation(float G, float MultDist, int delaiCreation)
        {
            _G = G;
            _multDist = MultDist;
            _timer = new TimerIsole(delaiCreation);
        }

        public override void Deplace(SystemeParticules2D.SystemeParticules2D s, Temps maintenant, Color couleur)
        {
            if (_timer.Ecoule() && (s._nbParticules < s.NB_MAX_PARTICULES))
            {
                int indice = s.FindUnusedParticle();
                double angle = DisplayedObject.FloatRandom(0, (float)(Math.PI * 2.0));
                double distance = DisplayedObject.FloatRandom(0.01f, SystemeParticules2D.SystemeParticules2D.MAX_X);

                s._particules[indice].x = (float)(Math.Sin(angle) * distance);
                s._particules[indice].y = (float)(Math.Cos(angle) * distance);
                s._particules[indice].vx = (float)(Math.Sin(angle + (float)(Math.PI / 2.0)) / Math.Sqrt(distance * _multDist)) * _G * 2.0f;
                s._particules[indice].vy = (float)(Math.Cos(angle + (float)(Math.PI / 2.0)) / Math.Sqrt(distance * _multDist)) * _G * 2.0f;
                s._particules[indice].alpha = 1f;
                s._particules[indice].debutVie = maintenant.totalMilliSecondes;

                s._particules[indice].taille = _taille * DisplayedObject.FloatRandom(0.9f, 1.1f);
                s._particules[indice].active = true;
                s._particules[indice].textureIndex = r.Next(0, s._listeTextures.Count);

                s._particules[indice]._couleur[0] = couleur.R / 256.0f;
                s._particules[indice]._couleur[1] = couleur.G / 256.0f;
                s._particules[indice]._couleur[2] = couleur.B / 256.0f;
                s._particules[indice]._couleur[3] = 1;
                s._particules[indice]._couleurIndividuelle = true;
                s.Trier = true;
            }
        }
    }
}