//-----------------------------------------------------------------------
// <copyright file="Rom.cs" company="none">
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
namespace Nitro
{
    using System;
    using System.Collections.Generic;
    using System.Text;
	using Libgame;
	using Mono.Addins;
    
     /* ROM sections:
      * 
      * Header (0x0000-0x4000)
      * ARM9 Binary
      *   |_ARM9
      *   |_ARM9 tail data
      *   |_ARM9 Overlays Tables
      *   |_ARM9 Overlays
      * ARM7 Binary
      *   |_ARM7
      *   |_ARM7 Overlays Tables
      *   |_ARM7 Overlays
      * FNT (File Name Table)
      *   |_Main tables
      *   |_Subtables (names)
      * FAT (File Allocation Table)
      *   |_Files offset
      *     |_Start offset
      *     |_End offset
      * Banner
      *   |_Header 0x20
      *   |_Icon (Bitmap + palette) 0x200 + 0x20
      *   |_Game titles (Japanese, English, French, German, Italian, Spanish) 6 * 0x100
      * Files...
      */
    
	[Extension]
    /// <summary>
    /// Class to manage internally a ROM file.
    /// </summary>
	public sealed class Rom : Format
    {       
        private RomHeader header;
        private Banner banner;
        private FileSystem fileSys;
        
        /// <summary>
        /// Gets the root folder of the ROM.
        /// </summary>
		public GameFolder Root
        {
            get { return this.fileSys.Root; }
        }
        
		public override string FormatName {
			get { return "Nitro.ROM"; }
		}

        /// <summary>
        /// Write a new ROM file.
        /// </summary>
        /// <param name="str">Stream to write to.</param>
		public override void Write(DataStream strOut)
        {
			DataStream headerStr  = new DataStream(new System.IO.MemoryStream(), 0, 0);
			DataStream fileSysStr = new DataStream(new System.IO.MemoryStream(), 0, 0);
			DataStream bannerStr  = new DataStream(new System.IO.MemoryStream(), 0, 0);

			this.fileSys.Write(fileSysStr);
			this.header.Write(headerStr);
			this.banner.Write(bannerStr);

			headerStr.WriteTo(strOut);
			fileSysStr.WriteTo(strOut);
			bannerStr.WriteTo(strOut);
			this.fileSys.AppendFiles(strOut);
			strOut.WriteUntilLength(FileSystem.PaddingByte, (int)this.header.CartridgeSize);

			headerStr.Dispose();
			fileSysStr.Dispose();
			bannerStr.Dispose();
        }
        
        /// <summary>
        /// Read the internal info of a ROM file.
        /// </summary>
        /// <param name="str">Stream to read from.</param>
		public override void Read(DataStream str)
        {
			this.header = new RomHeader();
			this.header.Read(str);

			str.Seek(this.header.BannerOffset, SeekMode.Origin);
			this.banner  = new Banner();
			this.banner.Read(str);

			this.fileSys = new FileSystem(str, this.header);
        }

		public override void Export(DataStream strOut)
		{
			throw new NotImplementedException();
		}

		public override void Import(DataStream strIn)
		{
			throw new NotImplementedException();
		}
    }
}
