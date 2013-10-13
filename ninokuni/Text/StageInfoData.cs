//-----------------------------------------------------------------------
// <copyright file="StageInfoData.cs" company="none">
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
	public class StageInfoData : XmlExportable
	{
		private Entry[] entries;

		public override string FormatName {
			get { return "Ninokuni.Comment_StageInfoData"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			uint numBlocks = reader.ReadUInt32();
			this.entries = new Entry[numBlocks];
			for (int i = 0; i < numBlocks; i++) {
				uint idx     = reader.ReadUInt32();
				int textSize = reader.ReadInt32();
				string text  = reader.ReadString(textSize);
				this.entries[i] = new Entry(idx, text.ApplyTable("replace", false));
			}
		}

		public override void Write(DataStream strOut)
		{
			Encoding encoding = Encoding.GetEncoding("shift_jis");
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, encoding);

			writer.Write((uint)this.entries.Length);
			foreach (Entry e in this.entries) {
				string text = e.Text.ApplyTable("replace", true);
				byte[] data = encoding.GetBytes(text);

				writer.Write(e.Index);
				writer.Write((uint)data.Length);
				writer.Write(data);
			}
		}

		protected override void Import(XElement root)
		{
			int i = 0;
			foreach (XElement e in root.Elements("String")) {
				if (i >= this.entries.Length)
					break;	// Throw warning

				this.entries[i] = this.entries[i].ChangeText(e.Value.FromXmlString(2, '<', '>'));
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
			public Entry(uint idx, string text)
				: this()
			{
				this.Index = idx;
				this.Text  = text;
			}

			public uint Index {
				get;
				private set;
			}

			public string Text {
				get;
				private set;
			}

			public Entry ChangeText(string newText)
			{
				return new Entry(this.Index, newText);
			}
		}
	}
}

