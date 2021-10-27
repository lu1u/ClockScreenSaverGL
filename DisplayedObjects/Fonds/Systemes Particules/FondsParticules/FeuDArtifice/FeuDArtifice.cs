using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using SharpGL;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D;
using ClockScreenSaverGL.DisplayedObjects.Fonds.SystemeParticules2D.Modificateurs;
using SharpGL.SceneGraph.Assets;
using ClockScreenSaverGL.Config;
using System.Threading.Tasks;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Particules
{
    class FeuDArtifice : SystemeParticules2D.SystemeParticules2D, IDisposable
    {

		#region Parametres
		const string CAT = "FeuDArtifice";
        CategorieConfiguration c;
		int NB_EMETTEURS;
		float GRAVITE_X;
		float GRAVITE_Y;
		float ALPHA_MODIFIEUR;
		float TAILLE_MODIFIEUR;
		float TAILLE_PARTICULE;
		float VITESSE_PARTICULE;
		#endregion

		public FeuDArtifice(OpenGL gl) : base(gl)
        {
			getConfiguration();
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

        public override CategorieConfiguration getConfiguration()
        {
			if ( c == null)
			{
				c = Configuration.getCategorie(CAT);
				NB_EMETTEURS = c.getParametre("Nb Emetteurs", 1);
				NB_MAX_PARTICULES = c.getParametre("Nb Particules", 5000);
				GRAVITE_X = c.getParametre("Gravite X", 0.0f);
				GRAVITE_Y = -c.getParametre("Gravite Y", 0.5f);
				ALPHA_MODIFIEUR = c.getParametre("Modifieur Alpha", 0.4f);
				TAILLE_MODIFIEUR = c.getParametre("Modifieur Taille", 0.001f);
				TAILLE_PARTICULE = c.getParametre("TailleParticule", 0.002f);
				VITESSE_PARTICULE = c.getParametre("VitesseParticule", 0.5f);
			}
			return c;
        }
    }
}
