using SharpGL;
using System;
/// <summary>
/// Moyen simple pour utiliser gl.Begin...gl.End
/// utilisation:
///         using(new GlBegin(gl, OpenGL.GL_QUADS)
///         {
///             ...
///         }
/// </summary>
namespace ClockScreenSaverGL.DisplayedObjects.OpenGLUtils
{
    public class GLBegin : IDisposable
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
        public GLBegin(OpenGL gl, uint glMode)
        {
            _gl = gl;
            gl.Begin(glMode);
        }

        public void Dispose()
        {
            _gl.End();
        }
    }
}
