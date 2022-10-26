using ClockScreenSaverGL.Config;

using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;

/// <summary>
/// Affichage du RUBAN de la machine de Turing
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Turing
{
    class Ruban
    {
        float _sensDeplacementRuban = 0.0f;
        float _decalageRuban = 0;
        Interpolateur _interpolateurRuban;
        public static readonly int NB_SYMBOLES = 3;     // Nombre de symboles dans la texture des chiffres du ruban
        float X_RUBAN;
        float Y_RUBAN;
        float HAUTEUR_RUBAN;
        float HAUTEUR_TETE;
        float LARGEUR_CASE_RUBAN;
        Texture _textureEngrenage, _textureSymboles;
        public Ruban(OpenGL gl)
        {
            _decalageRuban = 0;
        }

        public void Init(OpenGL gl, CategorieConfiguration c)
        {
            string REPERTOIRE_TURING = c.getParametre(MachineDeTuring.PARAM_REPERTOIRE, "turing");
            _textureEngrenage = new Texture();
            _textureEngrenage.Create(gl, c.getParametre("Ruban Engrenage", Config.Configuration.getImagePath(REPERTOIRE_TURING + @"\engrenage.png")));

            _textureSymboles = new Texture();
            _textureSymboles.Create(gl, c.getParametre("Ruban Symboles", Config.Configuration.getImagePath(REPERTOIRE_TURING + @"\Symboles ruban.png")));
        }

        public void getConfiguration(CategorieConfiguration c)
        {
            X_RUBAN = c.getParametre("Ruban X", 0.0f, (a) => { X_RUBAN = (float)Convert.ToDouble(a); });
            Y_RUBAN = c.getParametre("Ruban Y", -0.3f, (a) => { Y_RUBAN = (float)Convert.ToDouble(a); });
            HAUTEUR_RUBAN = c.getParametre("Ruban Hauteur", 0.12f, (a) => { HAUTEUR_RUBAN = (float)Convert.ToDouble(a); });
            LARGEUR_CASE_RUBAN = c.getParametre("Ruban Largeur case", 0.12f, (a) => { LARGEUR_CASE_RUBAN = (float)Convert.ToDouble(a); });
            HAUTEUR_TETE = c.getParametre("Tete Hauteur", 0.22f);
        }

        public void Dessine(OpenGL gl, Color couleur, char[] ruban, int indiceRuban)
        {
            gl.PushMatrix();
            gl.Translate(X_RUBAN, Y_RUBAN, 0);
            gl.Disable(OpenGL.GL_BLEND);


            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            // Engrenage 1
            _textureEngrenage.Bind(gl);
            gl.PushMatrix();
            gl.Translate(0, -HAUTEUR_TETE * 0.7f, 0);
            gl.Rotate(0, 0, _decalageRuban * 360.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0, 1.0f); gl.Vertex(-HAUTEUR_RUBAN * 0.7f, HAUTEUR_RUBAN * 0.7f, 0);
            gl.TexCoord(0, 0.0f); gl.Vertex(-HAUTEUR_RUBAN * 0.7f, -HAUTEUR_RUBAN * 0.7f, 0);
            gl.TexCoord(1, 0.0f); gl.Vertex(HAUTEUR_RUBAN * 0.7f, -HAUTEUR_RUBAN * 0.7f, 0);
            gl.TexCoord(1, 1.0f); gl.Vertex(HAUTEUR_RUBAN * 0.7f, HAUTEUR_RUBAN * 0.7f, 0);
            gl.End();
            gl.PopMatrix();

            // Engrenage 2
            gl.PushMatrix();
            gl.Translate(0, HAUTEUR_TETE * 0.85f, 0);
            gl.Rotate(0, 0, -_decalageRuban * 360.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0, 1.0f); gl.Vertex(-HAUTEUR_RUBAN, HAUTEUR_RUBAN, 0);
            gl.TexCoord(0, 0.0f); gl.Vertex(-HAUTEUR_RUBAN, -HAUTEUR_RUBAN, 0);
            gl.TexCoord(1, 0.0f); gl.Vertex(HAUTEUR_RUBAN, -HAUTEUR_RUBAN, 0);
            gl.TexCoord(1, 1.0f); gl.Vertex(HAUTEUR_RUBAN, HAUTEUR_RUBAN, 0);
            gl.End();
            gl.PopMatrix();

            gl.Color(couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            _textureSymboles.Bind(gl);
            gl.Begin(OpenGL.GL_QUADS);

            for (int i = 0; i < ruban.Length; i++)
            {
                float xGauche = (i - indiceRuban + (_decalageRuban * _sensDeplacementRuban) - 0.5f) * LARGEUR_CASE_RUBAN;
                float xDroite = xGauche + LARGEUR_CASE_RUBAN;
                if (xGauche < 1.0f && xDroite > -1.0f)
                {
                    int type = getTextureForChiffre(ruban[i]);

                    float textCoordGauche = (1.0f / NB_SYMBOLES) * type;
                    float textCoordDroite = (1.0f / NB_SYMBOLES) * (type + 1);

                    gl.TexCoord(textCoordGauche, 1.0f); gl.Vertex(xGauche, -HAUTEUR_RUBAN, 0);
                    gl.TexCoord(textCoordGauche, 0.0f); gl.Vertex(xGauche, HAUTEUR_RUBAN, 0);
                    gl.TexCoord(textCoordDroite, 0.0f); gl.Vertex(xDroite, HAUTEUR_RUBAN, 0);
                    gl.TexCoord(textCoordDroite, 1.0f); gl.Vertex(xDroite, -HAUTEUR_RUBAN, 0);
                }
            }

            gl.End();
            gl.PopMatrix();
        }

        private int getTextureForChiffre(char chiffre)
        {
            switch (chiffre)
            {
                case '1': return 2;
                case '0': return 1;
                default: return 0;
            }
        }

        public void InitDeplacement(MachineDeTuring.DEPLACEMENT dep, int dureeAnimation)
        {
            _interpolateurRuban = new Interpolateur(dureeAnimation);
            _interpolateurRuban.Start();
            switch (dep)
            {
                case MachineDeTuring.DEPLACEMENT.DROITE: _sensDeplacementRuban = -1.0f; break;
                case MachineDeTuring.DEPLACEMENT.GAUCHE: _sensDeplacementRuban = 1.0f; break;
                default: _sensDeplacementRuban = 0.0f; break;
            }
        }

        public bool AnimationDeplacement()
        {
            if (_interpolateurRuban.estFini())
            {
                _decalageRuban = 0;
                return true;
            }
            _decalageRuban = _interpolateurRuban.interpolationAnticipeDepasse();
            return false;
        }
    }
}
