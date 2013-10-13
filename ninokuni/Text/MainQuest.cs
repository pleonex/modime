//-----------------------------------------------------------------------
// <copyright file="MainQuest.cs" company="none">
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
	public class MainQuest : XmlExportable
	{
		private Block[] blocks;

		public override string FormatName {
			get { return "Ninokuni.MainQuest"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			ushort numBlocks = reader.ReadUInt16();
			this.blocks = new Block[numBlocks];
			for (int i = 0; i < numBlocks; i++) {
				reader.ReadUInt16();	// Block size

				this.blocks[i]    = new Block();
				this.blocks[i].Id = reader.ReadUInt32();
				this.blocks[i].Elements = new string[3];
				for (int j = 0; j < 3; j++) {
					ushort textSize = reader.ReadUInt16();
					this.blocks[i].Elements[j] = reader.ReadString(textSize).ApplyTable("replace", false);
				}

				reader.ReadByte();	// 0x00
			}
		}

		public override void Write(DataStream strOut)
		{
			Encoding encoding = Encoding.GetEncoding("shift_jis");
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, encoding);

			writer.Write((ushort)this.blocks.Length);
			foreach (Block b in this.blocks) {
				writer.Write(this.GetBlockSize(b));
				writer.Write(b.Id);

				foreach (string t in b.Elements) {
					byte[] data = encoding.GetBytes(t.ApplyTable("replace", true));
					writer.Write((ushort)data.Length);
					writer.Write(data);
				}

				writer.Write((byte)0x00);
			}
		}

		protected override void Import(XElement root)
		{
			List<Block> blocks = new List<Block>();
			foreach (XElement xmlBlock in root.Elements("Block")) {
				Block b = new Block();
				b.Id = Convert.ToUInt32(xmlBlock.Attribute("ID").Value, 16);

				int i = 0;
				b.Elements = new string[3];
				foreach (XElement xmlElement in xmlBlock.Elements("String"))
					b.Elements[i++] = xmlElement.Value.FromXmlString(3, '<', '>');

				blocks.Add(b);
			}

			this.blocks = blocks.ToArray();
		}

		protected override void Export(XElement root)
		{
			foreach (Block b in this.blocks) {
				XElement xmlBlock = new XElement("Block");
				xmlBlock.SetAttributeValue("ID", b.Id.ToString("X"));
				xmlBlock.Add(b.Elements.Select<string, XElement>(
					s => new XElement("String", s.ToXmlString(3, '<', '>'))
				));
				root.Add(xmlBlock);
			}
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}

		private ushort GetBlockSize(Block b)
		{
			Encoding encoding = Encoding.GetEncoding("shift_jis");
			int size = 4;	// Id

			foreach (string t in b.Elements)
				size += encoding.GetByteCount(t) + 2;	// Size + text

			size += 1;	// Null byte

			return (ushort)size;
		}

		private class Block
		{
			public uint Id {
				get;
				set;
			}

			public string[] Elements {
				get;
				set;
			}
		}
	}
}

