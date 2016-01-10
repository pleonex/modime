//-----------------------------------------------------------------------
// <copyright file="Script.cs" company="none">
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
// <date>12/10/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni.Script
{
	[Extension]
	/// <summary>
	/// Proxy class. Until the Spanish translation is finished
	/// the code of the script editor won't be released
	/// so I'm using this class to connect with the script editor
	/// </summary>
	public class Script : Format
	{
		private NinoScritor.Script script;
		private string scriptName;

		public override string FormatName {
			get { return "Ninokuni.Script"; }
		}

		public override void Initialize(GameFile file, params object[] parameters)
		{
			this.scriptName = ((XElement)parameters[0]).Value;
			base.Initialize(file, parameters);
		}

		public override void Read(DataStream strIn)
		{
			DataStream temp = new DataStream(new System.IO.MemoryStream(), 0, 0);
			strIn.WriteTo(temp);
			temp.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

			Configuration config = Configuration.GetInstance();

			script = new NinoScritor.Script(
				temp.BaseStream,
				scriptName,
				System.IO.Path.Combine(config.AppPath, "temp_names.xml"),
				System.IO.Path.Combine(config.AppPath, "temp_context.xml"),
				System.IO.Path.Combine(config.AppPath, "temp_replace.xml"),
				""
			);

			temp.Dispose();

            // Add dummy file to be able to trigger post-xml parser like Spreadsheets.
            File.AddFile(new GameFile(
                "script",
                new DataStream(new System.IO.MemoryStream(), 0, 0)));
		}

		public override void Write(DataStream strOut)
		{
			script.Write(strOut.BaseStream);
			strOut.SetLength(strOut.BaseStream.Length);
		}

		public override void Import(params DataStream[] strIn)
		{
			script.ImportXML(strIn[0].BaseStream);
		}

		public override void Export(params DataStream[] strOut)
		{
			script.ExportXML(strOut[0].BaseStream);
			strOut[0].SetLength(strOut[0].BaseStream.Length);
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			// Nothing to free as far I know
		}
	}
}

