//-----------------------------------------------------------------------
// <copyright file="DungeonList.cs" company="none">
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
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni
{
	[Extension]
	public class DungeonList : XmlExportable
	{
		private Entry[] entries;

		public override string FormatName {
			get { return "Ninokuni.DungeonList"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			ushort numBlocks = reader.ReadUInt16();
			reader.ReadUInt16();	// Total number of subblocks
			this.entries = new Entry[numBlocks];

			for (int i = 0; i < numBlocks; i++) {
				this.entries[i] = new Entry();
				this.entries[i].Title = reader.ReadString(0x40).ApplyTable("replace", false);

				ushort subBlocks = reader.ReadUInt16();
				this.entries[i].Dungeons = new Dungeon[subBlocks];
				for (int j = 0; j < subBlocks; j++)
					this.entries[i].Dungeons[j] = new Dungeon() {
						Text       = reader.ReadString(0x40).ApplyTable("replace", false),
						ScriptName = reader.ReadString(0x08)
					};
			}
		}

		public override void Write(DataStream strOut)
		{
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			int numSubBlocks = 0;
			foreach (Entry e in this.entries)
				numSubBlocks += e.Dungeons.Length;

			writer.Write((ushort)this.entries.Length);
			writer.Write((ushort)numSubBlocks);

			foreach (Entry e in this.entries) {
				writer.Write(e.Title.ApplyTable("replace", true), 0x40);
				writer.Write((ushort)e.Dungeons.Length);

				foreach (Dungeon d in e.Dungeons) {
					writer.Write(d.Text.ApplyTable("replace", true), 0x40);
					writer.Write(d.ScriptName.ApplyTable("replace", true), 0x08);
				}
			}
		}

		protected override void Import(XElement root)
		{
			List<Entry> entries = new List<Entry>();
			foreach (XElement e in root.Elements("Block")) {
				string title = e.Element("Title").Value.FromXmlString('<', '>');
				Dungeon[] dungeons = e.Elements("Message").
				                     Select<XElement, Dungeon>(
					                     xmsg => new Dungeon() { 
												Text       = xmsg.Value.FromXmlString('<', '>'),
												ScriptName = xmsg.Attribute("Script").Value
										}
					                 ).ToArray();
				entries.Add(new Entry() { Title = title, Dungeons = dungeons });
			}

			this.entries = entries.ToArray();
		}

		protected override void Export(XElement root)
		{
			foreach (Entry e in this.entries) {
				XElement block = new XElement("Block");
				block.Add(new XElement("Title", e.Title.ToXmlString(3, '<', '>')));

				foreach (Dungeon d in e.Dungeons) {
					XElement xmlDungeon = new XElement("Message");
					xmlDungeon.Value = d.Text.ToXmlString(3, '<', '>');
					xmlDungeon.SetAttributeValue("Script", d.ScriptName);
					block.Add(xmlDungeon);
				}
			}
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}

		private class Entry
		{
			public string Title {
				get;
				set;
			}

			public Dungeon[] Dungeons {
				get;
				set;
			}
		}

		private class Dungeon
		{
			public string Text {
				get;
				set;
			}

			public string ScriptName {
				get;
				set;
			}
		}
	}
}

