// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil;

public static class ResourceHelper
{
    public static string GetEmbeddedResource(string name)
    {
        using var stream = typeof(ApplicationBuilderExtensions).Assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
