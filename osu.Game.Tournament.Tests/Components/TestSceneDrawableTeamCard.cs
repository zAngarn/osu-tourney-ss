// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneDrawableTeamCard : TournamentTestScene
    {
        private readonly TournamentTeam team = new TournamentTeam
        {
            Acronym = { Value = "ESC" },
            FlagName = { Value = "ESC" },
            FullName = { Value = "ESCRUPULILLO" },
            LastYearPlacing = { Value = 10 },
            Seed = { Value = "#12" },
            Players =
            {
                new TournamentUser { Username = "ESCRUPULILLO", Rank = 1200 },
            }
        };

        public TestSceneDrawableTeamCard()
        {
            Children = new Drawable[]
            {
                new DrawableTeamCard(team, Colour4.FromHex("#FF714D"))
            };
        }
    }
}
