using System.IO;
using System.Reflection;

namespace Sejil
{
    public static class ResourceHelper
    {
        public static string GetEmbeddedResource(string name)
        {
#if NETSTANDARD1_6
            using (var stream = typeof(ApplicationBuilderExtensions).GetTypeInfo().Assembly.GetManifestResourceStream(name))
#elif NETSTANDARD2_0
            using (var stream = typeof(ApplicationBuilderExtensions).Assembly.GetManifestResourceStream(name))
#endif
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }        
    }
}