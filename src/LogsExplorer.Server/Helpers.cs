using System.IO;

namespace LogsExplorer.Server
{
    public static class Helpers
    {
        public static string GetEmbeddedResource(string name)
        {
            using (var stream = typeof(ApplicationBuilderExtensions).Assembly.GetManifestResourceStream(name))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }        
    }
}