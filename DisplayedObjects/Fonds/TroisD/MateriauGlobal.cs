﻿using ClockScreenSaverGL.Config;
using SharpGL;
using System;
using System.Drawing;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds.TroisD
{
    public abstract class MateriauGlobal : TroisD
    {
        private const string CONF_LIGHT_AMBIENT = "Material Light Ambient";
        private const string CONF_LIGHT_DIFFUSE = "Material Light Diffuse";
        private const string CONF_LIGHT_SPECULAR = "Material Light Specular";
        private const string CONF_COL_AMBIENT = "Material Ambient";
        private const string CONF_COL_DIFFUSE = "Material Diffuse";
        private const string CONF_COL_SPECULAR = "Material Specular";
        private const string CONF_COL_COLOR = "Material Color";
        private const string CONF_SHININESS = "Material Shininess";


        //protected enum VALEUR_MODIFIEE { LAMBIENT = 0, LDIFFUSE = 1, LSPECULAR = 2, AMBIENT = 3, DIFFUSE = 4, SPECULAR = 5, COLOR = 6, SHININESS = 7 };
        protected float[] COL_AMBIENT = { 0.21f, 0.12f, 0.05f, 1.0f };
        protected float[] COL_DIFFUSE = { 0.7f, 0.72f, 0.78f, 1.0f };
        protected float[] COL_SPECULAR = { 0.7f, 0.7f, 0.7f, 1.0f };
        protected float[] COL_COLOR = { 0.7f, 0.7f, 0.7f, 1.0f };
        protected float SHININESS = 18f;

        protected float[] LIGHTPOS = { -2, 1.5f, -2.5f, 1.0f };
        protected float[] LIG_SPECULAR = { 1.0f, 1.0f, 1.0f };
        protected float[] LIG_AMBIENT = { 0.5f, 0.5f, 0.5f };
        protected float[] LIG_DIFFUSE = { 1.0f, 1.0f, 1.0f };
        protected float RATIO_COULEUR = 1.0f / 256.0f;

        //protected VALEUR_MODIFIEE valModifie = VALEUR_MODIFIEE.AMBIENT;
        public MateriauGlobal(OpenGL gl) : base(gl, 0, 0, 0, 0)
        {
            CategorieConfiguration c = GetConfiguration();

            float val = c.GetParametre(CONF_LIGHT_AMBIENT, 0.5f, (a) =>
            {
                LIG_AMBIENT[0] = (float)Convert.ToDouble(a);
                LIG_AMBIENT[1] = (float)Convert.ToDouble(a);
                LIG_AMBIENT[2] = (float)Convert.ToDouble(a);
            });
            LIG_AMBIENT[0] = val;
            LIG_AMBIENT[1] = val;
            LIG_AMBIENT[2] = val;

            val = c.GetParametre(CONF_LIGHT_DIFFUSE, 1.0f, (a) =>
           {
               LIG_DIFFUSE[0] = (float)Convert.ToDouble(a);
               LIG_DIFFUSE[1] = (float)Convert.ToDouble(a);
               LIG_DIFFUSE[2] = (float)Convert.ToDouble(a);
           });
            LIG_DIFFUSE[0] = val;
            LIG_DIFFUSE[1] = val;
            LIG_DIFFUSE[2] = val;

            val = c.GetParametre(CONF_LIGHT_SPECULAR, 1.0f, (a) =>
           {
               LIG_SPECULAR[0] = (float)Convert.ToDouble(a);
               LIG_SPECULAR[1] = (float)Convert.ToDouble(a);
               LIG_SPECULAR[2] = (float)Convert.ToDouble(a);
           });
            LIG_SPECULAR[0] = val;
            LIG_SPECULAR[1] = val;
            LIG_SPECULAR[2] = val;

            val = c.GetParametre(CONF_COL_AMBIENT, 0.2f, (a) =>
           {
               COL_AMBIENT[0] = (float)Convert.ToDouble(a);
               COL_AMBIENT[1] = (float)Convert.ToDouble(a);
               COL_AMBIENT[2] = (float)Convert.ToDouble(a);
           });
            COL_AMBIENT[0] = val;
            COL_AMBIENT[1] = val;
            COL_AMBIENT[2] = val;

            val = c.GetParametre(CONF_COL_DIFFUSE, 0.2f, (a) =>
           {
               COL_DIFFUSE[0] = (float)Convert.ToDouble(a);
               COL_DIFFUSE[1] = (float)Convert.ToDouble(a);
               COL_DIFFUSE[2] = (float)Convert.ToDouble(a);
           });
            COL_DIFFUSE[0] = val;
            COL_DIFFUSE[1] = val;
            COL_DIFFUSE[2] = val;

            val = c.GetParametre(CONF_COL_SPECULAR, 0.7f, (a) =>
           {
               COL_SPECULAR[0] = (float)Convert.ToDouble(a);
               COL_SPECULAR[1] = (float)Convert.ToDouble(a);
               COL_SPECULAR[2] = (float)Convert.ToDouble(a);
           });
            COL_SPECULAR[0] = val;
            COL_SPECULAR[1] = val;
            COL_SPECULAR[2] = val;

            val = c.GetParametre(CONF_COL_COLOR, 0.7f, (a) =>
           {
               COL_COLOR[0] = (float)Convert.ToDouble(a);
               COL_COLOR[1] = (float)Convert.ToDouble(a);
               COL_COLOR[2] = (float)Convert.ToDouble(a);
           });
            COL_COLOR[0] = val;
            COL_COLOR[1] = val;
            COL_COLOR[2] = val;

            SHININESS = c.GetParametre(CONF_SHININESS, 45f, (a) => { SHININESS = (float)Convert.ToDouble(a); });
        }

        protected void setGlobalMaterial(OpenGL gl, Color couleur) => setGlobalMaterial(gl, couleur.R / 256.0f, couleur.G / 256.0f, couleur.B / 256.0f);

        protected void setGlobalMaterial(OpenGL gl, float R, float G, float B)
        {
            // Lumiere
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Disable(OpenGL.GL_LIGHT1);
            gl.Disable(OpenGL.GL_LIGHT2);
            float[] model_ambient = { 0.4f, 0.4f, 0.4f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, model_ambient);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, LIGHTPOS);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, LIG_SPECULAR);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, LIG_AMBIENT);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, LIG_DIFFUSE);

            // Aspect de la surface
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, COL_SPECULAR);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT, COL_AMBIENT);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE, COL_DIFFUSE);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, SHININESS);

            gl.Color(COL_COLOR[0] * R, COL_COLOR[1] * G, COL_COLOR[2] * B);
        }
    }

}
