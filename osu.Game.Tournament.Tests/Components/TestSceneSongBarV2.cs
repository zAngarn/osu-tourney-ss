// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    [TestFixture]
    public partial class TestSceneSongBarV2 : TournamentTestScene
    {
        private SongBarV2 songBarV2 = null!;
        private TournamentBeatmap ladderBeatmap = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup picks bans", () =>
            {
                ladderBeatmap = CreateSampleBeatmap();
                Ladder.CurrentMatch.Value!.PicksBansProtects.Add(new BeatmapChoice
                {
                    BeatmapID = ladderBeatmap.OnlineID,
                    Team = TeamColour.Red,
                    Type = ChoiceType.Pick,
                });
            });

            AddStep("create bar", () => Child = songBarV2 = new SongBarV2
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
            AddUntilStep("wait for loaded", () => songBarV2.IsLoaded);
        }

        [Test]
        public void TestSongBar()
        {
            AddStep("set beatmap", () =>
            {
                var beatmap = CreateAPIBeatmap(Ruleset.Value);

                beatmap.CircleSize = 3.4f;
                beatmap.ApproachRate = 6.8f;
                beatmap.OverallDifficulty = 5.5f;
                beatmap.StarRating = 4.56f;
                beatmap.Length = 123456;
                beatmap.BPM = 133;
                beatmap.OnlineID = ladderBeatmap.OnlineID;

                songBarV2.Beatmap = new TournamentBeatmap(beatmap);
                songBarV2.Mods = LegacyMods.None;
                songBarV2.BeatmapChoice = new BeatmapChoice
                {
                    TeamName = "Yuri Enjoyer",
                    Slot = "NM1",
                    Team = TeamColour.Blue,
                };
            });

            AddStep("set mods to HR", () =>
            {
                songBarV2.BeatmapChoice.Slot = "HR1";
                songBarV2.BeatmapChoice.TeamName = "Yuri Enjoyer";
                songBarV2.Mods = LegacyMods.HardRock;
            });
            AddStep("set mods to DT", () =>
            {
                songBarV2.BeatmapChoice.Slot = "DT1";
                songBarV2.BeatmapChoice.TeamName = "Yuri Enjoyer";
                songBarV2.Mods = LegacyMods.DoubleTime;
            });
            AddStep("unset mods", () =>
            {
                songBarV2.BeatmapChoice.Slot = "NM1";
                songBarV2.BeatmapChoice.TeamName = "Yuri Enjoyer";
                songBarV2.Mods = LegacyMods.None;
            });
            AddStep("set warmup", () =>
            {
                songBarV2.BeatmapChoice.Slot = "???";
                songBarV2.BeatmapChoice.TeamName = "offmatch";
                songBarV2.Mods = LegacyMods.None;
            });
            AddStep("set null beatmap", () => songBarV2.Beatmap = null);
        }
    }
}
