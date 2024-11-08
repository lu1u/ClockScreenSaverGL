using ClockScreenSaverGL.Config;
using ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD;
using SharpGL;
using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    /// <summary>
    /// Objet Heightmap chargé à partir d'un fichier image
    /// Avec ou sans calcul des normales
    /// Créé une calllist opengl affichable par callList
    /// </summary>
    public class HeightmapAsynchrone : ObjetAsynchrone, IDisposable
    {
        private readonly OpenGL _gl;
        private readonly string _nomFichier;
        private readonly bool _genererNormales;
        private readonly float _RapportAltitude;
        private readonly float _LargeurCarte;
        private readonly float _HauteurCarte;
        private int largeur;
        private int hauteur;
        private Vecteur3D[,] _normales;
        private byte[,] _heightmap;
        private uint _glListe;

        public int Largeur { get => largeur; set => largeur = value; }
        public int Hauteur { get => hauteur; set => hauteur = value; }

        public HeightmapAsynchrone(OpenGL gl, string nomFichier, float rapportAltitude, float largeurCarte, float hauteurCarte, bool normales = false)
        {
            _gl = gl;
            _nomFichier = nomFichier;
            _RapportAltitude = rapportAltitude;
            _LargeurCarte = largeurCarte;
            _HauteurCarte = hauteurCarte;
            _genererNormales = normales;
        }

        public void callList(OpenGL gl)
        {
            if (Pret)
                gl.CallList(_glListe);
        }

        protected override void InitAsynchrone()
        {
            _heightmap = ChargeFichierheightmap(_nomFichier);
            if (_genererNormales)
                _normales = CalculNormales(_heightmap);
        }

        protected override void InitSynchrone()
        {
            CreerObjet3D(_gl, _heightmap);
            //_heightmap = null;
            _normales = null;
        }

        /// <summary>
        /// Creer l'objet 3D (OpenGL List)
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="largeur"></param>
        /// <param name="hauteur"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CreerObjet3D(OpenGL gl, byte[,] heightmap)//, Vecteur3D[,] normales)
        {
            try
            {
                _glListe = gl.GenLists(1);
                gl.NewList(_glListe, OpenGL.GL_COMPILE);

                for (int x = 1; x < Largeur; x++)
                {
                    using (new GLBegin(gl, OpenGL.GL_QUAD_STRIP))
                        for (int z = 1; z < Hauteur; z++)
                        {
                            Vertex(gl, heightmap, x, z);
                            Vertex(gl, heightmap, x - 1, z);
                        }
                }
                gl.EndList();
            }
            catch (Exception ex)
            {
                Log.Instance.Error(ex.Message);
                Log.Instance.Error(ex.StackTrace);
                Log.Instance.Error("Grenoble.CreerObjet3D");
            }
        }

        /// <summary>
        /// Calcul des normales des facettes de la heightmap
        /// https://www.flipcode.com/archives/Calculating_Vertex_Normals_for_Height_Maps.shtml
        private Vecteur3D[,] CalculNormales(byte[,] heightmap)
        {
            Vecteur3D[,] normales = new Vecteur3D[Largeur, Hauteur];

            for (int z = 0; z < Hauteur; ++z)
                for (int x = 0; x < Largeur; ++x)
                {
                    float sx = heightmap[x < Largeur - 1 ? x + 1 : x, z] - heightmap[x > 0 ? x - 1 : x, z];
                    if (x == 0 || x == Largeur - 1)
                        sx *= 2.0f;

                    float sz = heightmap[x, z < Hauteur - 1 ? z + 1 : z] - heightmap[x, z > 0 ? z - 1 : z];
                    if (z == 0 || z == Hauteur - 1)
                        sz *= 2.0f;

                    normales[x, z] = new Vecteur3D(-sx, 8, sz);
                    normales[x, z].Normalize();
                }

            return normales;
        }
        /// <summary>
        /// Charge le fichier heightMap dans _heightMap, tableau de bytes
        /// </summary>
        /// <param name="nOM_FICHIER_HEIGHTMAP"></param>
        /// <exception cref="NotImplementedException"></exception>
        private byte[,] ChargeFichierheightmap(string nomFichier)
        {
            Bitmap bmp = (Bitmap)Image.FromFile(nomFichier);
            Largeur = bmp.Width;
            Hauteur = bmp.Height;
            byte[,] heightMap = new byte[Largeur, Hauteur];

            for (int l = 0; l < Hauteur; l++)
                for (int c = 0; c < Largeur; c++)
                {
                    Color p = bmp.GetPixel(c, l);
                    heightMap[c, l] = (byte)((p.R + p.G + p.B) / 3.0);
                }

            return heightMap;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Vertex(OpenGL gl, byte[,] heightmap, int x, int z)
        {
            if (_normales != null)
                _normales[x, z].Normal(gl);

            gl.TexCoord(calculTexX(x), calculTexZ(z));
            gl.Vertex(calculX(x), calculY(heightmap[x, z]), calculZ(z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] private byte HeightMap(byte[] heightmap, int x, int z) => heightmap[(z * Largeur) + x];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private float calculTexZ(int z) => z / (float)Hauteur;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private float calculTexX(int x) => x / (float)Largeur;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private float calculY(byte height) => height * _RapportAltitude;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private float calculX(int x) => x * _LargeurCarte / Largeur - _LargeurCarte / 2.0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] private float calculZ(int z) => (z - (Hauteur * 0.5f)) * _HauteurCarte / Hauteur;

        public void Dispose()
        {
            _gl.DeleteLists(_glListe, 1);
        }

        internal byte getAltitude(int x, int z)
        {
            return _heightmap[x, z];
        }
    }
}

