using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

/// <summary>
/// Sinusoides et leur somme
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    class Sinusoides : Fond
    {
        #region PARAMETRES
        private const string CAT = "Sinusoides";
        protected CategorieConfiguration c;
        private byte ALPHA_COURBE;
        private byte ALPHA_TOTAL;
        private int NB_COURBES;
        private int NB_SEGMENTS;
        private float TAILLE_TOTALE;
        private float FREQUENCE_MIN, FREQUENCE_MAX, PHASE_MIN, PHASE_MAX;
        private bool AFFICHER_AXES;
        private bool FONDU_AU_NOIR;
        #endregion

        // Donnees des sinusoides
        float[] _frequence;
        float[] _phase;
        float[] _amplitude;
        float[] _changementPhase;

        public override CategorieConfiguration getConfiguration()
        {
            if (c == null)
            {
                c = Configuration.getCategorie(CAT);
                AFFICHER_AXES = c.getParametre("Afficher axes", true, a => { AFFICHER_AXES = Convert.ToBoolean(a); });
                FONDU_AU_NOIR = c.getParametre("Fondu au noir", true, a => { FONDU_AU_NOIR = Convert.ToBoolean(a); });
                NB_SEGMENTS = c.getParametre("Nb Segments", 400, a => { NB_SEGMENTS = Convert.ToInt32(a); });
                NB_COURBES = c.getParametre("Nb Courbes", 5);
                ALPHA_COURBE = c.getParametre("Alpha Courbe", (byte)64, a => { ALPHA_COURBE = Convert.ToByte(a); });
                ALPHA_TOTAL = c.getParametre("Alpha Total", (byte)255, a => { ALPHA_COURBE = Convert.ToByte(a); });
                TAILLE_TOTALE = c.getParametre("Taille totale", 1.0f);
                PHASE_MIN = c.getParametre("Phase min", 0.2f);
                PHASE_MAX = c.getParametre("Phase max", 1.0f);
                FREQUENCE_MIN = c.getParametre("Fréquence min", 6f);
                FREQUENCE_MAX = c.getParametre("Fréquence max", 24f);
            }
            return c;
        }

        /// <summary>
        /// Initialisation des sinusoides
        /// </summary>
        /// <param name="gl"></param>
        protected override void Init(OpenGL gl)
        {
            c = getConfiguration();
            _frequence = new float[NB_COURBES];
            _phase = new float[NB_COURBES];
            _amplitude = new float[NB_COURBES];
            _changementPhase = new float[NB_COURBES];

            float total = 0.0f;
            for (int i = 0; i < NB_COURBES; i++)
            {
                _frequence[i] = FloatRandom(FREQUENCE_MIN, FREQUENCE_MAX);
                _phase[i] = FloatRandom(0, 2);
                _amplitude[i] = FloatRandom(0.1f, 2.0f);
                total += _amplitude[i];

                _changementPhase[i] = FloatRandom(PHASE_MIN, PHASE_MAX);
            }

            // Normaliser les amplitudes pour que le total fasse TAILLE_TOTALE
            for (int i = 0; i < NB_COURBES; i++)
                _amplitude[i] = _amplitude[i] * TAILLE_TOTALE / total;
        }

        public Sinusoides(OpenGL gl) : base(gl)
        {
            getConfiguration();
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float[] col = { couleur.R / 255.0f, couleur.G / 255.0f, couleur.B / 255.0f, 1 };
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_LINE_SMOOTH);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            float[] valeurs = new float[NB_SEGMENTS];
            float[] sommes = new float[NB_SEGMENTS];

            using (new Viewport2D(gl, -1.0f, -1.0f, 1.0f, 1.0f))
            {
                if (AFFICHER_AXES)
                    dessineAxes(gl, couleur);

                // Difference de couleur entre les courbes
                float hueChange = 1.0f / NB_COURBES;
                CouleurGlobale cG = new CouleurGlobale(couleur);
                // Dessiner les courbes
                for (int i = 0; i < NB_COURBES; i++)
                {
                    getCourbe(valeurs, _frequence[i], _phase[i], _amplitude[i]);

                    // Ajouter les valeurs au total
                    for (int j = 0; j < NB_SEGMENTS; j++)
                        sommes[j] += valeurs[j];

                    Color couleurCourbe = cG.getColorWithHueChange(i * hueChange);
                    dessineCourbe(gl, valeurs, couleurCourbe, ALPHA_COURBE);
                }

                dessineCourbe(gl, sommes, couleur, ALPHA_TOTAL);
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        // Dessine les axes horizontaux
        private void dessineAxes(OpenGL gl, Color couleur)
        {
            // Dessiner l'axe X
            Color alpha = getCouleurAvecAlpha(couleur, ALPHA_TOTAL);
            gl.Begin(OpenGL.GL_LINES);
            for (float y = -TAILLE_TOTALE; y <= TAILLE_TOTALE; y += TAILLE_TOTALE * 0.25f)
            {
                gl.Color(alpha.R, alpha.G, alpha.B, (byte)(255 - (Math.Abs(y) / TAILLE_TOTALE * 255.0f)));
                gl.Vertex(-1.0f, y, 0); gl.Vertex(1.0f, y, 0);
            }
            gl.End();
        }

        private void dessineCourbe(OpenGL gl, float[] valeurs, Color couleur, byte alpha)
        {
            gl.Begin(OpenGL.GL_QUAD_STRIP);
            for (int i = 0; i < valeurs.Length; i++)
            {
                float x = getX(i);
                if (FONDU_AU_NOIR)
                    gl.Color(0, 0, 0, 0);
                else
                    gl.Color(couleur.R, couleur.G, couleur.B, (byte)0);

                gl.Vertex(x, 0, 0);
                gl.Color(couleur.R, couleur.G, couleur.B, alpha); gl.Vertex(x, valeurs[i], 0);
            }
            gl.End();
        }

        /// <summary>
        /// Coordonnee X (-1.0..1.0) du segment i (O..NB_SEGMENTS)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] float getX(int i) => -1.01f + (2.02f / NB_SEGMENTS) * i;

        private void getCourbe(float[] valeurs, float longueurOnde, float phase, float amplitude)
        {
            for (int i = 0; i < NB_SEGMENTS; i++)
                valeurs[i] = (float)Math.Sin((getX(i) * longueurOnde) + phase) * amplitude;
        }

        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            return true;
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif
            for (int i = 0; i < NB_COURBES; i++)
                _phase[i] -= (maintenant.intervalleDepuisDerniereFrame * _changementPhase[i]);

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }
    }
}
