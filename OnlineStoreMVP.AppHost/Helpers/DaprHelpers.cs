using System.Collections.Immutable;

namespace OnlineStoreMVP.AppHost.Helpers;

internal static class DaprHelpers
{
    public static ImmutableHashSet<string> GetDaprComponentYamlPath()
    {
        // runtime base path of the executing assembly (works for publish and run)
        var basePath = AppContext.BaseDirectory;

        // primary relative folder where your YAML components live inside the project
        var relativeYamlFolder = Path.Combine("Configurations", "dapr-components");

        // combine to get the runtime folder where the YAML files should be located
        var yamlFolder = Path.Combine(basePath, relativeYamlFolder);

        // fallback: when running from IDE some paths differ � try traversing up to project layout
        if (!Directory.Exists(yamlFolder))
        {
            var alt = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", relativeYamlFolder));
            if (Directory.Exists(alt))
            {
                yamlFolder = alt;
            }
        }

        // optional runtime check (log or throw as appropriate)
        if (!Directory.Exists(yamlFolder))
        {
            // decide whether to throw or log; here we throw so you'll notice missing files during startup
            throw new DirectoryNotFoundException($"Dapr YAML folder not found: {yamlFolder}");
        }

        return [yamlFolder];
    }
}
