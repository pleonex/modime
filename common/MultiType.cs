//-----------------------------------------------------------------------
// <copyright file="MultiType.cs" company="none">
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
// <date>09/10/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Libgame;
using Libgame.IO;
using Mono.Addins;

namespace Common
{
	[Extension]
	public class MultiType : Format
	{
		private DataStream data;

		public override string FormatName {
			get { return "Common.MultiType"; }
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
			XElement xmlImport = ((XElement)this.parameters[0]).Element("Import");

			foreach (XElement xmlType in xmlImport.Elements("Type")) {
				string name         = xmlType.Element("Name").Value;
				XElement parameters = xmlType.Element("Parameters");
				int[] streams       = xmlType.Element("ImportedStreams").Value.
				                      Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
				                      Select( idx => int.Parse(idx) ).ToArray();

				Format format = FileManager.GetFormat(name);
				format.Initialize(this.File, parameters);

				format.Read(this.data);
				format.Import(strIn.Where( (str, idx) => streams.Contains(idx) ).ToArray());

				this.data.Dispose();
				this.data = new DataStream(new System.IO.MemoryStream(), 0, 0);
				format.Write(this.data);
				format.Dispose();
			}

			this.File.Format = this;	// That changes when calling Initalize in subformats
		}

		public override void Export(params DataStream[] strOut)
		{
			// TODO: MultyType.Export
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
	}
}

