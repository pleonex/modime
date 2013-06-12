//-----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="none">
// Copyright (C) 2013
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by 
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful, 
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details. 
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see "http://www.gnu.org/licenses/". 
// </copyright>
// <author>pleoNeX</author>
// <email>benito356@gmail.com</email>
// <date>12/06/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Modime
{
	public static class Configuration
	{
		private static string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		private static string xmlName = "ExampleConfig.xml";

		private static Dictionary<string, string> relativePaths;

		static Configuration()
		{
			ReadConfig();
		}

		public static string ResolvePath(string path)
		{
			if (!path.Contains("{$") || string.IsNullOrEmpty(path))
				return path;

			int pos = path.IndexOf('{');
			while (pos != -1) {
				// Get variable
				string variable = path.Substring(pos + 2, path.IndexOf('}', pos) - (pos + 2));
				path = path.Replace("{$" + variable + "}", relativePaths[variable]);

				pos = path.IndexOf('{', pos);
			}

			return path;
		}

		private static void ReadConfig()
		{
			XDocument doc = XDocument.Load(Path.Combine(appPath, xmlName));
			XElement root = doc.Root;

			if (root.Name != "Configuration")
				throw new Exception();

			// Get relative paths
			relativePaths = new Dictionary<string, string>();
			foreach (XElement xrel in root.Element("RelativePaths").Elements("Path")) {
				string variable = xrel.Element("Variable").Value;
				string path = xrel.Element("Location").Value;

				path = ResolvePath(path);
				relativePaths.Add(variable, path);
			}
		}
	}
}

