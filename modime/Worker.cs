//-----------------------------------------------------------------------
// <copyright file="Worker.cs" company="none">
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
using System.Linq;
using System.Xml.Linq;
using Libgame;
using Mono.Addins;

namespace Modime
{
	public class Worker
	{
		private XDocument edit;
		private FileManager fileManager;
		private Configuration config;
		private List<string> updateQueue;

		public Worker(XDocument xmlGame, XDocument xmlEdit, FileContainer root)
		{
			Configuration.Initialize(xmlEdit);
			this.config = Configuration.GetInstance();
			FileManager.Initialize(root, xmlGame);
			this.fileManager = FileManager.GetInstance();

			this.edit = xmlEdit;
			this.updateQueue = new List<string>();
		}

		public Worker(string xmlGame, string xmlEdit, FileContainer root)
			: this(XDocument.Load(xmlGame), XDocument.Load(xmlEdit), root)
		{
		}

		public void Import()
		{
			XElement files = edit.Root.Element("Files");

			int i = 1;
			int x = Console.CursorLeft;
			int y = Console.CursorTop;
			int updX = x;
			int updY = y + 1;
			int totalImports = files.Elements("File").Count();

			foreach (XElement fileEdit in files.Elements("File")) {
				Console.SetCursorPosition(x, y);
				Console.WriteLine("Import file {0:0000} of {1:0000}", i++, totalImports);
				Console.SetCursorPosition(updX, updY);

				string path = fileEdit.Element("Path").Value;
				string[] import = fileEdit.Elements("Import").
				                  Select(f => this.config.ResolvePath(f.Value)).
				                  ToArray();

				GameFile file = fileManager.RescueFile(path);
				file.Format.Read();
				file.Format.Import(import);
				this.UpdateQueue(file);

				updX = Console.CursorLeft;
				updY = Console.CursorTop;
			}
		}

		public void Write(params string[] outputPath)
		{
			// Write files data
			Console.WriteLine("Writing {0:0000} internal files...", this.updateQueue.Count);
			foreach (string filePath in this.updateQueue) {
				GameFile file = this.fileManager.Root.SearchFile(filePath) as GameFile;
				if (file == null)
					throw new Exception("File not found.");

				file.Format.Write();
			}

			this.updateQueue.Clear();

			// Write to output files
			Console.WriteLine("Writing {0:0000} external files...", outputPath.Length);
			if (fileManager.Root is GameFile) {
				if (outputPath.Length != 1)
					throw new ArgumentException("Only one file can be written");

				if (System.IO.File.Exists(outputPath[0]))
					System.IO.File.Delete(outputPath[0]);
				((GameFile)fileManager.Root).Stream.WriteTo(outputPath[0]);
			} else if (fileManager.Root is GameFolder) {
				if (outputPath.Length != fileManager.Root.Files.Count)
					throw new ArgumentException("There are not enough output paths.");

				for (int i = 0; i < fileManager.Root.Files.Count; i++) {
					GameFile f = fileManager.Root.Files[i] as GameFile;
					if (f != null) {
						if (System.IO.File.Exists(outputPath[i]))
							System.IO.File.Delete(outputPath[i]);
						f.Stream.WriteTo(outputPath[i]);
					}
				}
			}
		}

		private void UpdateQueue(GameFile file)
		{
			foreach (GameFile dependency in file.Dependencies)
				this.UpdateQueue(dependency);

			// Checks if it's in the queue
			if (this.updateQueue.Contains(file.Path))
				return;

			// Get the higher position in the queue of it's dependencies
			int depTopPosition = this.updateQueue.Count;
			foreach (GameFile dependency in file.Dependencies) {
				int depPosition = this.updateQueue.IndexOf(dependency.Path);
				if (depPosition < depTopPosition)
					depTopPosition = depPosition;
			}

			// Insert the file just above the top dependency
			this.updateQueue.Insert(depTopPosition, file.Path);
		}
	}
}

