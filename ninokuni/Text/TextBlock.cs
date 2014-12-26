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
		Block[] blocks;
		bool    hasNumBlock;
		bool    isEncoded;
		bool    wOriginal;
		bool    nullTermina;
		int     textSize;
		int     dataSize;
		int     longTxtSize;
		string  fileName;

		public override string FormatName {
			get { return string.Format("Ninokuni.{0}", fileName); }
		}

		public override void Initialize(GameFile file, params object[] parameters)
		{
			base.Initialize(file, parameters);

			hasNumBlock = (bool)parameters[0];
			isEncoded   = (bool)parameters[1];
			wOriginal   = (bool)parameters[2];
			nullTermina = (bool)parameters[3];
			textSize    = (int)parameters[4];
			dataSize    = (int)parameters[5];
			longTxtSize = (int)parameters[6];
			fileName    = (string)parameters[7];
		}

		public override void Read(DataStream strIn)
		{
			DataStream data;

			// Simple NOT encoding
			if (isEncoded) {
				data = new DataStream(new System.IO.MemoryStream(), 0, 0);
				Codec(strIn, data);	// Decode strIn to data
			} else {
				data = strIn;
			}

			data.Seek(0, SeekMode.Origin);
			var reader = new DataReader(data, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			int blockSize = textSize + dataSize;
			int numBlocks = hasNumBlock ? reader.ReadUInt16() : (int)(data.Length / blockSize);

			blocks = new Block[numBlocks];
			for (int i = 0; i < numBlocks; i++)
				blocks[i] = new Block(
					reader.ReadString(textSize).ApplyTable("replace", false),
					reader.ReadBytes(dataSize)
				);

			// Free the decoded data
			if (isEncoded)
				data.Dispose();
		}

		public override void Write(DataStream strOut)
		{
			var data   = new DataStream(new System.IO.MemoryStream(), 0, 0);
			var writer = new DataWriter(data, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			if (hasNumBlock)
				writer.Write((ushort)blocks.Length);

			foreach (Block b in blocks) {
				if (fileName == "MagicParam")
					writer.Write(b.Text.ApplyTable("replace", true), textSize + 8, nullTermina);
				else
					writer.Write(b.Text.ApplyTable("replace", true), textSize, nullTermina);
				writer.Write(b.Data);

				// Hack for imagen name: new long name field
				if (longTxtSize > 0) {
					writer.Write(b.LongText ?? b.Text, longTxtSize, "replace", true);
				}
			}

			if (isEncoded) {
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
				if (idx >= blocks.Length)
					break;	// Show warning

				if (e.Attribute("LongName") != null)
					blocks[idx].LongText = e.Attribute("LongName").Value;

				blocks[idx++].Text = e.Value.FromXmlString('<', '>');
			}
		}

		protected override void Export(XElement root)
		{
			foreach (Block b in blocks) {
				var e = new XElement("String", b.Text.ToXmlString(2, '<', '>'));
				if (wOriginal && !b.Text.Contains("\n"))
					e.SetAttributeValue("Original", b.Text);
				root.Add(e);
			}
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			// Nothing to do
		}

		static void Codec(DataStream strIn, DataStream strOut)
		{
			while (!strIn.EOF) {
				strOut.WriteByte((byte)~strIn.ReadByte());
			}
		}

		struct Block
		{
			public Block(string text, byte[] data)
				: this()
			{
				Text = text;
				Data = data;
			}

			public string Text {
				get;
				set;
			}

			public byte[] Data {
				get;
				private set;
			}

			public string LongText {
				get;
				set;
			}
		}
	}
}

