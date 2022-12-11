using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.OpenGLUtils;
using SharpGL;
///
/// Une ligne d'actualite extraite d'un flux RSS
///
///
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ClockScreenSaverGL.DisplayedObjects.PanneauActualites
{
    internal class LigneActu
    {
        public static readonly char[] TRIM_CARACTERES = { ' ', '\n', '\r' };

        private TextureActualité _texture;

        private class TextureActualité : TextureAsynchrone
        {
            private string _cheminFichier;
            private string _cheminImage;
            public float largeur { get; private set; }
            public float hauteur { get; private set; }
            public TextureActualité(OpenGL gl, string fichier, string image) : base(gl, null)
            {
                _gl = gl;
                _cheminFichier = fichier;
                _cheminImage = image;
            }

            /// <summary>
            /// Creation de la bitmap meteo en arriere plan
            /// </summary>
            protected override void InitAsynchrone()
            {
                Bitmap bitmap;
                string titre, source, description, date;
                float largeurTitre, largeurSource, largeurDesc = 0;
                float hauteurTitre, hauteurSource, hauteurDesc = 0;
                litFichierActu(out titre, out source, out description, out date, out bitmap);

                // Creer la texture representant le texte de cette information
                using (Font fTitre = new Font(FontFamily.GenericSansSerif, TAILLE_TITRE, FontStyle.Bold))
                using (Font fDescription = new Font(FontFamily.GenericSansSerif, TAILLE_DESCRIPTION, FontStyle.Regular))
                using (Font fSource = new Font(FontFamily.GenericSansSerif, TAILLE_SOURCE, FontStyle.Italic))
                {
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        SizeF sz = g.MeasureString(titre, fTitre);
                        largeurTitre = sz.Width;
                        hauteurTitre = sz.Height * 1.1f;

                        sz = g.MeasureString(source, fSource);
                        largeurSource = sz.Width;
                        hauteurSource = sz.Height * 1.05f;

                        //if (afficheDesc)
                        {
                            sz = g.MeasureString(description, fDescription);
                            largeurDesc = Math.Min(sz.Width, SystemInformation.VirtualScreen.Width * 0.75f);
                            hauteurDesc = sz.Height * 2.0f;
                        }

                        largeur = Math.Max(largeurSource, Math.Max(largeurTitre, largeurDesc)) + 50;
                        hauteur = HAUTEUR_BANDEAU;
                        if (bitmap != null)
                            largeur += bitmap.Width;
                    }

                    // Creation de la texture a partir d'une bitmap
                    _bitmap = new Bitmap((int)Math.Ceiling(largeur), (int)Math.Ceiling(hauteur), PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(_bitmap))
                    {
                        float x = 0;
                        float y = 0;
                        g.DrawString(source + " - " + date, fSource, Brushes.White, x, y);
                        y += hauteurSource;

                        if (bitmap != null)
                        {
                            g.DrawImage(bitmap, x, y);
                            x += bitmap.Width;
                            largeurDesc -= bitmap.Width;
                        }

                        g.DrawString(titre, fTitre, Brushes.White, x, y);
                        y += hauteurTitre;

                        TextRenderer.DrawText(g, description, fDescription, new Rectangle((int)x, (int)y, (int)largeurDesc, (int)hauteurDesc * 4), Color.White,
                            TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
                    }
                }
            }

            /// <summary>
            /// Retaille l'image pour qu'elle tienne dans le bandeau d'affichage
            /// </summary>
            /// <param name="image"></param>
            /// <returns></returns>
            private Image retailleImage(Image image)
            {
                int nouvelleHauteur = (int)(HAUTEUR_BANDEAU - (TAILLE_SOURCE + TAILLE_TITRE) * 0.9);
                int nouvelleLargeur = (int)(image.Width * (nouvelleHauteur / (float)image.Height));

                var destRect = new Rectangle(0, 0, nouvelleLargeur, nouvelleHauteur);
                var destImage = new Bitmap(nouvelleLargeur, nouvelleHauteur);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                }

                return destImage;
            }

            private void litFichierActu(out string titre, out string source, out string description, out string date, out Bitmap bitmap)
            {
                Log.instance.verbose($"Lecture fichier actualité {_cheminFichier}");
                try
                {
                    //if (File.Exists(_fichier))
                    {
                        //using (var reader = File.OpenText("Words.txt"))
                        //{
                        //    var fileText = await reader.ReadToEndAsync();
                        //    return fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        //}

                        using (StreamReader file = new StreamReader(_cheminFichier))
                        {
                            source = file.ReadLine();
                            date = file.ReadLine() + " " + file.ReadLine();
                            titre = file.ReadLine();
                            description = file.ReadLine();
                            file.Close();
                        }
                    }
                    //else
                    //{
                    //    titre = "Impossible de lire l'information";
                    //    source = "";
                    //    description = "";
                    //    date = "";
                    //}

                    if (File.Exists(_cheminImage))
                    {
                        bitmap = (Bitmap)retailleImage(Bitmap.FromFile(_cheminImage));
                        bitmap = DisplayedObject.BitmapDesaturee(bitmap, SATURATION_IMAGES);
                    }
                    else
                    {
                        bitmap = null;
                    }
                }
                catch (Exception e)
                {
                    titre = "Erreur de lecture de l'information";
                    source = "Fichier: " + _cheminFichier;
                    description = e.Message;
                    date = "";
                    bitmap = null;
                }

                Log.instance.verbose("Actualité lue");
            }
        }


        public float hauteur
        {
            get { if (_texture == null) return 0; else return _texture.hauteur; }
        }
        public float largeur
        {
            get { if (_texture == null) return 0; else return _texture.largeur; }
        }

        internal static int TAILLE_TITRE;
        internal static int TAILLE_DESCRIPTION;
        internal static int TAILLE_SOURCE;
        public LigneActu(OpenGL gl, string fichier, string image)
        {
            _texture = new TextureActualité(gl, fichier, image);
            _texture.Init();
        }

        internal static float SATURATION_IMAGES;
        internal static int HAUTEUR_BANDEAU;


        internal void affiche(OpenGL gl, float x, float y)
        {
            if (_texture?.Pret == true)
            {
                _texture.texture.Bind(gl);
                gl.Begin(OpenGL.GL_QUADS);
                gl.TexCoord(0.0f, 0.0f); gl.Vertex(x, y);
                gl.TexCoord(0.0f, 1.0f); gl.Vertex(x, y - _texture.hauteur);
                gl.TexCoord(1.0f, 1.0f); gl.Vertex(x + _texture.largeur, y - _texture.hauteur);
                gl.TexCoord(1.0f, 0.0f); gl.Vertex(x + _texture.largeur, y);
                gl.End();
            }
        }
        internal void Clear()
        {
            _texture = null;
        }
    }
}
