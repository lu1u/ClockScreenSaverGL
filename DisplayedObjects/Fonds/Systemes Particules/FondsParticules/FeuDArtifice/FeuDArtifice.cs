using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class FeuDArtifice : SystemeParticules2D.SystemeParticules2D, IDisposable
    {

        #region Parametres
        private const string CAT = "FeuDArtifice";
        private CategorieConfiguration c;
        private int NB_EMETTEURS;
        private float GRAVITE_X;
        private float GRAVITE_Y;
        private float ALPHA_MODIFIEUR;
        private float TAILLE_MODIFIEUR;
        private float TAILLE_PARTICULE;
        private float VITESSE_PARTICULE;
        #endregion

        public FeuDArtifice(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            for (int i = 0; i < NB_EMETTEURS; i++)
                AjouteEmetteur(new EmetteurFeuArtifice(TAILLE_PARTICULE, VITESSE_PARTICULE, 2000));

            AttributBlend = PARTICULES_BLEND_ADDITIVE;

            AjouteModificateur(new ModificateurExclusion(MIN_X,
                MIN_Y, MAX_X, MAX_Y,
                ModificateurExclusion.Exclusions.EXCLURE_A_DROITE | ModificateurExclusion.Exclusions.EXCLURE_A_GAUCHE | ModificateurExclusion.Exclusions.EXCLURE_EN_DESSOUS));

            AjouteModificateur(new ModificateurLife());
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurGravite(GRAVITE_X, GRAVITE_Y));
            AjouteModificateur(new ModificateurAlpha(ALPHA_MODIFIEUR));
            AjouteModificateur(new ModificateurTaille(TAILLE_MODIFIEUR));
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_EMETTEURS = c.GetParametre("Nb Emetteurs", 1);
                NB_MAX_PARTICULES = c.GetParametre("Nb Particules", 5000);
                GRAVITE_X = c.GetParametre("Gravite X", 0.0f);
                GRAVITE_Y = -c.GetParametre("Gravite Y", 0.5f);
                ALPHA_MODIFIEUR = c.GetParametre("Modifieur Alpha", 0.4f);
                TAILLE_MODIFIEUR = c.GetParametre("Modifieur Taille", 0.001f);
                TAILLE_PARTICULE = c.GetParametre("TailleParticule", 0.002f);
                VITESSE_PARTICULE = c.GetParametre("VitesseParticule", 0.5f);
            }
            return c;
        }
    }
}
