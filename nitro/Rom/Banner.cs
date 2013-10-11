//-----------------------------------------------------------------------
// <copyright file="Banner.cs" company="none">
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
// <date>16/02/2013</date>
//-----------------------------------------------------------------------
namespace Nitro.Rom
{
	using System;
	using System.Text;
	using Libgame;
	using Libgame.IO;

	/// <summary>
	/// Represents the banner of a NDS game ROM.
	/// </summary>
	public sealed class Banner : Format
	{
		private ushort version; 		// Always 1
		private ushort crc16; 			// CRC-16 of structure, not including first 32 bytes
		private byte[] reserved; 		// 28 bytes
		private byte[] tileData; 		// 512 bytes
		private byte[] palette; 		// 32 bytes
		private string japaneseTitle; 	// 256 bytes
		private string englishTitle; 	// 256 bytes
		private string frenchTitle; 	// 256 bytes
		private string germanTitle; 	// 256 bytes
		private string italianTitle; 	// 256 bytes
		private string spanishTitle; 	// 256 bytes

		public override string FormatName {
			get { return "Nitro.Banner"; }
		}

		/// <summary>
		/// Gets the size of the banner (with padding).
		/// </summary>
		public static uint Size {
			get { return 0x840 + 0x1C0; }
		}

		public void UpdateCrc()
		{
			// Write temporaly the banner
			DataStream data = new DataStream(new System.IO.MemoryStream(), 0, 0);
			this.Write(data);

			data.Seek(0x20, SeekMode.Origin);
			this.crc16 = Libgame.Utils.Checksums.Crc16(data, 0x64);

			data.Dispose();
		}

		/// <summary>
		/// Write the banner to a stream.
		/// </summary>
		/// <param name="str">Stream to write to.</param>
		public override void Write(DataStream str)
		{
			DataWriter dw = new DataWriter(str, EndiannessMode.LittleEndian, Encoding.Unicode);
            
			dw.Write(this.version);
			dw.Write(this.crc16);
			dw.Write(this.reserved);
			dw.Write(this.tileData);
			dw.Write(this.palette);
			dw.Write(this.japaneseTitle, 0x100);
			dw.Write(this.englishTitle,  0x100);
			dw.Write(this.frenchTitle,   0x100);
			dw.Write(this.germanTitle,   0x100);
			dw.Write(this.italianTitle,  0x100);
			dw.Write(this.spanishTitle,  0x100);
			dw.Flush();

			str.WritePadding(FileSystem.PaddingByte, FileSystem.PaddingAddress);            
			str.Flush();
		}

		/// <summary>
		/// Read a banner from a stream.
		/// </summary>
		/// <param name="str">Stream to read from.</param>
		public override void Read(DataStream str)
		{
			DataReader dr = new DataReader(str, EndiannessMode.LittleEndian, Encoding.Unicode);
            
			this.version = dr.ReadUInt16();
			this.crc16 = dr.ReadUInt16();
			this.reserved = dr.ReadBytes(0x1C);
			this.tileData = dr.ReadBytes(0x200);
			this.palette = dr.ReadBytes(0x20);
			this.japaneseTitle = dr.ReadString(0x100);
			this.englishTitle = dr.ReadString(0x100);
			this.frenchTitle = dr.ReadString(0x100);
			this.germanTitle = dr.ReadString(0x100);
			this.italianTitle = dr.ReadString(0x100);
			this.spanishTitle = dr.ReadString(0x100);
		}

		// What about exporting the Icon + some kind of XML file
		// with title info to import later?
		public override void Export(params DataStream[] strOut)
		{
			throw new NotImplementedException();
		}
		public override void Import(params DataStream[] strIn)
		{
			throw new NotImplementedException();
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			// Nothing to do here... No "disposing" resources here
		}
	}
}
