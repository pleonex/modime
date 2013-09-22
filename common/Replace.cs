//-----------------------------------------------------------------------
// <copyright file="Replace.cs" company="none">
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
// <date>22/09/2013</date>
//-----------------------------------------------------------------------
using System;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Common
{
	[Extension]
	public class Replace : Format
	{
		private DataStream data;

		public override string FormatName {
			get { return "Common.Replace"; }
		}

		public override void Read(DataStream strIn)
		{
			this.data = new DataStream(strIn, strIn.Offset, strIn.Length);
		}

		public override void Write(DataStream strOut)
		{
			this.data.WriteTo(strOut);
		}

		public override void Import(params DataStream[] strIn)
		{
			if (this.data != null)
				this.data.Dispose();
			this.data = new DataStream(strIn[0], strIn[0].Offset, strIn[0].Length);
		}

		public override void Export(params DataStream[] strOut)
		{
			this.data.WriteTo(strOut[0]);
		}
	}
}

