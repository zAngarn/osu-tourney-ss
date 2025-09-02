// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamIntro;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneTeamIntroScreen : TournamentScreenTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        [BackgroundDependencyLoader]
        private void load()
        {
            setMatchDate(TimeSpan.FromMinutes(4));

            Add(new TeamIntroScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }

        private void setMatchDate(TimeSpan relativeTime)
            // Humanizer cannot handle negative timespans.
            => AddStep($"start time is {relativeTime}", () =>
            {
                var match = CreateSampleMatch();
                match.Date.Value = DateTimeOffset.Now + relativeTime;
                ladder.CurrentMatch.Value = match;
            });
    }
}
