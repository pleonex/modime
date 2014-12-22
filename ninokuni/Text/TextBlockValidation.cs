//-----------------------------------------------------------------------
// <copyright file="TextBlockValidation.cs" company="none">
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
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni
{
	[Extension]
	public class TextBlockValidation : FormatValidation
	{
		private static readonly Dictionary<int, object[]> SupportedFiles = new Dictionary<int, object[]> {
			//  ID			    HasNumBlock  Enc.  wOrig  Null   Text   Data   		Name
			{ 0x006A, new object[] { true,  false, false, true,  0x030, 0x50, "DebugBattleSettings" } },
			{ 0x2A94, new object[] { false, false, false, true,  0x040, 0x00, "ImagenArea"          } },
			{ 0x2A95, new object[] { false, false, true,  false, 0x008, 0x00, "ImagenName"          } },
			{ 0x2A96, new object[] { true,  true,  true,  false, 0x010, 0xA4, "ImagenParam"         } },
			{ 0x2A97, new object[] { false, false, false, true,  0x100, 0x00, "ImagenText"          } },
			{ 0x2A98, new object[] { false, false, false, true,  0x072, 0x00, "EquipGetInfo"        } },
			{ 0x2A99, new object[] { false, false, false, true,  0x0C4, 0x00, "EquipItemInfo"       } },
			{ 0x2A9A, new object[] { false, false, false, true,  0x051, 0x00, "EquipItemLinkInfo"   } },
			{ 0x2A9B, new object[] { true,  true,  true,  true,  0x020, 0x30, "EquipItemParam"      } },
			{ 0x2A9C, new object[] { false, false, false, true,  0x072, 0x00, "ItemGetInfo"         } },
			{ 0x2A9D, new object[] { false, false, false, true,  0x0C4, 0x00, "ItemInfo"            } },
			{ 0x2A9E, new object[] { false, false, false, true,  0x051, 0x00, "ItemLinkInfo"        } },
			{ 0x2A9F, new object[] { true,  true,  true,  true,  0x020, 0x4C, "ItemParam"           } },
			{ 0x2AA0, new object[] { false, false, false, true,  0x072, 0x00, "SpItemGetInfo"       } },
			{ 0x2AA1, new object[] { false, false, false, true,  0x0C4, 0x00, "SpItemInfo"          } },
			{ 0x2AA2, new object[] { true,  true,  true,  true,  0x020, 0x10, "SpItemParam"         } },
			{ 0x2AA3, new object[] { true,  false, false, true,  0x0C4, 0x00, "MagicInfo"           } },
			{ 0x2AA4, new object[] { true,  false, true,  true,  0x012, 0x2E, "MagicParam"          } },
			{ 0x2AA6, new object[] { true,  false, true,  true,  0x011, 0x2C, "PlayerName"          } },
			{ 0x2AAB, new object[] { true,  false, false, true,  0x0C4, 0x00, "SkillInfo"           } },
			{ 0x2AAC, new object[] { true,  false, true,  true,  0x012, 0x2A, "SkillParam"          } }
		};
		// SkillParam checked for null

		public override Type FormatType {
			get { return typeof(TextBlock); }
		}

		protected override ValidationResult TestByTags(IDictionary<string, object> tags)
		{
			if (tags.ContainsKey("_GameCode_") && (string)tags["_GameCode_"] == "B2KJ") {
				if (SupportedFiles.ContainsKey((ushort)tags["Id"]))
					return ValidationResult.Sure;
			}

			return ValidationResult.No;
		}

		protected override ValidationResult TestByData(DataStream stream)
		{
			return ValidationResult.Invalid;
		}

		protected override ValidationResult TestByRegexp(string filepath, string filename)
		{
			return ValidationResult.Invalid;
		}

		protected override string[] GuessDependencies(GameFile file)
		{
			// No extra dependency
			return null;
		}

		protected override object[] GuessParameters(GameFile file)
		{
			return SupportedFiles[(ushort)file.Tags["Id"]];
		}
	}
}

