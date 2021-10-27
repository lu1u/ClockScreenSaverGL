using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClockScreenSaverGL.Config;
using SharpGL;

/// <summary>
/// Affichage des actualites, extraites de flux RSS
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.PanneauActualites
{
    internal class Actualites : DisplayedObject, IDisposable
    {
        #region Parametres
        public const string CAT = "Actualites";
        protected static CategorieConfiguration c;
        public int NB_JOURS_MAX_INFO;
        public int HAUTEUR_BANDEAU;
        public float VITESSE;
        public int MIN_LIGNES;
        public int MAX_LIGNES;
        public int MAX_LIGNES_PAR_SOURCE;
        public int TAILLE_SOURCE;
        public int TAILLE_TITRE;
        public int TAILLE_DESCRIPTION;
        public static bool AFFICHE_DESCRIPTION;
        public bool AFFICHE_IMAGES;
        public float SATURATION_IMAGES;
        public float LIGHT_FACTOR;

        #endregion

        private float _decalageX = SystemInformation.VirtualScreen.Width;
        public static int _derniereAffichee;

        private ActuFactory _actuFactory;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="gl"></param>
        public Actualites(OpenGL gl) : base(gl)
        {
            getConfiguration();
            LigneActu.TAILLE_TITRE = TAILLE_TITRE;
            LigneActu.TAILLE_DESCRIPTION = TAILLE_DESCRIPTION;
            LigneActu.TAILLE_SOURCE = TAILLE_SOURCE;
            LigneActu.HAUTEUR_BANDEAU = HAUTEUR_BANDEAU;
            LigneActu.SATURATION_IMAGES = SATURATION_IMAGES;
            _actuFactory = new ActuFactory();
        }

        public override void Init(OpenGL gl)
        {
            _actuFactory.Init(gl);
        }


        public override void Dispose()
        {
            base.Dispose();
            _actuFactory.Dispose();
        }
        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                NB_JOURS_MAX_INFO = c.getParametre("Nb jours info max", 4);
                HAUTEUR_BANDEAU = c.getParametre("Hauteur bandeau", 150);
                VITESSE = c.getParametre("Vitesse", 75.0f);
                MIN_LIGNES = c.getParametre("Nb lignes min", 50);
                MAX_LIGNES = c.getParametre("Nb lignes max", 100);
                MAX_LIGNES_PAR_SOURCE = c.getParametre("Nb lignes max par source", 10);
                TAILLE_SOURCE = c.getParametre("Taille fonte source", 16);
                TAILLE_TITRE = c.getParametre("Taille fonte titre", 30);
                TAILLE_DESCRIPTION = c.getParametre("Taille fonte description", 14);
                AFFICHE_DESCRIPTION = c.getParametre("Affiche Description", true);
                AFFICHE_IMAGES = c.getParametre("Affiche Images", true);
                SATURATION_IMAGES = c.getParametre("Saturation images", 0.5f);
                LIGHT_FACTOR = c.getParametre("Luminosité", 5.0f);
            }
            return c;
        }
        /// <summary>
        /// Affichage deroulant des actualites
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        /// <param name="couleur"></param>
        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            couleur = CouleurGlobale.Light(couleur, LIGHT_FACTOR);

            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.LoadIdentity();
            gl.Ortho2D(0, tailleEcran.Width, 0, tailleEcran.Height);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Color(0.1f, 0.1f, 0.1f, 0.55f); // Fond sombre
            gl.Rect(tailleEcran.Left, tailleEcran.Top + HAUTEUR_BANDEAU, tailleEcran.Right, tailleEcran.Top);

            float x = tailleEcran.Left + _decalageX;
            _derniereAffichee = 0;

            #region LignesActu
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 1.0f);

            if (_actuFactory._lignes != null)
                try
                {
                    foreach (LigneActu l in _actuFactory._lignes)
                    {
                        l.affiche(gl, x, tailleEcran.Top + HAUTEUR_BANDEAU, AFFICHE_DESCRIPTION);
                        x += l.largeur;
                        _derniereAffichee++;
                        if (x > tailleEcran.Right)
                            break;
                    }
                }
                catch (Exception)
                {
                }


            #endregion

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PopMatrix();

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            _decalageX -= VITESSE * maintenant.intervalleDepuisDerniereFrame;

            if (_actuFactory._lignes != null)
            {
                if (_actuFactory._lignes.Count > 1)
                    if (_decalageX + _actuFactory._lignes[0].largeur < 0)
                    {
                        // Deplacer la ligne vers la fin du tableau
                        _decalageX += _actuFactory._lignes[0].largeur;
                        LigneActu premiereLigne = _actuFactory._lignes[0];
                        premiereLigne.Clear();

                        _actuFactory._lignes.RemoveAt(0);
                        _actuFactory._lignes.Add(premiereLigne);
                    }
            }
        }

        public override bool KeyDown(Form f, Keys k)
        {
            if (Keys.J.Equals(k))
            {
                if (_actuFactory._lignes?.Count >= 1)
                    lock (_actuFactory._lignes)
                        _actuFactory._lignes.RemoveAt(0);
                return true;
            }
            if (Keys.E.Equals(k))
            {
                lock (_actuFactory._lignes)
                    _actuFactory._lignes?.Clear();
                AFFICHE_DESCRIPTION = !AFFICHE_DESCRIPTION;
                c.setParametre("Affiche Description", AFFICHE_DESCRIPTION);
                c.flush();
                return true;
            }
            if (Keys.I.Equals(k))
            {
                lock (_actuFactory._lignes)
                    _actuFactory._lignes?.Clear();
                AFFICHE_IMAGES = !AFFICHE_IMAGES;
                c.setParametre("Affiche Images", AFFICHE_DESCRIPTION);
                c.flush();
                return true;
            }

            return base.KeyDown(f, k);
        }
    }
}
