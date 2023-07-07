/**
* Digital Voice Modem - Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Bridge
*
*/
/*
*   Copyright (C) 2022 by Bryan Biedenkapp N2PLL
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU Affero General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU Affero General Public License for more details.
*/

using System;
using System.Reflection;

using fnecore.Utility;

namespace dvmbridge
{
    /// <summary>
    /// Static class to build the engine version string.
    /// </summary>
    public class AssemblyVersion
    {
        private static DateTime creationDate = new DateTime(2012, 5, 6);

        /// <summary>Name of the assembly</summary>
        public static string _NAME;

        /// <summary>Constructed full version string.</summary>
        public static string _VERSION;

        /// <summary>Version of the assembly</summary>
        public static SemVersion _SEM_VERSION;

        /// <summary>Build date of the assembly.</summary>
        public static string _BUILD_DATE;

        /// <summary>Copyright string contained within the assembly.</summary>
        public static string _COPYRIGHT;

        /// <summary>Company string contained within the assembly.</summary>
        public static string _COMPANY;

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes static members of the <see cref="AssemblyVersion"/> class.
        /// </summary>
        static AssemblyVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
#if DEBUG
            _SEM_VERSION = new SemVersion(asm, "DEBUG_DNR");
#else
            _SEM_VERSION = new SemVersion(asm);
#endif

            AssemblyProductAttribute asmProd = asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0] as AssemblyProductAttribute;
            AssemblyCopyrightAttribute asmCopyright = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
            AssemblyCompanyAttribute asmCompany = asm.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0] as AssemblyCompanyAttribute;

            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(asm.GetName().Version.Build).AddSeconds(asm.GetName().Version.Revision * 2);
            TimeSpan dateDifference = buildDate - creationDate;

            int totalMonths = (int)Math.Round(Math.Round(dateDifference.TotalDays, MidpointRounding.AwayFromZero) / 12, MidpointRounding.AwayFromZero) + 1;

            _NAME = asmProd.Product;

            _BUILD_DATE = buildDate.ToShortDateString() + " at " + buildDate.ToShortTimeString();

            _COPYRIGHT = asmCopyright.Copyright;
            _COMPANY = asmCompany.Company;

            _VERSION = $"{_NAME} {_SEM_VERSION.ToString()} (Built: {_BUILD_DATE})";
        }
    } // public class AssemblyVersion
} // namespace dvmbridge
