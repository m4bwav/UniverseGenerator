#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>The kinds of soft problem the generator reports instead of throwing.</summary>
    public enum WarningCode
    {
        /// <summary>A galaxy's shape could not hold every system asked for, so it has fewer.</summary>
        SystemsTrimmed,

        /// <summary><see cref="GeneratorOptions.Arms"/> was set for a galaxy whose shape has no arms, so it changed nothing.</summary>
        ArmsIgnored,

        /// <summary>A spiral or barred shape was asked for with fewer than 80 systems, where arms do not read on a map.</summary>
        ShapeTooSmall,

        /// <summary>A system's name had to take a number to stay unique in its galaxy.</summary>
        NameNumbered,
    }

    /// <summary>A soft problem: generation went on, and this says what was changed or ignored, and why.</summary>
    public sealed record GeneratorWarning
    {
        /// <summary>The kind of problem.</summary>
        public WarningCode Code { get; init; }

        /// <summary>What happened, in words, naming the setting or object.</summary>
        public string Message { get; init; } = "";

        /// <summary>The message.</summary>
        public override string ToString() => Message;
    }

    /// <summary>The soft checks shared by <see cref="GeneratorOptions.Check"/> and the galaxy level.</summary>
    internal static class Diagnostics
    {
        /// <summary>Spirals and bars read on a map only from this many systems (plan D16, Stage 1 contact sheets).</summary>
        public const int FirstSpiralCount = 80;

        public static void Shape(List<GeneratorWarning> warnings, GalaxyShape requested, int systems, int? arms)
        {
            var spiral = requested == GalaxyShape.Spiral || requested == GalaxyShape.Barred;
            if (spiral && systems < FirstSpiralCount)
            {
                warnings.Add(new GeneratorWarning
                {
                    Code = WarningCode.ShapeTooSmall,
                    Message = $"A {Name(requested)} galaxy of {systems} systems: arms read on a map only from {FirstSpiralCount} systems.",
                });
            }

            if (arms.HasValue && requested != GalaxyShape.Auto && !spiral)
            {
                warnings.Add(new GeneratorWarning
                {
                    Code = WarningCode.ArmsIgnored,
                    Message = $"Arms = {arms.Value} changes nothing: a {Name(requested)} galaxy has no arms.",
                });
            }
            else if (arms.HasValue && requested == GalaxyShape.Auto && systems < FirstSpiralCount)
            {
                warnings.Add(new GeneratorWarning
                {
                    Code = WarningCode.ArmsIgnored,
                    Message = $"Arms = {arms.Value} changes nothing: under {FirstSpiralCount} systems, Auto picks an elliptical, ring or irregular galaxy.",
                });
            }
        }

        private static string Name(GalaxyShape shape) => shape switch
        {
            GalaxyShape.Spiral => "spiral",
            GalaxyShape.Barred => "barred",
            GalaxyShape.Elliptical => "elliptical",
            GalaxyShape.Ring => "ring",
            GalaxyShape.Irregular => "irregular",
            GalaxyShape.Colliding => "colliding",
            GalaxyShape.Starburst => "starburst",
            GalaxyShape.Clustered => "clustered",
            _ => "auto",
        };
    }
}
