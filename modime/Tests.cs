// -----------------------------------------------------------------------
// <copyright file="Tests.cs" company="none">
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
// <author>pleonex</author>
// <email>benito356@gmail.com</email>
// <date>22/09/2013</date>
// -----------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Libgame;
using Libgame.IO;

namespace Modime
{
	public static class Tests
	{
		private static string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		private static void TestNdsRomRead(string romPath, string filePath, string outPath)
		{
			DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
			Format romFormat = FileManager.GetFormat("Rom");

			GameFolder main = new GameFolder("main");
			GameFile rom  = new GameFile(Path.GetFileName(romPath), romStream, romFormat);
			main.AddFile(rom);
			romFormat.Initialize(rom);

			XDocument xmlGame = new XDocument();	// TODO: Replace with ExampleGame.xml
			xmlGame.Add(new XElement("GameInfo", new XElement("Files")));
			FileManager.Initialize(main, FileInfoCollection.FromXml(xmlGame));

			GameFile file = FileManager.GetInstance().RescueFile(filePath);
			if (file != null)
				file.Stream.WriteTo(outPath);

			romStream.Dispose();
		}

		private static void TestNdsRomWrite(string romPath)
		{
			DataStream outStream = new DataStream(new MemoryStream(), 0, 0);
			DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
			Format romFormat = FileManager.GetFormat("Rom");

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

		private static void TestNinokuniExportImport(string romPath, string filePath)
		{
			DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
			Format romFormat = FileManager.GetFormat("Rom");
			Format subtitleFormat = FileManager.GetFormat("Subtitle");

			GameFile rom  = new GameFile(Path.GetFileName(romPath), romStream, romFormat);
			romFormat.Initialize(rom);

			XDocument xmlGame = XDocument.Load(Path.Combine(AppPath, "ExampleGame.xml"));
			XDocument xmlEdit = XDocument.Load(Path.Combine(AppPath, "ExampleEdition.xml"));
			FileManager.Initialize(rom, FileInfoCollection.FromXml(xmlGame));
			Configuration.Initialize(xmlEdit);

			GameFile file = FileManager.GetInstance().RescueFile(filePath);
			subtitleFormat.Initialize(file);
			file.Format.Read();
			file.Format.Import("/home/benito/Dropbox/Ninokuni español/Texto/Subs peli/s01.xml");
			file.Format.Write();

			romFormat.Write();
			rom.Stream.WriteTo("/lab/nds/projects/generic/test.nds");
			romStream.Dispose();
		}

		private static void TestXmlImporting(string romPath)
		{
			DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
			GameFile rom  = new GameFile(Path.GetFileName(romPath), romStream);

			XDocument xmlGame = XDocument.Load(Path.Combine(AppPath, "ExampleGame.xml"));
			XDocument xmlEdit = XDocument.Load(Path.Combine(AppPath, "ExampleEdition.xml"));

			Worker worker = new Worker(xmlGame, xmlEdit, rom);
			worker.Import();
			worker.Write("/lab/nds/projects/generic/test.nds");
		}
	}
}

