//-----------------------------------------------------------------------
// <copyright file="TextBlock.cs" company="none">
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
// <date>12/10/2013</date>
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
	public class TextBlock : XmlExportable
	{
		private Block[] blocks;
		private bool    hasNumBlock;
		private bool    isEncoded;
		private bool    wOriginal;
		private int     textSize;
		private int     dataSize;
		private string  fileName;

		public override string FormatName {
			get { return string.Format("Ninokuni.{0}", this.fileName); }
		}

		public override void Initialize(GameFile file, params object[] parameters)
		{
			base.Initialize(file, parameters);

			this.hasNumBlock = (bool)parameters[0];
			this.isEncoded   = (bool)parameters[1];
			this.wOriginal   = (bool)parameters[2];
			this.textSize    = (int)parameters[3];
			this.dataSize    = (int)parameters[4];
			this.fileName    = (string)parameters[5];
		}

		public override void Read(DataStream strIn)
		{
			DataStream data = null;

			// Simple NOT encoding
			if (this.isEncoded) {
				data = new DataStream(new System.IO.MemoryStream(), 0, 0);
				Codec(strIn, data);	// Decode strIn to data
			} else {
				data = strIn;
			}

			data.Seek(0, SeekMode.Origin);
			DataReader reader = new DataReader(data, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			int blockSize = this.textSize + this.dataSize;
			int numBlocks = this.hasNumBlock ? reader.ReadUInt16() : (int)(data.Length / blockSize);

			this.blocks = new Block[numBlocks];
			for (int i = 0; i < numBlocks; i++)
				this.blocks[i] = new Block(
					reader.ReadString(this.textSize).ApplyTable("replace", false),
					reader.ReadBytes(this.dataSize)
				);

			// Free the decoded data
			if (this.isEncoded)
				data.Dispose();
		}

		public override void Write(DataStream strOut)
		{
			DataStream data   = new DataStream(new System.IO.MemoryStream(), 0, 0);
			DataWriter writer = new DataWriter(data, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			if (this.hasNumBlock)
				writer.Write((ushort)this.blocks.Length);

			foreach (Block b in this.blocks) {
				if (this.fileName == "MagicParam")
					writer.Write(b.Text.ApplyTable("replace", true), this.textSize + 8);
				else
					writer.Write(b.Text.ApplyTable("replace", true), this.textSize);
				writer.Write(b.Data);
			}

			if (this.isEncoded) {
				data.Seek(0, SeekMode.Origin);
				Codec(data, strOut);	// Encode data to strOut
			} else {
				data.WriteTo(strOut);
			}
		}

		protected override void Import(XElement root)
		{
			int idx = 0;
			foreach (XElement e in root.Elements("String")) {
				// We can't create new blocks since we don't know the binary data
				if (idx >= this.blocks.Length)
					break;	// Show warning

				this.blocks[idx++].Text = e.Value.FromXmlString('<', '>');
			}
		}

		protected override void Export(XElement root)
		{
			foreach (Block b in this.blocks) {
				XElement e = new XElement("String", b.Text.ToXmlString(2, '<', '>'));
				if (this.wOriginal && !b.Text.Contains("\n"))
					e.SetAttributeValue("Original", b.Text);
				root.Add(e);
			}
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			// Nothing to do
		}

		private static void Codec(DataStream strIn, DataStream strOut)
		{
			while (!strIn.EOF) {
				strOut.WriteByte((byte)~strIn.ReadByte());
			}
		}

		private struct Block
		{
			public Block(string text, byte[] data)
				: this()
			{
				this.Text = text;
				this.Data = data;
			}

			public string Text {
				get;
				set;
			}

			public byte[] Data {
				get;
				private set;
			}
		}
	}
}

