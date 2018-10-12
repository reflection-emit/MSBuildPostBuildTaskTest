using Newtonsoft.Json;
using System.IO;

namespace PostBuildFileModder
{
    internal sealed class ConfigModel
    {
        private const string ConfigFilename = "postbuildfilemodder.json";

        [JsonProperty("custom")]
        public CustomFunctions[] CustomFunctions { get; set; }

        [JsonProperty("file-filter")]
        public string[] FileFilter { get; set; }

        [JsonProperty("include-subdirs")]
        public bool IncludeSubdirs { get; set; }

        public static ConfigModel Load(string projectDirectory)
        {
            var path = Path.Combine(projectDirectory,ConfigFilename);

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    "The configuration file for the 'PostBuildFileModder' was not found." +
                    $"Please create a json file named '{ConfigFilename}' in the project's root directory.\r\n" +
                    "Example json config:\r\n" +
                    Properties.Resources.ExampleJson);

            return JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(path));
        }
    }

    internal sealed class CustomFunctions
    {
        [JsonProperty("code")]
        public string[] Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}