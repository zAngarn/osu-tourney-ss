// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
                }
            };
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            mainContainer.Clear();

            if (match.NewValue == null)
                return;

            const float y_flag_offset = 292;

            const float y_offset = 460;

            mainContainer.Children = new Drawable[]
            {
                new RoundDisplay(match.NewValue)
                {
                    Margin = new MarginPadding { Top = 80 }
                },
                new DrawablePlayerCard(match.NewValue.Team1.Value!, Colour4.FromHex("ed6dac"))
                {
                    Margin = new MarginPadding { Left = 196, Top = 150 },
                    Scale = new Vector2(1.5f),
                },
                new DrawablePlayerCard(match.NewValue.Team2.Value!, Colour4.FromHex("6ddded"))
                {
                    Margin = new MarginPadding { Left = 512, Top = 150 },
                    Scale = new Vector2(1.5f),
                },
                new TournamentSpriteText
                {
                    Text = "vs",
                    Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 112),
                    Margin = new MarginPadding { Left = 648, Top = 240 },
                    Shadow = false,
                },
                new TournamentSpriteText
                {
                    Text = "EMPIEZA",
                    Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 56),
                    Margin = new MarginPadding { Left = 615, Top = 610 },
                    Colour = Colour4.FromHex("757575"),
                    Shadow = false,
                },
                new ScheduleMatchDate(match.NewValue.Date.Value, 40, true)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = 70 }
                }
            };
        }
    }

    public partial class ScheduleMatchDate : DrawableDate
    {
        public ScheduleMatchDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
            : base(date, textSize, italic)
        {
        }

        protected override string Format() => $"{base.Format()}";
    }
}
