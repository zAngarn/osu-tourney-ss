// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneTournamentBeatmapPanelV2 : TournamentTestScene
    {
        /// <remarks>
        /// Warning: the below API instance is actually the online API, rather than the dummy API provided by the test.
        /// It cannot be trivially replaced because setting <see cref="OsuTestScene.UseOnlineAPI"/> to <see langword="true"/> causes <see cref="OsuTestScene.API"/> to no longer be usable.
        /// </remarks>
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = 3588631 });
            req.Success += success;
            api.Queue(req);
        }

        private void success(APIBeatmap beatmap)
        {
            Add(new TournamentBeatmapPanelV2(new TournamentBeatmap(beatmap), "NM", "NM4")
            {
                Margin = new MarginPadding(10),
                Scale = new Vector2(2.5f)
            });
        }
    }
}
