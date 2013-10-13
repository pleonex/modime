//-----------------------------------------------------------------------
// <copyright file="System.cs" company="none">
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
// <date>13/10/2013</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni
{
	[Extension]
	public class SystemText : XmlExportable
	{
		private Entry[] entries;

		public override string FormatName {
			get { return "Ninokuni.System"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			ushort numEntries = reader.ReadUInt16();
			this.entries = new Entry[numEntries];
			for (int i = 0; i < numEntries; i++) {
				this.entries[i] = new Entry();
				this.entries[i].Id   = reader.ReadUInt32();
				this.entries[i].Text = reader.ReadString(typeof(ushort), "replace", false);
			}
		}

		public override void Write(DataStream strOut)
		{
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			writer.Write((ushort)this.entries.Length);
			foreach (Entry e in this.entries) {
				writer.Write(e.Id);
				writer.Write(e.Text, typeof(ushort), "replace", true);
			}
		}

		protected override void Import(XElement root)
		{
			List<Entry> entries = new List<Entry>();
			foreach (XElement e in root.Elements("String"))
				entries.Add(new Entry() {
					Id = Convert.ToUInt32(e.Attribute("ID").Value, 16),
					Text = e.Value.FromXmlString(2, '<', '>')
				});

			this.entries = entries.ToArray();
		}

		protected override void Export(XElement root)
		{
			foreach (Entry e in this.entries) {
				XElement xmlEntry = new XElement("String");
				xmlEntry.SetAttributeValue("ID", e.Id.ToString("X"));
				xmlEntry.Value = e.Text.ToXmlString(2, '<', '>');
				root.Add(xmlEntry);
			}
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}

		private class Entry
		{
			public uint Id {
				get;
				set;
			}

			public string Text {
				get;
				set;
			}
		}
	}
}

