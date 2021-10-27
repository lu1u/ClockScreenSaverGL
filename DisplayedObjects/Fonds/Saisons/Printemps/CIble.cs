using SharpGL;
using System.Drawing;


namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps
{
    public class Cible
    {
        public Vecteur3D Position;
        public Branch ClosestBranch;

        public Cible(Vecteur3D position)
        {
            Position = position;
        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Green, Position.X, Position.Y, 2, 2);
        }

        public void Draw(OpenGL gl)
        {
            gl.Vertex(Position.X, Position.Y);
        }
    }
}
