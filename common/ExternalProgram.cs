//-----------------------------------------------------------------------
// <copyright file="ExternalProgram.cs" company="none">
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
// <date>22/09/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Common
{
	[Extension]
	public class ExternalProgram : Format
	{
		DataStream data;

		public override string FormatName {
			get { return "Common.ExternalProgram"; }
		}

		public override void Read(DataStream strIn)
		{
			data = new DataStream(strIn, 0, strIn.Length);
		}

		public override void Write(DataStream strOut)
		{
			data.WriteTo(strOut);
		}

		public override void Import(params DataStream[] strIn)
		{
			Configuration config = Configuration.GetInstance();
			XElement xmlImport = ((XElement)parameters[0]).Element("Import");

			string inUnixRunOn = xmlImport.Element("InUnixRunOn").Value;
			string arguments   = config.ResolvePathInVariable(xmlImport.Element("Arguments").Value);
			string programPath = config.ResolvePath(xmlImport.Element("Path").Value);
			string copyTo      = config.ResolvePath(xmlImport.Element("CopyTo").Value);
			string outputPath  = config.ResolvePath(xmlImport.Element("OutputFile").Value);
			bool autoremoveOut = bool.Parse(xmlImport.Element("OutputFile").Attribute("autoremove").Value);
			bool autoremoveCpy = bool.Parse(xmlImport.Element("CopyTo").Attribute("autoremove").Value);
			XElement envVars   = xmlImport.Element("EnvironmentVariables");

			// Resolve variables
			var tempFiles = new string[strIn.Length];
			arguments = ResolveVariables(arguments, tempFiles, strIn);
			if (copyTo != "$stdIn")
				copyTo = ResolveVariables(copyTo, tempFiles, strIn);
			if (outputPath != "$stdOut")
				outputPath = ResolveVariables(outputPath, tempFiles, strIn);
			
			// Write the data stream to a temp file
			if (copyTo != "$stdIn" && !string.IsNullOrEmpty(copyTo))
				data.WriteTo(copyTo);

			if (config.OsName == "Unix" && !string.IsNullOrEmpty(inUnixRunOn)) {
				arguments = string.Format("\"{0}\" {1}", programPath, arguments);
				programPath = inUnixRunOn;
			}

			// Call to the program
			var startInfo = new ProcessStartInfo();
			startInfo.FileName        = programPath;
			startInfo.Arguments       = arguments;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow  = true;
			startInfo.ErrorDialog     = false;
			startInfo.RedirectStandardInput  = (copyTo == "$stdIn");
			startInfo.RedirectStandardOutput = true;

			// Set environmet variables
			if (envVars != null) {
				foreach (XElement variable in envVars.Elements("Variable"))
					startInfo.EnvironmentVariables.Add(
						variable.Element("Name").Value,
						variable.Element("Value").Value
					);
			}

			var program = new Process();
			program.StartInfo = startInfo;
			program.Start();

			if (copyTo == "$stdIn") {
				data.BaseStream.Seek(data.Offset, System.IO.SeekOrigin.Begin);
				data.BaseStream.CopyTo(program.StandardInput.BaseStream);
				program.StandardInput.Dispose();
			}

			if (data != null)
				data.Dispose();

			if (outputPath == "$stdOut") {
				var ms = new System.IO.MemoryStream();
				program.StandardOutput.BaseStream.CopyTo(ms);
				data = new DataStream(ms, 0, ms.Length);
			}

			program.WaitForExit();
			program.Dispose();

			// Read the data from the output file
			if (outputPath != "$stdOut") {
				var fileStream = new DataStream(
					outputPath,
					System.IO.FileMode.Open,
					System.IO.FileAccess.Read,
					System.IO.FileShare.ReadWrite
				);
				data = new DataStream(new System.IO.MemoryStream(), 0, 0);

				fileStream.WriteTo(data);
				fileStream.Dispose();
			}

			// Remove temp files
			for (int i = 0; i < tempFiles.Length && !UseFilePaths; i++)
				if (!string.IsNullOrEmpty(tempFiles[i]))
					System.IO.File.Delete(tempFiles[i]);

			if (autoremoveCpy && System.IO.File.Exists(copyTo))
				System.IO.File.Delete(copyTo);

			if (autoremoveOut && System.IO.File.Exists(outputPath))
				System.IO.File.Delete(outputPath);
		}

		public override void Export(params DataStream[] strOut)
		{
			// TODO: ExternalProgram.Export
			throw new NotImplementedException();
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			if (freeManagedResourcesAlso) {
				if (data != null) {
					data.Dispose();
					data = null;
				}
			}
		}

		bool UseFilePaths {
			get {
				var element = ((XElement)parameters[0]).Element("Import");
				return element.Element("UseFilePaths") != null && 
					element.Element("UseFilePaths").Value == "true";
			}
		}

		string ResolveVariables(string s, string[] tempFiles, DataStream[] strIn)
		{
			const string pattern = @"\$([a-z]+)(\d+)(?::([a-z]+))?";
			MatchCollection matches = Regex.Matches(s, pattern);
			var snew = new StringBuilder(s);


			foreach (Match m in matches) {
				// Get result and captures
				string result = m.Value;
				string variable = m.Groups[1].Value;
				int num = int.Parse(m.Groups[2].Value);
				string tile = (m.Groups.Count == 4) ? m.Groups[3].Value : string.Empty;

				string replace = string.Empty;

				if (variable == "import") {
					// If the file path has not been assigned yet
					if (string.IsNullOrEmpty(tempFiles[num])) {
						// Save files into temp files
						if (!UseFilePaths) {
							tempFiles[num] = System.IO.Path.GetTempFileName();
							strIn[num].WriteTo(tempFiles[num]);
						// Use the imported file paths
						} else {
							tempFiles[num] = ImportedPaths[num];
						}
					}

					string path = tempFiles[num];
					if (string.IsNullOrEmpty(tile))
						replace = path;
					else if (tile == "path")
						replace = System.IO.Path.GetDirectoryName(path);
					else if (tile == "name")
						replace = System.IO.Path.GetFileName(path);
				} else {
					throw new FormatException("Unknown variable");
				}

				if (string.IsNullOrEmpty(replace))
					throw new FormatException("Unknown tile");
			
				int index = snew.ToString().IndexOf(result, StringComparison.Ordinal);
				snew.Replace(result, replace, index, m.Length);	// Replace only current match
			}

			return snew.ToString();
		}
	}
}

