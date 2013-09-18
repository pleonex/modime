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
using System.Xml.Linq;
using Modime.IO;

namespace Modime
{
	public class Worker
	{
		private XDocument game;
		private XDocument edit;
		private FileContainer  root;

		private List<string> updateQueue;

		public Worker(string gameXml, string editXml, FileContainer root)
		{
			this.game = XDocument.Load(gameXml);
			this.edit = XDocument.Load(editXml);
			this.root = root;
			this.updateQueue = new List<string>();
		}

		public void Import()
		{
			// TODO: Support import from more than one file (ie: png + xml for nftr)
			XElement files = edit.Root.Element("Files");

			foreach (XElement fileEdit in files.Elements("File")) {
				string path   = fileEdit.Element("Path").Value;
				string import = fileEdit.Element("Import").Value;

				GameFile file = this.RescueFile(path);
				file.Format.Read();
				file.Format.Import(import);
				this.UpdateQueue(file);
			}
		}

		public void Write(string outputPath)
		{
			foreach (string filePath in this.updateQueue) {
				GameFile file = this.root.SearchFile(filePath) as GameFile;
				if (file == null)
					throw new Exception("File not found.");

				file.Format.Write();
			}

			this.updateQueue.Clear();
		}

		public GameFile RescueFile(string gameFilePath)
		{
			XElement fileInfo = this.GetFileInfo(gameFilePath);

			if (fileInfo == null) {
				// Error
				return null;
			}

			// Resolve dependencies
			List<GameFile> depends = new List<GameFile>();
			foreach (XElement xmlDepend in fileInfo.Elements("DependsOn")) {
				GameFile dependency = this.RescueFile(xmlDepend.Value);
				depends.Add(dependency);

				dependency.Format.Read();
			}

			// Get type of dependency
			Type t = Type.GetType(fileInfo.Element("Type").Value, true, false);

			// Get file
			GameFile file = root.SearchFile(gameFilePath) as GameFile;
			if (file == null)
				throw new Exception("File not found.");

			file.AddDependencies(depends.ToArray());

			// Set type and read
			if (file.Format == null)
				file.SetFormat(t, null);

			return file;
		}

		private XElement GetFileInfo(string path)
		{
			XElement files = game.Root.Element("Files");
			foreach (XElement fileInfo in files.Elements("FileInfo")) {
				if (fileInfo.Element("Path").Value == path)
					return fileInfo;
			}

			return null;
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
				if (depPosition > depTopPosition)
					depTopPosition = depPosition;
			}

			// Insert the file just above the top dependency
			this.updateQueue.Insert(depTopPosition, file.Path);
		}
	}
}

