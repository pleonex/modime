//-----------------------------------------------------------------------
// <copyright file="ArmFile.cs" company="none">
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
// <date>28/02/2013</date>
//-----------------------------------------------------------------------
namespace Nitro
{
    using System;
    using System.Collections.Generic;
	using Libgame;
    
    /// <summary>
    /// Description of ArmFile.
    /// </summary>
	public class ArmFile : GameFile
    {        
        private byte[] unknownTail;
        
		private ArmFile(string name, DataStream str)
			: base(name, str)
		{
		}   
		     
        /// <summary>
        /// Gets a value indicating whether represents the ARM9 or ARM7.
        /// </summary>
        public bool IsArm9 {
			get;
			private set;
        }
        
        /// <summary>
        /// Gets or sets the entry address of the ARM.
        /// </summary>
        public uint EntryAddress {
			get;
			set;
        }
        
        /// <summary>
        /// Gets or sets the auto-load address of the ARM.
        /// </summary>
        public uint Autoload {
			get;
			set;
        }
        
        /// <summary>
        /// Gets or sets the address of the ARM in the RAM.
        /// </summary>
        public uint RamAddress {
			get;
			set;
        }
        
        /// <summary>
        /// Create a new ARM file from the info of the ROM header.
        /// </summary>
        /// <param name="str">Stream to read the unknown tail.</param> 
        /// <param name="header">Header of the current ROM.</param>
        /// <param name="romPath">Path to the current ROM.</param>
        /// <param name="isArm9">Indicates if must create an ARM9 or ARM7 file.</param>
        /// <returns>ARM file.</returns>
		public static ArmFile FromStream(DataStream str, RomHeader header, bool isArm9)
        {
            ArmFile arm;

			if (isArm9) {
				arm = new ArmFile("ARM9.bin", new DataStream(str, header.Arm9Offset, header.Arm9Size));
				arm.IsArm9 = true;
				arm.EntryAddress = header.Arm9EntryAddress;
				arm.Autoload     = header.Arm9Autoload;
				arm.RamAddress   = header.Arm9RamAddress;
			} else {
				arm = new ArmFile("ARM7.bin", new DataStream(str, header.Arm7Offset, header.Arm7Size));
				arm.IsArm9 = false;
                arm.EntryAddress = header.Arm7EntryAddress;
				arm.Autoload     = header.Arm7Autoload;
				arm.RamAddress   = header.Arm7RamAddress;
            }
            
            // Set the unknown tail
			if (isArm9) {   
                // It's after the ARM9 file.
				str.Seek(header.Arm9Offset + header.Arm9Size, SeekMode.Origin);
                
				// Read until reachs padding byte
                List<byte> tail = new List<byte>();
                byte b = (byte)str.ReadByte();
				while (b != 0xFF) {
                    tail.Add(b);
                    b = (byte)str.ReadByte();
                }
                
                arm.unknownTail = tail.ToArray();
            }
            
            return arm;
        }
        
        /// <summary>
        /// Update and write the Arm file
        /// </summary>
        /// <param name="str">Stream to write</param>
        /// <param name="header">Rom header to update.</param>
        /// <param name="shiftOffset">Amount of data before that stream.</param>
		public void UpdateAndWrite(DataStream str, RomHeader header, uint shiftOffset)
        {
			if (this.IsArm9) {
				header.Arm9Autoload     = this.Autoload;
				header.Arm9EntryAddress = this.EntryAddress;
				header.Arm9RamAddress   = this.RamAddress;
				header.Arm9Size         = (uint)this.Length;
				if (this.Length > 0x00)
                    header.Arm9Offset = (uint)(shiftOffset + str.Position);
                else
                    header.Arm9Offset = 0x00;
			} else {
				header.Arm7Autoload     = this.Autoload;
				header.Arm7EntryAddress = this.EntryAddress;
				header.Arm7RamAddress   = this.RamAddress;
				header.Arm7Size         = (uint)this.Length;
				if (this.Length > 0x00)
                    header.Arm7Offset = (uint)(shiftOffset + str.Position);
                else
                    header.Arm7Offset = 0x00;
            }
            
            // Write the file
			this.Stream.WriteTo(str);
            
            // Write the unknown tail
            if (this.unknownTail != null)
                str.Write(this.unknownTail, 0, this.unknownTail.Length);
        }
        
        /// <summary>
        /// Gets the unknown tail bytes.
        /// </summary>
        /// <returns>ARM tail</returns>
        public byte[] GetTail()
        {
            return this.unknownTail;
        }
    }
}
