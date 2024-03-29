﻿using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using ClockScreenSaverGL.Trajectoires;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class AttracteurParticules : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        #region Parametres
        private const string CAT = "AttracteurParticules";
        protected CategorieConfiguration c;
        private int NB_EMETTEURS;
        private int NB_ATTRACTEURS;
        private int NB_PARTICULES_EMISES;
        private float ALPHA_MODIFIEUR;
        private float TAILLE_PARTICULE;
        private float VITESSE_PARTICULE;
        private float VITESSE_EMETTEUR;
        private float VITESSE_ATTRACTEUR;
        private float G;
        #endregion

        public AttracteurParticules(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            AjouteTexture(c.GetParametre("Particule", Configuration.GetImagePath("particule.png")), 1);
            for (int i = 0; i < NB_EMETTEURS; i++)
            {
                Trajectoire t = new TrajectoireOvale(0, 0, MAX_X * 0.8f, MAX_Y * 0.8f, VITESSE_EMETTEUR * FloatRandom(0.5f, 1.5f), -(float)Math.PI / 2.0f);
                AjouteEmetteur(new EmetteurJet(TAILLE_PARTICULE, VITESSE_PARTICULE, NB_PARTICULES_EMISES, t));
            }
            AttributBlend = PARTICULES_BLEND_ADDITIVE;
            AjouteModificateur(new ModificateurExclusion(MIN_X * 2, MIN_Y * 2, MAX_X * 2, MAX_Y * 2,
                ModificateurExclusion.Exclusions.EXCLURE_TOUT));

            AjouteModificateur(new ModificateurAlpha(ALPHA_MODIFIEUR, true));
            AjouteModificateur(new ModificateurLife());
            AjouteModificateur(new ModificateurVitesseLineaire());
            for (int i = 0; i < NB_ATTRACTEURS; i++)
            {
                Trajectoire t = new TrajectoireOvale(0, 0, MAX_X * 0.4f, MAX_Y * 0.4f, -VITESSE_ATTRACTEUR * FloatRandom(0.5f, 1.5f), (float)Math.PI / 2.0f);
                AjouteModificateur(new ModificateurAttracteur(t, FloatRandom(G * 0.5f, G * 2.0f)));
            }
            AjouteModificateur(new ModificateurAlpha(ALPHA_MODIFIEUR));
        }

        public override CategorieConfiguration GetConfiguration()
        {

            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_EMETTEURS = c.GetParametre("Nb Emetteurs", 1);
                NB_ATTRACTEURS = c.GetParametre("Nb Emetteurs", 1);
                NB_MAX_PARTICULES = c.GetParametre("Nb Particules", 100000);
                NB_PARTICULES_EMISES = c.GetParametre("Nb ParticulesEmises", 10);
                ALPHA_MODIFIEUR = c.GetParametre("Modifieur Alpha", 0.002f);
                TAILLE_PARTICULE = c.GetParametre("TailleParticule", 0.012f);
                VITESSE_PARTICULE = c.GetParametre("VitesseParticule", 0.04f);
                VITESSE_EMETTEUR = c.GetParametre("VitesseEmetteur", 0.05f);
                VITESSE_ATTRACTEUR = c.GetParametre("VitesseAttracteur", 0.02f);
                G = c.GetParametre("G", 0.3f);
            }
            return c;
        }
    }
}
