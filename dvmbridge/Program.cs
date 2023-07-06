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
using System.IO;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using FneLogLevel = dvmbridge.FNE.LogLevel;
using dvmbridge.FNE.Utility;

namespace dvmbridge
{
    /// <summary>
    /// 
    /// </summary>
    public enum ERRNO : int
    {
        /// <summary>
        /// No error
        /// </summary>
        ENOERR = 0,
        /// <summary>
        /// Bad commandline options
        /// </summary>
        EBADOPTIONS = 1,
        /// <summary>
        /// Missing configuration file
        /// </summary>
        ENOCONFIG = 2
    } // public enum ERRNO : int

    /// <summary>
    /// This class serves as the entry point for the application.
    /// </summary>
    public class Program
    {
        private static ConfigurationObject config;

        /*
        ** Properties
        */

        /// <summary>
        /// Gets the instance of the <see cref="ConfigurationObject"/>.
        /// </summary>
        public static ConfigurationObject Configuration => config;

        /// <summary>
        /// Gets the <see cref="fnecore.LogLevel"/>.
        /// </summary>
        public static FneLogLevel FneLogLevel
        {
            get;
            private set;
        } = FneLogLevel.INFO;

        /*
        ** Methods
        */

        /// <summary>
        /// Internal helper to prints the program usage.
        /// </summary>
        private static void Usage(OptionSet p)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string fileName = Path.GetFileName(assembly.Location);

            Console.WriteLine(AssemblyVersion._VERSION);
            Console.WriteLine(AssemblyVersion._COPYRIGHT + "., All Rights Reserved.");
            Console.WriteLine();

            Console.WriteLine(string.Format("usage: {0} [-h | --help] [-c | --config <path to configuration file>] [-l | --log-on-console]",
                Path.GetFileNameWithoutExtension(fileName)));
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config =>
                    {
                        config.ClearProviders();
                        config.AddProvider(new SerilogLoggerProvider(Log.Logger));
                    });
                    services.AddHostedService<Service>();
                });

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            const string defaultConfigFile = "config.yml";
            bool showHelp = false, showLogOnConsole = false;
            string configFile = string.Empty;

            // command line parameters
            OptionSet options = new OptionSet()
            {
                { "h|help", "show this message and exit", v => showHelp = v != null },
                { "c=|config=", "sets the path to the configuration file", v => configFile = v },
                { "l|log-on-console", "shows log on console", v => showLogOnConsole = v != null },
            };

            // attempt to parse the commandline
            try
            {
                options.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("error: invalid arguments");
                Usage(options);
                Environment.Exit((int)ERRNO.EBADOPTIONS);
            }

            // show help?
            if (showHelp)
            {
                Usage(options);
                Environment.Exit((int)ERRNO.ENOERR);
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            string executingPath = Path.GetDirectoryName(assembly.Location);

            // do we some how have a "null" config file?
            if (configFile == null)
            {
                if (File.Exists(Path.Combine(new string[] { executingPath, defaultConfigFile })))
                    configFile = Path.Combine(new string[] { executingPath, defaultConfigFile });
                else
                {
                    Console.WriteLine("error: cannot read the configuration file");
                    Environment.Exit((int)ERRNO.ENOCONFIG);
                }
            }

            // do we some how have a empty config file?
            if (configFile == string.Empty)
            {
                if (File.Exists(Path.Combine(new string[] { executingPath, defaultConfigFile })))
                    configFile = Path.Combine(new string[] { executingPath, defaultConfigFile });
                else
                {
                    Console.WriteLine("error: cannot read the configuration file");
                    Environment.Exit((int)ERRNO.ENOCONFIG);
                }
            }

            try
            {
                using (FileStream stream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (TextReader reader = new StreamReader(stream))
                    {
                        string yml = reader.ReadToEnd();

                        // setup the YAML deseralizer for the configuration
                        IDeserializer ymlDeserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();

                        config = ymlDeserializer.Deserialize<ConfigurationObject>(yml);
                    }
                }
            }
            catch
            {
                Console.WriteLine($"error: cannot read the configuration file, {configFile}");
                Environment.Exit((int)ERRNO.ENOCONFIG);
            }

            // setup logging configuration
            LoggerConfiguration logConfig = new LoggerConfiguration();
            logConfig.MinimumLevel.Debug();
            const string logTemplate = "{Level:u1}: {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}{Exception}";

            // setup file logging
            LogEventLevel fileLevel = LogEventLevel.Information;
            switch (config.Log.FileLevel)
            {
                case 1:
                    fileLevel = LogEventLevel.Debug;
                    FneLogLevel = FneLogLevel.DEBUG;
                    break;
                case 2:
                case 3:
                default:
                    fileLevel = LogEventLevel.Information;
                    FneLogLevel = FneLogLevel.INFO;
                    break;
                case 4:
                    fileLevel = LogEventLevel.Warning;
                    FneLogLevel = FneLogLevel.WARNING;
                    break;
                case 5:
                    fileLevel = LogEventLevel.Error;
                    FneLogLevel = FneLogLevel.ERROR;
                    break;
                case 6:
                    fileLevel = LogEventLevel.Fatal;
                    FneLogLevel = FneLogLevel.FATAL;
                    break;
            }

            logConfig.WriteTo.File(Path.Combine(new string[] { config.Log.FilePath, config.Log.FileRoot + "-.log" }), fileLevel, logTemplate, rollingInterval: RollingInterval.Day);

            // setup console logging
            if (showLogOnConsole)
            {
                LogEventLevel dispLevel = LogEventLevel.Information;
                switch (config.Log.DisplayLevel)
                {
                    case 1:
                        dispLevel = LogEventLevel.Debug;
                        FneLogLevel = FneLogLevel.DEBUG;
                        break;
                    case 2:
                    case 3:
                    default:
                        dispLevel = LogEventLevel.Information;
                        FneLogLevel = FneLogLevel.INFO;
                        break;
                    case 4:
                        dispLevel = LogEventLevel.Warning;
                        FneLogLevel = FneLogLevel.WARNING;
                        break;
                    case 5:
                        dispLevel = LogEventLevel.Error;
                        FneLogLevel = FneLogLevel.ERROR;
                        break;
                    case 6:
                        dispLevel = LogEventLevel.Fatal;
                        FneLogLevel = FneLogLevel.FATAL;
                        break;
                }

                logConfig.WriteTo.Console(dispLevel, logTemplate);
            }

            // initialize logger
            Log.Logger = logConfig.CreateLogger();

            Log.Logger.Information(AssemblyVersion._VERSION);
            Log.Logger.Information(AssemblyVersion._COPYRIGHT + "., All Rights Reserved.");

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "An unhandled exception occurred"); // TODO: make this less terse
            }
        }
    } // public class Program
} // namespace dvmbridge
