using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using ClockScreenSaverGL.Trajectoires;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    class AttracteurParticules : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        #region Parametres
        const string CAT = "AttracteurParticules";
        protected CategorieConfiguration c;
        int NB_EMETTEURS;
        int NB_ATTRACTEURS;
        int NB_PARTICULES_EMISES;
        float ALPHA_MODIFIEUR;
        float TAILLE_PARTICULE;
        float VITESSE_PARTICULE;
        float VITESSE_EMETTEUR;
        float VITESSE_ATTRACTEUR;
        float G;
        #endregion

        public AttracteurParticules(OpenGL gl) : base(gl)
        {
            getConfiguration();
            AjouteTexture(c.getParametre("Particule", Configuration.getImagePath("particule.png")), 1);
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

        public override CategorieConfiguration getConfiguration()
        {

            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NB_EMETTEURS = c.getParametre("Nb Emetteurs", 1);
                NB_ATTRACTEURS = c.getParametre("Nb Emetteurs", 1);
                NB_MAX_PARTICULES = c.getParametre("Nb Particules", 100000);
                NB_PARTICULES_EMISES = c.getParametre("Nb ParticulesEmises", 10);
                ALPHA_MODIFIEUR = c.getParametre("Modifieur Alpha", 0.002f);
                TAILLE_PARTICULE = c.getParametre("TailleParticule", 0.012f);
                VITESSE_PARTICULE = c.getParametre("VitesseParticule", 0.04f);
                VITESSE_EMETTEUR = c.getParametre("VitesseEmetteur", 0.05f);
                VITESSE_ATTRACTEUR = c.getParametre("VitesseAttracteur", 0.02f);
                G = c.getParametre("G", 0.3f);
            }
            return c;
        }
    }
}
