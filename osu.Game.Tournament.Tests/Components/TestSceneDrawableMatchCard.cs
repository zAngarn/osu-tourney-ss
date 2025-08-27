// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneDrawableMatchCard : TournamentTestScene
    {
        private TournamentMatch match = null!;

        [Test]
        public void TestBasic()
        {
            AddStep("setup", () =>
            {
                match = new TournamentMatch(
                    new TournamentTeam { FlagName = { Value = "SOR" }, FullName = { Value = "Sorangan" }, Acronym = { Value = "SOR" }, },
                    new TournamentTeam { FlagName = { Value = "ESC" }, FullName = { Value = "ESCRUPULILLO" }, Acronym = { Value = "ESC" }, })
                {
                    Team1Score = { Value = 0 },
                    Team2Score = { Value = 0 },
                };

                Children = new Drawable[]
                {
                    new DrawableMatchCard(match),
                };
            });
        }

        [Test]
        public void TestDrawableMatchCard()
        {
            AddStep("start match", () => match.StartMatch());
            AddStep("change score blue", () => match.Team1Score.Value++);
            AddStep("change score red", () => match.Team2Score.Value++);
            AddStep("reset match", () => match.Reset());
        }
    }
}
