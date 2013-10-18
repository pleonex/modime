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
			Console.WriteLine("/*");
			Console.WriteLine("****************************");
			Console.WriteLine("**         ModiMe         **");
			Console.WriteLine("** The new way to modify  **");
			Console.WriteLine("**       your games       **");
			Console.WriteLine("****************************");
			Console.WriteLine("~~~~~~~~ by pleoNeX ~~~~~~~~");
			Console.WriteLine("| Version: {0,-16}|", Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("~~~~~ Good RomHacking! ~~~~~");
			Console.WriteLine("*/");
			Console.WriteLine();

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
				string xmlGame    = args[argIdx + 1];
				string xmlEdit    = args[argIdx + 2];
				string outputFile = args[argIdx + 4];
				string inputFile  = args[argIdx + 3];
				string filename   = (inputNames.Length > 0) ? inputNames[0] : Path.GetFileName(inputFile);

				Console.WriteLine("Time to import!");
				Console.WriteLine("From {0}", inputFile);
				Console.WriteLine("To   {0}", outputFile);
				Console.WriteLine("Game specs:   {0}", xmlGame);
				Console.WriteLine("Modify specs: {0}", xmlEdit);
				Console.WriteLine("Now... Let's start!");

				DataStream stream = new DataStream(inputFile, FileMode.Open, FileAccess.Read);
				GameFile mainFile = new GameFile(filename, stream);

				Worker worker = new Worker(xmlGame, xmlEdit, mainFile);
				worker.Import();
				worker.Write(outputFile);
			} else {
				ShowHelp();
			}

			watch.Stop();
			Console.WriteLine();
			Console.WriteLine("Done! It took: {0}", watch.Elapsed);
			#if !DEBUG
			Console.Write("Press any key to quit . . .");
			Console.ReadKey(true);
			#endif
		}

		private static void ShowHelp()
		{
			Console.WriteLine("USAGE: modime [settings] [mode] [parameters]");
			Console.WriteLine("% Settings:");
			Console.WriteLine("\t--set-input-names=name1,name2\tSet the name of the input files.");
			Console.WriteLine("\t\t\t\t\tThat name will be used in the");
			Console.WriteLine("\t\t\t\t\tvirtual filesystem.");
			Console.WriteLine();
			Console.WriteLine("% Mode:");
			Console.WriteLine("\t-i\tOpen a file and run the import process from it.");
			Console.WriteLine("\t\tParameter0: XML Game Specification");
			Console.WriteLine("\t\tParameter1: XML Modify Specification");
			Console.WriteLine("\t\tParameter2: Input file path");
			Console.WriteLine("\t\tParameter3: Output file path");
		}
	}
}
