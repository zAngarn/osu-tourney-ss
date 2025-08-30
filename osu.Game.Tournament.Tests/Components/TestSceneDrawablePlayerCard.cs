// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneDrawablePlayerCard : TournamentTestScene
    {
        private DrawablePlayerCard playerCard;
        private TournamentSpriteText winnerTexto;

        [Test]
        [Ignore("roto")]
        public void TestBasic()
        {
            AddStep("setup", () =>
            {
                var team = new TournamentTeam
                {
                    FlagName = { Value = "ESC" },
                    FullName = { Value = "Escrupulillo" },
                    Seed = { Value = "#5" },
                    Players =
                    {
                        new TournamentUser { Username = "Escrupulillo", Rank = 12500, CountryRank = 105 },
                    },
                };

                Children = new Drawable[]
                {
                    winnerTexto = new TournamentSpriteText
                    {
                        Text = "WINNER!",
                        Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 100),
                        Margin = new MarginPadding { Top = 300, Left = 200 }
                    },
                    /*playerCard = new DrawablePlayerCard(team, Colour4.Aqua)
                    {
                        Scale = new Vector2(3),
                    },*/
                };
            });
        }
    }
}
