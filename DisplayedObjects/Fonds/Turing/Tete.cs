using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Turing
{
    internal class Tete
    {
        #region Parametres
        #endregion
        private Interpolateur _interpolateurTete;
        private float _valeurTete = 1.0f;
        private float _valeurEcriture = 1.0f;
        private Texture _textureSymbolesProgramme, _textureTete;
        private float X_TETE, Y_TETE, LARGEUR_TETE, HAUTEUR_TETE, LARGEUR_CASE_RUBAN, HAUTEUR_RUBAN;
        public Tete(OpenGL gl)
        {

        }

        public void Init(OpenGL gl, CategorieConfiguration c)
        {
            string REPERTOIRE_TURING = c.getParametre(MachineDeTuring.PARAM_REPERTOIRE, "turing");
            _textureSymbolesProgramme = new Texture();
            _textureSymbolesProgramme.Create(gl, c.getParametre("Ruban Symboles", Config.Configuration.getImagePath(REPERTOIRE_TURING + @"\Symboles ruban.png")));

            _textureTete = new Texture();
            _textureTete.Create(gl, c.getParametre("Tete Fond", Configuration.getImagePath(REPERTOIRE_TURING + @"\Tete.png")));
        }

        public void getConfiguration(CategorieConfiguration c)
        {
            X_TETE = c.getParametre("Tete X", 0.0f, (a) => { X_TETE = (float)Convert.ToDouble(a); });
            Y_TETE = c.getParametre("Tete Y", -0.3f, (a) => { Y_TETE = (float)Convert.ToDouble(a); });
            LARGEUR_TETE = c.getParametre("Tete Largeur", 0.15f, (a) => { LARGEUR_TETE = (float)Convert.ToDouble(a); });
            HAUTEUR_TETE = c.getParametre("Tete Hauteur", 0.22f, (a) => { HAUTEUR_TETE = (float)Convert.ToDouble(a); });
            LARGEUR_CASE_RUBAN = c.getParametre(MachineDeTuring.PARAM_LARGEUR_CASE_RUBAN, 0.12f);
            HAUTEUR_RUBAN = c.getParametre(MachineDeTuring.PARAM_LARGEUR_CASE_RUBAN, 0.12f);
        }

        public void Dessine(OpenGL gl, Color couleur, MachineDeTuring.ETAPE_TURING _etatOrdonnateur, MachineDeTuring.VALEUR valeurAEcrire)
        {
            gl.PushMatrix();
            gl.Translate(X_TETE, Y_TETE, 0);
            int image = 0; // Pour la texture de la tete de lecture/ecriture

            if (_etatOrdonnateur == MachineDeTuring.ETAPE_TURING.ECRITURE)
            {
                image = 2;
                // Indicateur d'ecriture
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                _textureSymbolesProgramme.Bind(gl);
                gl.Color(couleur.R * _valeurTete / 256.0f, couleur.G * _valeurTete / 256.0f, couleur.B * _valeurTete / 256.0f, _valeurEcriture);
                int type = getTextureForChiffre(valeurAEcrire);

                float textCoordGauche = (1.0f / Ruban.NB_SYMBOLES) * type;
                float textCoordDroite = (1.0f / Ruban.NB_SYMBOLES) * (type + 1);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(textCoordGauche, 1.0f); gl.Vertex(-LARGEUR_CASE_RUBAN * 0.5f, -HAUTEUR_RUBAN, 0);
                gl.TexCoord(textCoordGauche, 0.0f); gl.Vertex(-LARGEUR_CASE_RUBAN * 0.5f, HAUTEUR_RUBAN, 0);
                gl.TexCoord(textCoordDroite, 0.0f); gl.Vertex(LARGEUR_CASE_RUBAN * 0.5f, HAUTEUR_RUBAN, 0);
                gl.TexCoord(textCoordDroite, 1.0f); gl.Vertex(LARGEUR_CASE_RUBAN * 0.5f, -HAUTEUR_RUBAN, 0);
                gl.End();
            }
            else if (_etatOrdonnateur == MachineDeTuring.ETAPE_TURING.LECTURE)
            {
                image = 1;
                // Indicateur de lecture
                float y = HAUTEUR_RUBAN - (HAUTEUR_RUBAN * _valeurTete * 2.0f);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_ONE, OpenGL.GL_ONE);
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);
                gl.Begin(OpenGL.GL_QUADS);
                gl.Vertex(-LARGEUR_CASE_RUBAN * 0.5f, y, 0);
                gl.Vertex(-LARGEUR_CASE_RUBAN * 0.5f, y - 0.05f, 0);
                gl.Vertex(LARGEUR_CASE_RUBAN * 0.5f, y - 0.05f, 0);
                gl.Vertex(LARGEUR_CASE_RUBAN * 0.5f, y, 0);
                gl.End();
            }

            // Tete
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);
            _textureTete.Bind(gl);
            float xGauche = (1.0f / 3.0f) * image;
            float xDroite = (1.0f / 3.0f) * (image + 1);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(xGauche, 1.0f); gl.Vertex(-LARGEUR_TETE, -HAUTEUR_TETE, 0);
            gl.TexCoord(xGauche, 0.0f); gl.Vertex(-LARGEUR_TETE, HAUTEUR_TETE, 0);
            gl.TexCoord(xDroite, 0.0f); gl.Vertex(LARGEUR_TETE, HAUTEUR_TETE, 0);
            gl.TexCoord(xDroite, 1.0f); gl.Vertex(LARGEUR_TETE, -HAUTEUR_TETE, 0);
            gl.End();

            gl.PopMatrix();
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

        public void TeteInitAnimationLecture(int duree)
        {
            _interpolateurTete = new Interpolateur(duree);
            _interpolateurTete.Start();
        }

        public bool TeteAnimationLecture()
        {
            if (_interpolateurTete.estFini())
            {
                return true;
            }
            _valeurTete = _interpolateurTete.interpolationLineaire();
            return false;
        }

        public void TeteInitAnimationEcriture(int duree)
        {
            _interpolateurTete = new Interpolateur(duree);
            _interpolateurTete.Start();
        }

        public bool TeteAnimationEcriture()
        {
            if (_interpolateurTete.estFini())
            {
                _valeurEcriture = 0.0f;
                return true;
            }
            _valeurEcriture = _interpolateurTete.interpolationRebondi();
            return false;
        }
    }
}
