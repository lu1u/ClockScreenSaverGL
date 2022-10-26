using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Assets;
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
    class LigneActu : IDisposable
    {
        public static readonly char[] TRIM_CARACTERES = { ' ', '\n', '\r' };

        private Texture _texture;
        public float largeur { get; private set; }
        public float hauteur { get; private set; }
        private string _fichier, _image;
        internal static int TAILLE_TITRE;
        internal static int TAILLE_DESCRIPTION;
        internal static int TAILLE_SOURCE;
        private Bitmap _bitmap;

        private bool initialisationencours = false;

        public LigneActu(string fichier, string image)
        {
            _fichier = fichier;
            _image = image;
            _texture = null;

            // Charge l'image associee
            _bitmap = ComputeHeavyOperations();
        }

        internal static float SATURATION_IMAGES;
        internal static int HAUTEUR_BANDEAU;

        public void Dispose()
        {
            //_texture?.Destroy(_gl);
        }

        internal void affiche(OpenGL gl, float x, float y, bool afficheDesc)
        {
            if (_texture == null)
            {
                CreerTexture(gl);
                return;
            }

            _texture.Bind(gl);
            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f); gl.Vertex(x, y);
            gl.TexCoord(0.0f, 1.0f); gl.Vertex(x, y - hauteur);
            gl.TexCoord(1.0f, 1.0f); gl.Vertex(x + largeur, y - hauteur);
            gl.TexCoord(1.0f, 0.0f); gl.Vertex(x + largeur, y);
            gl.End();

        }

        /// <summary>
        /// Creation de la texture OpenGL qui permet d'afficher cette actualite
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="afficheDesc"></param>
        public void CreerTexture(OpenGL gl)
        {
            //if (initialisationencours || _texture != null)
            //    return;
            //
            if ( _bitmap == null)
                _bitmap = ComputeHeavyOperations();
            Texture texture = new Texture();
            texture.Create(gl, _bitmap);
            _bitmap.Dispose();
            _bitmap = null;

            _texture = texture;
        }

        private Bitmap ComputeHeavyOperations()
        {
            if (initialisationencours)
                return null;

            initialisationencours = true;
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
                Bitmap bmp = new Bitmap((int)Math.Ceiling(largeur), (int)Math.Ceiling(hauteur), PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
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

                    //if (afficheDesc)
                    TextRenderer.DrawText(g, description, fDescription, new Rectangle((int)x, (int)y, (int)largeurDesc, (int)hauteurDesc * 4), Color.White,
                        TextFormatFlags.Left |
                        TextFormatFlags.NoPrefix |
                          TextFormatFlags.TextBoxControl |
                          TextFormatFlags.WordBreak |
                          TextFormatFlags.EndEllipsis);

                    return bmp;
                }
            }
        }


        private void litFichierActu(out string titre, out string source, out string description, out string date, out Bitmap bitmap)
        {
            Log.instance.verbose($"Lecture fichier actualité {_fichier}");
            try
            {
                //if (File.Exists(_fichier))
                {
                    //using (var reader = File.OpenText("Words.txt"))
                    //{
                    //    var fileText = await reader.ReadToEndAsync();
                    //    return fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    //}

                    using (StreamReader file = new StreamReader(_fichier))
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

                if (File.Exists(_image))
                {
                    bitmap = (Bitmap)retailleImage(Bitmap.FromFile(_image));
                    bitmap = DisplayedObject.BitmapDesaturee((Bitmap)bitmap, SATURATION_IMAGES);
                }
                else
                {
                    bitmap = null;
                }
            }
            catch (Exception e)
            {
                titre = "Erreur de lecture de l'information";
                source = "Fichier: " + _fichier;
                description = e.Message;
                date = "";
                bitmap = null;
            }

            Log.instance.verbose("Actualité lue");
        }

        internal void Clear()
        {
            //_texture?.Destroy(_gl);
            _texture = null;
        }

        /// <summary>
        /// Retaille l'image pour qu'elle tienne dans le bandeau d'affichage
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private Image retailleImage(Image image)
        {
            int nouvelleHauteur = (int)(HAUTEUR_BANDEAU - (TAILLE_SOURCE + TAILLE_TITRE) * 0.9);
            int nouvelleLargeur = (int)(image.Width * ((float)nouvelleHauteur / (float)image.Height));

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
    }
}
