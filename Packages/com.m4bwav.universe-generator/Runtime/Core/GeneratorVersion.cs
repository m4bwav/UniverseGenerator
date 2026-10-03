#nullable enable
namespace UniverseGeneration
{
    /// <summary>
    /// The generator version is part of every seed's identity (plan D4). Within one version a seed gives the same output
    /// forever; a change that would alter any seed's output adds a version and keeps the old one selectable.
    /// </summary>
    internal static class GeneratorVersion
    {
        public const int Current = 1;

        public static bool IsSupported(int version) => version == 1;
    }
}
