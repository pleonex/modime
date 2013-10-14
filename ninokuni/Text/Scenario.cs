//-----------------------------------------------------------------------
// <copyright file="Scenario.cs" company="none">
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
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;


namespace Ninokuni
{
	[Extension]
	public class Scenario : XmlExportable
	{
		private uint type;
		private Block[] blocks;

		public override string FormatName {
			get { return "Ninokuni.Scenario"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn);
			this.type = reader.ReadUInt32();

			this.blocks = new Block[3];
			this.blocks[0] = Block.FromStream(strIn, typeof(Entry1));
			this.blocks[1] = Block.FromStream(strIn, typeof(Entry2));
			this.blocks[2] = Block.FromStream(strIn, typeof(Entry3));
		}

		public override void Write(DataStream strOut)
		{
			DataWriter writer = new DataWriter(strOut);
			writer.Write(this.type);

			foreach (Block b in this.blocks)
				b.Write(strOut);
		}

		protected override void Import(XElement root)
		{
			int i = 0;
			foreach (XElement e in root.Elements("String")) {
				if (i >= this.blocks[0].Entries.Count)
					break;	// Show warning

				this.blocks[0].Entries[i].Id = Convert.ToUInt32(e.Attribute("ID").Value, 16);
				((Entry1)this.blocks[0].Entries[i]).Text = e.Value.FromXmlString('<', '>');
				i++;
			}
		}

		protected override void Export(XElement root)
		{
			foreach (Entry1 e in this.blocks[0].Entries) {
				XElement xmlEntry = new XElement("String");
				xmlEntry.Value = e.Text.ToXmlString(2, '<', '>');
				xmlEntry.SetAttributeValue("ID", e.Id.ToString("X"));
				root.Add(xmlEntry);
			}
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}

		private class Block
		{
			public List<Entry> Entries {
				get;
				private set;
			}

			public static Block FromStream(DataStream stream, Type entryType)
			{
				Block b = new Block();
				DataReader reader = new DataReader(stream);
				reader.ReadUInt32();	// Block size

				b.Entries = new List<Entry>();
				while (true) {
					uint id = reader.ReadUInt32();
					if (id == 0xFFFFFFFF)
						break;
					else
						stream.Seek(-0x04, SeekMode.Current);

					Entry entry = (Entry)Activator.CreateInstance(entryType);
					entry.Read(stream);
					b.Entries.Add(entry);
				}

				return b;
			}

			public void Write(DataStream stream)
			{
				DataWriter writer = new DataWriter(stream);
				writer.Write(this.GetSize());

				foreach (Entry e in this.Entries)
					e.Write(stream);

				writer.Write(0xFFFFFFFF);
			}

			private uint GetSize()
			{
				uint size = 4;	// Final null 32bit value
				foreach (Entry e in this.Entries)
					size += e.Size;
				return size;
			}
		}

		private class Entry
		{
			public virtual uint Size {
				get { return 4; }
			}
			
			public uint Id {
				get;
				set;
			}

			public virtual void Read(DataStream stream)
			{
				DataReader reader = new DataReader(stream);
				this.Id = reader.ReadUInt32();
			}

			public virtual void Write(DataStream stream)
			{
				DataWriter writer = new DataWriter(stream);
				writer.Write(this.Id);
			}
		}

		private class Entry1 : Entry
		{
			public string Text {
				get;
				set;
			}

			public override uint Size {
				get {
					return base.Size + 1 +
						(uint)Encoding.GetEncoding("shift_jis").GetByteCount(
							this.Text.ApplyTable("replace", true));
				}
			}

			public override void Read(DataStream stream)
			{
				base.Read(stream);

				DataReader reader = new DataReader(
					                    stream,
					                    EndiannessMode.LittleEndian,
					                    Encoding.GetEncoding("shift_jis"));

				byte textSize = reader.ReadByte();
				this.Text = reader.ReadString(textSize).ApplyTable("replace", false);
				this.Text = GetFurigana(this.Text);
			}

			public override void Write(DataStream stream)
			{
				base.Write(stream);
				Encoding encoding = Encoding.GetEncoding("shift_jis");
				DataWriter writer = new DataWriter(
					                    stream,
					                    EndiannessMode.LittleEndian,
					                    encoding);

				string text = SetFurigana(this.Text);
				byte[] data = encoding.GetBytes(text.ApplyTable("replace", true));
				writer.Write((byte)data.Length);
				writer.Write(data);
			}

			private static string GetFurigana(string text)
			{
				StringBuilder s = new StringBuilder(text);
				bool startFurigana = false;

				for (int i = 0; i < s.Length; i++) {
					if (s[i] == '\x01') {
						s[i] = '<';
						startFurigana = true;
					} else if (s[i] == '\n' && startFurigana) {
						s[i] = ':';
						startFurigana = false;
					} else if (s[i] == '\n') {
						s[i] = '>';
					}
				}

				return s.ToString();
			}

			private static string SetFurigana(string text)
			{
				StringBuilder s = new StringBuilder(text);
				s.Replace('<', '\x01');
				s.Replace(':', '\n');
				s.Replace('>', '\n');

				return s.ToString();
			}
		}

		private class Entry2 : Entry
		{
			public string Text {
				get;
				set;
			}

			public ushort Unknown {
				get;
				set;
			}

			public override uint Size {
				get {
					return base.Size + 1 + 2 +
						(uint)Encoding.GetEncoding("shift_jis").GetByteCount(this.Text);
				}
			}

			public override void Read(DataStream stream)
			{
				base.Read(stream);

				DataReader reader = new DataReader(
					stream,
					EndiannessMode.LittleEndian,
					Encoding.GetEncoding("shift_jis"));

				byte textSize = reader.ReadByte();
				this.Text = reader.ReadString(textSize);
				this.Unknown = reader.ReadUInt16();
			}

			public override void Write(DataStream stream)
			{
				base.Write(stream);
				Encoding encoding = Encoding.GetEncoding("shift_jis");
				DataWriter writer = new DataWriter(
					                    stream,
					                    EndiannessMode.LittleEndian,
					                    encoding);
				byte[] data = encoding.GetBytes(this.Text);
				writer.Write((byte)data.Length);
				writer.Write(data);
				writer.Write(this.Unknown);
			}
		}

		private class Entry3 : Entry
		{
			public ushort Unknown {
				get;
				set;
			}

			public string Text {
				get;
				set;
			}

			public ushort Unknown2 {
				get;
				set;
			}

			public override uint Size {
				get {
					return base.Size + 1 + 4 +
						(uint)Encoding.GetEncoding("shift_jis").GetByteCount(this.Text);
				}
			}

			public override void Read(DataStream stream)
			{
				base.Read(stream);

				DataReader reader = new DataReader(
					stream,
					EndiannessMode.LittleEndian,
					Encoding.GetEncoding("shift_jis"));

				byte entrySize = reader.ReadByte();
				this.Unknown = reader.ReadUInt16();
				this.Text = reader.ReadString(entrySize - 4);
				this.Unknown2 = reader.ReadUInt16();
			}

			public override void Write(DataStream stream)
			{
				base.Write(stream);
				Encoding encoding = Encoding.GetEncoding("shift_jis");
				DataWriter writer = new DataWriter(
					                    stream,
					                    EndiannessMode.LittleEndian,
					                    encoding);
				byte[] data = encoding.GetBytes(this.Text);
				writer.Write(this.Unknown);
				writer.Write((byte)(data.Length + 4));
				writer.Write(data);
				writer.Write(this.Unknown2);
			}
		}
	}
}
