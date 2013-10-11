//-----------------------------------------------------------------------
// <copyright file="BinaryText.cs" company="none">
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
// <date>11/10/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Text;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Common
{
	[Extension]
	public class BinaryText : Format
	{
		private DataStream data;

		public override string FormatName {
			get { return "Common.BinaryText"; }
		}

		public override void Read(DataStream strIn)
		{
			// Copy to a new DataStream to allow write operations
			// We don't want to change the original file data
			this.data = new DataStream(new System.IO.MemoryStream(), 0, 0);
			strIn.WriteTo(this.data);
		}

		public override void Write(DataStream strOut)
		{
			this.data.WriteTo(strOut);
		}

		public override void Import(params DataStream[] strIn)
		{
			// TODO: Encoding and Endianess should be specified by a parameter
			DataWriter dw = new DataWriter(this.data, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));
			XDocument xmlDoc = XDocument.Load(strIn[0].BaseStream);

			foreach (XElement xmlBlock in xmlDoc.Root.Elements()) {
				if (!xmlBlock.Name.LocalName.StartsWith("Block"))
					continue;

				foreach (XElement xmlText in xmlBlock.Elements("Text")) {
					uint   offset = Convert.ToUInt32(xmlText.Attribute("Offset").Value, 16);
					int    size   = Convert.ToInt32(xmlText.Attribute("Size").Value, 10);
					string text   = xmlText.Value.FromXmlString(3, '【', '】').ApplyTable("replace", true);

					// Write string
					try {
						this.data.Seek(offset, SeekMode.Origin);
						dw.Write(text, size);
					} catch {
						// Do nothing, don't copy the new string
					}
				}
			}
		}

		public override void Export(params DataStream[] strOut)
		{
			// Huh?... Automatic method to do it? Maybe giving address & size...
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
