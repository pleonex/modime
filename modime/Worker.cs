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
using Libgame;
using Mono.Addins;

namespace Modime
{
	public class Worker
	{
		private XDocument game;
		private XDocument edit;
		private FileContainer  root;

		private List<string> updateQueue;

		public Worker(XDocument gameXml, XDocument editXml, FileContainer root)
		{
			this.game = gameXml;
			this.edit = editXml;
			this.root = root;
			this.updateQueue = new List<string>();
		}

		public Worker(string gameXml, string editXml, FileContainer root)
			: this(XDocument.Load(gameXml), XDocument.Load(editXml), root)
		{
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
			if (fileInfo != null)
				return this.RescueFileInfo(gameFilePath, fileInfo);
			else
				return this.RescueFileNoInfo(gameFilePath);
		}

		private GameFile RescueFileNoInfo(string gameFilePath)
		{
			// 1.- Gets dependencies
			// Since no info of dependencies is given, it will search them in two steps.
			List<GameFile> depends = new List<GameFile>();

			// 1.1.- Gets dependencies to get file data.
			// It will be the previous GameFile that contains that file.
			// Reading that file it's expected to get file data.
			string prevContainer = gameFilePath.GetPreviousPath();
			if (!string.IsNullOrEmpty(prevContainer)) {
				GameFile dependency = this.RescueFile(prevContainer);
				if (dependency != null) {
					dependency.Format.Read();
					depends.Add(dependency);
				}
			}

			// We should be able to get the file now
			FileContainer searchFile = root.SearchFile(gameFilePath);
			GameFile file =  searchFile as GameFile;
			// If we're trying to get the dependency and found a folder, pass its the "dependency"
			if (file == null && searchFile is GameFolder) {
				if (depends.Count > 0)
					return depends[0];
				else
					return null;	// Folder without dependencies
			} else if (file == null) {
				throw new Exception("File not found.");
			}

			// 1.2.- Gets dependencies to be able to parse data.
			// It will try to guess the file type using FormatValidation classes.
			// If one of the matches, it will provide the dependencies.
			foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes(typeof(FormatValidation))) {
				FormatValidation validation = (FormatValidation)node.CreateInstance();
				validation.AutosetFormat = true;	// If it matches set format to the file.
				validation.RunTests(file);

				if (validation.Result) {
					foreach (string dependencyPath in validation.Dependencies) {
						GameFile dependency = this.RescueFile(dependencyPath);
						depends.Add(dependency);
						dependency.Format.Read();
					}
					break;
				}
			}

			// Set dependencies
			file.AddDependencies(depends.ToArray());

			return file;
		}

		private GameFile RescueFileInfo(string gameFilePath, XElement fileInfo)
		{
			// Resolve dependencies
			List<GameFile> depends = new List<GameFile>();

			foreach (XElement xmlDepend in fileInfo.Elements("DependsOn")) {
				GameFile dependency = this.RescueFile(xmlDepend.Value);
				depends.Add(dependency);
				dependency.Format.Read();
			}

			// Get file
			FileContainer searchFile = root.SearchFile(gameFilePath);
			GameFile file =  searchFile as GameFile;
			if (file == null) {
				throw new Exception("File not found.");
			}

			// Add dependencies
			file.AddDependencies(depends.ToArray());

			// Get type of dependency from info
			Type t = Type.GetType(fileInfo.Element("Type").Value, true, false);

			// Set type
			if (file.Format == null)
				file.SetFormat(t);

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

