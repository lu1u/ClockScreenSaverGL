using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Turing
{
    /// <summary>
    /// Affichage du programme
    /// </summary>
    internal class Programme
    {
        #region Parametres
        private float X_PROGRAMME, Y_PROGRAMME;
        private float HAUTEUR_PROGRAMME, LARGEUR_PROGRAMME;
        private float LARGEUR_CASE_PROGRAMME;
        private float HAUTEUR_CASE_PROGRAMME;
        #endregion

        private Texture _textureProgramme, _textureSymbolesProgramme, _textureTeteProgramme;
        private Interpolateur _interpolateurProgramme, _interpolateurChangeEtatProgramme;
        private float _positionIndicateur;
        private float _cibleIndicateur;
        private float _departIndicateur;
        private float _progressionEtat;
        private Bitmap _bitmap;

        public Programme(OpenGL gl)
        {

        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="c"></param>
        /// <param name="nom"></param>
        /// <param name="etat"></param>
        /// <param name="nbEtats"></param>
        public void Init(OpenGL gl, CategorieConfiguration c, string nom, string description, int etat, int nbEtats, string commentaire)
        {
            _positionIndicateur = -1;
            _cibleIndicateur = 0;
            _progressionEtat = 0f;

            string REPERTOIRE_TURING = c.getParametre(MachineDeTuring.PARAM_REPERTOIRE, "turing");
            _textureProgramme = new Texture();
            _textureProgramme.Create(gl, c.getParametre("Programme Fond", Configuration.getImagePath(REPERTOIRE_TURING + @"\Programme.png")));
            _textureSymbolesProgramme = new Texture();
            _textureSymbolesProgramme.Create(gl, c.getParametre("Programme Symboles", Configuration.getImagePath(REPERTOIRE_TURING + @"\Symboles programme.png")));

            CreateBitmap(gl, nom, description, etat, nbEtats, commentaire);
        }

        public void getConfiguration(CategorieConfiguration c)
        {
            X_PROGRAMME = c.getParametre("Programme X", -0.95f, (a) => { X_PROGRAMME = (float)Convert.ToDouble(a); });
            Y_PROGRAMME = c.getParametre("Programme Y", 0.55f, (a) => { Y_PROGRAMME = (float)Convert.ToDouble(a); });
            HAUTEUR_PROGRAMME = c.getParametre("Programme Hauteur", 0.6f, (a) => { HAUTEUR_PROGRAMME = (float)Convert.ToDouble(a); });
            LARGEUR_PROGRAMME = c.getParametre("Programme Largeur", 0.6f, (a) => { LARGEUR_PROGRAMME = (float)Convert.ToDouble(a); });
            HAUTEUR_CASE_PROGRAMME = c.getParametre("Programme Hauteur Case", LARGEUR_PROGRAMME / 4.0f, (a) => { HAUTEUR_CASE_PROGRAMME = (float)Convert.ToDouble(a); });
            LARGEUR_CASE_PROGRAMME = c.getParametre("Programme Largeur Case", HAUTEUR_PROGRAMME / 4.0f, (a) => { LARGEUR_CASE_PROGRAMME = (float)Convert.ToDouble(a); });
        }

        public void Dessine(OpenGL gl, Color couleur, MachineDeTuring.Instruction[] instructions, MachineDeTuring.ETAPE_TURING etatOrdonnateur)
        {
            gl.PushMatrix();
            gl.Translate(X_PROGRAMME, Y_PROGRAMME, 0);

            // Entete du programme
            _textureTeteProgramme.Bind(gl);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);
            gl.TexCoord(0, 0.0f); gl.Vertex(0.01f, HAUTEUR_CASE_PROGRAMME * 2.7f, 0);
            gl.TexCoord(0, 1.0f); gl.Vertex(0.01f, HAUTEUR_CASE_PROGRAMME * 1.5f, 0);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(LARGEUR_PROGRAMME - 0.01f, HAUTEUR_CASE_PROGRAMME * 1.5f, 0);
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(LARGEUR_PROGRAMME - 0.01f, HAUTEUR_CASE_PROGRAMME * 2.7f, 0);
            gl.End();

            // Cases du programme
            _textureSymbolesProgramme.Bind(gl);
            afficheCaseProgramme(gl, 0, 0, MachineDeTuring.SYMBOLE_LECTURE, couleur);
            afficheCaseProgramme(gl, 1, 0, MachineDeTuring.SYMBOLE_ECRITURE, couleur);
            afficheCaseProgramme(gl, 2, 0, MachineDeTuring.SYMBOLE_DEPLACEMENT, couleur);
            afficheCaseProgramme(gl, 3, 0, MachineDeTuring.SYMBOLE_ETAT, couleur);

            for (int i = 0; i < instructions.Length; i++)
            {
                afficheCaseProgramme(gl, 0, i + 1, getTextureForChiffre(" 01"[i]), couleur);
                afficheCaseProgramme(gl, 1, i + 1, getTextureForChiffre(instructions[i]._valeurAEcrire), couleur);
                afficheCaseProgramme(gl, 2, i + 1, getTextureForChiffre(instructions[i]._decaleRuban), couleur);
                afficheCaseProgramme(gl, 3, i + 1, getTextureForEtat(instructions[i]._etatSuivant), couleur);
            }

            // Indicateur horizontal d'instruction active
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_ONE, OpenGL.GL_ONE);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Color(couleur.R / 512.0f, couleur.G / 512.0f, couleur.B / 512.0f);
            gl.PushMatrix();
            {
                gl.Translate(0, -(HAUTEUR_CASE_PROGRAMME * (_positionIndicateur + 1)), 0);
                gl.Begin(OpenGL.GL_QUADS);
                {
                    gl.Vertex(0, 0, 0);
                    gl.Vertex(0, HAUTEUR_CASE_PROGRAMME, 0);
                    gl.Vertex(LARGEUR_CASE_PROGRAMME * 4.0f, HAUTEUR_CASE_PROGRAMME, 0);
                    gl.Vertex(LARGEUR_CASE_PROGRAMME * 4.0f, 0, 0);
                }
                gl.End();
            }
            gl.PopMatrix();
            //
            //// Indicateur d'étape dans l'instruction
            int noColonne = getNoColonneFromEtatOrdonnateur(etatOrdonnateur);
            if (noColonne >= 0)
            {
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_ONE, OpenGL.GL_ONE);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                float ratioCouleur = 1.0f / 512.0f;
                if (etatOrdonnateur == MachineDeTuring.ETAPE_TURING.CHANGE_ETAT)
                    ratioCouleur = _progressionEtat / 512.0f;
                gl.Color(couleur.R * ratioCouleur, couleur.G * ratioCouleur, couleur.B * ratioCouleur);
                gl.PushMatrix();
                {
                    gl.Translate(LARGEUR_CASE_PROGRAMME * noColonne, 0, 0);
                    gl.Begin(OpenGL.GL_QUADS);
                    {
                        gl.Vertex(0, HAUTEUR_CASE_PROGRAMME, 0);
                        gl.Vertex(0, HAUTEUR_CASE_PROGRAMME * -3.0f, 0);
                        gl.Vertex(LARGEUR_CASE_PROGRAMME, HAUTEUR_CASE_PROGRAMME * -3.0f, 0);
                        gl.Vertex(LARGEUR_CASE_PROGRAMME, HAUTEUR_CASE_PROGRAMME, 0);
                    }
                    gl.End();
                }
                gl.PopMatrix();
            }

            // Fond
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _textureProgramme.Bind(gl);
            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0, 1.0f); gl.Vertex(-0.01f, -HAUTEUR_PROGRAMME + 0.12f, 0);
            gl.TexCoord(0, 0.0f); gl.Vertex(-0.01f, HAUTEUR_PROGRAMME - 0.1f, 0);
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(LARGEUR_PROGRAMME + 0.01f, HAUTEUR_PROGRAMME - 0.1f, 0);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(LARGEUR_PROGRAMME + 0.01f, -HAUTEUR_PROGRAMME + 0.12f, 0);
            gl.End();

            gl.PopMatrix();
        }

        private void afficheCaseProgramme(OpenGL gl, int x, int y, int texture, Color couleur)
        {
            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);

            float xCoordGauche = texture / (float)MachineDeTuring.NB_SYMBOLES;
            float xCoordDroite = (texture + 1) / (float)MachineDeTuring.NB_SYMBOLES;

            gl.PushMatrix();
            gl.Translate(LARGEUR_CASE_PROGRAMME * x, -HAUTEUR_CASE_PROGRAMME * y, 0);

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(xCoordGauche, 1.0f); gl.Vertex(0, 0, 0);
            gl.TexCoord(xCoordGauche, 0.0f); gl.Vertex(0, HAUTEUR_CASE_PROGRAMME, 0);
            gl.TexCoord(xCoordDroite, 0.0f); gl.Vertex(LARGEUR_CASE_PROGRAMME, HAUTEUR_CASE_PROGRAMME, 0);
            gl.TexCoord(xCoordDroite, 1.0f); gl.Vertex(LARGEUR_CASE_PROGRAMME, 0, 0);
            gl.End();
            gl.PopMatrix();
        }

        private int getTextureForEtat(int etape)
        {
            if (etape >= 0 && etape <= 9)
                return MachineDeTuring.SYMBOLE_ZERO + etape;
            else
                return MachineDeTuring.SYMBOLE_VIDE;
        }



        private int getTextureForChiffre(char chiffre)
        {
            switch (chiffre)
            {
                case '1': return MachineDeTuring.SYMBOLE_UN;
                case '0': return MachineDeTuring.SYMBOLE_ZERO;
                default: return MachineDeTuring.SYMBOLE_VIDE;
            }
        }
        private int getTextureForChiffre(MachineDeTuring.VALEUR chiffre)
        {
            switch (chiffre)
            {
                case MachineDeTuring.VALEUR.UN: return MachineDeTuring.SYMBOLE_UN;
                case MachineDeTuring.VALEUR.ZERO: return MachineDeTuring.SYMBOLE_ZERO;
                default: return MachineDeTuring.SYMBOLE_VIDE;
            }
        }
        private int getTextureForChiffre(MachineDeTuring.DEPLACEMENT chiffre)
        {
            switch (chiffre)
            {
                case MachineDeTuring.DEPLACEMENT.DROITE: return MachineDeTuring.SYMBOLE_DROITE;
                case MachineDeTuring.DEPLACEMENT.GAUCHE: return MachineDeTuring.SYMBOLE_GAUCHE;
                default: return 0;
            }
        }

        public void InitAnimationRechercheInstruction(int instructionCible, int dureeAnimation)
        {
            _interpolateurProgramme = new Interpolateur(dureeAnimation);
            _interpolateurProgramme.Start();
            _cibleIndicateur = instructionCible;
            _departIndicateur = _positionIndicateur;
        }

        public bool AnimationRechercheInstruction()
        {
            if (_interpolateurProgramme.estFini())
            {
                return true;
            }
            //_decalageIndicateurProgramme = (_instructionCible-_instructionActive)*_interpolateurProgramme.interpolationAccelereDecelere();
            _positionIndicateur = _departIndicateur + (_cibleIndicateur - _departIndicateur) * _interpolateurProgramme.interpolationAnticipeDepasse();
            return false;
        }

        public void InitAnimationChangeEtat(int dureeAnimation)
        {
            _interpolateurChangeEtatProgramme = new Interpolateur(dureeAnimation);
            _interpolateurChangeEtatProgramme.Start();
        }
        public bool AnimationChangeEtat()
        {
            if (_interpolateurChangeEtatProgramme.estFini())
            {
                _progressionEtat = 0;
                return true;
            }

            _progressionEtat = _interpolateurChangeEtatProgramme.interpolationRebondi();
            return false;
        }


        private int getNoColonneFromEtatOrdonnateur(MachineDeTuring.ETAPE_TURING etatOrdonnateur)
        {
            switch (etatOrdonnateur)
            {
                case MachineDeTuring.ETAPE_TURING.DEBUT: return -1;
                case MachineDeTuring.ETAPE_TURING.LECTURE: return 0;
                case MachineDeTuring.ETAPE_TURING.ECRITURE: return 1;
                case MachineDeTuring.ETAPE_TURING.DEPLACEMENT: return 2;
                case MachineDeTuring.ETAPE_TURING.CHANGE_ETAT: return 3;
                default: return -1;
            }
        }

        /// <summary>
        /// Creation de la texture pour afficher l'entete du programme
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="noEtat"></param>
        /// <param name="nom"></param>
        protected void CreateBitmap(OpenGL gl, string nom, string description, int etat, int nbEtats, string commentaire)
        {
            _bitmap?.Dispose();
            string etats = $"{etat}/{nbEtats}\n{commentaire}";
            using (Font f = new Font(FontFamily.GenericSerif, 256))
            using (Font fDesc = new Font(FontFamily.GenericSerif, 128))
            {
                SizeF tailleNom;
                SizeF tailleDesc = new SizeF();
                SizeF tailleEtats;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    tailleNom = g.MeasureString(nom, f);
                    if (description.Length > 0)
                        tailleDesc = g.MeasureString(description, fDesc);

                    tailleEtats = g.MeasureString(etats, fDesc);
                }


                _bitmap = new Bitmap((int)Math.Max(tailleNom.Width, Math.Max(tailleDesc.Width, tailleEtats.Width)),
                    (int)(tailleNom.Height + tailleDesc.Height + tailleEtats.Height), PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    float Y = 0;
                    g.DrawString(nom, f, Brushes.White, 0, Y);
                    Y += tailleNom.Height;
                    if (description.Length > 0)
                    {
                        g.DrawString(description, fDesc, Brushes.White, 0, Y);
                        Y += tailleDesc.Height;
                    }

                    g.DrawString(etats, fDesc, Brushes.White, 0, Y);
                }
            }
            _textureTeteProgramme = new Texture();
            _textureTeteProgramme.Create(gl, _bitmap);
        }
    }
}
