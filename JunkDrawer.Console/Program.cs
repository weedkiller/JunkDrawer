﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JunkDrawer {

    public class Program {

        private const int ERROR = 1;

        static void Main(string[] args) {

            if (args == null || args.Length == 0) {
                Console.Error.WriteLine("You must pass in the name of a file you'd like to import into your junk drawer.");
                WriteUsage();
                Environment.Exit(ERROR);
            }

            var cfg = LoadConfiguration(args);
            var logger = new JunkLogger(cfg) { Name = Path.GetFileNameWithoutExtension(args[0]) };

            var request = new Request(args[0], cfg, logger);
            if (!request.IsValid) {
                logger.Error(request.Message);
                Environment.Exit(ERROR);
            }

            try {
                new JunkImporter(logger).Import(request);
            } catch (Exception ex) {
                logger.EntityError(request.FileInfo.Name, ex, ex.Message);
            }

        }

        private static JunkCfg LoadConfiguration(IList<string> args) {

            FileInfo fileInfo = null;

            if (args.Count > 1 && File.Exists(args[1])) {
                fileInfo = new FileInfo(args[1]);
            } else {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (path != null) {
                    var def = path.TrimEnd('\\') + "\\default.xml";
                    fileInfo = new FileInfo(def);
                }
            }

            if (fileInfo == null) {
                Console.Error.WriteLine("Could not load JunkDrawer configuration.  Make sure default.xml is located where jd.exe is, or pass in a configuration file as the second argument.");
                Environment.Exit(ERROR);
            }

            var cfg = new JunkCfg(File.ReadAllText(fileInfo.FullName));
            var problems = cfg.Problems();
            if (!problems.Any())
                return cfg;

            foreach (var problem in problems) {
                Console.Error.WriteLine(problem);
            }
            Environment.Exit(ERROR);
            return cfg;
        }

        private static void WriteUsage() {

            Console.WriteLine("Usage: jd.exe <filename> (<config>)");
        }
    }
}
