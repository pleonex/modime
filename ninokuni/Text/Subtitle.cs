//-----------------------------------------------------------------------
// <copyright file="Subtitle.cs" company="none">
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
// <date>21/09/2013</date>
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
	public class Subtitle : XmlExportable
	{
		private const char Split = '\x3';
		private const char ExtraSplit = '\xD';

		private List<SubtitleEntry> entries;

		public override string FormatName {
			get { return "Ninokuni.Subtitle"; }
		}

		public override void Read(DataStream strIn)
		{
			this.entries = new List<SubtitleEntry>();

			// Gets all the lines
			DataReader reader = new DataReader(strIn, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));
			string[] lines = reader.ReadString().Split(ExtraSplit);

			for (int i = 0; i < lines.Length; i++) {
				SubtitleEntry entry = new SubtitleEntry();

				if (lines[i].Length == 0)
					continue;

				if (lines[i][0] == '#') {
					entry.Type = SubtitleType.Comment;
					// UNDONE: entry.Data = Table.JapToLatin(lines[i].Substring(1));
				} else if (lines[i].StartsWith("/stream", StringComparison.Ordinal)) {
					entry.Type = SubtitleType.Voice;
					entry.Data = lines[i].Substring(8);
				} else if (lines[i].StartsWith("/sync", StringComparison.Ordinal)) {
					entry.Type = SubtitleType.SyncTime;
					entry.Data = lines[i].Substring(6);
				} else if (lines[i].StartsWith("/clear", StringComparison.Ordinal)) {
					entry.Type = SubtitleType.Clear;
				} else {
					entry.Type = SubtitleType.Text;
					// UNDONE: entry.Data = Table.JapToLatin(lines[i]);
				}

				this.entries.Add(entry);
			}
		}

		public override void Write(DataStream strOut)
		{
			StringBuilder text = new StringBuilder();

			foreach (SubtitleEntry entry in this.entries) {
				switch (entry.Type) {
				case SubtitleType.Text:     
					// UNDONE: text.Append(Table.LatinToJap(this.subs[i].Data));
					break;

				case SubtitleType.SyncTime:
					text.AppendFormat("/sync {0}", entry.Data);
					break;

				case SubtitleType.Voice:
					text.AppendFormat("/stream {0}", entry.Data);
					break;

				case SubtitleType.Comment:
					// UNDONE: text.AppendFormat("#{0}", Table.LatinToJap(this.subs[i].Data));
					break;

				case SubtitleType.Clear:
					text.Append("/clear");
					break;
				}

				text.Append(ExtraSplit);
				text.Append(Split);
			}

			DataWriter writer = new DataWriter(strOut, EndiannessMode.LittleEndian, Encoding.GetEncoding("shift_jis"));
			writer.Write(text.ToString());
			writer.Flush();
		}

		protected override void Import(XElement root)
		{
			this.entries = new List<SubtitleEntry>();

			foreach (XElement el in root.Elements()) {
				SubtitleEntry entry = new SubtitleEntry();

				switch (el.Name.LocalName) {
				case "Text":
					entry.Type = SubtitleType.Text;
					// UNDONE: entry.Data = TextFormatter.Reformat(el.Value, 2);
					break;

				case "Comment":
					entry.Type = SubtitleType.Comment;
					// UNDONE: entry.Data = TextFormatter.Reformat(el.Value, 2);
					break;

				case "Voice":
					entry.Type = SubtitleType.Voice;
					entry.Data = el.Attribute("File").Value;
					break;

				case "Sync":
					entry.Type = SubtitleType.SyncTime;
					entry.Data = el.Attribute("Time").Value;
					break;

				case "Clear":
					entry.Type = SubtitleType.Clear;
					break;

				default:
					throw new FormatException("Invalid XML tag.");
				}

				this.entries.Add(entry);
			}
		}

		protected override void Export(XElement root)
		{
			foreach (SubtitleEntry entry in this.entries) {
				XElement el = null;

				switch (entry.Type) {
				case SubtitleType.Text:
					// UNDONE: el = new XElement("Text", TextFormatter.Format(this.subs[i].Data, 2));
					break;

				case SubtitleType.SyncTime:
					el = new XElement("Sync");
					el.SetAttributeValue("Time", entry.Data);
					break;

				case SubtitleType.Voice: 
					el = new XElement("Voice"); 
					el.SetAttributeValue("File", entry.Data);
					break;

				case SubtitleType.Comment: 
					// UNDONE: el = new XElement("Comment", TextFormatter.Format(this.subs[i].Data, 2)); 
					break;

				case SubtitleType.Clear:
					el = new XElement("Clear"); 
					break;
				}

				root.Add(el);
			}
		}

		private enum SubtitleType
		{
			Text,
			SyncTime,
			Voice,
			Comment,
			Clear
		}

		private struct SubtitleEntry
		{
			public SubtitleType Type
			{
				get;
				set;
			}

			public string Data
			{
				get;
				set;
			}
		}
	}
}
