
using ClockScreenSaverGL.Config;
using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD.Grilles
{
    internal class Cubes : GrilleBase
    {
        #region Parametres
        public const string CAT = "Cubes";
        private CategorieConfiguration c;
        private int NB_CUBES_X;
        private int NB_CUBES_Y;
        private int NB_CUBES_Z;
        private float TAILLE_CUBE;
        private float ECART_CUBE;
        #endregion



        public Cubes(OpenGL gl) : base(gl)
        {
            GetConfiguration();
            VITESSE_ROTATION = c.GetParametre("Vitesse Rotation", 0.5f);
            TRANSLATE_Z = ECART_CUBE * NB_CUBES_Z * -0.55f;
            LIGHTPOS[0] = ECART_CUBE * NB_CUBES_X;
            LIGHTPOS[1] = ECART_CUBE * NB_CUBES_Y;
            LIGHTPOS[2] = ECART_CUBE * NB_CUBES_Z;
            fogEnd = ECART_CUBE * NB_CUBES_X * 0.5f;
        }


        public override CategorieConfiguration GetConfiguration()
        {
            if (c == null)
            {
                c = Configuration.GetCategorie(CAT);
                NB_CUBES_X = c.GetParametre("Nb Cubes X", 30);
                NB_CUBES_Y = c.GetParametre("Nb Cubes Y", 30);
                NB_CUBES_Z = c.GetParametre("Nb Cubes Z", 30);
                TAILLE_CUBE = c.GetParametre("Taille cubes", 0.06f);
                ECART_CUBE = c.GetParametre("Ecart cubes", 0.4f);
            }
            return c;
        }
        protected override void GenererListe(OpenGL gl)
        {
            float ORIGINE_X = -(NB_CUBES_X * 0.5f) * ECART_CUBE;
            float ORIGINE_Y = -(NB_CUBES_Y * 0.5f) * ECART_CUBE;
            float ORIGINE_Z = -(NB_CUBES_Z * 0.5f) * ECART_CUBE;
            gl.Begin(OpenGL.GL_QUADS);

            for (int x = 0; x < NB_CUBES_X; x++)
                for (int y = 0; y < NB_CUBES_Y; y++)
                    for (int z = 0; z < NB_CUBES_Z; z++)
                        Brique(gl, ORIGINE_X + x * ECART_CUBE, ORIGINE_Y + y * ECART_CUBE, ORIGINE_Z + z * ECART_CUBE,
                            TAILLE_CUBE, TAILLE_CUBE, TAILLE_CUBE);

            gl.End();
        }


    }
}
