// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public partial class TeamWinScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;

        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        private TourneyVideo blueWinVideo = null!;
        private TourneyVideo redWinVideo = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                blueWinVideo = new TourneyVideo("teamwin-blue")
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                redWinVideo = new TourneyVideo("teamwin-red")
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            currentCompleted.BindValueChanged(_ => update());
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            currentCompleted.UnbindBindings();

            if (match.NewValue == null)
                return;

            currentCompleted.BindTo(match.NewValue.Completed);
            update();
        }

        private bool firstDisplay = true;

        private void update() => Scheduler.AddOnce(() =>
        {
            var match = CurrentMatch.Value;

            if (match?.Winner == null)
            {
                mainContainer.Clear();
                return;
            }

            redWinVideo.Alpha = match.WinnerColour == TeamColour.Red ? 1 : 0;
            blueWinVideo.Alpha = match.WinnerColour == TeamColour.Blue ? 1 : 0;

            if (firstDisplay)
            {
                if (match.WinnerColour == TeamColour.Red)
                    redWinVideo.Reset();
                else
                    blueWinVideo.Reset();
                firstDisplay = false;
            }

            mainContainer.Children = new Drawable[]
            {
                new DrawableTeamFlag(match.Winner)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(40, 190),
                    Scale = new Vector2(2f)
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = 190,
                    X = 400,
                    Spacing = new Vector2(10f),
                    Children = new Drawable[]
                    {
                        new RoundDisplay(match)
                        {
                            Margin = new MarginPadding { Bottom = 10, Left = 10 },
                            Colour = Colour4.FromHex("#ec675d"),
                        },
                        new DrawableTeamTitleWithHeader(match.Winner),
                        new DrawableTeamHeader(Colour4.Black, match.Winner),
                    }
                },
            };
            mainContainer.FadeOut();
            mainContainer.Delay(2000).FadeIn(1600, Easing.OutQuint);
        });
    }
}
