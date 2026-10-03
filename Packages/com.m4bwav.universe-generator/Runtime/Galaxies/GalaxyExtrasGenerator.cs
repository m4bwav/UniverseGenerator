#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// The galaxy extras (ai-docs/notes/2026-10-03-galaxy-extras-design.md): factions over lanes, points of interest,
    /// hazards, a monument per region and beacons. Each draws from its own stream of the galaxy's seed and reads only the
    /// finished map, which it never changes, so no system and no earlier golden file moves.
    /// </summary>
    internal static class GalaxyExtrasGenerator
    {
        internal sealed class Extras
        {
            public Faction[] Factions = Array.Empty<Faction>();
            public PointOfInterest[] Points = Array.Empty<PointOfInterest>();
            public GalaxyHazard[] Hazards = Array.Empty<GalaxyHazard>();
            public Monument[] Monuments = Array.Empty<Monument>();
            public Beacon[] Beacons = Array.Empty<Beacon>();
        }

        // Empire, Republic, Corporate, Theocracy, Guild, then Pirates when a pirate faction may be drawn.
        private static readonly int[] s_lawfulKinds = { 25, 20, 20, 15, 20 };
        private static readonly int[] s_anyKinds = { 25, 20, 20, 15, 20, 8 };

        private static readonly string[][] s_factionNames =
        {
            new[] { "{0} Hegemony", "{0} Dominion", "Empire of {0}" },
            new[] { "{0} Republic", "Free Worlds of {0}", "{0} Union" },
            new[] { "{0} Combine", "{0} Consortium", "{0} Syndicate" },
            new[] { "Synod of {0}", "{0} Covenant", "Faithful of {0}" },
            new[] { "{0} Compact", "{0} Guild", "Concord of {0}" },
            new[] { "{0} Raiders", "Free Captains of {0}", "{0} Corsairs" },
        };

        private static readonly string[] s_factionNouns =
        {
            "an empire ruled from", "a republic governed from", "a corporate state run from", "a theocracy ruled from",
            "a guild league based at", "pirate clans based at",
        };

        // Ruins, Wreck, Cache, Anomaly, Outpost, Shrine (precursor sites come only from the trail).
        private static readonly PointOfInterestKind[] s_pointKinds =
        {
            PointOfInterestKind.Ruins, PointOfInterestKind.Wreck, PointOfInterestKind.Cache, PointOfInterestKind.Anomaly,
            PointOfInterestKind.Outpost, PointOfInterestKind.Shrine,
        };

        private static readonly int[] s_pointWeights = { 20, 20, 15, 15, 15, 15 };

        private static readonly string[] s_trails = { "Lantern Path", "Elder Road", "Silent Choir", "Seven Keys", "Ashen Thread", "First Map" };

        private static readonly string[] s_ruinNames = { "the Fallen City", "the Broken Spires", "the Silent Halls", "the Buried Vaults" };

        private static readonly string[] s_ships = { "Valiant", "Morrow", "Ashen Star", "Long Patience", "Kestrel", "Iron Psalm", "Far Promise", "Heron" };

        private static readonly string[][] s_pointTexts =
        {
            new[] { "the ruins of a city older than the lanes", "towers half buried in dust, their makers long gone" },
            new[] { "a warship broken in an old battle for this crossing", "a hulk drifting near the lanes, its logs still intact" },
            new[] { "a hidden store of fuel and parts, unclaimed", "a smugglers' stash, sealed and forgotten" },
            new[] { "a light that moves against the stars", "a pocket of space where instruments disagree" },
            new[] { "an abandoned outpost at the edge of charted space", "a listening post whose crew left in a hurry" },
            new[] { "a pilgrims' shrine, tended by a few", "a shrine where travellers leave tokens for safe passage" },
        };

        private static readonly int[] s_hazardWeights = { 35, 25, 15, 25 };

        private static readonly string[] s_hazardRoots =
        {
            "Veil", "Crimson", "Shroud", "Ember", "Azure", "Hollow", "Wraith", "Lantern", "Ashen", "Pale", "Thorn", "Weeping",
            "Iron", "Gilded", "Sable", "Mourning",
        };

        private static readonly string[] s_hazardNames = { "Nebula", "Storm", "Rift", "Dark" };

        private static readonly string[] s_hazardEffects =
        {
            "a nebula: sensors are blind inside it",
            "a lasting ion storm: shields and drives fail without warning",
            "a rift in space: the lanes shift and travel takes longer",
            "a dark cloud: no starlight, so pilots navigate by beacon",
            "radiation from the active core: shields wear down and crews sicken",
        };

        // ColossalStatue, Necropolis, Archive, Battlefield, GreatGate, Observatory, Shrine, Market, by region theme.
        private static readonly int[] s_monumentDefault = { 1, 1, 1, 1, 1, 1, 1, 1 };

        private static readonly (string Theme, int[] Weights)[] s_monumentThemes =
        {
            ("frontier", new[] { 1, 1, 1, 4, 1, 2, 1, 1 }),
            ("old empire", new[] { 4, 2, 2, 1, 3, 1, 1, 1 }),
            ("pirate haven", new[] { 1, 1, 1, 3, 1, 1, 1, 3 }),
            ("trade corridor", new[] { 1, 1, 1, 1, 2, 1, 1, 4 }),
            ("precursor ruins", new[] { 1, 4, 3, 1, 2, 1, 1, 1 }),
            ("nebula maze", new[] { 1, 1, 1, 1, 1, 4, 2, 1 }),
            ("mining belt", new[] { 2, 1, 1, 1, 1, 1, 2, 3 }),
            ("quarantine zone", new[] { 1, 3, 2, 1, 1, 2, 2, 1 }),
            (GalaxyGenerator.FrontierTheme, new[] { 1, 1, 1, 4, 1, 3, 1, 1 }),
        };

        private static readonly string[] s_monumentNames =
        {
            "the Colossus of {0}", "the {0} Necropolis", "the Archive of {0}", "the {0} Battlefield", "the {0} Arch",
            "the {0} Observatory", "the {0} Sanctum", "the Great Market of {0}",
        };

        private static readonly string[][] s_monumentTexts =
        {
            new[] { "a statue the height of a mountain, of a ruler no one agrees on", "a giant figure carved from a moon, facing the core" },
            new[] { "a city of tombs on a dead world", "catacombs that run through a hollowed moon" },
            new[] { "a vault of records older than any living power", "a library ship anchored here for centuries" },
            new[] { "a field of wrecks from the battle that set the region's borders", "a graveyard of fleets, still salvaged and still mourned" },
            new[] { "a ring of stone and metal wide enough for fleets to pass", "an arch built across the lane by an empire that wanted to be remembered" },
            new[] { "a telescope the size of a city, still charting the galaxy", "an array of dishes that listens to the core" },
            new[] { "a holy place where pilgrims gather from across the region", "a temple built around a light no one can explain" },
            new[] { "a market where the whole region trades", "a bazaar of docked ships that never closes" },
        };

        private static readonly string[] s_beaconNames = { "the {0} Pulsar", "the {0} Lantern", "the {0} Glow", "{0} Light", "the {0} Signal" };

        private static readonly string[] s_beaconTexts =
        {
            "a pulsar whose beat every navigator times by",
            "a giant star bright enough to steer by from anywhere in the galaxy",
            "the glowing disk around a black hole, seen from every system",
            "an ancient navigation light that every chart marks",
            "a tower broadcasting on every channel, heard across the galaxy",
        };

        public static Extras Generate(ulong seed, GalaxyLayout layout, GalaxyRegion[] regions, MapEntry[] map, CoreActivity activity, double coreRadius)
        {
            var n = map.Length;
            var adjacent = new List<int>[n];
            for (var i = 0; i < n; i++)
            {
                adjacent[i] = new List<int>();
            }

            var bridgeEnd = new bool[n];
            foreach (var (a, b, bridge) in layout.Lanes)
            {
                adjacent[a].Add(b);
                adjacent[b].Add(a);
                if (bridge)
                {
                    bridgeEnd[a] = bridgeEnd[b] = true;
                }
            }

            foreach (var list in adjacent)
            {
                list.Sort();
            }

            var extras = new Extras { Factions = Factions(Seeds.Stream(seed, "factions"), regions, map, adjacent) };
            extras.Points = Points(Seeds.Stream(seed, "points"), layout.Core, regions, map, adjacent, bridgeEnd);
            extras.Hazards = Hazards(Seeds.Stream(seed, "hazards"), regions, map, activity, coreRadius);
            extras.Monuments = Monuments(Seeds.Stream(seed, "monuments"), layout.Core, regions, map, extras.Points);
            extras.Beacons = Beacons(Seeds.Stream(seed, "beacons"), map);
            return extras;
        }

        /// <summary>Hops from <paramref name="from"/> to every system over lanes; -1 where no route exists.</summary>
        private static int[] Bfs(List<int>[] adjacent, int from)
        {
            var d = new int[adjacent.Length];
            for (var i = 0; i < d.Length; i++)
            {
                d[i] = -1;
            }

            var queue = new Queue<int>();
            d[from] = 0;
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                foreach (var t in adjacent[s])
                {
                    if (d[t] < 0)
                    {
                        d[t] = d[s] + 1;
                        queue.Enqueue(t);
                    }
                }
            }

            return d;
        }

        private static string Format(string template, string name) => string.Format(CultureInfo.InvariantCulture, template, name);

        /// <summary>
        /// Capitals spread at least 3 hops apart, then breadth-first growth over lanes, one ring per round, factions in
        /// index order; lawful factions stop at danger 9 (plan idea 22). Writes each map entry's faction and border.
        /// </summary>
        private static Faction[] Factions(Pcg32 rng, GalaxyRegion[] regions, MapEntry[] map, List<int>[] adjacent)
        {
            var n = map.Length;
            var depth = 10;
            foreach (var m in map)
            {
                depth = Math.Max(depth, m.Hops);
            }

            var count = n < 8 ? 1 : rng.Range(2, Math.Min(6, 2 + n / 25));
            var pirateCapital = -1;
            if (count >= 2)
            {
                foreach (var r in regions)
                {
                    if (r.Theme == "pirate haven" || r.Theme == GalaxyGenerator.FrontierTheme)
                    {
                        pirateCapital = r.Centre;
                        break;
                    }
                }
            }

            var owner = new int[n];
            var near = new int[n];
            for (var i = 0; i < n; i++)
            {
                owner[i] = -1;
                near[i] = int.MaxValue;
            }

            var kinds = new FactionKind[count];
            var capitals = new int[count];
            var reach = new int[count];
            var variants = new int[count];
            var candidates = new List<int>();
            for (var f = 0; f < count; f++)
            {
                var pirate = f == count - 1 && pirateCapital >= 0;
                kinds[f] = pirate ? FactionKind.Pirates : (FactionKind)rng.Weighted(f == 0 || pirateCapital >= 0 ? s_lawfulKinds : s_anyKinds);
                if (pirate)
                {
                    capitals[f] = pirateCapital;
                }
                else
                {
                    candidates.Clear();
                    var best = -1;
                    for (var i = 0; i < n; i++)
                    {
                        if (owner[i] >= 0 || i == pirateCapital)
                        {
                            continue;
                        }

                        if (f == 0 ? map[i].Danger <= 4 : near[i] >= 3)
                        {
                            candidates.Add(i);
                        }

                        best = Math.Max(best, near[i]);
                    }

                    if (candidates.Count == 0)
                    {
                        for (var i = 0; i < n; i++)
                        {
                            if (owner[i] < 0 && i != pirateCapital && (f == 0 || near[i] == best))
                            {
                                candidates.Add(i);
                            }
                        }
                    }

                    var weights = new int[candidates.Count];
                    for (var c = 0; c < weights.Length; c++)
                    {
                        var danger = map[candidates[c]].Danger;
                        weights[c] = kinds[f] == FactionKind.Pirates ? danger : 11 - danger;
                    }

                    capitals[f] = candidates[rng.Weighted(weights)];
                }

                owner[capitals[f]] = f;
                var hops = Bfs(adjacent, capitals[f]);
                for (var i = 0; i < n; i++)
                {
                    if (hops[i] >= 0 && hops[i] < near[i])
                    {
                        near[i] = hops[i];
                    }
                }

                // Reach in hops, scaled to maps deeper than 10 hops so factions on large maps are not small bubbles.
                reach[f] = (kinds[f] == FactionKind.Empire ? rng.Range(3, 6) : rng.Range(2, 5)) * depth / 10;
                variants[f] = rng.NextInt(3);
            }

            // Growth: no draws, so the result depends only on the capitals, kinds and reaches above.
            var frontier = new List<int>[count];
            var most = 0;
            for (var f = 0; f < count; f++)
            {
                frontier[f] = new List<int> { capitals[f] };
                most = Math.Max(most, reach[f]);
            }

            for (var round = 1; round <= most; round++)
            {
                for (var f = 0; f < count; f++)
                {
                    if (round > reach[f])
                    {
                        continue;
                    }

                    var next = new List<int>();
                    foreach (var s in frontier[f])
                    {
                        foreach (var t in adjacent[s])
                        {
                            if (owner[t] < 0 && (kinds[f] == FactionKind.Pirates || map[t].Danger <= 8))
                            {
                                owner[t] = f;
                                next.Add(t);
                            }
                        }
                    }

                    frontier[f] = next;
                }
            }

            var held = new int[count];
            for (var i = 0; i < n; i++)
            {
                var contested = false;
                if (owner[i] >= 0)
                {
                    held[owner[i]]++;
                    foreach (var t in adjacent[i])
                    {
                        contested |= owner[t] >= 0 && owner[t] != owner[i];
                    }
                }

                map[i] = map[i] with { Faction = owner[i] >= 0 ? owner[i] : (int?)null, Contested = contested };
            }

            var factions = new Faction[count];
            for (var f = 0; f < count; f++)
            {
                var capital = map[capitals[f]].Name;
                var k = (int)kinds[f];
                factions[f] = new Faction
                {
                    Index = f,
                    Name = Format(s_factionNames[k][variants[f]], capital),
                    Kind = kinds[f],
                    Capital = capitals[f],
                    Systems = held[f],
                    Description = s_factionNouns[k] + " " + capital + ", holding " + held[f].ToString(CultureInfo.InvariantCulture) + (held[f] == 1 ? " system" : " systems"),
                };
            }

            return factions;
        }

        /// <summary>The precursor trail across regions, then sites placed by their kind's rules (plan ideas 23 and A6).</summary>
        private static PointOfInterest[] Points(Pcg32 rng, int core, GalaxyRegion[] regions, MapEntry[] map, List<int>[] adjacent, bool[] bridgeEnd)
        {
            var n = map.Length;
            var taken = new bool[n];
            var points = new List<PointOfInterest>();
            var mostHops = 0;
            foreach (var m in map)
            {
                mostHops = Math.Max(mostHops, m.Hops);
            }

            if (n >= 30)
            {
                var length = rng.Range(3, 5);
                var eligible = new List<int>();
                foreach (var r in regions)
                {
                    if (r.Age != StellarAge.Young)
                    {
                        eligible.Add(r.Index);
                    }
                }

                if (eligible.Count >= 2)
                {
                    length = Math.Min(length, eligible.Count);
                    var hops = new Dictionary<int, int[]>();
                    foreach (var r in eligible)
                    {
                        hops[r] = Bfs(adjacent, regions[r].Centre);
                    }

                    var used = new List<int> { eligible[rng.NextInt(eligible.Count)] };
                    while (used.Count < length)
                    {
                        int pick = -1, farthest = -1;
                        foreach (var r in eligible)
                        {
                            if (used.Contains(r))
                            {
                                continue;
                            }

                            var nearest = int.MaxValue;
                            foreach (var u in used)
                            {
                                nearest = Math.Min(nearest, hops[u][regions[r].Centre]);
                            }

                            if (nearest > farthest)
                            {
                                farthest = nearest;
                                pick = r;
                            }
                        }

                        used.Add(pick);
                    }

                    var trail = s_trails[rng.NextInt(s_trails.Length)];
                    var sites = new int[length];
                    var members = new List<int>();
                    for (var k = 0; k < length; k++)
                    {
                        members.Clear();
                        for (var i = 0; i < n; i++)
                        {
                            if (map[i].Region == used[k] && !taken[i])
                            {
                                members.Add(i);
                            }
                        }

                        sites[k] = members[rng.NextInt(members.Count)];
                        taken[sites[k]] = true;
                    }

                    for (var k = 0; k < length; k++)
                    {
                        var step = (k + 1).ToString(CultureInfo.InvariantCulture);
                        var of = length.ToString(CultureInfo.InvariantCulture);
                        points.Add(new PointOfInterest
                        {
                            Kind = PointOfInterestKind.PrecursorSite,
                            System = sites[k],
                            Name = "the " + trail + " at " + map[sites[k]].Name,
                            Text = k + 1 < length
                                ? "site " + step + " of " + of + " of a precursor trail; its markings point to " + map[sites[k + 1]].Name
                                : "the last of " + of + " sites of a precursor trail; whatever it guards is here",
                            ChainStep = k + 1,
                            ChainLength = length,
                        });
                    }
                }
            }

            var coreRegion = map[core].Region;
            var count = Math.Max(1, rng.Range(n / 10, n / 6));
            var weights = new int[n];
            for (var p = 0; p < count; p++)
            {
                var kind = s_pointKinds[rng.Weighted(s_pointWeights)];
                var total = 0;
                for (var i = 0; i < n; i++)
                {
                    var m = map[i];
                    var theme = regions[m.Region].Theme;
                    var w = taken[i] ? 0 : kind switch
                    {
                        PointOfInterestKind.Ruins => regions[m.Region].Age == StellarAge.Young ? 0 : theme == "precursor ruins" ? 3 : 1,
                        PointOfInterestKind.Wreck => m.Chokepoint || bridgeEnd[i] ? 3 : 1,
                        PointOfInterestKind.Cache => m.Chokepoint || m.Region == coreRegion ? 0 : 1,
                        PointOfInterestKind.Anomaly => theme == "nebula maze" || theme == "quarantine zone" ? 3 : 1,
                        PointOfInterestKind.Outpost => m.Hops * 2 >= mostHops ? 3 : 1,
                        _ => 1,
                    };
                    weights[i] = w;
                    total += w;
                }

                // The text and name draws come first, so a kind with no place left still uses the same draws.
                var text = s_pointTexts[Array.IndexOf(s_pointKinds, kind)][rng.NextInt(2)];
                var flavour = rng.NextInt(8);
                if (total == 0)
                {
                    continue;
                }

                var roll = rng.NextInt(total);
                var system = 0;
                while (roll >= weights[system])
                {
                    roll -= weights[system];
                    system++;
                }

                taken[system] = true;
                var at = map[system].Name;
                var name = kind switch
                {
                    PointOfInterestKind.Ruins => s_ruinNames[flavour % s_ruinNames.Length] + " of " + at,
                    PointOfInterestKind.Wreck => "the wreck of the " + s_ships[flavour % s_ships.Length],
                    PointOfInterestKind.Cache => at + " Cache",
                    PointOfInterestKind.Anomaly => "the " + at + " Anomaly",
                    PointOfInterestKind.Outpost => at + " Outpost",
                    _ => "the Shrine of " + at,
                };
                points.Add(new PointOfInterest { Kind = kind, System = system, Name = name, Text = text });
            }

            return points.ToArray();
        }

        /// <summary>Drawn areas, a nebula over every "nebula maze" region, and the active core's radiation.</summary>
        private static GalaxyHazard[] Hazards(Pcg32 rng, GalaxyRegion[] regions, MapEntry[] map, CoreActivity activity, double coreRadius)
        {
            var n = map.Length;
            var hazards = new List<GalaxyHazard>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            GalaxyHazard Make(HazardKind kind, string name, double x, double y, double radius)
            {
                var unique = name;
                for (var serial = 2; !names.Add(unique); serial++)
                {
                    unique = name + " " + serial.ToString(CultureInfo.InvariantCulture);
                }

                var inside = new List<int>();
                for (var i = 0; i < n; i++)
                {
                    double dx = map[i].X - x, dy = map[i].Y - y;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        inside.Add(i);
                    }
                }

                return new GalaxyHazard { Kind = kind, Name = unique, X = x, Y = y, Radius = radius, Systems = inside, Effect = s_hazardEffects[(int)kind] };
            }

            string Name(int kind) => "the " + s_hazardRoots[rng.NextInt(s_hazardRoots.Length)] + " " + s_hazardNames[kind];

            var count = rng.Range(1, 1 + n / 40);
            for (var h = 0; h < count; h++)
            {
                var kind = rng.Weighted(s_hazardWeights);
                var centre = rng.NextInt(n);
                var radius = DMath.Round(rng.Range(80, 220), 0);
                hazards.Add(Make((HazardKind)kind, Name(kind), map[centre].X, map[centre].Y, radius));
            }

            foreach (var r in regions)
            {
                if (r.Theme == "nebula maze")
                {
                    var radius = DMath.Round(rng.Range(120, 220), 0);
                    hazards.Add(Make(HazardKind.Nebula, Name((int)HazardKind.Nebula), map[r.Centre].X, map[r.Centre].Y, radius));
                }
            }

            if (activity != CoreActivity.Quiet && coreRadius > 0)
            {
                hazards.Add(Make(HazardKind.RadiationZone, "the Core Glare", 0, 0, coreRadius));
            }

            return hazards.ToArray();
        }

        /// <summary>One monument per region (plan D24), in a system of the region, of a kind weighted by its theme.</summary>
        private static Monument[] Monuments(Pcg32 rng, int core, GalaxyRegion[] regions, MapEntry[] map, PointOfInterest[] points)
        {
            var busy = new HashSet<int> { core };
            foreach (var p in points)
            {
                busy.Add(p.System);
            }

            var monuments = new Monument[regions.Length];
            var members = new List<int>();
            var free = new List<int>();
            for (var r = 0; r < regions.Length; r++)
            {
                members.Clear();
                free.Clear();
                for (var i = 0; i < map.Length; i++)
                {
                    if (map[i].Region == r)
                    {
                        members.Add(i);
                        if (!busy.Contains(i))
                        {
                            free.Add(i);
                        }
                    }
                }

                var from = free.Count > 0 ? free : members;
                var system = from[rng.NextInt(from.Count)];
                var weights = s_monumentDefault;
                foreach (var (theme, w) in s_monumentThemes)
                {
                    if (theme == regions[r].Theme)
                    {
                        weights = w;
                    }
                }

                var kind = rng.Weighted(weights);
                monuments[r] = new Monument
                {
                    Region = r,
                    System = system,
                    Kind = (MonumentKind)kind,
                    Name = Format(s_monumentNames[kind], regions[r].Name),
                    Text = s_monumentTexts[kind][rng.NextInt(2)],
                };
            }

            return monuments;
        }

        /// <summary>Two to four beacons, the first at a bright star, then spread by farthest point (one under 10 systems, none for one).</summary>
        private static Beacon[] Beacons(Pcg32 rng, MapEntry[] map)
        {
            var n = map.Length;
            if (n < 2)
            {
                return Array.Empty<Beacon>();
            }

            var count = n < 10 ? 1 : rng.Range(2, 4);

            // The first beacon is a bright star (giant, supergiant, neutron star or black hole) when the map has one.
            var bright = new List<int>();
            for (var i = 0; i < n; i++)
            {
                var c = map[i].StarClass;
                if (c == StarClass.Giant || c == StarClass.Supergiant || c == StarClass.NeutronStar || c == StarClass.BlackHole)
                {
                    bright.Add(i);
                }
            }

            var chosen = new List<int> { bright.Count > 0 ? bright[rng.NextInt(bright.Count)] : rng.NextInt(n) };
            while (chosen.Count < count)
            {
                int far = -1;
                var farthest = -1.0;
                for (var i = 0; i < n; i++)
                {
                    var nearest = double.MaxValue;
                    foreach (var c in chosen)
                    {
                        double dx = map[i].X - map[c].X, dy = map[i].Y - map[c].Y;
                        nearest = Math.Min(nearest, dx * dx + dy * dy);
                    }

                    if (nearest > farthest)
                    {
                        farthest = nearest;
                        far = i;
                    }
                }

                chosen.Add(far);
            }

            var beacons = new Beacon[count];
            for (var b = 0; b < count; b++)
            {
                var star = map[chosen[b]].StarClass;
                var kind = star == StarClass.NeutronStar ? BeaconKind.Pulsar
                    : star == StarClass.Giant || star == StarClass.Supergiant ? BeaconKind.BeaconStar
                    : star == StarClass.BlackHole ? BeaconKind.AccretionGlow
                    : rng.Chance(1, 2) ? BeaconKind.NavigationBeacon : BeaconKind.SignalTower;
                beacons[b] = new Beacon
                {
                    System = chosen[b],
                    Kind = kind,
                    Name = Format(s_beaconNames[(int)kind], map[chosen[b]].Name),
                    Text = s_beaconTexts[(int)kind],
                };
            }

            return beacons;
        }
    }
}
