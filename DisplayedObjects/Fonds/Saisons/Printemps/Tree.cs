﻿using SharpGL;
using System;
using System.Collections.Generic;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.Printemps
{
    public class Tree
    {
        public bool DoneGrowing = false;
        private Vecteur3D Position;
        private readonly int NB_CIBLES;
        private readonly int LARGEUR_ARBRE;
        private readonly int HAUTEUR_ARBRE;
        private readonly int HAUTEUR_TRONC;
        private readonly int DISTANCE_MIN;
        private readonly int DISTANCE_MAX;
        private readonly int LONGUEUR_BRANCHE;
        private readonly float LARGEUR_TRONC;
        private readonly Vecteur3D POUSSEE = new Vecteur3D(-1, 0, 0);
        private static readonly float RATIO_TAILLE_PARENT = 1.01f;
        private Branch _racineArbre;
        private List<Cible> _ciblesBranches;
        private Dictionary<Vecteur3D, Branch> _branches;
        private List<Feuille> _feuilles;
        private Random r = new Random();
        private float _oscillation;

        public Tree(float X, float Y, float Z, float LargeurTronc, int LargeurArbre, int HauteurArbre, int LongueurBranche, int DistanceMin, int DistanceMax, int NbCibles, int HauteurTronc)
        {
            Position = new Vecteur3D(X, Y, Z);
            LARGEUR_TRONC = LargeurTronc;
            LARGEUR_ARBRE = LargeurArbre;
            HAUTEUR_ARBRE = HauteurArbre;
            LONGUEUR_BRANCHE = LongueurBranche;
            DISTANCE_MAX = DistanceMax;
            DISTANCE_MIN = DistanceMin;
            NB_CIBLES = NbCibles;
            HAUTEUR_TRONC = HauteurTronc;

            GenerateCrown();
            GenereTronc();

            _feuilles = new List<Feuille>();
            Feuille.InitTexture();
        }

        private void GenerateCrown()
        {
            _ciblesBranches = new List<Cible>();
            GenereFeuilles(_ciblesBranches, Position.X - HAUTEUR_TRONC - LARGEUR_ARBRE / 2,
                                   Position.Y,
                                   LARGEUR_ARBRE / 2,
                                   HAUTEUR_ARBRE / 2, NB_CIBLES);
        }

        private void GenereFeuilles(List<Cible> Leaves, float X, float Y, int Largeur, int Hauteur, int NbFeuilles)
        {
            float angleX, dx, dy, angleZ, dz;
            for (int i = 0; i < NbFeuilles; i++)
            {
                angleX = r.Next(0, (int)(Math.PI * 400.0)) / 100.0f;
                angleZ = r.Next(0, (int)(Math.PI * 400.0)) / 100.0f;
                dx = r.Next(0, Largeur * 10) / 10.0f;
                dy = r.Next(0, Hauteur * 10) / 10.0f;
                dz = r.Next(0, Hauteur * 10) / 10.0f;

                Leaves.Add(new Cible(new Vecteur3D(X + (float)Math.Sin(angleX) * dx,
                                    Y + (float)Math.Cos(angleX) * dy, (float)Math.Sin(angleZ) * dz)));
            }
        }

        private void GenereTronc()
        {
            _branches = new Dictionary<Vecteur3D, Branch>();

            _racineArbre = new Branch(null, Position, POUSSEE);
            _racineArbre.Size = LARGEUR_TRONC;
            _branches.Add(_racineArbre.Position, _racineArbre);

            Branch current = new Branch(_racineArbre, new Vecteur3D(Position.X - LONGUEUR_BRANCHE, Position.Y, 0), POUSSEE);
            _branches.Add(current.Position, current);

            //Keep growing trunk upwards until we reach a leaf      
            while ((_racineArbre.Position - current.Position).Length() < HAUTEUR_TRONC)
            {
                Branch trunk = new Branch(current, new Vecteur3D(current.Position.X - LONGUEUR_BRANCHE, current.Position.Y, 0), POUSSEE);
                _branches.Add(trunk.Position, trunk);
                current = trunk;
            }
        }

        public void Grow()
        {
            if (DoneGrowing) return;
            DoneGrowing = true;

            //If no leaves left, we are done
            if (_ciblesBranches.Count == 0) { DoneGrowing = true; return; }

            //process the leaves
            for (int i = 0; i < _ciblesBranches.Count; i++)
            {
                bool leafRemoved = false;

                _ciblesBranches[i].ClosestBranch = null;

                //Find the nearest branch for this leaf
                foreach (Branch b in _branches.Values)
                {
                    Vecteur3D direction = _ciblesBranches[i].Position - b.Position;
                    float distance = (float)Math.Round(direction.Length());            //distance to branch from leaf
                    direction.Normalize();

                    if (distance <= DISTANCE_MIN)            //Min leaf distance reached, we remove it
                    {
                        _feuilles.Add(new Feuille(b.Position));

                        _ciblesBranches.Remove(_ciblesBranches[i]);
                        i--;
                        leafRemoved = true;
                        break;
                    }
                    else if (distance <= DISTANCE_MAX)       //branch in range, determine if it is the nearest
                    {
                        if (_ciblesBranches[i].ClosestBranch == null)
                            _ciblesBranches[i].ClosestBranch = b;
                        else if ((_ciblesBranches[i].Position - _ciblesBranches[i].ClosestBranch.Position).Length() > distance)
                            _ciblesBranches[i].ClosestBranch = b;
                    }
                }

                if (!leafRemoved)
                {
                    //Set the grow parameters on all the closest branches that are in range
                    if (_ciblesBranches[i].ClosestBranch != null)
                    {
                        Vecteur3D dir = _ciblesBranches[i].Position - _ciblesBranches[i].ClosestBranch.Position;
                        dir.Normalize();
                        _ciblesBranches[i].ClosestBranch.GrowDirection += dir;       //add to grow direction of branch
                        _ciblesBranches[i].ClosestBranch.GrowCount++;
                    }

                    _ciblesBranches[i].Position.X += r.Next(-1, 2);
                    _ciblesBranches[i].Position.Y += r.Next(-1, 2);
                    _ciblesBranches[i].Position.Z += r.Next(-1, 2);
                    DoneGrowing = false;
                }


            }

            //Generate the new branches
            HashSet<Branch> newBranches = new HashSet<Branch>();
            foreach (Branch b in _branches.Values)
            {

                if (b.GrowCount > 0)    //if at least one leaf is affecting the branch
                {
                    Vecteur3D avgDirection = b.GrowDirection / b.GrowCount;
                    avgDirection.Normalize();

                    Branch newBranch = new Branch(b, b.Position + avgDirection * LONGUEUR_BRANCHE, avgDirection);

                    newBranches.Add(newBranch);
                    b.Reset();
                }
            }

            if (newBranches.Count == 0) { DoneGrowing = true; return; }

            //Add the new branches to the tree
            foreach (Branch b in newBranches)
            {
                //Check if branch already exists.  These cases seem to happen when leaf is in specific areas
                Branch existing;
                if (!_branches.TryGetValue(b.Position, out existing))
                {
                    _branches.Add(b.Position, b);
                    DoneGrowing = false;
                    //increment the size of the older branches, direct path to root
                    b.Size = Branch.LARGEUR_INITIALE;
                    Branch p = b.Parent;
                    while (p != null)
                    {
                        if (p.Parent != null)
                            p.Parent.Size = p.Size * RATIO_TAILLE_PARENT;

                        p = p.Parent;

                    }
                }
            }

            for (int i = 0; i < _ciblesBranches.Count; i++)
            {
                _ciblesBranches[i].Position.X += r.Next(-1, 2);
                _ciblesBranches[i].Position.Y += r.Next(-1, 2);
            }
        }



        public void Draw(OpenGL gl)
        {
            //gl.PointSize(4);
            //gl.Begin(OpenGL.GL_POINTS);
            //foreach (Cible b in _ciblesBranches )
            //    b.Draw(gl);
            //gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            foreach (Branch b in _branches.Values)
                b.Draw(gl, 0, _oscillation * (b.Position.X - Position.X));
            gl.End();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Color(0.2, 0.2, 0.2, 0.9);
            foreach (Feuille f in _feuilles)
            {
                f.Grow(0.1f);
                f.Draw(gl, 0, _oscillation * (f.Position.X - Position.X));
            }
            gl.Disable(OpenGL.GL_BLEND);
        }
        internal void Oscillation(float p)
        {
            _oscillation = p;
        }
    }
}
