// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneMapPoolScreenV2 : TournamentScreenTestScene
    {
        private PicksBansScreen screen = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(screen = new PicksBansScreen { Width = 1.3f });
        }

        [Test]
        public void TestFewMaps()
        {
            AddStep("load few maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 6; i++)
                    addBeatmap("NM", "NM" + i);

                for (int i = 0; i < 4; i++)
                    addBeatmap("HD", "HD" + i);

                for (int i = 0; i < 4; i++)
                    addBeatmap("HR", "HR" + i);

                for (int i = 0; i < 4; i++)
                    addBeatmap("DT", "DT" + i);
            });
        }

        private void addBeatmap(string mods, string slot)
        {
            Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Add(new RoundBeatmap
            {
                Beatmap = CreateSampleBeatmap(),
                Mods = mods,
                Slot = slot,
            });
        }
    }
}
