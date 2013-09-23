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
			this.data.Dispose();

			// Call the program in the parameter using the data in strIn
			Configuration config = Configuration.GetInstance();
			string outputPath  = config.ResolvePath((string)parameters[0]);
			string programPath = config.ResolvePath((string)parameters[1]);
			string arguments   = (string)parameters[2];

			// Write the data stream to a temp file
			string tempFile = System.IO.Path.GetTempFileName();
			strIn[0].WriteTo(tempFile);

			// Call to the program
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.Arguments = arguments;
			startInfo.CreateNoWindow = true;
			startInfo.FileName = programPath;
			startInfo.UseShellExecute = false;

			Process program = new Process();
			program.StartInfo = startInfo;
			program.Start();
			program.WaitForExit();

			System.IO.File.Delete(tempFile);

			// Read the data from the output file
			this.data = new DataStream(outputPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		}

		public override void Export(params DataStream[] strOut)
		{
			this.data.WriteTo(strOut[0]);
		}
	}
}

