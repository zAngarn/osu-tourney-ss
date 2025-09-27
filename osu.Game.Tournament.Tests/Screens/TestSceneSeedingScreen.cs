// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneSeedingScreen : TournamentScreenTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo
        {
            Teams =
            {
                new TournamentTeam
                {
                    FullName = { Value = "ESCRUPULILLO" },
                    Acronym = { Value = "ESC" },
                    FlagName = { Value = "ESC" },
                    Seed = { Value = "#2" },
                    Players =
                    {
                        new TournamentUser { Username = "Escrupulillo", Rank = 12500, CountryRank = 105 },
                    },
                    SeedingResults =
                    {
                        new SeedingResult
                        {
                            Mod = { Value = "NM" },
                            Seed = { Value = 4 },
                            Beatmaps = new List<SeedingBeatmap>()
                            {
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 100000,
                                    Seed = { Value = 3 }
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 200000,
                                    Seed = { Value = 2 }
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 500000,
                                    Seed = { Value = 1 }
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 1000000,
                                    Seed = { Value = 4 }
                                },
                            }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "HD" },
                            Seed = { Value = 4 },
                            Beatmaps = new List<SeedingBeatmap>()
                            {
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 100000,
                                    Seed = { Value = 3 }
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 200000,
                                    Seed = { Value = 2 }
                                },
                            }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "HR" },
                            Seed = { Value = 4 },
                            Beatmaps = new List<SeedingBeatmap>()
                            {
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 50000,
                                    Seed = { Value = 30 }
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 200000,
                                    Seed = { Value = 2 }
                                },
                            }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "DT" },
                            Seed = { Value = 4 },
                            Beatmaps = new List<SeedingBeatmap>()
                            {
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 100000,
                                    Seed = { Value = 3 }
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 200000,
                                    Seed = { Value = 2 }
                                },
                            }
                        },
                    }
                }
            }
        };

        [Test]
        public void TestBasic()
        {
            AddStep("create seeding screen", () => Add(new SeedingScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            }));

            AddStep("set team to CRUPU", () => this.ChildrenOfType<SettingsTeamDropdown>().Single().Current.Value = ladder.Teams.Single());
        }
    }
}
