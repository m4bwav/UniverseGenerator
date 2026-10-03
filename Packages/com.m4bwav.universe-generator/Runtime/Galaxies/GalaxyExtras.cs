#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>What kind of power a faction is.</summary>
    public enum FactionKind
    {
        /// <summary>Ruled by one throne; reaches farthest.</summary>
        Empire,

        /// <summary>Worlds that elect their rulers.</summary>
        Republic,

        /// <summary>A company that owns its worlds.</summary>
        Corporate,

        /// <summary>Ruled by a faith.</summary>
        Theocracy,

        /// <summary>A league of traders, miners or pilots.</summary>
        Guild,

        /// <summary>Raiders; the only faction that takes the most dangerous systems.</summary>
        Pirates,
    }

    /// <summary>A power that holds systems of a galaxy, grown over its lanes from a capital (plan idea 22).</summary>
    public sealed record Faction
    {
        /// <summary>Its index in <see cref="Galaxy.Factions"/>, as in <see cref="MapEntry.Faction"/>.</summary>
        public int Index { get; init; }

        /// <summary>Its name, such as "Vega Hegemony"; unique in the galaxy.</summary>
        public string Name { get; init; } = "";

        /// <summary>What kind of power it is.</summary>
        public FactionKind Kind { get; init; }

        /// <summary>The index of its capital system.</summary>
        public int Capital { get; init; }

        /// <summary>How many systems it holds, its capital included.</summary>
        public int Systems { get; init; }

        /// <summary>One line in plain words, such as "an empire ruled from Vega, holding 14 systems".</summary>
        public string Description { get; init; } = "";
    }

    /// <summary>What a point of interest is.</summary>
    public enum PointOfInterestKind
    {
        /// <summary>The ruins of an old settlement; only where the stars are old enough for history.</summary>
        Ruins,

        /// <summary>A site of a precursor trail that crosses the galaxy, each site pointing to the next.</summary>
        PrecursorSite,

        /// <summary>A wreck from a battle; battles happen at chokepoints and bridges.</summary>
        Wreck,

        /// <summary>A hidden cache of supplies; never at a chokepoint or in the core region.</summary>
        Cache,

        /// <summary>Something sensors cannot explain.</summary>
        Anomaly,

        /// <summary>An abandoned outpost, mostly near the edge of the map.</summary>
        Outpost,

        /// <summary>A pilgrims' shrine.</summary>
        Shrine,
    }

    /// <summary>A site the galaxy's story places in one system (plan ideas 23 and A6).</summary>
    public sealed record PointOfInterest
    {
        /// <summary>What it is.</summary>
        public PointOfInterestKind Kind { get; init; }

        /// <summary>The index of its system; no system holds two points of interest.</summary>
        public int System { get; init; }

        /// <summary>Its name, such as "the Broken Spires of Vega".</summary>
        public string Name { get; init; } = "";

        /// <summary>One line in plain words.</summary>
        public string Text { get; init; } = "";

        /// <summary>Its step on the precursor trail, from 1; 0 when it is not on the trail.</summary>
        public int ChainStep { get; init; }

        /// <summary>How many sites the precursor trail has; 0 when it is not on the trail.</summary>
        public int ChainLength { get; init; }
    }

    /// <summary>What a hazard area is.</summary>
    public enum HazardKind
    {
        /// <summary>A nebula: sensors are blind inside it.</summary>
        Nebula,

        /// <summary>A lasting ion storm: shields and drives fail.</summary>
        IonStorm,

        /// <summary>A rift in space: lanes shift and travel is slower.</summary>
        GravityRift,

        /// <summary>A dark cloud: no starlight, so pilots navigate by beacon.</summary>
        DarkCloud,

        /// <summary>Radiation from an active galactic core.</summary>
        RadiationZone,
    }

    /// <summary>An area of a galaxy where travel is harder. It does not change any system's danger.</summary>
    public sealed record GalaxyHazard
    {
        /// <summary>What it is.</summary>
        public HazardKind Kind { get; init; }

        /// <summary>Its name, such as "the Veil Nebula"; unique in the galaxy.</summary>
        public string Name { get; init; } = "";

        /// <summary>Its centre across, in game units.</summary>
        public double X { get; init; }

        /// <summary>Its centre up, in game units.</summary>
        public double Y { get; init; }

        /// <summary>Its radius in game units.</summary>
        public double Radius { get; init; }

        /// <summary>The indexes of the systems inside it, in order.</summary>
        public IReadOnlyList<int> Systems { get; init; } = Array.Empty<int>();

        /// <summary>What it does to travellers, in plain words.</summary>
        public string Effect { get; init; } = "";
    }

    /// <summary>What a region's monument is (plan D24).</summary>
    public enum MonumentKind
    {
        /// <summary>A giant statue of a ruler or hero.</summary>
        ColossalStatue,

        /// <summary>A city of the dead.</summary>
        Necropolis,

        /// <summary>A vault of records.</summary>
        Archive,

        /// <summary>The field of a famous battle.</summary>
        Battlefield,

        /// <summary>A vast arch across a lane.</summary>
        GreatGate,

        /// <summary>A great telescope.</summary>
        Observatory,

        /// <summary>A holy place.</summary>
        Shrine,

        /// <summary>A market known across the galaxy.</summary>
        Market,
    }

    /// <summary>The one monument of a region (plan D24), chosen to fit the region's theme.</summary>
    public sealed record Monument
    {
        /// <summary>The index of its region in <see cref="Galaxy.Regions"/>.</summary>
        public int Region { get; init; }

        /// <summary>The index of its system, which lies in the region.</summary>
        public int System { get; init; }

        /// <summary>What it is.</summary>
        public MonumentKind Kind { get; init; }

        /// <summary>Its name, such as "the Colossus of Vela Reach".</summary>
        public string Name { get; init; } = "";

        /// <summary>One line in plain words.</summary>
        public string Text { get; init; } = "";
    }

    /// <summary>What a beacon is.</summary>
    public enum BeaconKind
    {
        /// <summary>A neutron star's pulse.</summary>
        Pulsar,

        /// <summary>A giant or supergiant star, bright across the galaxy.</summary>
        BeaconStar,

        /// <summary>The glowing disk around a black hole.</summary>
        AccretionGlow,

        /// <summary>An ancient navigation light.</summary>
        NavigationBeacon,

        /// <summary>A tower broadcasting on every channel.</summary>
        SignalTower,
    }

    /// <summary>A beacon seen or heard from anywhere in its galaxy (plan D24); beacons are spread across the map.</summary>
    public sealed record Beacon
    {
        /// <summary>The index of its system.</summary>
        public int System { get; init; }

        /// <summary>What it is; a star beacon always matches the system's star.</summary>
        public BeaconKind Kind { get; init; }

        /// <summary>Its name, such as "the Vega Pulsar".</summary>
        public string Name { get; init; } = "";

        /// <summary>One line in plain words.</summary>
        public string Text { get; init; } = "";
    }
}
