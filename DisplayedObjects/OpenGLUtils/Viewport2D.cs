using SharpGL;
using System;


/// <summary>
/// Moyen simple pour passer en viewport 2D sur toute la fenetre
/// utilisation:
///         using(new Viewport2D(gl, left, top, right,bottom)
///         {
///             ...
///         }
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    public class Viewport2D : IDisposable
    {
        private OpenGL _gl;
        /// <summary>
        /// Constructeur
        /// Parametres: coordonnees "logiques" des coins du viewport
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public Viewport2D(OpenGL gl, float left, float top, float right, float bottom)
        {
            _gl = gl;
            gl.PushMatrix();
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.LoadIdentity();
            gl.Ortho2D(left, right, top, bottom);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        public void Dispose()
        {
            _gl.MatrixMode(OpenGL.GL_PROJECTION);
            _gl.PopMatrix();
            _gl.MatrixMode(OpenGL.GL_MODELVIEW);
            _gl.PopMatrix();
        }
    }
}
