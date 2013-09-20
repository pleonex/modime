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
			TestNdsRom("/store/Juegos/NDS/Ninokuni [CLEAN].nds");

			// Soon...

			Console.WriteLine("Done!");
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

		private static void TestNdsRom(string romPath)
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

			GameFile file = worker.RescueFile("/main/Ninokuni [CLEAN].nds/ROM/data/UI/Menu/Skin/2/MainMenu/bg_a.n2d");
			if (file != null)
				Console.WriteLine("Good :)");
		}
	}
}
