// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class MatchScoreDisplay : CompositeDrawable
    {
        private const float bar_height = 18;
        private const float font_size = 50;

        public BindableLong Team1Score = new BindableLong();
        public BindableLong Team2Score = new BindableLong();

        protected MatchScoreCounter Score1Text = null!;
        protected MatchScoreCounter Score2Text = null!;

        private Drawable score1Bar = null!;
        private Drawable score2Bar = null!;

        private Drawable redBgWin = null!;
        private Drawable blueBgWin = null!;
        private Container bgWin = null!;
        private Container bgGris = null!;

        private Drawable redHeadScoreCounter = null!;
        private Drawable blueHeadScoreCounter = null!;

        private MatchScoreDiffCounter scoreDiffText = null!;

        private bool isAnimated = false;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new[]
            {
                bgWin = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 28,
                    Width = 208 / 1920f,
                    Masking = true,
                    CornerRadius = 14,
                    Margin = new MarginPadding { Top = -4 },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        redBgWin = new Box
                        {
                            Name = "bg win animacion rojo",
                            RelativeSizeAxes = Axes.X,
                            Height = 28,
                            Width = 1f,
                            Alpha = 0,
                            Colour = Colour4.FromHex("#FF714D"),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        blueBgWin = new Box
                        {
                            Name = "bg win animacion azul",
                            RelativeSizeAxes = Axes.X,
                            Height = 28,
                            Width = 1f,
                            Alpha = 0,
                            Colour = Colour4.FromHex("#4DDBFF"),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                    }
                },
                bgGris = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 20,
                    Width = 200 / 1920f,
                    Masking = true,
                    CornerRadius = 10,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Name = "top bar gray l (static)",
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Width = 0.5f,
                            Colour = Colour4.FromHex("#282828"),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight
                        },
                        new Box
                        {
                            Name = "top bar grat r (static)",
                            RelativeSizeAxes = Axes.X,
                            Height = 20,
                            Width = 0.5f,
                            Colour = Colour4.FromHex("#282828"),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft
                        },
                    }
                },
                redHeadScoreCounter = new Circle
                {
                    Height = 13f,
                    RelativeSizeAxes = Axes.X,
                    Width = 16 / 1920f,
                    Colour = Colour4.FromHex("#FF714D"),
                    Margin = new MarginPadding { Top = 3 },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                blueHeadScoreCounter = new Circle
                {
                    Height = 13f,
                    RelativeSizeAxes = Axes.X,
                    Width = 16 / 1920f,
                    Colour = Colour4.FromHex("#4DDBFF"),
                    Margin = new MarginPadding { Top = 3 },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                score1Bar = new Box
                {
                    Name = "top bar red",
                    RelativeSizeAxes = Axes.X,
                    Height = 12,
                    Width = 0,
                    Colour = Colour4.FromHex("#FF714D"),
                    Margin = new MarginPadding { Top = 4 },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight
                },
                score2Bar = new Box
                {
                    Name = "top bar blue",
                    RelativeSizeAxes = Axes.X,
                    Height = 12,
                    Width = 0,
                    Colour = Colour4.FromHex("#4DDBFF"),
                    Margin = new MarginPadding { Top = 4 },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = font_size + bar_height,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        Score1Text = new MatchScoreCounter
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                        Score2Text = new MatchScoreCounter
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                    }
                },
                scoreDiffText = new MatchScoreDiffCounter
                {
                    Anchor = Anchor.TopCentre,
                    Margin = new MarginPadding
                    {
                        Top = 0,
                        Horizontal = 8
                    },
                    Alpha = 0
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Team1Score.BindValueChanged(_ => updateScores());
            Team2Score.BindValueChanged(_ => updateScores(), true);
        }

        private void updateScores()
        {
            Score1Text.Current.Value = Team1Score.Value;
            Score2Text.Current.Value = Team2Score.Value;

            int comparison = Team1Score.Value.CompareTo(Team2Score.Value);

            if (comparison > 0)
            {
                Score1Text.Winning = true;
                Score2Text.Winning = false;
            }
            else if (comparison < 0)
            {
                Score1Text.Winning = false;
                Score2Text.Winning = true;
            }
            else
            {
                Score1Text.Winning = false;
                Score2Text.Winning = false;
            }

            var winningBar = Team1Score.Value > Team2Score.Value ? score1Bar : score2Bar;
            var losingBar = Team1Score.Value <= Team2Score.Value ? score1Bar : score2Bar;

            long diff = Math.Max(Team1Score.Value, Team2Score.Value) - Math.Min(Team1Score.Value, Team2Score.Value);

            losingBar.ResizeWidthTo(0, 400, Easing.OutQuint);
            winningBar.ResizeWidthTo(Math.Min(0.05f, MathF.Pow(diff / (1500000f * 16), 0.5f) / 2), 400, Easing.OutQuint);

            scoreDiffText.Alpha = diff != 0 ? 1 : 0;
            scoreDiffText.Current.Value = -diff;
            scoreDiffText.Origin = Team1Score.Value > Team2Score.Value ? Anchor.TopLeft : Anchor.TopRight;
        }

        public void AnimateWin()
        {
            if (isAnimated) return;

            Team1Score.UnbindEvents();
            Team2Score.UnbindEvents();

            var winningBar = Team1Score.Value > Team2Score.Value ? score1Bar : score2Bar;
            var losingBar = Team1Score.Value <= Team2Score.Value ? score1Bar : score2Bar;

            if (winningBar.Colour == Colour4.FromHex("#FF714D"))
            {
                using (BeginDelayedSequence(200))
                {
                    winningBar.ResizeWidthTo(0, 500, Easing.OutQuint);
                    scoreDiffText.FadeOut(300, Easing.OutQuint);
                    redHeadScoreCounter.MoveToOffset(new Vector2(0, 0.5f), 100, Easing.OutQuint);
                    blueHeadScoreCounter.MoveToOffset(new Vector2(0, 0.5f), 100, Easing.OutQuint);

                    using (BeginDelayedSequence(500))
                    {
                        losingBar.FadeOut(500, Easing.OutQuint);
                        blueHeadScoreCounter.FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(1000))
                    {
                        redBgWin.FadeIn(250, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(2000))
                    {
                        bgGris.ResizeWidthTo(24 / 1920f, 500, Easing.OutQuint);
                        bgWin.ResizeWidthTo(36 / 1920f, 500, Easing.OutQuint);
                    }
                }
            }
            else
            {
                using (BeginDelayedSequence(200))
                {
                    winningBar.ResizeWidthTo(0, 500, Easing.OutQuint);
                    scoreDiffText.FadeOut(300, Easing.OutQuint);
                    blueHeadScoreCounter.MoveToOffset(new Vector2(0, 0.5f), 100, Easing.OutQuint);
                    redHeadScoreCounter.MoveToOffset(new Vector2(0, 0.5f), 100, Easing.OutQuint);

                    using (BeginDelayedSequence(500))
                    {
                        losingBar.FadeOut(500, Easing.OutQuint);
                        redHeadScoreCounter.FadeOut(500, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(1000))
                    {
                        blueBgWin.FadeIn(250, Easing.OutQuint);
                    }

                    using (BeginDelayedSequence(2000))
                    {
                        bgGris.ResizeWidthTo(24 / 1920f, 500, Easing.OutQuint);
                        bgWin.ResizeWidthTo(36 / 1920f, 500, Easing.OutQuint);
                    }
                }
            }

            isAnimated = true;
        }

        public void ResetWinTransforms()
        {
            if (!isAnimated) return;

            using (BeginDelayedSequence(200))
            {
                bgGris.ResizeWidthTo(200 / 1920f, 500, Easing.OutQuint);
                bgWin.ResizeWidthTo(208 / 1920f, 500, Easing.OutQuint);

                using (BeginDelayedSequence(1000))
                {
                    blueHeadScoreCounter.MoveToOffset(new Vector2(0, -0.5f), 100, Easing.OutQuint);
                    redHeadScoreCounter.MoveToOffset(new Vector2(0, -0.5f), 100, Easing.OutQuint);
                    blueBgWin.FadeOut(500, Easing.OutQuint);
                    redBgWin.FadeOut(500, Easing.OutQuint);
                }

                using (BeginDelayedSequence(1500))
                {
                    redHeadScoreCounter.FadeIn(500, Easing.OutQuint);
                    blueHeadScoreCounter.FadeIn(500, Easing.OutQuint);
                    score1Bar.FadeIn(500, Easing.OutQuint);
                    score2Bar.FadeIn(500, Easing.OutQuint);
                }

                using (BeginDelayedSequence(2000))
                {
                    updateScores();
                    Team1Score.BindValueChanged(_ => updateScores());
                    Team2Score.BindValueChanged(_ => updateScores(), true);
                    scoreDiffText.FadeIn(300, Easing.OutQuint);
                }
            }

            isAnimated = false;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            Score1Text.X = -160;
            Score2Text.X = 160;
            Score1Text.Y = -8;
            Score2Text.Y = -8;

            redHeadScoreCounter.X = -score1Bar.DrawWidth;
            blueHeadScoreCounter.X = score2Bar.DrawWidth;
        }

        protected partial class MatchScoreCounter : CommaSeparatedScoreCounter
        {
            private OsuSpriteText displayedSpriteText = null!;

            public MatchScoreCounter()
            {
                Margin = new MarginPadding { Top = bar_height, Horizontal = 10 };
            }

            public bool Winning
            {
                set => updateFont(value);
            }

            protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
            {
                displayedSpriteText = s;
                displayedSpriteText.Spacing = new Vector2(-6);
                updateFont(false);
            });

            private void updateFont(bool winning)
                => displayedSpriteText.Font = winning
                    ? OsuFont.Torus.With(weight: FontWeight.Bold, size: font_size * 0.8f, fixedWidth: true)
                    : OsuFont.Torus.With(weight: FontWeight.Regular, size: font_size * 0.6f, fixedWidth: true);
        }

        private partial class MatchScoreDiffCounter : CommaSeparatedScoreCounter
        {
            protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(s =>
            {
                s.Spacing = new Vector2(-2);
                s.Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: bar_height, fixedWidth: true);
            });
        }
    }
}
