using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Particules;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.FontaineParticulesPluie
{
    internal class FontaineParticulesPluie : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        #region Parametres
        private const string CAT = "Fontaine Pluie";
        protected CategorieConfiguration c;
        private int NB_EMETTEURS = 16;// c.getParametre("NB Emetteurs", 8);
        private float MODIFICATEUR_TAILLE = 0.01f;// c.getParametre("Modifieur Taille", 1.01f);
        private float MODIFICATEUR_ALPHA = 0.2f;// c.getParametre("Modifieur Alpha", 0.75f);
        private float VITESSE_X = 0.1f;
        private float VITESSE_Y = 0.7f;
        private float GRAVITE_X = 0.1f;
        private float GRAVITE_Y = -0.4f;
        #endregion

        public FontaineParticulesPluie(OpenGL gl) : base(gl)
        {
            getConfiguration();

            //AttributBlend = SystemeParticules.PARTICULES_BLEND_ADDITIVE;
            couleurParticules = COULEUR_PARTICULES.NOIR;
            typeFond = TYPE_FOND.FOND_COULEUR;
            for (int i = 0; i < NB_EMETTEURS; i++)
                AjouteEmetteur(new EmetteurPluie(VITESSE_X, VITESSE_Y));

            AjouteTexture(c.getParametre("Flares", Configuration.getImagePath(@"ete\flares.png")), 4);

            AjouteModificateur(new ModificateurExclusion(MIN_X, MIN_Y, MAX_X, MAX_Y, ModificateurExclusion.Exclusions.EXCLURE_EN_DESSOUS));
            AjouteModificateur(new ModificateurLife());
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurGravite(GRAVITE_X, GRAVITE_Y));
            AjouteModificateur(new ModificateurAlpha(MODIFICATEUR_ALPHA));
            AjouteModificateur(new ModificateurTaille(MODIFICATEUR_TAILLE));
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NB_MAX_PARTICULES = c.getParametre("Nb Particules", 2000);
                NB_EMETTEURS = c.getParametre("NB Emetteurs", 16);
                MODIFICATEUR_TAILLE = c.getParametre("Modifieur Taille", 0.01f);
                MODIFICATEUR_ALPHA = c.getParametre("Modifieur Alpha", 0.2f);
                VITESSE_X = 0.1f;
                VITESSE_Y = 0.7f;
                GRAVITE_X = 0.1f;
                GRAVITE_Y = -0.4f;
            }
            return c;
        }
    }
}
