// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public partial class TeamWinScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;

        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#262626"),
                },
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                    TriangleScale = 2,
                    Colour = Colour4.FromHex("#282828")
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.2f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.FromHex("#303030"),
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
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

        private void update() => Scheduler.AddOnce(() =>
        {
            var match = CurrentMatch.Value;

            if (match?.Winner == null)
            {
                mainContainer.Clear();
                return;
            }

            Colour4 winnerColour = Color4Extensions.FromHex("#4DDBFF");

            if (match.WinnerColour == TeamColour.Red)
            {
                winnerColour = Color4Extensions.FromHex("#FF714D");
            }

            mainContainer.Children = new Drawable[]
            {
                new DrawableTeamCard(match.Winner, winnerColour)
                {
                    Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Left = -900, Top = 300 },
                },
                new RoundDisplay(match)
                {
                    Margin = new MarginPadding { Left = 80, Top = 50 },
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 250,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = $"¡Ganador! ({match.Team1Score.Value} - {match.Team2Score.Value})",
                            Font = OsuFont.BalooDa.With(size: 80, weight: FontWeight.Black),
                            Margin = new MarginPadding { Bottom = 40 },
                        },
                    }
                },
            };

            mainContainer.FadeOut();
            mainContainer.MoveToOffset(new Vector2(0, 100), 0, Easing.OutQuint);

            using (BeginDelayedSequence(1000))
            {
                mainContainer.MoveToOffset(new Vector2(0, -100), 1600, Easing.OutQuint);
                mainContainer.FadeIn(1600, Easing.OutQuint);
            }
        });
    }
}
