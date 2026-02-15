// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TeamDisplay : DrawableTournamentTeam
    {
        private readonly TeamScore score;

        private readonly TournamentSpriteText teamNameText;

        private readonly TournamentSpriteText teamRankText;

        private readonly Bindable<string> teamName = new Bindable<string>("???");

        private bool showScore;

        public bool ShowScore
        {
            get => showScore;
            set
            {
                if (showScore == value)
                    return;

                showScore = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        public TeamDisplay(TournamentTeam? team, TeamColour colour, Bindable<int?> currentTeamScore, int pointsToWin)
            : base(team)
        {
            AutoSizeAxes = Axes.Both;

            bool flip = colour == TeamColour.Red;

            Colour4 color = Colour4.FromHex("#FF714D");
            MarginPadding marginPaddingScores = new MarginPadding { Left = -20, Top = 14 };

            if (colour == TeamColour.Blue)
            {
                color = Colour4.FromHex("4DDBFF");
                marginPaddingScores = new MarginPadding { Right = -20, Top = 14 };
            }

            var anchor = flip ? Anchor.TopLeft : Anchor.TopRight;

            Flag.RelativeSizeAxes = Axes.None;
            Flag.Origin = anchor;
            Flag.Scale = TournamentGame.FACTOR_DE_REESCALADO_1080;
            Flag.Anchor = anchor;

            Margin = new MarginPadding(55);

            InternalChild = new Container
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            Flag,
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Origin = anchor,
                                Anchor = anchor,
                                Spacing = new Vector2(-5),
                                Children = new Drawable[]
                                {
                                    teamNameText = new TournamentSpriteText
                                    {
                                        Font = OsuFont.BalooDa.With(weight: FontWeight.Black, size: 36),
                                        Colour = color,
                                        Origin = anchor,
                                        Anchor = anchor,
                                        Margin = new MarginPadding { Top = 30 }
                                    },
                                    teamRankText = new TournamentSpriteText
                                    {
                                        Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 48),
                                        Colour = Colour4.White,
                                        Origin = anchor,
                                        Anchor = anchor,
                                        Margin = new MarginPadding { Top = -10 },
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(5),
                                        Origin = anchor,
                                        Anchor = anchor,
                                        Children = new Drawable[]
                                        {
                                            score = new TeamScore(currentTeamScore, colour, pointsToWin)
                                            {
                                                Origin = anchor,
                                                Anchor = anchor,
                                                Margin = marginPaddingScores,
                                            }
                                        }
                                    },
                                }
                            },
                            new DrawableTeamSeed(Team)
                            {
                                Scale = new Vector2(0.5f),
                                Origin = anchor,
                                Anchor = anchor,
                            },
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateDisplay();
            FinishTransforms(true);

            if (Team != null)
                teamName.BindTo(Team.FullName);

            teamName.BindValueChanged(name =>
            {
                teamNameText.Text = name.NewValue;
                teamRankText.Text = $"#{Team?.AverageRank.ToString("####") ?? "0"}";
            }, true);
        }

        private void updateDisplay()
        {
            score.FadeTo(ShowScore ? 1 : 0, 200);
        }
    }
}
