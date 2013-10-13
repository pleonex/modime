//-----------------------------------------------------------------------
// <copyright file="SubQuest.cs" company="none">
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
	public class SubQuest : XmlExportable
	{
		private uint     id;
		private string[] startBlocks;
		private byte[]   unknown;
		private string[] endBlocks;
		private byte[][] unknown2;

		public override string FormatName {
			get { return "Ninokuni.SubQuest"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));
			this.id = reader.ReadUInt32();

			this.startBlocks = new string[4];
			for (int i = 0; i < this.startBlocks.Length; i++) {
				ushort textSize = reader.ReadUInt16();
				this.startBlocks[i] = reader.ReadString(textSize).ApplyTable("replace", false);
			}

			this.unknown = reader.ReadBytes(0x0D);

			byte numEndBlocks = reader.ReadByte();
			this.endBlocks = new string[numEndBlocks];
			for (int i = 0; i < this.endBlocks.Length; i++) {
				ushort size = reader.ReadUInt16();
				this.endBlocks[i] = reader.ReadString(size).ApplyTable("replace", false);
			}

			byte numUnknown = reader.ReadByte();
			this.unknown2 = new byte[numUnknown][];
			for (int i = 0; i < this.unknown2.Length; i++) {
				byte dataSize = reader.ReadByte();
				this.unknown2[i] = reader.ReadBytes(dataSize + 4);
			}
		}

		public override void Write(DataStream strOut)
		{
			Encoding encoding = Encoding.GetEncoding("shift_jis");
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, encoding);
			writer.Write(this.id);

			foreach (string s in this.startBlocks) {
				byte[] data = encoding.GetBytes(s.ApplyTable("replace", true));
				writer.Write((ushort)data.Length);
				writer.Write(data);
			}

			writer.Write(this.unknown);

			writer.Write((byte)this.endBlocks.Length);
			foreach (string s in this.endBlocks) {
				byte[] data = encoding.GetBytes(s.ApplyTable("replace", true));
				writer.Write((ushort)data.Length);
				writer.Write(data);
			}

			writer.Write((byte)this.unknown2.Length);
			foreach (byte[] unk in this.unknown2) {
				writer.Write((byte)(unk.Length - 4));
				writer.Write(unk);
			}
		}

		protected override void Import(XElement root)
		{
			int i = 0;
			XElement xmlStartBlocks = root.Element("StartBlocks");
			foreach (XElement e in xmlStartBlocks.Elements("String"))
				this.startBlocks[i++] = e.Value.FromXmlString(3, '<', '>');

			i = 0;
			XElement xmlEndBlocks = root.Element("FinalBlocks");
			foreach (XElement e in xmlEndBlocks.Elements("String"))
				this.endBlocks[i++] = e.Value.FromXmlString(3, '<', '>');
		}

		protected override void Export(XElement root)
		{
			XElement xmlStartBlocks = new XElement("StartBlocks");
			root.Add(xmlStartBlocks);
			foreach (string s in this.startBlocks)
				xmlStartBlocks.Add(new XElement("String", s.ToXmlString(3, '<', '>')));

			XElement xmlEndBlocks = new XElement("FinalBlocks");
			root.Add(xmlEndBlocks);
			foreach (string s in this.endBlocks)
				xmlEndBlocks.Add(new XElement("String", s.ToXmlString(3, '<', '>')));
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
		}
	}
}

