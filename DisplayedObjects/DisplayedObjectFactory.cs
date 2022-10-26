using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds;
using ClockScreenSaverGL.DisplayedObjects.Fonds.FontaineParticulesPluie;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Gravity;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Particules;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Saisons.Ete;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Boids;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Grilles;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.MarchingCubes;
using ClockScreenSaverGL.DisplayedObjects.Fonds.Turing;
using ClockScreenSaverGL.DisplayedObjects.Metaballes;
using ClockScreenSaverGL.DisplayedObjects.Saisons;
using SharpGL;
using System;
using System.Windows.Forms;

namespace ClockScreenSaverGL.DisplayedObjects
{
    class DisplayedObjectFactory
    {
        public const string CAT = "Saison";
        
        #region Fonds
        public enum FONDS
        {
            ESPACE, TROISDPIPES, COURONNES, GRILLE, PARTICULES_GRAVITATION, METABALLES, BOIDS_OISEAUX, MULTICHAINES, NUAGES, MOLECULE, PARTICULES_PLUIE, 
            CARRE_ESPACE, ENCRE, REBOND, ESCALIER, TUNNEL, NEIGE_META, DOUBLE_PENDULE, LIFE, TERRE,
            BACTERIES, PARTICULES1, COULEUR, FUSEES, ARTIFICE, NOIR, ATTRACTEUR, NEBULEUSE, VIELLES_TELES, GRAVITE, ENGRENAGES, CUBES, 
            BOIDS_POISSONS, EPICYCLE2,
            MYRIADE, CONSOLE, MOTO, MARCHING_CUBES, TRIANGLES, EPICYCLE, TURING, /*MOIRE,*//*DONJON, */ ADN, LIFE_SIM, FOURMIS, SINUSOIDE
        };

        public const FONDS PREMIER_FOND = FONDS.ESPACE;
        public const FONDS DERNIER_FOND = FONDS.SINUSOIDE;
        #endregion

