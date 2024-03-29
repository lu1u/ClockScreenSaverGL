﻿///
/// Affichage d'une molecule a partir d'un fichier PDB

using ClockScreenSaverGL.Config;
using SharpGL;
using SharpGL.SceneGraph.Quadrics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    internal class Molecule : MateriauGlobal, IDisposable
    {
        #region Parametres
        private const string CAT = "Molecules";
        private CategorieConfiguration c;
        private int TAILLE_FONTE;
        private float ZCAMERA;
        private float VITESSE_CAM;
        private float ECHELLE_RAYON;
        private float RAYON_LIENS;
        private int FICHIER_EN_COURS;
        private int DETAIL_ATOMES;
        private int DETAIL_LIENS;
        private TimerIsole _changeMolecule;
        #endregion

        private class Atome
        {
            public float x, y, z;
            public float rayon;
            public string nom;
            public string index;
            public Color couleur;
        }

        private class Lien
        {
            public float x1, y1, z1;
            public float x2, y2, z2;
        }

        private List<Atome> _atomes;
        private List<Lien> _liens;
        private List<string> _fichiers;
        private OpenGLFonte _fonte;

        private float _angleCamera = 90;
        private Sphere _sphere;
        private Cylinder _cylindre;
        private string _nomMolecule;

        public Molecule(OpenGL gl) : base(gl)
        {
            _atomes = new List<Atome>();
            _liens = new List<Lien>();
            GetConfiguration();
            _changeMolecule = new TimerIsole(c.GetParametre("Delai change", 60000));

            _fonte = new OpenGLFonte(gl, OpenGLFonte.CARACTERES, TAILLE_FONTE, FontFamily.GenericSansSerif, FontStyle.Bold);

            _sphere = new Sphere();
            _sphere.CreateInContext(gl);
            _sphere.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            _sphere.NormalOrientation = SharpGL.SceneGraph.Quadrics.Orientation.Outside;
            _sphere.QuadricDrawStyle = SharpGL.SceneGraph.Quadrics.DrawStyle.Fill;
            _sphere.Slices = DETAIL_ATOMES;
            _sphere.Stacks = DETAIL_ATOMES;
            _sphere.Radius = 1.0f;

            _cylindre = new Cylinder();
            _cylindre.CreateInContext(gl);
            _cylindre.NormalGeneration = SharpGL.SceneGraph.Quadrics.Normals.Smooth;
            _cylindre.NormalOrientation = SharpGL.SceneGraph.Quadrics.Orientation.Outside;
            _cylindre.QuadricDrawStyle = SharpGL.SceneGraph.Quadrics.DrawStyle.Fill;
            _cylindre.Slices = DETAIL_LIENS;
            _cylindre.Stacks = DETAIL_LIENS;
            _cylindre.TopRadius = 0.5f;
            _cylindre.BaseRadius = 0.5f;
            _cylindre.Height = 1.0f;

            LitFichiers();
            ProchainFichier();
            LIGHTPOS = new float[] { -5, -2f, 0, 1 };
        }

        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                TAILLE_FONTE = c.GetParametre("Taille Fonte", 32);
                ZCAMERA = c.GetParametre("ZCamera", -5.0f, (a) => { ZCAMERA = (float)Convert.ToDouble(a); });
                VITESSE_CAM = c.GetParametre("VitesseCamera", 5.0f, (a) => { VITESSE_CAM = (float)Convert.ToDouble(a); });
                ECHELLE_RAYON = c.GetParametre("Echelle rayon", 1.0f, (a) => { ECHELLE_RAYON = (float)Convert.ToDouble(a); });
                RAYON_LIENS = c.GetParametre("Rayon liens", 0.2f, (a) => { RAYON_LIENS = (float)Convert.ToDouble(a); });
                FICHIER_EN_COURS = c.GetParametre("Fichier en cours", -1, (a) => { FICHIER_EN_COURS = Convert.ToInt32(a); });
                DETAIL_ATOMES = c.GetParametre("Detail atomes", 5);
                DETAIL_LIENS = c.GetParametre("Detail liens", 5);

            }
            return c;
        }

        /// <summary>
        /// Lit le prochain fichier molecule dans le repertoire
        /// </summary>
        private void ProchainFichier()
        {
            FICHIER_EN_COURS++;
            if (FICHIER_EN_COURS >= _fichiers.Count)
                FICHIER_EN_COURS = 0;

            c.SetParametre("Fichier en cours", FICHIER_EN_COURS);
            c.Flush();

            LireFichierPdb(_fichiers[FICHIER_EN_COURS]);
        }

        /// <summary>
        /// Lit les fichiers pdb dans le repertoire des molecules
        /// </summary>
        private void LitFichiers()
        {
            _fichiers = new List<string>();

            string repertoire = Path.Combine(Config.Configuration.GetDataDirectory(), "molecules");
            string[] filePaths = Directory.GetFiles(repertoire);
            foreach (string filename in filePaths)
                _fichiers.Add(filename);
        }

        /// <summary>
        /// Lecture du fichier pdb
        /// </summary>
        /// <param name="v">Nom du fichier a lire</param>
        private void LireFichierPdb(string fichier)
        {
            _nomMolecule = Path.GetFileNameWithoutExtension(fichier);
            _atomes.Clear();
            _liens.Clear();
            StreamReader file = new StreamReader(fichier);
            string line;
            while ((line = file.ReadLine()) != null)
                if (line.StartsWith("ATOM") || line.StartsWith("HETATM"))
                    AjouteAtome(line, _atomes);
                else
                if (line.StartsWith("CONECT"))
                    AjouteLien(line, _liens);

            file.Close();

            float x = 0;
            float y = 0;
            float z = 0;

            foreach (Atome a in _atomes)
            {
                x += a.x;
                y += a.y;
                z += a.z;
            }

            x /= _atomes.Count;
            y /= _atomes.Count;
            z /= _atomes.Count;

            foreach (Atome a in _atomes)
            {
                a.x -= x;
                a.y -= y;
                a.z -= z;
            }

            foreach (Lien l in _liens)
            {
                l.x1 -= x;
                l.y1 -= y;
                l.z1 -= z;
                l.x2 -= x;
                l.y2 -= y;
                l.z2 -= z;
            }

            if (_liens == null || _liens.Count == 0)
                ECHELLE_RAYON = c.GetParametre("Echelle rayon", 1.0f) * 2.0f;
            else
                ECHELLE_RAYON = c.GetParametre("Echelle rayon", 1.0f);
        }

        /// <summary>
        /// Ajoute un atome 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="_liens"></param>
        private void AjouteLien(string line, List<Lien> _liens)
        {
            line = Nettoie(line);
            string[] mots = line.Split(' ');

            if (mots == null)
                return;
            if (mots.Length < 3)
                return;

            if (!mots[0].Equals("CONECT"))
                return;

            Atome atome1 = TrouveAtome(mots[1]);
            if (atome1 == null)
                return;

            int noColonne = 2;
            while (noColonne < mots.Length)
            {
                Atome atome2 = TrouveAtome(mots[noColonne]);
                if (atome2 != null)
                {
                    Lien lien = new Lien();
                    lien.x1 = atome1.x;
                    lien.y1 = atome1.y;
                    lien.z1 = atome1.z;

                    lien.x2 = atome2.x;
                    lien.y2 = atome2.y;
                    lien.z2 = atome2.z;
                    _liens.Add(lien);
                }
                noColonne++;
            }
        }

        private Atome TrouveAtome(string v)
        {
            foreach (Atome a in _atomes)
                if (a.index.Equals(v))
                    return a;

            return null;
        }

        private void AjouteAtome(string line, List<Atome> _atomes)
        {
            line = Nettoie(line);
            string[] mots = line.Split(' ');

            if (mots == null)
                return;
            if (mots.Length < 6)
                return;

            if (!mots[0].Equals("ATOM") && !mots[0].Equals("HETATM"))
                return;

            Atome atome = new Atome();
            atome.index = mots[1];
            atome.nom = mots[2];

            try
            {
                if (mots.Length > 8)
                {
                    atome.x = float.Parse(mots[5]);
                    atome.y = float.Parse(mots[6]);
                    atome.z = float.Parse(mots[7]);
                }
                else
                {
                    atome.x = float.Parse(mots[4]);
                    atome.y = float.Parse(mots[5]);
                    atome.z = float.Parse(mots[6]);
                }
            }
            catch (Exception)
            {

            }

            switch (atome.nom[0])
            {
                case 'C':
                    atome.couleur = Color.Black;
                    atome.rayon = 1.0f;
                    break;

                case 'N':
                    atome.couleur = Color.Blue;
                    atome.rayon = 1.0f;
                    break;

                case 'O':
                    atome.couleur = Color.Red;
                    atome.rayon = 1.0f;
                    break;

                case 'H':
                    atome.couleur = Color.Cyan;
                    atome.rayon = 0.6f;
                    break;


                default:
                    atome.couleur = Color.Green;
                    atome.rayon = 0.9f;
                    break;
            }

            _atomes.Add(atome);
        }

        private string Nettoie(string ligne)
        {
            string line = ligne.Replace("\t", " ");
            line = line.Replace(".", ",");
            string l = line.Replace("  ", " ");
            while (!line.Equals(l))
            {
                line = l;
                l = line.Replace("  ", " ");
            }

            return line;
        }




        public override void Deplace(Temps maintenant, Rectangle tailleEcran)
        {
            base.Deplace(maintenant, tailleEcran);
            _angleCamera += maintenant.intervalleDepuisDerniereFrame * VITESSE_CAM;


            if (maintenant.intervalleDepuisDerniereFrame > 0.5f)
            {
                // Trop lent: essayer de diminuer les details

                if (_sphere.Slices > 5)
                    _sphere.Slices--;

                if (_sphere.Stacks > 5)
                    _sphere.Stacks--;
            }
            else
            if (maintenant.intervalleDepuisDerniereFrame < 0.01f)
            {
                // Rapide : augmenter les details
                _sphere.Slices++;
                _sphere.Stacks++;
            }
            if (_changeMolecule.Ecoule())
                ProchainFichier();
        }


        public override bool ClearBackGround(OpenGL gl, Color c)
        {
            gl.ClearColor(0, 0, 0, 1.0f);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
        }

        public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
        {
#if TRACER
            RenderStart(CHRONO_TYPE.RENDER);
#endif
            // Nom
            gl.Disable(OpenGL.GL_DEPTH);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            using (new Viewport2D(gl, 0, 0, tailleEcran.Width - 1, tailleEcran.Height - 1))
                _fonte.drawOpenGL(gl, _nomMolecule, tailleEcran.Width * 0.1f, tailleEcran.Height * 0.2f, couleur);

            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.CullFace(OpenGL.GL_BACK);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Disable(OpenGL.GL_FOG);
            gl.Disable(OpenGL.GL_ALPHA_TEST);
            gl.Disable(OpenGL.GL_BLEND);
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            setGlobalMaterial(gl, couleur);
            gl.Translate(0, 0, ZCAMERA);
            gl.Rotate(_angleCamera, _angleCamera, _angleCamera);

            float[] col = { couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f, 0.5f };
            gl.Color(col);

            foreach (Lien l in _liens)
                renderCylinder(gl, l.x1, l.y1, l.z1, l.x2, l.y2, l.z2, RAYON_LIENS);

            foreach (Atome a in _atomes)
            {
                gl.PushMatrix();

                gl.Translate(a.x, a.y, a.z);
                _sphere.Radius = a.rayon * ECHELLE_RAYON;
                col[0] = (couleur.R + a.couleur.R) / 256.0f * COL_COLOR[0];
                col[1] = (couleur.G + a.couleur.G) / 256.0f * COL_COLOR[1];
                col[2] = (couleur.B + a.couleur.B) / 256.0f * COL_COLOR[2];
                gl.Color(col);
                _sphere.PushObjectSpace(gl);
                _sphere.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
                _sphere.PopObjectSpace(gl);

                gl.PopMatrix();
            }
#if TRACER
            RenderStop(CHRONO_TYPE.RENDER);
#endif
        }

        private void renderCylinder(OpenGL gl, float x1, float y1, float z1, float x2, float y2, float z2, float rayon)
        {
            float vx = x2 - x1;
            float vy = y2 - y1;
            float vz = z2 - z1;

            //handle the degenerate case of z1 == z2 with an approximation
            if (vz == 0)
                vz = .0001f;

            float v = (float)Math.Sqrt(vx * vx + vy * vy + vz * vz);
            float ax = 57.2957795f * (float)Math.Acos(vz / v);
            if (vz < 0.0)
                ax = -ax;
            float rx = -vy * vz;
            float ry = vx * vz;
            gl.PushMatrix();

            //draw the cylinder body
            gl.Translate(x1, y1, z1);
            gl.Rotate(ax, rx, ry, 0.0);
            _cylindre.BaseRadius = rayon;
            _cylindre.TopRadius = rayon;
            _cylindre.PushObjectSpace(gl);
            _cylindre.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            _cylindre.PopObjectSpace(gl);
            gl.PopMatrix();
        }

        public override void Dispose()
        {
            base.Dispose();
            _fonte.Dispose();
        }
    }
}
