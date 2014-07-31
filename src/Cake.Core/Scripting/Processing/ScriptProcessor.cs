﻿using System;
using System.Collections.Generic;
using System.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Cake.Core.Scripting.Processing
{
    /// <summary>
    /// Responsible for processing script files.
    /// </summary>
    public sealed class ScriptProcessor : IScriptProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;
        private readonly ScriptLineVisitor _visitor;
        private readonly ICakeLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptProcessor"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="log">The log.</param>
        public ScriptProcessor(IFileSystem fileSystem, ICakeEnvironment environment, ICakeLog log)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            _fileSystem = fileSystem;
            _environment = environment;
            _log = log;
            _visitor = new ScriptLineVisitor(fileSystem, environment);
        }

        /// <summary>
        /// Processes the specified script.
        /// </summary>
        /// <param name="path">The script path.</param>
        /// <param name="context">The context.</param>
        public void Process(FilePath path, ScriptProcessorContext context)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Already processed this script?
            if (context.HasScriptBeenProcessed(path.FullPath))
            {
                _log.Debug("Skipping {0} since it's already processed.", path.GetFilename().FullPath);
                return;
            }

            // Add the script.
            context.MarkScriptAsProcessed(path.MakeAbsolute(_environment).FullPath);

            // Read the source.
            _log.Debug("Processing {0}...", path.GetFilename().FullPath);
            var lines = ReadSource(path);

            // Iterate all lines in the script.
            foreach (var line in lines)
            {
                _visitor.Visit(this, context, path, line);
            }
        }

        private IEnumerable<string> ReadSource(FilePath path)
        {
            path = path.MakeAbsolute(_environment);

            // Get the file and make sure it exist.
            var file = _fileSystem.GetFile(path);
            if (!file.Exists)
            {
                var message = string.Format("Could not find script '{0}'.", path);
                throw new CakeException(message);
            }

            // Read the content from the file.
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream))
            {
                var code = reader.ReadToEnd();
                return string.IsNullOrWhiteSpace(code) 
                    ? new string[] { } 
                    : code.SplitLines();
            }
        }

    }
}
