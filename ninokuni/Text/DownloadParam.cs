//-----------------------------------------------------------------------
// <copyright file="DownloadParam.cs" company="none">
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
using System.Text;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni
{
	[Extension]
	public class DownloadParam : XmlExportable
	{
		private Entry[] entries;

		public override string FormatName {
			get { return "Ninokuni.DownloadParam"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			ushort numBlock = reader.ReadUInt16();
			this.entries = new Entry[numBlock];
			for (int i = 0; i < numBlock; i++)
				this.entries[i] = new Entry(
					reader.ReadUInt16(),
					reader.ReadUInt16(),
					reader.ReadString(0x30).ApplyTable("replace", false)
				);
		}

		public override void Write(DataStream strOut)
		{
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			writer.Write((ushort)this.entries.Length);
			foreach (Entry e in this.entries) {
				writer.Write(e.Unknown1);
				writer.Write(e.Unknown2);
				writer.Write(e.Text.ApplyTable("replace", true), 0x30);
			}
		}

		protected override void Import(XElement root)
		{
			int i = 0;
			foreach (XElement e in root.Elements("String")) {
				if (i >= this.entries.Length)
					break;	// Show warning

				this.entries[i] = this.entries[i].ChangeText(e.Value.FromXmlString('<', '>'));
				i++;
			}
		}

		protected override void Export(XElement root)
		{
			foreach (Entry e in this.entries)
				root.Add(new XElement("String", e.Text.ToXmlString(2, '<', '>')));
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}

		private struct Entry
		{
			public Entry(ushort unk1, ushort unk2, string text)
				: this()
			{
				this.Unknown1 = unk1;
				this.Unknown2 = unk2;
				this.Text     = text;
			}

			public ushort Unknown1 {
				get;
				private set;
			}

			// ID?
			public ushort Unknown2 {
				get;
				private set;
			}      

			// 0x30 bytes
			public string Text {
				get;
				private set;
			} 

			public Entry ChangeText(string newText)
			{
				return new Entry(this.Unknown1, this.Unknown2, newText);
			}
		}
	}
}

