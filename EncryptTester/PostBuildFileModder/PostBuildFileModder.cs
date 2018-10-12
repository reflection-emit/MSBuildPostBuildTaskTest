using Cauldron;
using Cauldron.Cryptography;
using CSScriptLibrary;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PostBuildFileModder
{
    public sealed class PostBuildFileModder : Task
    {
        private volatile int fileCounter = 0;

        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string ProjectConfiguration { get; set; }

        [Required]
        public string ProjectDirectory { get; set; }

        [Required]
        public string ProjectPlatform { get; set; }

        [Required]
        public string TargetPath { get; set; }

        public override bool Execute()
        {
            try
            {
                var targetAssembly = File.ReadAllBytes(this.TargetPath);
                var fileversion = FileVersionInfo.GetVersionInfo(this.TargetPath);

                var config = ConfigModel.Load(this.ProjectDirectory);
                var files = new ConcurrentBag<string>();

                var customFunctions = new List<(string name, MethodDelegate<string> func)>();

                if (config.CustomFunctions != null && config.CustomFunctions.Length > 0)
                    foreach (var item in config.CustomFunctions)
                    {
                        var code = $"string GetData(string arg){{{string.Join("\r\n", item.Code)}}}";
                        customFunctions.Add(("$" + item.Name, CSScript.CreateFunc<string>(code)));
                    }

                System.Threading.Tasks.Parallel.ForEach(config.FileFilter,
                    fileFilter =>
                        System.Threading.Tasks.Parallel.ForEach(Directory.GetFiles(this.OutputPath, fileFilter, config.IncludeSubdirs ? SearchOption.AllDirectories : SearchOption.AllDirectories),
                            filepath =>
                            {
                                files.Add(filepath);
                                this.Log.LogMessage(filepath);
                                this.fileCounter++;
                            })
                );

                System.Threading.Tasks.Parallel.ForEach(files.ToArray().Distinct(), file =>
                {
                    var parser = new FunctionParser(file);
                    parser.Modify("$datetimenow", x => DateTime.Now.ToString(x));
                    parser.Modify("$projectconfig", x => this.ProjectConfiguration);
                    parser.Modify("$projectplatform", x => this.ProjectPlatform);
                    parser.Modify("$isdebug", x => fileversion.IsDebug ? "true" : "false");
                    parser.Modify("$fileversion", x => fileversion.FileVersion);
                    parser.Modify("$productversion", x => fileversion.ProductVersion);
                    parser.Modify("$productname", x => fileversion.ProductName);
                    parser.Modify("$encrypt", x =>
                    {
                        /* Not secure and sad ... very crapy*/
                        var passphrase = $"{Path.GetFileName(this.TargetPath)}{Encoding.ASCII.GetString(targetAssembly)}{fileversion.FileVersion}{fileversion.ProductName}";
                        return Convert.ToBase64String(Aes.Encrypt(passphrase.GetHash(HashAlgorithms.Sha512).ToSecureString(), x));
                    });

                    foreach (var item in customFunctions)
                        parser.Modify(item.name, x => item.func(x));

                    parser.Save();
                });

                if (this.fileCounter == 0)
                    this.Log.LogWarning("No files found to modify");

                return true;
            }
            catch (Exception e)
            {
                this.Log.LogError(e.Message.ToString());
                return false;
            }
            finally
            {
                this.fileCounter = 0;
            }
        }
    }
}