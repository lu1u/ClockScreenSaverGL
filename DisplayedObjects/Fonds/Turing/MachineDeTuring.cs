using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Turing
{
    internal partial class MachineDeTuring : Fond
    {
        #region PARAMETRES
        private const string CAT = "Turing";
        protected CategorieConfiguration c;

        internal static readonly string PARAM_REPERTOIRE = "Répertoire turing";
        internal static readonly string PARAM_LARGEUR_CASE_RUBAN = "Ruban largeur case";
        internal static readonly string PARAM_HAUTEUR_RUBAN = "Ruban Hauteur";
        internal static readonly string PARAM_FICHIER_EN_COURS = "Programme en cours";

        private int FICHIER_EN_COURS;

        public float HAUTEUR_RUBAN = 0.12f;
        public float X_RUBAN = 0.0f;
        public float Y_RUBAN = -0.3f;
        public float LARGEUR_CASE_RUBAN = 0.12f;

        public float HAUTEUR_TETE = 0.22f;
        public float LARGEUR_TETE = 0.15f;
        public float X_TETE = 0.0f;
        public float Y_TETE = -0.3f;

        private readonly float X_PROGRAMME = -0.95f;
        private readonly float Y_PROGRAMME = 0.75f;
        private readonly float LARGEUR_PROGRAMME = 0.6f;

        private string REPERTOIRE_TURING = "turing";
        public int DUREE_ANIMATION = 2000;

        #endregion

        private readonly Texture _textureCircuit;

        private delegate bool AnimationDelegate();

        private AnimationDelegate _animation;
        private readonly Tete _tete;
        private readonly Ruban _ruban;
        private readonly Programme _programme;
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                REPERTOIRE_TURING = c.GetParametre(PARAM_REPERTOIRE, "turing");
                FICHIER_EN_COURS = c.GetParametre(PARAM_FICHIER_EN_COURS, 0);
                DUREE_ANIMATION = c.GetParametre("Duree animation", 2000, (a) => { DUREE_ANIMATION = Convert.ToInt32(a); });
            }
            return c;
        }


        /// <summary>
        /// Initialisation de la machine
        /// </summary>
        /// <param name="gl"></param>
        protected override void Init(OpenGL gl)
        {
            c = GetConfiguration();
            InitOrdonnateur();
            _indiceRuban = ruban.Length / 2;
            _tete.Init(gl, c);
            _ruban.Init(gl, c);
            _programme.Init(gl, c, _nom, _description, _etatActif, _etats.Count, _etats[_etatActif]._commentaire);
        }

        public MachineDeTuring(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            _tete = new Tete(gl);
            _tete.getConfiguration(c);

            _ruban = new Ruban();
            _ruban.GetConfiguration(c);

            _programme = new Programme();
            _programme.GetConfiguration(c);

            _textureCircuit = new Texture();
            _textureCircuit.Create(gl, c.GetParametre("Circuit", Configuration.GetImagePath(REPERTOIRE_TURING + @"\circuit.png")));
        }



        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            float[] col = { couleur.R / 255.0f, couleur.G / 255.0f, couleur.B / 255.0f, 1 };
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH);

            using (new Viewport2D(gl, -1.0f, -1.0f, 1.0f, 1.0f))
            {
                // Circuit (fond d'ecran fixe
                gl.Disable(OpenGL.GL_BLEND);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                _textureCircuit.Bind(gl);
                gl.Color(col);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0, 0.0f); gl.Vertex(X_PROGRAMME + LARGEUR_PROGRAMME, Y_PROGRAMME, 0);
                gl.TexCoord(0, 1.0f); gl.Vertex(X_PROGRAMME + LARGEUR_PROGRAMME, Y_TETE, 0);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(X_TETE + LARGEUR_TETE * 2.0f, Y_TETE, 0);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(X_TETE + LARGEUR_TETE * 2.0f, Y_PROGRAMME, 0);
                gl.End();

                _ruban.Dessine(gl, couleur, ruban, _indiceRuban);
                _tete.Dessine(gl, couleur, _etatOrdonnateur, _valeurAEcrire);
                _programme.Dessine(gl, couleur, _etats[_etatActif]._instructions, _etatOrdonnateur);
            }

#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
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

            if (_animation != null)
            {
                if (_animation())
                    EtapeSuivante();
            }
            else
                EtapeSuivante();

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif
        }



        public override string DumpRender()
        {
            string texte;
            switch (_etatOrdonnateur)
            {
                case ETAPE_TURING.DEBUT: texte = "début"; break;
                case ETAPE_TURING.CHANGE_ETAT: texte = "change état"; break;
                case ETAPE_TURING.DEPLACEMENT: texte = "déplacement"; break;
                case ETAPE_TURING.ECRITURE: texte = "écriture"; break;
                case ETAPE_TURING.LECTURE: texte = "lecture"; break;
                case ETAPE_TURING.RECHERCHE_INSTRUCTION: texte = "recherche instruction"; break;
                default: texte = "inconnu"; break;
            }
            return base.DumpRender() + " Etape: " + texte + ", instruction active " + _instructionActive;
        }
    }
}
