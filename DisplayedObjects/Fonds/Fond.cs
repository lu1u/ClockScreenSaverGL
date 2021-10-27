/*
 * Crée par SharpDevelop.
 * Utilisateur: lucien
 * Date: 22/12/2014
 * Heure: 15:06
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System.Drawing;
using System.Windows.Forms;
using ClockScreenSaverGL.Config;
using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects.Fonds
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public abstract class Fond : DisplayedObject
	{

		public Fond(OpenGL gl) : base(gl)
		{

		}

		public override bool ClearBackGround(OpenGL gl, Color c)
		{
			gl.ClearColor(0, 0, 0, 0);
			gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            return true;
		}

		//public override void AfficheOpenGL(OpenGL gl, Temps maintenant, Rectangle tailleEcran, Color couleur)
		//{
		//	base.AfficheOpenGL(gl, maintenant, tailleEcran, couleur);
		//}

		public virtual void fillConsole(OpenGL gl)
		{
			//getConfiguration()?.fillConsole(gl);
			string[] lignes = getConfiguration().getLignesParametres();
			Console c = Console.getInstance(gl);
			c.AddLigne(Color.LightGreen, "");
			//c.AddLigne(Color.LightGreen, getConfiguration().);
			c.AddLigne(Color.LightGreen, "");
            c.AddLigne(Color.LightGreen, "F1..F6: changer couleur");
            c.AddLigne(Color.LightGreen, "8/2 : changer le parametre courant");
			c.AddLigne(Color.LightGreen, "4/6 : modifier la valeur du parametre courant");
			c.AddLigne(Color.LightGreen, "Les valeurs en gris nécessitent de redémarrer le fond (touche R)");
			c.AddLigne(Color.LightGreen, "");

			foreach (string ligne in lignes)
				if (ligne.Length > 1)
				{
					Color col;
					switch (ligne[0])
					{
						case 'Y':
							col = Color.Yellow; break;
						case 'G':
							col = Color.Green; break;
						case 'W':
							col = Color.White; break;
						default:
							col = Color.Gray; break;
					}

					c.AddLigne(col, ligne.Substring(1));
				}
		}

		public override bool KeyDown(Form f, Keys k)
		{
			CategorieConfiguration c = getConfiguration();
			if (c?.KeyDown(k) == true)
				return true;

			return base.KeyDown(f, k);
		}
	}
}
