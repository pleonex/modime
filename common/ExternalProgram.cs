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
		private DataStream data;

		public override string FormatName {
			get { return "Common.ExternalProgram"; }
		}

		public override void Read(DataStream strIn)
		{
			this.data = new DataStream(strIn, strIn.Offset, strIn.Length);
		}

		public override void Write(DataStream strOut)
		{
			this.data.WriteTo(strOut);
		}

		public override void Import(params DataStream[] strIn)
		{
			Configuration config = Configuration.GetInstance();
			XElement xmlImport = ((XElement)parameters[0]).Element("Import");

			string inUnixRunOn = xmlImport.Element("InUnixRunOn").Value;
			string arguments   = xmlImport.Element("Arguments").Value;
			string programPath = config.ResolvePath(xmlImport.Element("Path").Value);
			string copyTo      = config.ResolvePath(xmlImport.Element("CopyTo").Value);
			string outputPath  = config.ResolvePath(xmlImport.Element("OutputFile").Value);
			bool autoremoveOut = bool.Parse(xmlImport.Element("OutputFile").Attribute("autoremove").Value);
			bool autoremoveCpy = bool.Parse(xmlImport.Element("CopyTo").Attribute("autoremove").Value);

			// Resolve variables
			string[] tempFiles = new string[strIn.Length];
			arguments = this.ResolveVariables(arguments, tempFiles);
			if (copyTo != "$stdIn")
				copyTo = this.ResolveVariables(copyTo, tempFiles);
			if (outputPath != "$stdOut")
				outputPath = this.ResolveVariables(outputPath, tempFiles);

			// Copy the import file data to temp files
			for (int i = 0; i < tempFiles.Length; i++)
				if (!string.IsNullOrEmpty(tempFiles[i]))
					strIn[i].WriteTo(tempFiles[i]);

			// Write the data stream to a temp file
			if (copyTo != "$stdIn" && !string.IsNullOrEmpty(copyTo))
				this.data.WriteTo(copyTo);

			if (config.OsName == "Unix" && !string.IsNullOrEmpty(inUnixRunOn)) {
				arguments = programPath + " " + arguments;	// The program now is one of the arguments
				programPath = inUnixRunOn;
			}

			// Call to the program
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName        = programPath;
			startInfo.Arguments       = arguments;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow  = true;
			startInfo.ErrorDialog     = false;
			startInfo.RedirectStandardInput  = (copyTo == "$stdIn") ? true : false;
			startInfo.RedirectStandardOutput = (outputPath == "$stdOut") ? true : false;

			Process program = new Process();
			program.StartInfo = startInfo;
			program.Start();

			if (copyTo == "$stdIn") {
				this.data.BaseStream.Seek(this.data.Offset, System.IO.SeekOrigin.Begin);
				this.data.BaseStream.CopyTo(program.StandardInput.BaseStream);
				program.StandardInput.Close();
			}

			if (this.data != null)
				this.data.Dispose();

			if (outputPath == "$stdOut") {
				System.IO.MemoryStream ms = new System.IO.MemoryStream();
				program.StandardOutput.BaseStream.CopyTo(ms);
				this.data = new DataStream(ms, 0, ms.Length);
			}

			program.WaitForExit();
			program.Close();

			// Read the data from the output file
			if (outputPath != "$stdOut")
				this.data = new DataStream(outputPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

			// Remove temp files
			for (int i = 0; i < tempFiles.Length; i++)
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
				if (this.data != null) {
					this.data.Dispose();
					this.data = null;
				}
			}
		}

		private string ResolveVariables(string s, string[] tempFiles)
		{
			string pattern = @"\$([a-z]+)(\d+)(?::([a-z]+))?";
			MatchCollection matches = Regex.Matches(s, pattern);
			StringBuilder snew = new StringBuilder(s);


			foreach (Match m in matches) {
				// Get result and captures
				string result = m.Value;
				string variable = m.Groups[1].Value;
				int num = int.Parse(m.Groups[2].Value);
				string tile = (m.Groups.Count == 4) ? m.Groups[3].Value : string.Empty;

				string replace = string.Empty;

				if (variable == "import") {
					if (string.IsNullOrEmpty(tempFiles[num]))
						tempFiles[num] = System.IO.Path.GetTempFileName();

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
			
				int index = snew.ToString().IndexOf(result);
				snew.Replace(result, replace, index, m.Length);	// Replace only current match
			}

			return snew.ToString();
		}
	}
}

