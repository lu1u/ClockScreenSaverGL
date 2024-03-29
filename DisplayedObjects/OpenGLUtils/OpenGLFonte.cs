﻿using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL
{
    internal class OpenGLFonte : IDisposable
    {
        public const String CARACTERES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyzàéèôïöë0123456789,;:!?./*=#&é\"'{([-|è`_\\ç^à@)]=}><+";
        private readonly OpenGL _gl;
        private readonly string _caracteres;
        private readonly float _hauteurSymbole;
        private Texture _texture;
        private float largeurTexture;
        private float[] _xCaractere;            // Coordonnees de chaque caractere dans la bitmap qui sert de texture (+1 element virtuel à droite du dernier caractere)

        /// <summary>
        /// Retourne une chaine comportant un fois et une seule tous les caracteres du texte donne en parametre
        /// Utile pour remplacer CARACTERES
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string getListeCaracteresUniques(string s)
        {
            string res = "";
            foreach (char c in s)
            {
                if (res.IndexOf(c) == -1)
                    res += c;
            }
            return res;
        }
        internal float Largeur(string texte)
        {
            float larg = 0;
            for (int i = 0; i < texte.Length; i++)
            {
                int Indice = getSymboleIndex(texte[i]);
                if (Indice != -1)
                    larg += (_xCaractere[Indice + 1] - _xCaractere[Indice]);
            }

            return larg;
        }

        public OpenGLFonte(OpenGL gl, string caracteres, float taille, FontFamily famille, FontStyle style)
        {
            _caracteres = caracteres;
            int nbSymboles = caracteres.Length;
            _gl = gl;
            _xCaractere = new float[nbSymboles + 1];

            using (Font f = new Font(famille, taille, style))
            {
                float xCaractere = 0;
                float largeur;
                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                {
                    for (int i = 0; i < nbSymboles; i++)
                    {
                        SizeF size = g.MeasureString(_caracteres[i].ToString(), f);
                        largeur = (float)Math.Round(size.Width);
                        _hauteurSymbole = Math.Max(_hauteurSymbole, size.Height);

                        _xCaractere[i] = xCaractere;        // Bord gauche du caractere
                        xCaractere += largeur;
                    }
                }
                _xCaractere[nbSymboles] = xCaractere;
                largeurTexture = xCaractere;
                using (Bitmap bmp = new Bitmap((int)Math.Ceiling(largeurTexture), (int)Math.Ceiling(_hauteurSymbole), PixelFormat.Format32bppArgb))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    _texture = new Texture();

                    for (int i = 0; i < nbSymboles; i++)
                    {
                        g.DrawString(_caracteres[i].ToString(), f, Brushes.White, _xCaractere[i], 0);
                    }
                    _texture.Create(gl, bmp);
                }
            }
        }

        internal float Hauteur()
        {
            return _hauteurSymbole;
        }

        public void drawOpenGL(OpenGL gl, char texte, float X, float Y, float R, float G, float B)
        {
            float[] col = { R, G, B, 1.0f };
            gl.Color(col);
            gl.PushAttrib(OpenGL.GL_ENABLE_BIT);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            _texture.Bind(gl);
            float XGauche = X;
            gl.Begin(OpenGL.GL_QUADS);

            int Indice = getSymboleIndex(texte);
            if (Indice != -1)
            {
                float largeurChiffre = _xCaractere[Indice + 1] - _xCaractere[Indice];
                float xTexture = _xCaractere[Indice] / largeurTexture;
                float xSuivant = _xCaractere[Indice + 1] / largeurTexture;

                gl.TexCoord(xTexture, 0.0f); gl.Vertex(X, Y + _hauteurSymbole);
                gl.TexCoord(xTexture, 1.0f); gl.Vertex(X, Y);
                gl.TexCoord(xSuivant, 1.0f); gl.Vertex(X + largeurChiffre, Y);
                gl.TexCoord(xSuivant, 0.0f); gl.Vertex(X + largeurChiffre, Y + _hauteurSymbole);

                X += largeurChiffre;
            }
            gl.End();
            gl.PopAttrib();
        }

        public void drawOpenGL(OpenGL gl, string texte, float X, float Y, Color couleur)
        {
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, couleur.A / 256.0f };
            gl.Color(col);
            gl.PushAttrib(OpenGL.GL_ENABLE_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_COLOR_MATERIAL);
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            _texture.Bind(gl);
            float XGauche = X;
            gl.Begin(OpenGL.GL_QUADS);
            for (int i = 0; i < texte.Length; i++)
            {
                if (texte[i] == '\n')
                {
                    X = XGauche;
                    Y += Hauteur();
                }

                int Indice = getSymboleIndex(texte[i]);
                if (Indice != -1)
                {
                    float largeurChiffre = _xCaractere[Indice + 1] - _xCaractere[Indice];
                    float xTexture = _xCaractere[Indice] / largeurTexture;
                    float xSuivant = _xCaractere[Indice + 1] / largeurTexture;

                    gl.TexCoord(xTexture, 0.0f); gl.Vertex(X, Y + _hauteurSymbole, 0);
                    gl.TexCoord(xTexture, 1.0f); gl.Vertex(X, Y, 0);
                    gl.TexCoord(xSuivant, 1.0f); gl.Vertex(X + largeurChiffre, Y, 0);
                    gl.TexCoord(xSuivant, 0.0f); gl.Vertex(X + largeurChiffre, Y + _hauteurSymbole, 0);

                    X += largeurChiffre;
                }
            }
            gl.End();
            gl.PopAttrib();
        }



        private int getSymboleIndex(char v)
        {
            for (int i = 0; i < _caracteres.Length; i++)
                if (v == _caracteres[i])
                    return i;

            return -1;
        }

        public void Dispose()
        {
            _texture?.Destroy(_gl);
        }
    }
}
