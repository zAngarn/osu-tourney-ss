// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneSS26Songbar : TournamentTestScene
    {
        private SS26Songbar songbar = null!;
        private TournamentBeatmap ladderBeatmap = null!;

        /// <remarks>
        /// Warning: the below API instance is actually the online API, rather than the dummy API provided by the test.
        /// It cannot be trivially replaced because setting <see cref="OsuTestScene.UseOnlineAPI"/> to <see langword="true"/> causes <see cref="OsuTestScene.API"/> to no longer be usable.
        /// </remarks>
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private APIBeatmap beatmap = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = 3260996 });
            req.Success += success;
            api.Queue(req);
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup picks bans", () =>
            {
                ladderBeatmap = CreateSampleBeatmap();
                Ladder.CurrentMatch.Value!.PicksBans.Add(new BeatmapChoice
                {
                    BeatmapID = ladderBeatmap.OnlineID,
                    Team = TeamColour.Red,
                    Type = ChoiceType.Pick,
                });
            });

            AddStep("create bar", () => Child = songbar = new SS26Songbar
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
            AddStep("set slot", () => songbar.Slot = "NM1");
            AddUntilStep("wait for loaded", () => songbar.IsLoaded);
        }

        [Test]
        public void TestSongBar()
        {
            AddStep("putos tests auto", () => Console.WriteLine("ye"));
            AddStep("set beatmap", () =>
            {
                songbar.Beatmap = new TournamentBeatmap(beatmap);
            });
        }

        private void success(APIBeatmap beatmap)
        {
            this.beatmap = beatmap;
        }
    }
}
