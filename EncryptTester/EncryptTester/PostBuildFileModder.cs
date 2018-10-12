using Cauldron;
using Cauldron.Cryptography;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;

namespace EncryptTester
{
    internal static class PostBuildFileModder
    {
        private static readonly SecureString passphrase;

        static PostBuildFileModder()
        {
            var path = typeof(PostBuildFileModder).Assembly.Location;
            var assemblyBinary = File.ReadAllBytes(path);
            var assemblyFileVersion = FileVersionInfo.GetVersionInfo(path);

            passphrase = $"{Path.GetFileName(path)}{Encoding.ASCII.GetString(assemblyBinary)}{assemblyFileVersion.FileVersion}{assemblyFileVersion.ProductName}"
                .GetHash(HashAlgorithms.Sha512)
                .ToSecureString();
        }

        public static SecureString Decrypt(string data) =>
            Encoding.UTF8.GetString(Aes.Decrypt(passphrase, Convert.FromBase64String(data))).ToSecureString();
    }

    internal sealed class EncryptedToSecureString : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(SecureString);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(SecureString))
                throw new JsonException($"Unable to convert '{objectType.FullName}' to '{typeof(SecureString).FullName}'");

            var encrypted = reader.Value as string;

            if (string.IsNullOrEmpty(encrypted))
                return null;

            return PostBuildFileModder.Decrypt(encrypted);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // No writing ...
        }
    }
}