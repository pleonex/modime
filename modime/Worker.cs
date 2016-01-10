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
using Libgame.Utils;

namespace Modime
{
	public class Worker
	{
		private readonly XDocument edit;
		private readonly FileManager fileManager;
		private readonly Configuration config;
		private readonly List<string> updateQueue;

		public Worker(XDocument xmlGame, XDocument xmlEdit, FileContainer root)
		{
			Configuration.Initialize(xmlEdit);
			this.config = Configuration.GetInstance();

			this.edit = xmlEdit;
			this.updateQueue = new List<string>();

			InitializeFileTypes(root, xmlGame);
			this.fileManager = FileManager.GetInstance();
		}

		public Worker(string xmlGame, string xmlEdit, FileContainer root)
			: this(XDocument.Load(xmlGame), XDocument.Load(xmlEdit), root)
		{
		}

		public bool Export()
		{
			XElement files = edit.Root.Element("Files");

			ConsoleCount count = new ConsoleCount(
				"Exporting file {0:0000} of {1:0000}",
				files.Elements("File").Count()
			);

			foreach (XElement fileEdit in files.Elements("File")) {
				count.Show();
				string path = fileEdit.Element("Path").Value;
				IList<string> exportPaths = fileEdit.Elements("Import")
													.Select(f => this.config.ResolvePath(f.Value))
													.ToList();

				foreach (string p in exportPaths.Select(f => System.IO.Path.GetDirectoryName(f)))
					if (!System.IO.Directory.Exists(p))
						System.IO.Directory.CreateDirectory(p);

				if (exportPaths.Any()) {
					GameFile file = fileManager.RescueFile(path);
					file.Format.Read();
					try {
						file.Format.Export(exportPaths.ToArray());
					} catch (Exception ex) {
						//Console.WriteLine("Can not export {0}", path);
						//Console.WriteLine(ex.Message);
						if (!(ex is NotImplementedException) && !(ex is NotSupportedException))
							return false;
					}
				}

				count.UpdateCoordinates();
			}

			return true;
		}

		public bool Import(Func<string, bool> importFilter)
		{
			XElement files = edit.Root.Element("Files");

			System.IO.StreamWriter skipFiles = null;
			ConsoleCount count = new ConsoleCount(
				"Importing file {0:0000} of {1:0000}",
				files.Elements().Count()
			);

			foreach (XElement fileEdit in files.Elements()) {
				count.Show();
				bool isVirtual = (fileEdit.Name == "VirtualFile");
				string path = fileEdit.Element("Path")?.Value ?? "<VirtualFile>";
				IList<string> import = fileEdit.Elements("Import")
											.Select(f => this.config.ResolvePath(f.Value))
											.ToList();
				bool internalFilter = Convert.ToBoolean(fileEdit.Element("InternalFilter")?.Value ?? "false");

				bool validFile = (!internalFilter || import.Any(importFilter));
				if (import.All(System.IO.File.Exists) && validFile) {
					try {
						if (!isVirtual) {
							GameFile file = fileManager.RescueFile(path);
							file.Format.Read();
							file.Format.Import(import.ToArray());
							this.UpdateQueue(file);
						} else {
							// It's a virtual file, just a layer
							// Just create the format and import.
							string type = fileEdit.Element("Type").Value;
							Format format = FileManager.GetFormat(type);
							format.Initialize(null, fileEdit.Element("Parameters"));
							format.Import(import.ToArray());
						}
					} catch (Exception ex) {
						Console.WriteLine("ERROR with {0}, {1}", path, import.FirstOrDefault());
						Console.WriteLine(ex);
						return false;
					}
				} else {
					if (skipFiles == null)
						skipFiles = new System.IO.StreamWriter("skipped.txt", false);

					foreach (string f in import)
						skipFiles.WriteLine(f);
					skipFiles.WriteLine();
				}

				count.UpdateCoordinates();
			}

			if (skipFiles != null)
				skipFiles.Close();

			return true;
		}

		public bool Import()
		{
			// Import every file
			return this.Import(f => true);
		}

		public bool Import(DateTime importFrom)
		{
			// Import only if it has been modified from date specified
			config.Extras["modime_dateFilter"] = importFrom;
			return this.Import(f => System.IO.File.GetLastWriteTime(f) > importFrom);
		}

		public void Write(params string[] outputPath)
		{
			// Check if we can write to the output paths
			GameFile rootFile = fileManager.Root as GameFile;
			if (rootFile != null && outputPath.Length != 1)
				throw new ArgumentException("Only ONE file need to be written");
			else if (rootFile == null && fileManager.Root.Files.Count != outputPath.Length)
				throw new ArgumentException("There are not enough output paths.");

			// Write files data to memory buffers
			ConsoleCount count = new ConsoleCount(
				"Writing internal file {0:0000} of {1:0000}", this.updateQueue.Count);

			bool outWritten = false;
			foreach (string filePath in this.updateQueue) {
				count.Show();

				// Get file to write
				GameFile file = this.fileManager.Root.SearchFile(filePath) as GameFile;
				if (file == null)
					throw new Exception("File " + filePath + " not found.");

				// Check if this is the output file to write
				// In this case, we will write directly to the output file
				if (file == rootFile) {
					file.Format.Write(outputPath[0]);
					outWritten = true;
					count.UpdateCoordinates();
					continue;
				}

				// TODO: Do the same in the case of more than one output files.

				file.Format.Write();
				count.UpdateCoordinates();
			}

			this.updateQueue.Clear();

			// Write to output files
			count = new ConsoleCount("Writing external file {0:0000} of {1:0000}", outputPath.Length);
			if (rootFile != null) {
				count.Show();
				if (!outWritten)
					rootFile.Stream.WriteTo(outputPath[0]);
			} else {
				for (int i = 0; i < fileManager.Root.Files.Count; i++) {
					GameFile f = fileManager.Root.Files[i] as GameFile;
					if (f != null) {
						count.Show();
						f.Stream.WriteTo(outputPath[i]);
						count.UpdateCoordinates();
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

		private void InitializeFileTypes(FileContainer root, XDocument xmlGame)
		{
			FileInfoCollection fileInfo = FileInfoCollection.FromXml(xmlGame);

			// Add the parameters to each file format.
			// They can be used to add specific properties of the project to the formats parsers.
			// E.g.: to pass the key of the GoogleSpreadsheets to parse.
			foreach (XElement fileEdit in edit.Root.Element("Files").Elements("File")) {
				// In order to set parameters the type must be defined in the XML. Otherwise it will defined in run-time
				string filePath = fileEdit.Element("Path").Value;
				if (!fileInfo.Contains(filePath))
					continue;

				FileInfo info = fileInfo[filePath];
				if (info.Parameters == null)
					info.Parameters = fileEdit.Element("Parameters");
				else
					info.Parameters.Add(fileEdit.Element("Parameters")?.Elements());
			}

			FileManager.Initialize(root, fileInfo);
		}
	}
}

