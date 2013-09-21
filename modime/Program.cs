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
using System.IO;
using System.Xml.Linq;
using Libgame;
using Libgame.IO;
using Mono.Addins;

[assembly:AddinRoot("modime", "0.2")]
[assembly:ImportAddinAssembly("libgame.dll")]

namespace Modime
{
	public static class MainClass
	{
		public static void Main(string[] args)
		{
			AddinManager.Initialize();
			AddinManager.Registry.Update();

			// Tests
			DateTime startTime = DateTime.Now;
//			TestNdsRomRead(
//				"/store/Juegos/NDS/Ninokuni [CLEAN].nds",
//				"/main/Ninokuni [CLEAN].nds/ROM/data/UI/Menu/Skin/2/MainMenu/bg_a.n2d",
//				"/lab/nds/projects/generic/bg_a.n2d");
			TestNdsRomWrite("/store/Juegos/NDS/Ninokuni [CLEAN].nds");
			DateTime endTime = DateTime.Now;

			// Soon...

			Console.WriteLine("Done! {0}", (endTime - startTime));
		}

		private static void CreateEmptyXmls(XDocument xmlGame, XDocument xmlEdit)
		{
			XElement gameRoot = new XElement("GameInfo");
			gameRoot.Add(new XElement("Files"));
			xmlGame.Add(gameRoot);

			XElement gameEdit = new XElement("GameChanges");
			gameEdit.Add(new XElement("Files"));
			xmlEdit.Add(gameEdit);
		}

		private static void TestNdsRomRead(string romPath, string filePath, string outPath)
		{
			DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
			Format romFormat = AddinManager.GetExtensionObjects<Format>()[0];

			GameFolder main = new GameFolder("main");
			// TEMPFIX:
			GameFolder superoot = new GameFolder("");
			superoot.AddFolder(main);
			// END TEMPFIX 
			GameFile   rom  = new GameFile(Path.GetFileName(romPath), romStream, romFormat);
			main.AddFile(rom);
			romFormat.Initialize(rom);

			XDocument xmlGame = new XDocument();
			XDocument xmlEdit = new XDocument();
			CreateEmptyXmls(xmlGame, xmlEdit);
			Worker worker = new Worker(xmlGame, xmlEdit, main);

			GameFile file = worker.RescueFile(filePath);
			if (file != null)
				file.Stream.WriteTo(outPath);

			romStream.Dispose();
		}

		private static void TestNdsRomWrite(string romPath)
		{
			DataStream outStream = new DataStream(new MemoryStream(), 0, 0);
			DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
			Format romFormat = AddinManager.GetExtensionObjects<Format>()[0];

			GameFile rom = new GameFile(Path.GetFileName(romPath), romStream, romFormat);
			romFormat.Initialize(rom);

			DateTime t1 = DateTime.Now;
			romFormat.Read();
			DateTime t2 = DateTime.Now;
			romFormat.Write(outStream);
			DateTime t3 = DateTime.Now;
			outStream.WriteTo("/lab/nds/test.nds");
			DateTime t4 = DateTime.Now;

			outStream.Dispose();
			romStream.Dispose();

			// Display time result
			Console.WriteLine("Time results:");
			Console.WriteLine("\tRead                    -> {0}", t2 - t1);
			Console.WriteLine("\tWrite into MemoryStream -> {0}", t3 - t2);
			Console.WriteLine("\tWrite into FileStream   -> {0}", t4 - t3);
		}
	}
}
