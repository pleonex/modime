//-----------------------------------------------------------------------
// <copyright file="SubtitleValidation.cs" company="none">
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Addins;
using Libgame;
using Libgame.IO;

namespace Ninokuni
{
	[Extension]
	public class SubtitleValidation : FormatValidation
	{
		public override Type FormatType {
			get { return typeof(Subtitle); }
		}

		protected override ValidationResult TestByTags(IDictionary<string, object> tags)
		{
			if ((string)tags["_GameCode_"] == "B2KJ")
				return ValidationResult.CouldBe;

			return ValidationResult.No;
		}

		protected override ValidationResult TestByData(DataStream stream)
		{
			return ValidationResult.Invalid;
		}

		protected override ValidationResult TestByRegexp(string filepath, string filename)
		{
			string pattern = @"^(?:s\d\dB?|Ev_(?:B|S\d)_\d\d)\.txt$";

			if (Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase)) {
				string filenameNoExt = filename.Remove(filename.IndexOf("."));

				if (filepath.EndsWith("/data/movie/" + filename) ||
					filepath.ToLower().EndsWith("/data/event/demo/" + filenameNoExt.ToLower() + "/" + filename.ToLower()))
					return ValidationResult.Sure;

				return ValidationResult.ShouldBe;
			}

			return ValidationResult.No;
		}

		protected override string[] GuessDependencies(GameFile file)
		{
			// No extra dependency
			return null;
		}

		protected override object[] GuessParameters(GameFile file)
		{
			// No extra parameters
			return null;
		}
	}
}

