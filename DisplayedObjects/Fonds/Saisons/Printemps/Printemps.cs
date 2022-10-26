/***
 * Printemps: un arbre qui pousse
 * Inspire de http://www.jgallant.com/blog/
 */

using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps
{
    internal class Printemps : Fond
    {
        #region PARAMETRES
        private const String CAT = "Tree";
        public CategorieConfiguration c;
        public byte ALPHA;
        public int DELAI_RECOMMENCE;
        public float LARGEUR_TRONC;
        public int HAUTEUR_TRONC;
        public int LARGEUR_ARBRE;
        public int HAUTEUR_ARBRE;
        public int LONGUEUR_BRANCHE;
        public int DISTANCE_MIN;
        public int DISTANCE_MAX;
        public int NB_CIBLES;
        private float _oscillation = 0;

        #endregion
        private DateTime _finCroissance;
        private Tree _tree;
        public Printemps(OpenGL gl, int LargeurEcran, int HauteurEcran) : base(gl)
        {
            getConfiguration();

            _tree = new Tree(LargeurEcran, HauteurEcran * FloatRandom(0.4f, 0.6f), 0,
                LARGEUR_TRONC, LARGEUR_ARBRE, HAUTEUR_ARBRE, LONGUEUR_BRANCHE, DISTANCE_MIN, DISTANCE_MAX, NB_CIBLES, HAUTEUR_TRONC);
        }


        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_BLEND);
            using (new Viewport2D(gl, 0, 0, tailleEcran.Width, tailleEcran.Height))
            {
                gl.Color(0.0f, 0.0f, 0.0f, 1.0f);
                gl.Disable(OpenGL.GL_BLEND);
                _tree.Draw(gl);
            }



#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }
        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                ALPHA = (byte)c.getParametre("ALPHA", 128);
                DELAI_RECOMMENCE = c.getParametre("Delai nouvel arbre", 10) * 1000;
                LARGEUR_TRONC = c.getParametre("Largeur Tronc", 10);
                HAUTEUR_TRONC = c.getParametre("Hauteur Tronc", 200);
                LARGEUR_ARBRE = c.getParametre("Largeur Arbre", 1200);
                HAUTEUR_ARBRE = c.getParametre("Hauteur Arbre", 400);
                LONGUEUR_BRANCHE = c.getParametre("Longueur Branche", 7);
                DISTANCE_MIN = c.getParametre("Distance Min", 5);
                DISTANCE_MAX = c.getParametre("Distance Max", 100);
                NB_CIBLES = c.getParametre("Nb Cibles", 200);

            }
            return c;
        }
        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            c = getCouleurOpaqueAvecAlpha(c, ALPHA);

            gl.ClearColor(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            if (_tree.DoneGrowing)
            {
                if (maintenant.temps.Subtract(_finCroissance).TotalMilliseconds > DELAI_RECOMMENCE)
                    _tree = new Tree(tailleEcran.Width, tailleEcran.Height * 0.3f, 0, LARGEUR_TRONC, LARGEUR_ARBRE, HAUTEUR_ARBRE, LONGUEUR_BRANCHE, DISTANCE_MIN, DISTANCE_MAX, NB_CIBLES, HAUTEUR_TRONC);
            }
            else
                _finCroissance = maintenant.temps;
            if (UneFrameSur(2))
                _tree.Grow();
            _oscillation += maintenant.intervalleDepuisDerniereFrame * 1.5f;
            _tree.Oscillation((float)Math.Sin(_oscillation) * 0.02f);
#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }
    }
}