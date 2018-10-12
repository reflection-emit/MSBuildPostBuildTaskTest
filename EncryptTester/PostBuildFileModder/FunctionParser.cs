using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PostBuildFileModder
{
    public sealed class FunctionParser
    {
        private readonly string path;
        private string body;

        public FunctionParser(string path)
        {
            this.path = path;
            this.body = File.ReadAllText(path);
        }

        public void Modify(string functionname, Func<string, string> func)
        {
            this.body = Regex.Replace(this.body, "\\" + functionname + @"\((.+?)\)", x =>
            {
                var data = x.Value?.EnclosedIn();

                if (string.IsNullOrEmpty(data))
                    return "";

                return func(data);
            });
        }

        public void Save() => File.WriteAllText(this.path, this.body);
    }
}