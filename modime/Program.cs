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

			Stopwatch watch = new Stopwatch();
			watch.Start();

			int argIdx = 0;

			// Get global settings
			string[] inputNames = new string[0];
			for (; argIdx < args.Length; argIdx++) {
				if (args[argIdx].StartsWith("--set-input-names=")) {
					inputNames = args[argIdx++].Substring(18).
					             Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					break;
				}
			}

			// Import. Single file as input
			if (args.Length >= 5 && args[argIdx] == "-i") {
				string filename = (inputNames.Length > 0) ? inputNames[0] : Path.GetFileName(args[argIdx + 3]);

				DataStream stream = new DataStream(args[argIdx + 3], FileMode.Open, FileAccess.Read);
				GameFile mainFile = new GameFile(filename, stream);

				string xmlGame = Path.Combine(AppPath, args[argIdx + 1]);
				string xmlEdit = Path.Combine(AppPath, args[argIdx + 2]);
				Worker worker = new Worker(xmlGame, xmlEdit, mainFile);
				worker.Import();
				worker.Write(args[argIdx + 4]);
			}

			watch.Stop();
			Console.WriteLine("Done! It took: {0}", watch.Elapsed);
			#if !DEBUG
			Console.Write("Press any key to quit . . .");
			Console.ReadKey(true);
			#endif
		}

	}
}
