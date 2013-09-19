﻿//-----------------------------------------------------------------------
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
namespace Libgame
{
    using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Reflection;
    
    /// <summary>
    /// Description of GameFile.
    /// </summary>
    public class GameFile : FileContainer
    {
		private List<GameFile> dependencies = new List<GameFile>();

        public GameFile(string name, Stream stream, long offset, long length)
			: this(name, new DataStream(stream, offset, length))
        {
        }

		public GameFile(string Name, DataStream stream)
			: base(Name)
		{
			this.Stream = stream;
		}

		public GameFile(string name, Stream stream, long offset, long length, Format format, FileContainer parent)
			: this(name, new DataStream(stream, offset, length))
		{
		}

		public GameFile(string name, DataStream stream, Format format, FileContainer parent)
			: base(name)
		{
			this.Stream = stream;
			this.Format = format;

			parent.AddFile(this);
		}
                                               
        public long Length
        {
			get { return this.Stream.Length; }
        }

		public DataStream Stream
		{
			get;
			private set;
		}

        public Format Format {
			get;
			private set;
        }

		public DependecyCollection Dependencies {
			get { return new DependecyCollection(this.dependencies); }
		}

		public void SetFormat(string formatType, params Object[] parameters)
		{
			if (!string.IsNullOrEmpty(formatType))
				return;

			Type t = Type.GetType(formatType, true, false);
			this.SetFormat(t, parameters);
		}

		public void SetFormat(Type formatType, params Object[] parameters)
		{
			if (formatType == null)
				return;

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

		public void ChangeStream(DataStream newStream)
		{
			this.Stream.Dispose();
			this.Stream = newStream;
		}

		public void AddDependency(GameFile f)
		{
			this.dependencies.Add(f);
		}

		public void AddDependencies(GameFile[] fs)
		{
			foreach (GameFile f in fs)
				this.AddDependency(f);
		}

		public class DependecyCollection : ReadOnlyCollection<GameFile>
		{
			public DependecyCollection(IList<GameFile> dependencies)
				: base(dependencies)
			{
			}
		
			public GameFile this[string name] {
				get {
					foreach (GameFile file in this) {
						if (file.Name == name)
							return file;
					}

					return null;
				}
			}
		}
    }
}