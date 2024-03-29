﻿/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 26/06/2014
 * Heure: 09:50
 * Classe de base pour les objets graphiques de type texte
 */
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClockScreenSaverGL.DisplayedObjects.Textes
{
    public abstract class Texte : DisplayedObject, IDisposable
    {
        public Trajectoire _trajectoire;
        protected Font _fonte;
        protected byte _alpha;
        protected Bitmap _bitmap;
        protected Texture _texture = new Texture();

        /// <summary>
        /// Constructeur
        /// Initialise la trajectoire, la fonte et le niveau de transparence
        /// </summary>
        /// <param name="Px"></param>
        /// <param name="Py"></param>
        /// <param name="Vx"></param>
        /// <param name="Vy"></param>
        /// <param name="tailleFonte"></param>
        /// <param name="alpha"></param>
        public Texte(OpenGL gl) : base(gl)
        {

        }


        /// <summary>
        /// Deplace l'objet, en tenant compte de la derniere taille calculee de cet objet
        /// </summary>
        /// <param name="maintenant"></param>
        /// <param name="tailleEcran"></param>
        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.DEPLACE);
#endif

            _trajectoire.Avance(tailleEcran, _taille, maintenant);

#if TRACER
            RenderStop(CHRONO_TYPE.DEPLACE);
#endif

        }

        /// <summary>
        /// A implementer: retourner le texte
        /// </summary>
        /// <param name="maintenant"></param>
        /// <returns></returns>
        protected abstract SizeF getTexte(Temps maintenant, out string texte);
        protected virtual bool TexteChange() { return false; }
        protected virtual void drawOpenGL(OpenGL gl, Rectangle tailleEcran, Color couleur, Temps maintenant)
        {
            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, _alpha / 256.0f };
            gl.Color(col);
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            _texture.Bind(gl);
            gl.Translate(_trajectoire._Px, _trajectoire._Py, 0);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(0, _taille.Height);
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(0, 0);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(_taille.Width, 0);
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(_taille.Width, _taille.Height);
            gl.End();
        }


        protected virtual void CreateBitmap(OpenGL gl, Temps maintenant)
        {
            using (var c = new Chronometre("Texte CreateBitmap"))
            {
                _bitmap?.Dispose();

                string texte;
                _taille = getTexte(maintenant, out texte);

                _bitmap = new Bitmap((int)_taille.Width, (int)_taille.Height, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(_bitmap))
                    g.DrawString(texte, _fonte, Brushes.White, 0, 0);

                _texture.Create(gl, _bitmap);
            }
        }

        protected virtual Font CreerFonte(int tailleFonte)
        {
            return new Font(FontFamily.GenericSansSerif, tailleFonte, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            if (TexteChange() || _bitmap == null)
                CreateBitmap(gl, maintenant);

            if (_bitmap == null)
                return;

            gl.Clear(OpenGL.GL_DEPTH_BUFFER_BIT);
            using (Viewport2D v = new Viewport2D(gl, 0.0f, 0.0f, tailleEcran.Width, tailleEcran.Height))
            {
                gl.Disable(OpenGL.GL_LIGHTING);
                gl.Disable(OpenGL.GL_DEPTH);
                gl.Enable(OpenGL.GL_BLEND);
                gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                drawOpenGL(gl, tailleEcran, couleur, maintenant);
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        public override void Dispose()
        {
            base.Dispose();
            _fonte?.Dispose();
            _bitmap?.Dispose();
            _texture?.Destroy(_gl);
        }
    }
}
