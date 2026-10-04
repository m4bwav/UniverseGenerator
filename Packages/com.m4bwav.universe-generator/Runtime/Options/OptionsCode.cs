#nullable enable
using System;
using System.Globalization;
using System.Text;

namespace UniverseGeneration
{
    /// <summary>
    /// The text form of <see cref="GeneratorOptions"/>: the settings that differ from the defaults as
    /// <c>name=value</c> pairs joined by <c>&amp;</c>, in a fixed order, such as <c>systems=120&amp;shape=barred</c>. It is
    /// the query part of a link (<c>v1-my-seed/galaxy?systems=120</c>), so a shared address carries its options. Names and
    /// values are lowercase ASCII and never change within a major version; a new setting adds a name.
    /// </summary>
    internal static class OptionsCode
    {
        // The names, in the order ToCode writes them. Add a setting at the end and in both methods below.
        private static readonly string[] Names = { "systems", "shape", "starmix", "weirdness", "planets", "cluster", "epoch", "arms", "lanes", "danger", "names", "require" };

        // The guarantees in the order a code lists them, with their names; flags beyond these are refused by Validate.
        private static readonly (Guarantee Flag, string Name)[] s_guarantees =
        {
            (Guarantee.GardenWorld, "garden"), (Guarantee.OceanWorld, "ocean"), (Guarantee.PrecursorSite, "precursor"),
            (Guarantee.BlueStar, "blue"), (Guarantee.Giant, "giant"), (Guarantee.WhiteDwarf, "whitedwarf"),
            (Guarantee.NeutronStar, "neutron"), (Guarantee.BlackHole, "blackhole"), (Guarantee.SunLikeStar, "sunlike"),
        };

        public static string Write(GeneratorOptions o)
        {
            var d = Preset.Default;
            var text = new StringBuilder();
            if (o.Systems != d.Systems)
            {
                Add(text, "systems", Int(o.Systems));
            }

            if (o.Shape != d.Shape)
            {
                Add(text, "shape", ShapeName(o.Shape));
            }

            if (o.StarMix != d.StarMix)
            {
                Add(text, "starmix", o.StarMix == StarMix.Plausible ? "plausible" : "game");
            }

            if (o.Weirdness != d.Weirdness)
            {
                Add(text, "weirdness", Int(o.Weirdness));
            }

            if (o.MaxPlanetsPerSystem != d.MaxPlanetsPerSystem)
            {
                Add(text, "planets", Int(o.MaxPlanetsPerSystem));
            }

            if (o.ClusterKind != d.ClusterKind)
            {
                Add(text, "cluster", o.ClusterKind == ClusterKind.Group ? "group" : o.ClusterKind == ClusterKind.Cluster ? "cluster" : "auto");
            }

            if (o.Epoch != d.Epoch)
            {
                Add(text, "epoch", EpochName(o.Epoch));
            }

            if (o.Arms.HasValue)
            {
                Add(text, "arms", Int(o.Arms.Value));
            }

            if (o.ExtraLanes != d.ExtraLanes)
            {
                Add(text, "lanes", Int(o.ExtraLanes));
            }

            if (o.DangerShift != d.DangerShift)
            {
                Add(text, "danger", Int(o.DangerShift));
            }

            if (o.Names != d.Names)
            {
                Add(text, "names", o.Names == NameStyle.Invented ? "invented" : "catalogue");
            }

            if (o.Require != d.Require)
            {
                Add(text, "require", GuaranteeNames(o.Require));
            }

            return text.ToString();
        }

