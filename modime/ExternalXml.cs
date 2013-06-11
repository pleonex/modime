//-----------------------------------------------------------------------
// <copyright file="ExternalXml.cs" company="none">
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
// <date>18/04/2013</date>
//-----------------------------------------------------------------------
namespace Modime
{
    using System;
    using System.IO;
    using System.Xml.Linq;
    
    /// <summary>
    /// Description of ExternalXmlFormat.
    /// </summary>
    public abstract class ExternalXml : Format
    {
		protected ExternalXml(GameFile file)
			: base(file)
		{
		}
               
        public override void Import(Stream strIn, long size)
        {
            XDocument doc = XDocument.Load(strIn);
            
            if (doc.Root.Name.LocalName != this.FormatName)
                throw new FormatException();
            
            this.Import(doc.Root);
        }
        
		public override void Export(Stream strOut)
		{
			XDocument doc = new XDocument();
			doc.Declaration = new XDeclaration("1.0", "utf-8", "yes");

			XElement root = new XElement(this.FormatName);
			root.Add(this.Export());
			doc.Add(root);

			doc.Save(strOut, SaveOptions.None);
		}

        protected abstract void Import(XElement root);
        
        protected abstract XElement[] Export();
    }
}
