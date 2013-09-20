//-----------------------------------------------------------------------
// <copyright file="FileSystem.cs" company="none">
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
    using System.Linq;
    using System.Text;
	using Libgame;
    
    /// <summary>
    /// Represents the file system of a NDS game ROM.
    /// </summary>
	public sealed class FileSystem : Format
    {        
        private RomHeader header;
		private GameFolder root;         // Parent folder of the ROM file system.
		private GameFolder sysFolder;    // Folder with ARM, overlays and info files.
        
        /// <summary>
        /// Initializes a new instance of the FileSystem class.
        /// </summary>
        /// <param name="str">Stream to read from.</param>
        /// <param name="header">Header of the ROM.</param>
        /// <param name="romPath">Path of the current ROM (to create the file system).</param>
		public FileSystem(DataStream str, RomHeader header)
        {
            this.header = header;            
            this.Read(str);
        }
        
        /// <summary>
        /// Gets the padding used in the ROM.
        /// </summary>
        public static int PaddingAddress
        {
            get { return 0x200; }
        }
        
        /// <summary>
        /// Gets the padding byte used in the ROM.
        /// </summary>
        public static byte PaddingByte
        {
            get { return 0xFF; }
        }
        
        /// <summary>
        /// Gets the parent folder of the game.
        /// </summary>
		public GameFolder Root
        {
            get { return this.root; }
        }
        
        /// <summary>
        /// Gets the folder with system files.
        /// </summary>
		public GameFolder SystemFolder
        {
            get { return this.sysFolder; }
        }
        
		public override string FormatName {
			get { return "Nitro.FileSystem"; }
		}

        /// <summary>
        /// Write the file system to the stream.
        /// </summary>
        /// <param name="str">Stream to write to.</param>
		public override void Write(DataStream str)
        {
            // The order is: ARM9 - Overlays9 - ARM7 - Overlays7 - FNT - FAT
            this.WriteArm(str, true);
            this.WriteArm(str, false);
            
            int numOverlays = this.CountOverlays(false) + this.CountOverlays(true);
            
			Fnt fnt = new Fnt();
			fnt.Initialize(null, this.root, numOverlays);
            this.header.FntOffset = (uint)(this.header.HeaderSize + str.Position);
            long fntStartOffset = str.Position;
            fnt.Write(str);
            this.header.FntSize = (uint)(str.Position - fntStartOffset);
			str.WritePadding(FileSystem.PaddingByte, FileSystem.PaddingAddress);
            
			GameFolder tmpFolder = new GameFolder(string.Empty);
			tmpFolder.AddFolders(this.sysFolder.Folders);	// Overlay folders
			tmpFolder.AddFolder(this.root);					// Game files
            
			// Write File Allocation Table
			Fat fat = new Fat();
			fat.Initialize(null, tmpFolder, Banner.Size + this.header.HeaderSize);
            fat.Write(str);
			str.WritePadding(FileSystem.PaddingByte, FileSystem.PaddingAddress);

			this.header.FatOffset = (uint)(this.header.HeaderSize + str.Position);
			this.header.FatSize = fat.Size;
        }
                
        /// <summary>
        /// Appends all the files listed before in the Write method to a file.
        /// </summary>
        /// <param name="fileOut">File to append the files.</param>
		public void AppendFiles(DataStream strOut)
        {                        
			Fat fat = new Fat();
			fat.Initialize(null, this.root);
			fat.WriteFiles(strOut);
        }
        
        /// <summary>
        /// Read the file system of the ROM and create the folder tree.
        /// </summary>
        /// <param name="str">Stream to read the file system.</param>
		public override void Read(DataStream str)
        {
			// Read File Allocation Table
			DataStream fatStr = new DataStream(str, this.header.FatOffset, this.header.FatSize);
			Fat fat = new Fat();
			fat.Read(fatStr);
			fatStr.Dispose();

			// Read File Name Table
			str.Seek(header.FntOffset, SeekMode.Origin);
			Fnt fnt = new Fnt();
			fnt.Read(str);
            this.root = fnt.CreateTree(fat.GetFiles());
            
			this.sysFolder = new GameFolder("System");
            
            this.sysFolder.AddFile(ArmFile.FromStream(str, this.header, true));
            this.sysFolder.AddFolder(OverlayFolder.FromTable(str, this.header, true, fat.GetFiles()));
            
            this.sysFolder.AddFile(ArmFile.FromStream(str, this.header, false));
            this.sysFolder.AddFolder(OverlayFolder.FromTable(str, this.header, false, fat.GetFiles()));
        }
        
		private void WriteArm(DataStream str, bool isArm9)
        {
            // Write the ARM file.
			foreach (GameFile file in this.sysFolder.Files) {
                ArmFile arm = file as ArmFile;
				if (arm == null || arm.IsArm9 != isArm9)
					continue;

                arm.UpdateAndWrite(str, this.header, this.header.HeaderSize);
				str.WritePadding(FileSystem.PaddingByte, FileSystem.PaddingAddress);
            }
            
            // Write the overlay table and overlays
			foreach (GameFolder folder in this.sysFolder.Folders) {
                OverlayFolder overlayFolder = folder as OverlayFolder;
				if (overlayFolder == null || overlayFolder.IsArm9 != isArm9)
					continue;

                overlayFolder.WriteTable(str, this.header, this.header.HeaderSize);
				str.WritePadding(FileSystem.PaddingByte, FileSystem.PaddingAddress);
                    
				foreach (GameFile file in overlayFolder.Files) {
					OverlayFile overlay = file as OverlayFile;
					if (overlay == null)
						continue;

					overlay.WriteAddress = (uint)(this.header.HeaderSize + str.Position);
					overlay.Stream.WriteTo(str);
					str.WritePadding(FileSystem.PaddingByte, FileSystem.PaddingAddress);
				}
            }
        }
        
        private int CountOverlays(bool isArm9)
        {
            int count = 0;
            
			foreach (GameFolder folder in this.sysFolder.Folders) {
                OverlayFolder overlayFolder = folder as OverlayFolder;
                if (overlayFolder == null || overlayFolder.IsArm9 != isArm9)                   
                    continue;
                
				foreach (GameFile file in overlayFolder.Files) {
                    if (file is OverlayFile)
                        count++;
                }
            }
            
            return count;
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