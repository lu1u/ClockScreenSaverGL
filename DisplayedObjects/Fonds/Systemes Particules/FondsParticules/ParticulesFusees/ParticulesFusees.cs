using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    internal class ParticulesFusees : SystemeParticules2D.SystemeParticules2D, IDisposable
    {
        private const string CAT = "ParticulesFusees";
        protected CategorieConfiguration c;
        private int NB_EMETTEURS;
        private float GRAVITE_X;
        private float GRAVITE_Y;
        private float ALPHA_MODIFIEUR;
        private float TAILLE_MODIFIEUR;
        private float TAILLE_PARTICULE;
        private float VITESSE_ANGLE;
        private float VITESSE_PARTICULE;
        private float VITESSE_FUSEE;

        public ParticulesFusees(OpenGL gl) : base(gl)
        {
            getConfiguration();
            for (int i = 0; i < NB_EMETTEURS; i++)
                AjouteEmetteur(new EmetteurFusee(TAILLE_PARTICULE, VITESSE_ANGLE, VITESSE_PARTICULE, VITESSE_FUSEE));

            AttributBlend = PARTICULES_BLEND_ADDITIVE;
            typeFond = TYPE_FOND.FOND_COULEUR;
            couleurParticules = COULEUR_PARTICULES.BLANC;
            AjouteTexture(c.getParametre("Nuages petits", Config.Configuration.getImagePath("nuages_petits.png")), 3);

            AjouteModificateur(new ModificateurExclusion(MIN_X, MIN_Y, MAX_X, MAX_Y,
                ModificateurExclusion.Exclusions.EXCLURE_AU_DESSUS | ModificateurExclusion.Exclusions.EXCLURE_A_DROITE | ModificateurExclusion.Exclusions.EXCLURE_A_GAUCHE));
            AjouteModificateur(new ModificateurLife());
            AjouteModificateur(new ModificateurVitesseLineaire());
            AjouteModificateur(new ModificateurGravite(GRAVITE_X, GRAVITE_Y));
            AjouteModificateur(new ModificateurAlpha(ALPHA_MODIFIEUR));
            AjouteModificateur(new ModificateurTaille(TAILLE_MODIFIEUR));
        }

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NB_EMETTEURS = c.getParametre("Nb Emetteurs", 5);
                NB_MAX_PARTICULES = c.getParametre("Nb Particules", 1000);
                GRAVITE_X = c.getParametre("Gravite X", 0.02f);
                GRAVITE_Y = c.getParametre("Gravite Y", 0.02f);
                ALPHA_MODIFIEUR = c.getParametre("Modifieur Alpha", 0.6f);
                TAILLE_MODIFIEUR = c.getParametre("Modifieur Taille", 0.05f);
                TAILLE_PARTICULE = c.getParametre("TailleParticule", 0.01f);
                VITESSE_ANGLE = c.getParametre("VitesseAngle", 0.5f);
                VITESSE_PARTICULE = c.getParametre("VitesseParticule", 0.2f);
                VITESSE_FUSEE = c.getParametre("VitesseParticule", 0.5f);

            }
            return c;
        }
    }
}
