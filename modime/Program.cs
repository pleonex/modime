//-----------------------------------------------------------------------
// <copyright file="FileContainer.cs" company="none">
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
// <date>11/06/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mono.Addins;
using Libgame;
using Libgame.IO;

[assembly:AddinRoot("modime", "0.2")]
[assembly:ImportAddinAssembly("libgame.dll")]

namespace Modime
{
	public static class MainClass
	{
		private static string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static void Main(string[] args)
		{
			// Initializations
			AddinManager.Initialize();
			AddinManager.Registry.Update();

			// DEBUG
			args = new string[] { 
				"-i",
				"ExampleGame.xml",
				"Ninokuni español.xml",
				"/store/Juegos/NDS/Ninokuni [CLEAN].nds",
				"/lab/nds/projects/ninokuni/Alpha.nds"
			 };

			Stopwatch watch = new Stopwatch();
			watch.Start();

			// Tests
//			TestNdsRomRead(
//				"/store/Juegos/NDS/Ninokuni [CLEAN].nds",
//				"/main/Ninokuni [CLEAN].nds/ROM/data/movie/s01.txt",
//				"/lab/nds/projects/generic/s01.txt");
//			TestNdsRomWrite("/store/Juegos/NDS/Ninokuni [CLEAN].nds");
//			TestNinokuniExportImport(
//				"/store/Juegos/NDS/Ninokuni [CLEAN].nds",
//				"/Ninokuni [CLEAN].nds/ROM/data/movie/s01.txt");
//			TestXmlImporting("/store/Juegos/NDS/Ninokuni [CLEAN].nds");
//			return;

			// Import. Single file as input
			if (args.Length == 5 && args[0] == "-i") {
				DataStream stream = new DataStream(args[3], FileMode.Open, FileAccess.Read);
				GameFile mainFile = new GameFile(Path.GetFileName(args[3]), stream);

				string xmlGame = Path.Combine(AppPath, args[1]);
				string xmlEdit = Path.Combine(AppPath, args[2]);
				Worker worker = new Worker(xmlGame, xmlEdit, mainFile);
				worker.Import();
				worker.Write(args[4]);
			}

			watch.Stop();
			Console.WriteLine("Done! It tooks: {0}", watch.Elapsed);
			//Console.Write("Press any key to quit . . .");
			//Console.ReadKey(true);
		}

	}
}
