using SharpGL;
using System;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
    internal class PolygonMode : IDisposable
    {
        private int[] _polyMode = new int[1];
        private float[] _lineWidth = new float[1];
        private OpenGL _gl;

        public PolygonMode(OpenGL gl, uint polyMode, float lineWidth)
        {
            _gl = gl;
            gl.GetInteger(SharpGL.Enumerations.GetTarget.PolygonMode, _polyMode);
            gl.GetFloat(SharpGL.Enumerations.GetTarget.LineWidth, _lineWidth);

            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, polyMode);
            gl.LineWidth(lineWidth);
        }

        public void Dispose()
        {
            _gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, (uint)_polyMode[0]);
            _gl.LineWidth(_lineWidth[0]);
        }
    }
}