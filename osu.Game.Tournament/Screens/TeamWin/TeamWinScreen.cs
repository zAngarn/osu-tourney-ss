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

            string scoreString = match.Team1Score.Value + " - " + match.Team2Score.Value;

            var colour = Colour4.FromHex(match.WinnerColour == TeamColour.Blue ? "6ddded" : "ed6dac");

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
                new DrawablePlayerCard(match.Winner, colour) { Scale = new Vector2(2f), X = 200, Y = 220 },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 260,
                    Children = new Drawable[]
                    {
                        new RoundDisplay(match)
                        {
                            Margin = new MarginPadding { Bottom = 100, Left = -20 },
                            Scale = new Vector2(1.2f),
                        },
                        /*new TournamentSpriteText
                        {
                            Text = scoreString,
                            Font = OsuFont.Torus.With(size: 30, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Bottom = 0 },
                        },*/
                    }
                },
            };
            mainContainer.FadeOut();
            mainContainer.Delay(2000).FadeIn(1600, Easing.OutQuint);
        });
    }
}
