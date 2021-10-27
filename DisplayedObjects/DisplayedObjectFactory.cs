using System;
using System.IO;
using System.Reflection;

using SharpGL;

namespace ClockScreenSaverGL.DisplayedObjects
{
	class DisplayedObjectFactory
	{
		public const String REPERTOIRE_BIBLIOTHEQUES = "Displayed Objects";
		public const String EXTENSION_BIBLIOTHEQUES = "*.dll";

		public static String[] GetDisplayedObjects()
		{
			
			String repertoireDLLs = Path.Combine(new FileInfo((Assembly.GetExecutingAssembly().Location)).Directory.FullName, REPERTOIRE_BIBLIOTHEQUES);
			try
			{
				return Directory.GetFiles(repertoireDLLs, EXTENSION_BIBLIOTHEQUES);
			}
			catch (Exception)
			{
				return null ;
			}
		}

		DisplayedObject createFromDLL(string dllName)
		{
			DisplayedObject o;
			try
			{
				Assembly assembly = Assembly.LoadFrom(dllName);
				Type type = assembly.GetType("DisplayedObject.DisplayedObject");
				return Activator.CreateInstance(type) as DisplayedObject;
			}
			catch(Exception)
			{
				o = null;
			}
			return o;
		}
	}
}