        public static GeneratorOptions Read(string code)
        {
            var o = Preset.Default;
            var text = code.Length > 0 && code[0] == '?' ? code.Substring(1) : code;
            if (text.Length == 0)
            {
                return o;
            }

            var seen = new bool[Names.Length];
            foreach (var pair in text.Split('&'))
            {
                var eq = pair.IndexOf('=');
                if (eq <= 0 || eq == pair.Length - 1)
                {
                    throw Bad($"\"{pair}\" is not a name=value pair, as in systems=120.");
                }

                var name = pair.Substring(0, eq);
                var value = pair.Substring(eq + 1);
                var at = Array.IndexOf(Names, name);
                if (at < 0)
                {
                    throw Bad($"\"{name}\" is not a setting; the settings are {string.Join(", ", Names)}.");
                }

                if (seen[at])
                {
                    throw Bad($"\"{name}\" appears twice.");
                }

                seen[at] = true;
                switch (name)
                {
                    case "systems":
                        o = o with { Systems = ReadInt(name, value) };
                        break;
                    case "shape":
                        o = o with { Shape = ReadShape(value) };
                        break;
                    case "starmix":
                        o = o with { StarMix = value == "game" ? StarMix.Game : value == "plausible" ? StarMix.Plausible : throw Choice(name, value, "game, plausible") };
                        break;
                    case "weirdness":
                        o = o with { Weirdness = ReadInt(name, value) };
                        break;
                    case "planets":
                        o = o with { MaxPlanetsPerSystem = ReadInt(name, value) };
                        break;
                    case "cluster":
                        o = o with { ClusterKind = value == "auto" ? ClusterKind.Auto : value == "group" ? ClusterKind.Group : value == "cluster" ? ClusterKind.Cluster : throw Choice(name, value, "auto, group, cluster") };
                        break;
                    case "epoch":
                        o = o with { Epoch = ReadEpoch(value) };
                        break;
                    case "arms":
                        o = o with { Arms = ReadInt(name, value) };
                        break;
                    case "lanes":
                        o = o with { ExtraLanes = ReadInt(name, value) };
                        break;
                    case "danger":
                        o = o with { DangerShift = ReadInt(name, value) };
                        break;
                    case "require":
                        o = o with { Require = ReadGuarantees(value) };
                        break;
                    default:
                        o = o with { Names = value == "catalogue" ? NameStyle.Catalogue : value == "invented" ? NameStyle.Invented : throw Choice(name, value, "catalogue, invented") };
                        break;
                }
            }

            o.Validate();
            return o;
        }

        private static void Add(StringBuilder text, string name, string value)
        {
            if (text.Length > 0)
            {
                text.Append('&');
            }

            text.Append(name).Append('=').Append(value);
        }

        private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static int ReadInt(string name, string value)
        {
            var negative = value[0] == '-';
            var digits = negative ? value.Substring(1) : value;
            if (digits.Length == 0 || digits.Length > 9 || (digits.Length > 1 && digits[0] == '0'))
            {
                throw Bad($"{name} must be a whole number such as 12; got \"{value}\".");
            }

            var v = 0;
            foreach (var c in digits)
            {
                if (c < '0' || c > '9')
                {
                    throw Bad($"{name} must be a whole number such as 12; got \"{value}\".");
                }

                v = v * 10 + (c - '0');
            }

            return negative ? -v : v;
        }

        private static string ShapeName(GalaxyShape shape) => shape switch
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

        private static GalaxyShape ReadShape(string value) => value switch
        {
            "auto" => GalaxyShape.Auto,
            "spiral" => GalaxyShape.Spiral,
            "barred" => GalaxyShape.Barred,
            "elliptical" => GalaxyShape.Elliptical,
            "ring" => GalaxyShape.Ring,
            "irregular" => GalaxyShape.Irregular,
            "colliding" => GalaxyShape.Colliding,
            "starburst" => GalaxyShape.Starburst,
            "clustered" => GalaxyShape.Clustered,
            _ => throw Choice("shape", value, "auto, spiral, barred, elliptical, ring, irregular, colliding, starburst, clustered"),
        };

        private static string EpochName(Epoch epoch) => epoch switch
        {
            Epoch.Young => "young",
            Epoch.Mature => "mature",
            Epoch.Old => "old",
            _ => "auto",
        };

        private static Epoch ReadEpoch(string value) => value switch
        {
            "auto" => Epoch.Auto,
            "young" => Epoch.Young,
            "mature" => Epoch.Mature,
            "old" => Epoch.Old,
            _ => throw Choice("epoch", value, "auto, young, mature, old"),
        };

        // Joined by commas, as in require=garden,blackhole.
        private static string GuaranteeNames(Guarantee require)
        {
            var text = new StringBuilder();
            foreach (var (flag, name) in s_guarantees)
            {
                if ((require & flag) != 0)
                {
                    text.Append(text.Length > 0 ? "," : "").Append(name);
                }
            }

            return text.Length == 0 ? "none" : text.ToString();
        }

        private static Guarantee ReadGuarantees(string value)
        {
            if (value == "none")
            {
                return Guarantee.None;
            }

            var require = Guarantee.None;
            foreach (var part in value.Split(','))
            {
                var found = false;
                foreach (var (flag, name) in s_guarantees)
                {
                    if (part == name)
                    {
                        if ((require & flag) != 0)
                        {
                            throw Bad($"require lists \"{name}\" twice.");
                        }

                        require |= flag;
                        found = true;
                    }
                }

                if (!found)
                {
                    throw Bad($"require takes none or a comma-separated list of garden, ocean, precursor, blue, giant, whitedwarf, neutron, blackhole, sunlike; got \"{part}\".");
                }
            }

            return require;
        }

        private static ArgumentException Choice(string name, string value, string choices) =>
            Bad($"{name} must be one of {choices}; got \"{value}\".");

        private static ArgumentException Bad(string message) => new ArgumentException(message);
    }
}
