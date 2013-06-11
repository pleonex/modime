//-----------------------------------------------------------------------
// <copyright file="GameFile.cs" company="none">
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
// <date>11/06/2013</date>
//-----------------------------------------------------------------------
namespace Modime
{
    using System;
	using System.IO;
	using System.Reflection;
    
    /// <summary>
    /// Description of GameFile.
    /// </summary>
    public class GameFile : FileContainer
    {
        public GameFile(string name, Stream stream, long offset, long size)
            : base(name)
        {
            this.Stream = stream;
            this.Offset = offset;
            this.Size = size;
        }
        
        public GameFile(string name, Stream stream, long offset, long size, Format format, FileContainer parent)
            : base(name)
        {
            this.Stream = stream;
            this.Offset = offset;
            this.Size = size;
			this.Format = format;

			parent.AddFile(this);
        }
                                       
        public long Offset
        {
            get;
            private set;
        }
        
        public long Size
        {
            get;
            private set;
        }
        
        public Format Format {
			get;
			private set;
        }
        
        public Stream Stream
        {
            get;
            private set;
        }

		public void SetFormat(string formatType, params Object[] parameters)
		{
			Type t = Type.GetType(formatType, true, false);
			this.SetFormat(t, parameters);
		}

		public void SetFormat(Type formatType, params Object[] parameters)
		{
			// Check type
			if (!formatType.IsSubclassOf(typeof(Format)))
				throw new ArgumentException("Invalid type. Must inherit from Format.");

			// Add this file as parameter
			Object[] newParams = new object[parameters.Length + 1];
			newParams[0] = this;
			Array.Copy(parameters, 0, newParams, 1, parameters.Length);

			// Create instance
			this.Format = (Format)Activator.CreateInstance(formatType, newParams);
		}
    }
}
