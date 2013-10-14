//-----------------------------------------------------------------------
// <copyright file="BattleHelp.cs" company="none">
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
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni
{
	[Extension]
	public class BattleHelp : XmlExportable
	{
		private string[] text;

		public override string FormatName {
			get { return "Ninokuni.BattleHelp"; }
		}

		public override void Read(DataStream strIn)
		{
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			ushort num_block = reader.ReadUInt16();
			this.text = new string[num_block];

			for (int i = 0; i < num_block; i++) {
				this.text[i] = reader.ReadString(0x80);
				this.text[i] = this.text[i].Replace("\\n", "\n");
				this.text[i] = this.text[i].ApplyTable("replace", false);
			}
		}

		public override void Write(DataStream strOut)
		{
			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));

			writer.Write((ushort)this.text.Length);
			for (int i = 0; i < this.text.Length; i++) {
				string text = this.text[i].Replace("\n", "\\n");
				text = text.ApplyTable("replace", true);
				writer.Write(text, 0x80);
			}

			writer.Flush();
		}

		protected override void Import(XElement root)
		{
			List<string> entries = new List<string>();
			foreach (XElement child in root.Elements("String")) {
				entries.Add(child.Value.FromXmlString('<', '>'));
			}

			this.text = entries.ToArray();
		}

		protected override void Export(XElement root)
		{
			foreach (string t in this.text)
				root.Add(new XElement("String", t.ToXmlString(2, '<', '>')));
		}

		protected override void Dispose(bool freeManagedResourcesAlso)
		{
			// No managed resource to free
		}
	}
}

