using System.IO;
using Newtonsoft.Json;
using System;

namespace DebugMod.Helpers;

internal static class JsonUtil
{
    private static readonly JsonSerializer _js;

    public static T Deserialize<T>(string embeddedResourcePath)
    {
        using StreamReader sr = new(typeof(JsonUtil).Assembly.GetManifestResourceStream(embeddedResourcePath) ?? throw new ArgumentNullException($"Embedded resource {embeddedResourcePath} missing"));
        using JsonTextReader jtr = new(sr);
        return _js.Deserialize<T>(jtr);
    }
    
    static JsonUtil()
    {
        _js = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };
    }
}
