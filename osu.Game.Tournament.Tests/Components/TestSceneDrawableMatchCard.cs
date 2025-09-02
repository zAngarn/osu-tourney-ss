// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneDrawableMatchCard : TournamentTestScene
    {
        private TournamentMatch match = null!;
        private DrawableMatchCard card = null!;

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
                    Round = { Value = new TournamentRound { Name = { Value = "???" }, BestOf = { Value = 13 } } },
                };

                Children = new Drawable[]
                {
                    card = new DrawableMatchCard(match)
                    {
                        Scale = new Vector2(2f),
                    },
                    new TournamentSpriteText
                    {
                        Text = match.Round.Value.BestOf.Value.ToString()
                    }
                };
            });
        }

        [Test]
        public void TestDrawableMatchCard()
        {
            AddStep("change score red", () => match.Team1Score.Value++);
            AddStep("change score blue", () => match.Team2Score.Value++);
            AddStep("show scores", () => card.Started = true);
            AddStep("hide scores", () => card.Started = false);
        }
    }
}
