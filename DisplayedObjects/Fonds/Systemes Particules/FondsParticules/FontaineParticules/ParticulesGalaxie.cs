﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class ParticulesGalaxie : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        private const String CAT = "Particules galaxies";
        private CategorieConfiguration c;
        private int NB_EMETTEURS;
        private float ALPHA_MODIFIEUR;
        private float TAILLE_MODIFIEUR;
        private float TAILLE_PARTICULE;
        private float VITESSE_ANGLE;
        private float VITESSE_PARTICULE;
        private Texture[] _texture = new Texture[3];

        public ParticulesGalaxie(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            AjouteTexture(c.GetParametre("nuages petits", Configuration.GetImagePath("nuages_petits.png")), 3);

            for (int i = 0; i < NB_EMETTEURS; i++)
                AjouteEmetteur(new EmetteurGalaxie(TAILLE_PARTICULE, VITESSE_ANGLE * FloatRandom(0.9f, 1.2f), VITESSE_PARTICULE * FloatRandom(0.9f, 1.2f), random.Next(2, 8)));

            AttributBlend = PARTICULES_BLEND_ADDITIVE;

            AjouteModificateur(new ModificateurExclusion(MIN_X * 1.1f, MIN_Y * 1.1f, MAX_X * 1.1f, MAX_Y * 1.1f, ModificateurExclusion.Exclusions.EXCLURE_TOUT));

            AjouteModificateur(new ModificateurLife());
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurAlpha(ALPHA_MODIFIEUR));
            AjouteModificateur(new ModificateurTaille(TAILLE_MODIFIEUR));
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_EMETTEURS = c.GetParametre("Nb Emetteurs", 2);
                NB_MAX_PARTICULES = c.GetParametre("Nb Particules", 10000);
                ALPHA_MODIFIEUR = c.GetParametre("Modifieur Alpha", 0.1f, (a) => { ALPHA_MODIFIEUR = (float)Convert.ToDouble(a); });
                TAILLE_MODIFIEUR = c.GetParametre("Modifieur Taille", 0.02f);
                TAILLE_PARTICULE = c.GetParametre("TailleParticule", 0.01f);
                VITESSE_ANGLE = c.GetParametre("VitesseAngle", 2.0f);
                VITESSE_PARTICULE = c.GetParametre("VitesseParticule", 0.1f);
            }
            return c;
        }
    }
}
