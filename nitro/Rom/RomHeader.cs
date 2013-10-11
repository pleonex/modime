//-----------------------------------------------------------------------
// <copyright file="RomHeader.cs" company="none">
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
	using Libgame;
	using Libgame.IO;

    /// <summary>
    /// Header of the ROM.
    /// </summary>
	public sealed class RomHeader : Format
    {
        private const int MinCartridge = 17;
        
        private char[] gameTitle;
        private char[] gameCode;
        private char[] makerCode;
        private byte unitCode;
        private byte encryptionSeed;
        private uint cartridgeSize;     // Can change
        private byte[] reserved;
        private byte romVersion;
        private byte internalFlags;
        private uint arm9Offset;
        private uint arm9EntryAddress;
        private uint arm9RamAddress; 
        private uint arm9Size;          // Can change
        private uint arm7Offset;
        private uint arm7EntryAddress;
        private uint arm7RamAddress;
        private uint arm7Size;
        private uint fntOffset;         // Can change
        private uint fntSize;           // Can change
        private uint fatOffset;         // Can change
        private uint fatSize;           // Can change
        private uint ov9TableOffset;    // Can change
        private uint ov9TableSize;      // Can change
        private uint ov7TableOffset;
        private uint ov7TableSize;
        private uint flagsRead;         // Control register flags for read
        private uint flagsInit;         // Control register flags for init
        private uint bannerOffset;      // Can change
        private ushort secureCRC16;     // Secure area CRC16 0x4000 - 0x7FFF
        private ushort romTimeout;
        private uint arm9Autoload;
        private uint arm7Autoload;
        private ulong secureDisable;    // Magic number for unencrypted mode
        private uint romSize;           // Can change
        private uint headerSize;        // Can change
        private byte[] reserved2;       // 56 bytes
        private byte[] nintendoLogo;    // 156 bytes
        private ushort logoCRC16;
        private ushort headerCRC16;
        private bool secureCRC;
        private bool logoCRC;
        private bool headerCRC;
        private uint debugRomOffset;    // only if debug
        private uint debugSize;         // version with
        private uint debugRamAddress;   // 0 = none, SIO and 8 MB
        private uint reserved3;         // Zero filled transfered and stored but not used
        private byte[] unknown;         // Rest of the header
           
        #region Properties
        /// <summary>
        /// Gets or sets the short game title.
        /// </summary>
        public string GameTitle
        {
            get { return new string(this.gameTitle); }
            set { this.gameTitle = value.ToCharArray(); }
        }

        /// <summary>
        /// Gets or sets the game code.
        /// </summary>
        public string GameCode
        {
            get { return new string(this.gameCode); }
            set { this.gameCode = value.ToCharArray(); }
        }

        /// <summary>
        /// Gets or sets the maker code.
        /// </summary>
        public string MakerCode
        {
            get { return new string(this.makerCode); }
            set { this.makerCode = value.ToCharArray(); }
        }

        /// <summary>
        /// Gets or sets the unit code.
        /// </summary>
        public byte UnitCode
        {
            get { return this.unitCode; }
            set { this.unitCode = value; }
        }

        public byte EncryptionSeed {
            get { return encryptionSeed; }
            set { encryptionSeed = value; }
        }

        public uint CartridgeSize {
            get { return cartridgeSize; }
            set { cartridgeSize = value; }
        }

        public byte[] Reserved {
            get { return reserved; }
            set { reserved = value; }
        }

        public byte RomVersion {
            get { return romVersion; }
            set { romVersion = value; }
        }

        public byte InternalFlags {
            get { return internalFlags; }
            set { internalFlags = value; }
        }

        public uint Arm9Offset {
            get { return arm9Offset; }
            set { arm9Offset = value; }
        }

        public uint Arm9EntryAddress {
            get { return arm9EntryAddress; }
            set { arm9EntryAddress = value; }
        }

        public uint Arm9RamAddress {
            get { return arm9RamAddress; }
            set { arm9RamAddress = value; }
        }

        public uint Arm9Size {
            get { return arm9Size; }
            set { arm9Size = value; }
        }

        public uint Arm7Offset {
            get { return arm7Offset; }
            set { arm7Offset = value; }
        }

        public uint Arm7EntryAddress {
            get { return arm7EntryAddress; }
            set { arm7EntryAddress = value; }
        }

        public uint Arm7RamAddress {
            get { return arm7RamAddress; }
            set { arm7RamAddress = value; }
        }

        public uint Arm7Size {
            get { return arm7Size; }
            set { arm7Size = value; }
        }

        public uint FntOffset {
            get { return fntOffset; }
            set { fntOffset = value; }
        }

        public uint FntSize {
            get { return fntSize; }
            set { fntSize = value; }
        }

        public uint FatOffset {
            get { return fatOffset; }
            set { fatOffset = value; }
        }

        public uint FatSize {
            get { return fatSize; }
            set { fatSize = value; }
        }

        public uint Ov9TableOffset {
            get { return ov9TableOffset; }
            set { ov9TableOffset = value; }
        }

        public uint Ov9TableSize {
            get { return ov9TableSize; }
            set { ov9TableSize = value; }
        }

        public uint Ov7TableOffset {
            get { return ov7TableOffset; }
            set { ov7TableOffset = value; }
        }

        public uint Ov7TableSize {
            get { return ov7TableSize; }
            set { ov7TableSize = value; }
        }

        public uint FlagsRead {
            get { return flagsRead; }
            set { flagsRead = value; }
        }

        public uint FlagsInit {
            get { return flagsInit; }
            set { flagsInit = value; }
        }

        public uint BannerOffset {
            get { return bannerOffset; }
            set { bannerOffset = value; }
        }

        public ushort SecureCRC16 {
            get { return secureCRC16; }
            set { secureCRC16 = value; }
        }

        public ushort RomTimeout {
            get { return romTimeout; }
            set { romTimeout = value; }
        }

        public uint Arm9Autoload {
            get { return arm9Autoload; }
            set { arm9Autoload = value; }
        }

        public uint Arm7Autoload {
            get { return arm7Autoload; }
            set { arm7Autoload = value; }
        }

        public ulong SecureDisable {
            get { return secureDisable; }
            set { secureDisable = value; }
        }

        public uint RomSize {
            get { return romSize; }
            set { romSize = value; }
        }

        public uint HeaderSize {
            get { return headerSize; }
            set { headerSize = value; }
        }

        public byte[] Reserved2 {
            get { return reserved2; }
            set { reserved2 = value; }
        }

        public ushort LogoCRC16 {
            get { return logoCRC16; }
            set { logoCRC16 = value; }
        }

        public ushort HeaderCRC16 {
            get { return headerCRC16; }
            set { headerCRC16 = value; }
        }

        public bool SecureCRC {
            get { return secureCRC; }
            set { secureCRC = value; }
        }

        public bool LogoCRC {
            get { return logoCRC; }
            set { logoCRC = value; }
        }

        public bool HeaderCRC {
            get { return headerCRC; }
            set { headerCRC = value; }
        }

        public uint DebugRomOffset {
            get { return debugRomOffset; }
            set { debugRomOffset = value; }
        }

        public uint DebugSize {
            get { return debugSize; }
            set { debugSize = value; }
        }

        public uint DebugRamAddress {
            get { return debugRamAddress; }
            set { debugRamAddress = value; }
        }

        public uint Reserved3 {
            get { return reserved3; }
            set { reserved3 = value; }
        }
        #endregion
        
		public override string FormatName {
			get { return "Nitro.Header"; }
		}

		public void UpdateCrc()
		{
			// Write temporaly the header
			DataStream data = new DataStream(new System.IO.MemoryStream(), 0, 0);
			this.Write(data);

			data.Seek(0, SeekMode.Origin);
			this.HeaderCRC16 = Libgame.Utils.Checksums.Crc16(data, 0x15E);
			this.HeaderCRC   = true;

			data.Dispose();

			// The checksum of the logo won't be calculated again
			// since it must have always the original value if we
			// want to boot the game correctly (0xCF56)
		}

        /// <summary>
        /// Write the header of a NDS game ROM.
        /// </summary>
        /// <param name="str">Stream to write the header.</param>
		public override void Write(DataStream str)
        {
			DataWriter dw = new DataWriter(str);
            
			dw.Write(this.gameTitle);			// At 0x00
            dw.Write(this.gameCode);
			dw.Write(this.makerCode);			// At 0x10
            dw.Write(this.unitCode);
            dw.Write(this.encryptionSeed);
            dw.Write((byte)(Math.Log(this.cartridgeSize, 2) - MinCartridge));
            dw.Write(this.reserved);
            dw.Write(this.RomVersion);
            dw.Write(this.internalFlags);
			dw.Write(this.arm9Offset);			// At 0x20
            dw.Write(this.arm9EntryAddress);
            dw.Write(this.arm9RamAddress);
            dw.Write(this.arm9Size);
			dw.Write(this.arm7Offset);			// At 0x30
            dw.Write(this.arm7EntryAddress);
            dw.Write(this.arm7RamAddress);
            dw.Write(this.arm7Size);
			dw.Write(this.fntOffset);			// At 0x40
            dw.Write(this.fntSize);
            dw.Write(this.fatOffset);
            dw.Write(this.fatSize);
			dw.Write(this.ov9TableOffset);		// At 0x50
            dw.Write(this.ov9TableSize);
            dw.Write(this.ov7TableOffset);
            dw.Write(this.ov7TableSize);
            dw.Write(this.flagsRead);
            dw.Write(this.flagsInit);
            dw.Write(this.bannerOffset);
            dw.Write(this.secureCRC16);
            dw.Write(this.romTimeout);
            dw.Write(this.arm9Autoload);
            dw.Write(this.arm7Autoload);
            dw.Write(this.secureDisable);
            dw.Write(this.romSize);
            dw.Write(this.headerSize);
            dw.Write(this.reserved2);
            dw.Write(this.nintendoLogo);
            dw.Write(this.logoCRC16);
            dw.Write(this.headerCRC16);
            dw.Write(this.debugRomOffset);
            dw.Write(this.debugSize);
            dw.Write(this.debugRamAddress);
            dw.Write(this.reserved3);
            dw.Flush();

            dw.Write(this.unknown);       
            dw.Flush();
        }
        
        /// <summary>
        /// Read a the header from a NDS game ROM.
        /// </summary>
        /// <param name="str">Stream with the ROM. Must be at the correct position.</param>
		public override void Read(DataStream str)
        {
            long startPosition = str.Position;
			DataReader dr = new DataReader(str);
            
			this.gameTitle        = dr.ReadChars(12);
			this.gameCode         = dr.ReadChars(4);
			this.makerCode        = dr.ReadChars(2);
			this.unitCode         = dr.ReadByte();
			this.encryptionSeed   = dr.ReadByte();
			this.cartridgeSize    = (uint)(1 << (MinCartridge + dr.ReadByte()));
			this.reserved         = dr.ReadBytes(9);
			this.RomVersion       = dr.ReadByte();
			this.internalFlags    = dr.ReadByte();
			this.Arm9Offset       = dr.ReadUInt32();
            this.Arm9EntryAddress = dr.ReadUInt32();
			this.Arm9RamAddress   = dr.ReadUInt32();
			this.Arm9Size         = dr.ReadUInt32();
			this.Arm7Offset       = dr.ReadUInt32();
            this.Arm7EntryAddress = dr.ReadUInt32();
			this.Arm7RamAddress   = dr.ReadUInt32();
			this.Arm7Size         = dr.ReadUInt32();
			this.fntOffset        = dr.ReadUInt32();
			this.fntSize          = dr.ReadUInt32();
			this.FatOffset        = dr.ReadUInt32();
			this.FatSize          = dr.ReadUInt32();
			this.Ov9TableOffset   = dr.ReadUInt32();
			this.Ov9TableSize     = dr.ReadUInt32();
			this.Ov7TableOffset   = dr.ReadUInt32();
			this.Ov7TableSize     = dr.ReadUInt32();
			this.flagsRead        = dr.ReadUInt32();
			this.flagsInit        = dr.ReadUInt32();
			this.bannerOffset     = dr.ReadUInt32();
			this.secureCRC16      = dr.ReadUInt16();
			this.RomTimeout       = dr.ReadUInt16();
			this.Arm9Autoload     = dr.ReadUInt32();
			this.Arm7Autoload     = dr.ReadUInt32();
			this.secureDisable    = dr.ReadUInt64();
			this.RomSize          = dr.ReadUInt32();
			this.headerSize       = dr.ReadUInt32();
			this.reserved2        = dr.ReadBytes(56);
			this.nintendoLogo     = dr.ReadBytes(156);
			this.logoCRC16        = dr.ReadUInt16();
			this.headerCRC16      = dr.ReadUInt16();
			this.debugRomOffset   = dr.ReadUInt32();
			this.debugSize        = dr.ReadUInt32();
			this.debugRamAddress  = dr.ReadUInt32();
			this.reserved3        = dr.ReadUInt32();
            
            int unknownSize = (int)(this.headerSize - (str.Position - startPosition));
			this.unknown    = dr.ReadBytes(unknownSize);
        } 

		// Maybe in a future, by importing/exporting some kind of extadata like a XML
		// we could generate a ROM file from raw.
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
