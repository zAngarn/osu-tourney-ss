// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamIntro
{
    public partial class TeamIntroScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("teamintro")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            };
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            mainContainer.Clear();

            if (match.NewValue == null)
                return;

            const float x_centro = 600;

            mainContainer.Children = new Drawable[]
            {
                new RoundDisplay(match.NewValue)
                {
                    Position = new Vector2(800, 800),
                    Colour = Colour4.FromHex("#F24998"),
                },
                new TournamentSpriteText
                {
                    Position = new Vector2(x_centro, 150),
                    Text = match.NewValue.Team1.Value!.ToString(),
                    Font = OsuFont.Inter.With(size: 30, weight: FontWeight.Bold),
                    Colour = Colour4.FromHex("#A1548C"),
                },
                new TournamentSpriteText
                {
                    Position = new Vector2(x_centro, 180),
                    Text = match.NewValue.Team1.Value!.AverageRank.ToLocalisableString() + " [#" + match.NewValue.Team1.Value.Seed + "]",
                    Font = OsuFont.Inter.With(size: 16, weight: FontWeight.Bold),
                    Colour = Colour4.FromHex("#A1548C"),
                },
                new DrawableTeamFlag(match.NewValue.Team1.Value)
                {
                    Position = new Vector2(x_centro, 200),
                    Scale = new Vector2(2f),
                },
                new DrawableTeamFlag(match.NewValue.Team2.Value)
                {
                    Position = new Vector2(x_centro, 420),
                    Scale = new Vector2(2f),
                },
                new TournamentSpriteText
                {
                    Position = new Vector2(x_centro, 575),
                    Text = match.NewValue.Team2.Value!.AverageRank.ToLocalisableString() + " [#" + match.NewValue.Team2.Value.Seed + "]",
                    Font = OsuFont.Inter.With(size: 16, weight: FontWeight.Bold),
                    Colour = Colour4.FromHex("#3D6C74"),
                },
                new TournamentSpriteText
                {
                    Position = new Vector2(x_centro, 590),
                    Text = match.NewValue.Team2.Value!.ToString(),
                    Font = OsuFont.Inter.With(size: 30, weight: FontWeight.Bold),
                    Colour = Colour4.FromHex("#3D6C74"),
                },
            };
        }
    }
}