        enum SAISON { HIVER = 0, PRINTEMPS = 1, ETE = 2, AUTOMNE = 3 };
        /// <summary>
        /// Retourne la saison, (calcul tres approximatif)
        /// </summary>
        /// <returns></returns>
        private static SAISON getSaison()
        {
             CategorieConfiguration c = Configuration.getCategorie(CAT);
            int PRINTEMPS = c.getParametre("Printemps", 80);
            int ETE = c.getParametre("Ete", 172);
            int AUTOMNE = c.getParametre("Automne", 265);
            int HIVER = c.getParametre("Hiver", 356);
            int forceSaison = c.getParametre("Force saison", -1);

            if (forceSaison != -1)
                // Forcage de la saison
                return (SAISON)forceSaison;

            DateTime date = DateTime.Now;

            int quantieme = date.DayOfYear;
            // Hiver : jusqu'a l'equinoxe de printemps
            if (quantieme < PRINTEMPS)
                return SAISON.HIVER;

            // Printemps: jusqu'au solstice d'ete
            if (quantieme <= ETE)
                return SAISON.PRINTEMPS;

            // Ete: jusqu'a l'equinoxe d'automne
            if (quantieme < AUTOMNE)
                return SAISON.ETE;

            // Automne : jusqu'au solstice d'hiver
            if (quantieme < HIVER)
                return SAISON.AUTOMNE;

            return SAISON.HIVER;
        }
        public static Fond getObjetFond(OpenGL gl, FONDS Type, bool initial, bool fondDeSaison)
        {
            if (fondDeSaison && initial)
            {
                // Si l'option 'fond de saison' est selectionnee, l'economiseur commence par celui ci
                // Note: il n'apparaissent plus dans le cycle de changement du fond
                switch (getSaison())
                {
                    case SAISON.HIVER:
                        return new Hiver(gl);
                    case SAISON.PRINTEMPS:
                        return new Printemps(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                    case SAISON.ETE:
                        return new Ete(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                    case SAISON.AUTOMNE:
                        return new Automne(gl);
                }
            }

            //Type = FONDS.MOTO;

            switch (Type)
            {
                case FONDS.METABALLES: return new Neige(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.ENCRE: return new Encre(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.BACTERIES: return new Bacteries(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.LIFE: return new Life(gl);
                case FONDS.NOIR: return new Noir(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.COURONNES: return new Couronnes(gl);
                case FONDS.COULEUR: return new Couleur(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case FONDS.ESPACE: return new Espace(gl);
                case FONDS.TUNNEL: return new Tunnel(gl);
                case FONDS.CARRE_ESPACE: return new CarresEspace(gl);
                case FONDS.PARTICULES_GRAVITATION: return new GravitationParticules(gl);
                case FONDS.NUAGES: return new Nuages2(gl);
                case FONDS.TERRE: return new TerreOpenGL(gl);
                case FONDS.PARTICULES1: return new ParticulesGalaxie(gl);
                case FONDS.PARTICULES_PLUIE: return new FontaineParticulesPluie(gl);
                case FONDS.FUSEES: return new ParticulesFusees(gl);
                case FONDS.MULTICHAINES: return new MultiplesChaines(gl);
                case FONDS.VIELLES_TELES: return new ViellesTeles(gl);
                case FONDS.ARTIFICE: return new FeuDArtifice(gl);
                case FONDS.ATTRACTEUR: return new AttracteurParticules(gl);
                case FONDS.GRAVITE: return new Gravitation(gl);
                case FONDS.REBOND: return new RebondParticules(gl);
                case FONDS.ENGRENAGES: return new Engrenages(gl);
                case FONDS.CUBES: return new Cubes(gl);
                case FONDS.NEBULEUSE: return new Nebuleuse(gl);
                case FONDS.ADN: return new ADN(gl);
                case FONDS.BOIDS_OISEAUX: return new BoidsOiseaux(gl);
                case FONDS.BOIDS_POISSONS: return new BoidsPoissons(gl);
                case FONDS.MOLECULE: return new Molecule(gl);
                case FONDS.MYRIADE: return new Myriade(gl);
                case FONDS.CONSOLE: return new VielleConsole(gl);
                case FONDS.GRILLE: return new Grille(gl);
                case FONDS.ESCALIER: return new Escaliers(gl);
                case FONDS.MOTO: return new Moto(gl);
                case FONDS.MARCHING_CUBES: return new MarchingCubes(gl);
                case FONDS.TRIANGLES: return new Triangles(gl);
                case FONDS.EPICYCLE: return new Epicycle(gl);
                case FONDS.EPICYCLE2: return new Epicycle2(gl);
                case FONDS.DOUBLE_PENDULE: return new PenduleDouble(gl);
                case FONDS.TROISDPIPES: return new TroisDPipes(gl);
                //case FONDS.MOIRE: return new Moire(gl);
                case FONDS.TURING: return new MachineDeTuring(gl);
                case FONDS.SINUSOIDE: return new Sinusoides(gl);
                //case FONDS.DONJON: return new Donjon(gl);
                case FONDS.LIFE_SIM: return new LifeSimulation(gl);
                case FONDS.FOURMIS: return new Fourmis(gl);
                default:
                    return new Metaballes.Metaballes(gl);
            }

        }

        public static DisplayedObject InitObjet(OpenGL gl, Random r)
        {
            switch (r.Next(18))
            {
                case 0: return new Neige(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case 1: return new Encre(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case 2: return new Bacteries(gl, SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
                case 3: return new Life(gl);
                case 4: return new Couronnes(gl);
                case 5: return new Nuages2(gl);
                case 6: return new Tunnel(gl);
                case 7: return new CarresEspace(gl);
                case 8: return new GravitationParticules(gl);
                case 9: return new TerreOpenGL(gl);
                case 10: return new ParticulesGalaxie(gl);
                case 11: return new ParticulesFusees(gl);
                case 12: return new FeuDArtifice(gl);
                case 13: return new AttracteurParticules(gl);
                case 14: return new Engrenages(gl);
                case 15: return new ADN(gl);
                case 16: return new Cubes(gl);
                case 17: return new Moto(gl);
                default:
                    return new Metaballes.Metaballes(gl);
            }
        }
    }
}
