// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace Sejil;

public static class ResourceHelper
{
    public static string GetEmbeddedResource(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
