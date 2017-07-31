using System.IO;
using System.Reflection;

namespace Sejil
{
    public static class Helpers
    {
        public static string GetEmbeddedResource(string name)
        {
            using (var stream = typeof(ApplicationBuilderExtensions).GetTypeInfo().Assembly.GetManifestResourceStream(name))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }        
    }
}