//-----------------------------------------------------------------------
// <copyright file="OverlayFile.cs" company="none">
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
	using Libgame;   
    
    /// <summary>
    /// Represents an overlay file.
    /// </summary>
	public class OverlayFile : GameFile
    {
        private uint overlayId;
        private uint ramAddress;
        private uint ramSize;
        private uint bssSize;
        private uint staticInitStart;
        private uint staticInitEnd;
        private uint encodedSize;
        private bool isEncoded;
        private uint writeAddress;

		private OverlayFile(GameFile baseFile)
			: base(baseFile.Name, baseFile.Stream)
        {
			this.Tags["Id"] = baseFile.Tags["Id"];
            this.Name = baseFile.Name;
        }
                
        #region Properties
        
        /// <summary>
        /// Gets the size of the overlay table entry.
        /// </summary>
        public static uint TableEntrySize
        {
            get { return 0x20; }
        }
        
        /// <summary>
        /// Gets or sets the ID of the overlay.
        /// </summary>
        public uint OverlayId
        {
            get { return this.overlayId; }
            set { this.overlayId = value; }
        }

        /// <summary>
        /// Gets or sets the address where the overlay will be load in the RAM.
        /// </summary>
        public uint RamAddress
        {
            get { return this.ramAddress; }
            set { this.ramAddress = value; }
        }

        /// <summary>
        /// Gets or sets the amount of bytes to load in RAM of the overlay.
        /// </summary>
        public uint RamSize
        {
            get { return this.ramSize; }
            set { this.ramSize = value; }
        }

        /// <summary>
        /// Gets or sets the size of the BSS data region.
        /// </summary>
        public uint BssSize
        {
            get { return this.bssSize; }
            set { this.bssSize = value; }
        }

        /// <summary>
        /// Gets or sets the static initialization start address.
        /// </summary>
        public uint StaticInitStart
        {
            get { return this.staticInitStart; }
            set { this.staticInitStart = value; }
        }
        
        /// <summary>
        /// Gets or sets the static initialization end address.
        /// </summary>
        public uint StaticInitEnd
        {
            get { return this.staticInitEnd; }
            set { this.staticInitEnd = value; }
        }

        /// <summary>
        /// Gets or sets the size of the overlay encoded. 0 if no encoding.
        /// </summary>
        public uint EncodedSize
        {
            get { return this.encodedSize; }
            set { this.encodedSize = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the overlay is encoding.
        /// </summary>
        public bool IsEncoded
        {
            get { return this.isEncoded; }
            set { this.isEncoded = value; }
        }
        
        /// <summary>
        /// Gets or sets the address where this overlay will be written
        /// </summary>
        public uint WriteAddress
        {
            get { return this.writeAddress; }
            set { this.writeAddress = value; }
        }
        
        #endregion
        
        /// <summary>
        /// Create a new overlay file from the info in the overlay table.
        /// </summary>
        /// <param name="str">Stream to read the table.</param>
        /// <param name="listFiles">List of files where the overlay must be.</param>
        /// <returns>Overlay file.</returns>
		public static OverlayFile FromTable(DataStream str, GameFile[] listFiles)
        {
			DataReader dr = new DataReader(str);
            
			str.Seek(0x18, SeekMode.Current);
            uint fileId = dr.ReadUInt32();
			str.Seek(-0x1C, SeekMode.Current);
            
            OverlayFile overlay = new OverlayFile(listFiles[fileId]);
            overlay.OverlayId = dr.ReadUInt32();
            overlay.RamAddress = dr.ReadUInt32();
            overlay.RamSize = dr.ReadUInt32();
            overlay.BssSize = dr.ReadUInt32();
            overlay.StaticInitStart = dr.ReadUInt32();
            overlay.StaticInitEnd = dr.ReadUInt32();
            dr.ReadUInt32();    // File ID again
            uint encodingInfo = dr.ReadUInt32();
            overlay.EncodedSize = encodingInfo & 0x00FFFFFF;
            overlay.isEncoded = (encodingInfo >> 24) == 1;
            
            return overlay;
        }
        
        /// <summary>
        /// Write the table info of this overlay.
        /// </summary>
        /// <param name="str">Stream to write to.</param>
		public void WriteTable(DataStream str)
        {
			DataWriter dw = new DataWriter(str);
            
            uint encodingInfo = this.encodedSize;
            encodingInfo += (uint)((this.isEncoded ? 1 : 0) << 24);
            
            dw.Write(this.overlayId);
            dw.Write(this.ramAddress);
            dw.Write(this.ramSize);
            dw.Write(this.bssSize);
            dw.Write(this.staticInitStart);
            dw.Write(this.staticInitEnd);
			dw.Write(uint.Parse(this.Tags["Id"]));
            dw.Write(encodingInfo);
        }
    }
}
