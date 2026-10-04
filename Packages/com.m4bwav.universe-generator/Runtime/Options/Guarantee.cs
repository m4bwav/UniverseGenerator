#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>
    /// Things every galaxy must hold at least one of, for <see cref="GeneratorOptions.Require"/>; combine them with
    /// <c>|</c>, as in <c>Guarantee.GardenWorld | Guarantee.BlackHole</c>. A galaxy that already has one changes nothing;
    /// one that lacks it gets it in a system its own <c>constraints</c> stream picks, and every other system stays as it was.
    /// </summary>
    [Flags]
    public enum Guarantee
    {
        /// <summary>Nothing guaranteed (the default).</summary>
        None = 0,

        /// <summary>A garden world, in the temperate zone of a star that can host one.</summary>
        GardenWorld = 1,

        /// <summary>An ocean world in a temperate zone.</summary>
        OceanWorld = 2,

        /// <summary>A precursor site (<see cref="PointOfInterestKind.PrecursorSite"/>).</summary>
        PrecursorSite = 4,

        /// <summary>A blue star: class O or B.</summary>
        BlueStar = 8,

        /// <summary>A giant or supergiant.</summary>
        Giant = 16,

        /// <summary>A white dwarf.</summary>
        WhiteDwarf = 32,

        /// <summary>A neutron star.</summary>
        NeutronStar = 64,

        /// <summary>A stellar black hole.</summary>
        BlackHole = 128,

        /// <summary>A yellow star like the Sun (class G).</summary>
        SunLikeStar = 256,
    }
}
