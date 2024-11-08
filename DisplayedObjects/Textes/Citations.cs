﻿/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 20/11/2014
 * Heure: 23:09
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace ClockScreenSaverGL.DisplayedObjects.Textes
{

    /// <summary>
    /// Description of Citations.
    /// </summary>
    public partial class Citations : Texte, IDisposable
    {
        #region Parametres
        private const string CAT = "Citation";
        protected static CategorieConfiguration c;
        public float RATIO_TAILLE_FONTE;
        private int DELAI_CHANGEMENT;    // x minutes entre les changements de citation
        private int TailleMax;
        private byte ALPHA;
        private float VX;
        private float VY;
        private int TAILLE_FONTE;
        #endregion
        private List<string> _citations;

        private string _citation;
        private string _auteur;
        private DateTime _changement;
        private int _derniereCitation;
        private RectangleF _rectCitation, _rectAuteur;
        private bool _citationChangee = false;
        private int _tailleFonte, _tailleFonteAuteur;
        public Citations(OpenGL gl, Form f, int Px, int Py) : base(gl)
        {
            GetConfiguration();
            _alpha = c.GetParametre("Alpha", (byte)160);
            int hauteurFonte = c.GetParametre("TailleFonte", 80);
            _fonte = CreerFonte(hauteurFonte);
            _trajectoire = new TrajectoireDiagonale(Px, SystemInformation.VirtualScreen.Height - hauteurFonte * 2, -c.GetParametre("VX", 15), 0);
            LireCitations();
            MelangerCitations();

            _derniereCitation = new Random().Next(0, _citations.Count - 1);
            ChoisitCitation(f);
        }
        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                RATIO_TAILLE_FONTE = c.GetParametre("RatioTailleFonte", 0.4f);
                DELAI_CHANGEMENT = 1000 * 60 * c.GetParametre("DelaiChange", 1);    // x minutes entre les changements de citation
                TailleMax = c.GetParametre("TailleMax", 48);
                ALPHA = c.GetParametre("Alpha", (byte)250);
                VX = c.GetParametre("VX", -15);
                VY = c.GetParametre("VX", 0);
                TAILLE_FONTE = c.GetParametre("Taille Fonte", 30);
            }
            return c;
        }
        public override void Dispose()
        {
            base.Dispose();
            _bitmap?.Dispose();
        }

        protected override bool TexteChange() { return _citationChangee; }


        /// <summary>
        /// Choisit la prochaine citation
        /// </summary>
        /// <param name="f"></param>
        private void ChoisitCitation(Form f)
        {
            Graphics g = f.CreateGraphics();
            prochaineCitation();
            g.Dispose();
        }

        /// Lire les citations a partir du fichier de donnees
        private void LireCitations()
        {
            _citations = new List<string>();
            string nomFichier = Path.Combine(Config.Configuration.GetDataDirectory(), "citations.txt");
            try
            {
                StreamReader file = new StreamReader(nomFichier, Encoding.UTF8);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length > 0)
                        if ((line.IndexOf("|") != -1) || (line[0] == '*'))
                            _citations.Add(line);
                }

                file.Close();
            }
            catch (Exception e)
            {
                _citations.Clear();
                _citations.Add("Erreur d'acces au fichier|" + e.Message);
                _citations.Add("Erreur d'acces au fichier|" + e.Message);
            }
        }

        /// <summary>
        /// Melanger les citations aleatoirement
        /// </summary>
        private void MelangerCitations()
        {
            int DeuxiemeIndice;
            for (int i = 0; i < _citations.Count; i++)
            {
                do
                {
                    DeuxiemeIndice = random.Next(0, _citations.Count);
                }
                while (DeuxiemeIndice == i);

                string temp = _citations[i];
                _citations[i] = _citations[DeuxiemeIndice];
                _citations[DeuxiemeIndice] = temp;
            }
        }

        /// <summary>
        /// Choisit une citation dans la liste
        /// Calcule une taille de texte adequate pour l'afficher
        /// </summary>
        /// <param name="g"></param>
        private void prochaineCitation()
        {
            // Puisque les citations ont ete melangees, on prend la suivante dans la liste
            if (_derniereCitation < (_citations.Count - 1))
                _derniereCitation++;
            else
                _derniereCitation = 0;

            // Separer la citation et l'auteur
            if (_citations[_derniereCitation].StartsWith("*"))
            {
                // C'est un 'Le saviez-vous'
                _citation = "Le saviez vous?";// Resources.LeSaviezVous;
                _auteur = _citations[_derniereCitation].Substring(1);
            }
            else
            {
                string[] words = _citations[_derniereCitation].Split('|');
                _citation = words[0];
                _auteur = "(" + words[1] + ")";// - {" + _derniereCitation + '/' + _citations.Length + '}';
            }

            _citation = _citation.Replace("/", "\n");
            _auteur = _auteur.Replace("/", "\n");
            // Choisir une taille de texte adequate
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                _tailleFonte = TAILLE_FONTE;// Math.Min(calculeTailleTexte(g, _citation), calculeTailleTexte(g, _auteur));
                _tailleFonteAuteur = _tailleFonte;

                // Calculer la taille du texte affiche
                SizeF stringSize = new SizeF();
                using (Font fonte = new Font(FontFamily.GenericSansSerif, _tailleFonte, FontStyle.Regular, GraphicsUnit.Pixel))
                    stringSize = g.MeasureString(_citation, fonte, SystemInformation.VirtualScreen.Width);
                _rectCitation = new RectangleF(0, 0, stringSize.Width, stringSize.Height);

                using (Font fonte = new Font(FontFamily.GenericSansSerif, _tailleFonteAuteur, FontStyle.Italic, GraphicsUnit.Pixel))
                    stringSize = g.MeasureString(_auteur, fonte, SystemInformation.VirtualScreen.Width);
                _rectAuteur = new RectangleF(0, 0, stringSize.Width, stringSize.Height);

                _taille = new SizeF(Math.Max(_rectCitation.Width, _rectAuteur.Width), (_rectCitation.Height + _rectAuteur.Height));
                _changement = DateTime.Now;
            }
            _citationChangee = true;
        }

        /// <summary>
        /// Calcule une taille de texte adequate pour afficher la chaine donnee sur l'ecran sans que celle
        /// ci ne depasse un certain ratio
        /// </summary>
        /// <param name="g"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        /*private int calculeTailleTexte(Graphics g, String s)
        {

            int LargeurEcran = (int)(SystemInformation.VirtualScreen.Width * RATIO_TAILLE_FONTE);
            int TailleFonte = 30;

            Font f = new Font(FontFamily.GenericSansSerif, TailleFonte, FontStyle.Regular, GraphicsUnit.Pixel);

            // Choisir la plus grande taille de fonte possible
            while ((g.MeasureString(s, f).Width < LargeurEcran) && (TailleFonte < TailleMax))
            {
                f.Dispose();
                f = new Font(FontFamily.GenericSansSerif, TailleFonte, FontStyle.Regular, GraphicsUnit.Pixel);

                TailleFonte++;
            }

            f.Dispose();
            return TailleFonte;
        }
        */
        /// <summary>
        ///  Pas utilisee
        /// </summary>
        /// <param name="maintenant"></param>
        /// <returns></returns>
        protected override SizeF getTexte(Temps maintenant, out string texte)
        {
            texte = _citation;
            return _taille;
        }

        /// <summary>
        /// Creer la bitmap contenant ce qu'on veut afficher
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="maintenant"></param>
        protected override void CreateBitmap(OpenGL gl, Temps maintenant)
        {
            _bitmap?.Dispose();

            string texte;
            _taille = getTexte(maintenant, out texte);

            _bitmap = new Bitmap((int)_taille.Width, (int)_taille.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                RectangleF rect = new RectangleF(0, 0, _rectCitation.Width, _rectCitation.Height);
                using (Font fonte = new Font(FontFamily.GenericSansSerif, _tailleFonte, FontStyle.Regular, GraphicsUnit.Pixel))
                    g.DrawString(_citation, fonte, Brushes.White, rect);

                rect.Offset(0, _rectCitation.Height);
                using (Font fonteAuteur = new Font(FontFamily.GenericSansSerif, _tailleFonteAuteur, FontStyle.Italic, GraphicsUnit.Pixel))
                    g.DrawString(_auteur, fonteAuteur, Brushes.LightGray, rect.Left, rect.Top);
            }

            _texture.Create(gl, _bitmap);
            _citationChangee = false;
            _trajectoire._Py = SystemInformation.VirtualScreen.Height - _taille.Height;
        }

        /// <summary>
        /// Pression sur une touche, si c'est 'C' : changer de citation et signaler qu'on a utilise la touche
        /// </summary>
        /// <param name="f"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public override bool KeyDown(Form f, Keys k)
        {
            if (TOUCHE_CITATION == k)
            {
                ChoisitCitation(f);
                return true;
            }

            return base.KeyDown(f, k); ;
        }

        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            base.Deplace(maintenant, tailleEcran);
            // Changer la citation toutes les n minutes
            if (DateTime.Now.Subtract(_changement).TotalMilliseconds > DELAI_CHANGEMENT)
                prochaineCitation();
        }

        /// <summary>
        /// Ajoute le message d'aide correspondant a cet objet graphique
        /// </summary>
        /// <param name="s"></param>
        public override void AppendHelpText(StringBuilder s)
        {
            //s.Append(Resources.AideCitation);
        }

        void IDisposable.Dispose()
        {
            _bitmap?.Dispose();
        }

        protected override void drawOpenGL(OpenGL gl, Rectangle tailleEcran, Color couleur, Temps maintenant)
        {
            couleur = new CouleurGlobale(couleur).GetColorWithHueChange(DEUX_PI);
            base.drawOpenGL(gl, tailleEcran, couleur, maintenant);
        }
    }
}
