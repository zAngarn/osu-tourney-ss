// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
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

            Colour4 colorTexto = match.WinnerColour == TeamColour.Red ? Colour4.FromHex("#A1548C") : Colour4.FromHex("#3D6C74");

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
                    Position = new Vector2(0, 0),
                    Scale = new Vector2(4f),
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 0,
                    Y = 190,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Colour = colorTexto,
                            Font = OsuFont.Inter.With(size: 16, weight: FontWeight.Regular),
                            Text = match.Round.Value!.ToString(),
                        },
                        new TournamentSpriteText
                        {
                            Colour = colorTexto,
                            Font = OsuFont.Inter.With(size: 36, weight: FontWeight.Bold),
                            Text = match.Winner.ToString(),
                        }
                    }
                },
            };
            mainContainer.FadeOut();
            mainContainer.Delay(2000).FadeIn(1600, Easing.OutQuint);
        });
    }
}
