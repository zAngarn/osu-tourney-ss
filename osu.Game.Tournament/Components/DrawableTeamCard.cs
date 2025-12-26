// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamCard : CompositeDrawable
    {
        private TournamentTeam team;

        public TournamentTeam Team
        {
            get => team;
            set
            {
                if (team == value) return;

                team = value;
                updateBindings();
            }
        }

        private readonly TournamentSpriteText seed;
        private readonly TournamentSpriteText teamName;
        private readonly TournamentSpriteText avgRank;
        public readonly DrawableTeamFlag TeamFlag;

        public DrawableTeamCard(TournamentTeam team, Colour4 color)
        {
            Margin = new MarginPadding(10);
            this.team = team;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Width = 350,
                    Height = 505,
                    Masking = true,
                    CornerRadius = 20f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = color
                        },
                        new Container
                        {
                            Margin = new MarginPadding(10),
                            RelativeSizeAxes = Axes.Both,
                            Width = 300f / 350f,
                            Height = 475f / 505f,
                            CornerRadius = 15f,
                            Masking = true,
                            Origin = Anchor.BottomRight,
                            Anchor = Anchor.BottomRight,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#262626"),
                                },
                            }
                        },
                        new Container // sin masking para el hack del corner radius
                        {
                            Margin = new MarginPadding(10),
                            RelativeSizeAxes = Axes.Both,
                            Width = 300f / 350f,
                            Height = 475f / 505f,
                            CornerRadius = 15f,
                            Origin = Anchor.BottomRight,
                            Anchor = Anchor.BottomRight,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = color,
                                    Width = 90f / 300f,
                                    Height = 90f / 475f,
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 180f / 300f,
                                    Height = 180f / 475f,
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Masking = true,
                                    CornerRadius = 90f,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Colour4.FromHex("#262626")
                                        },
                                        TeamFlag = new DrawableTeamFlag(team, new Vector2(150), 75)
                                        {
                                            Margin = new MarginPadding(20),
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Width = 55f / 180f,
                                            Height = 55f / 180f,
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            Masking = true,
                                            CornerRadius = 27.5f,
                                            Margin = new MarginPadding { Top = 20, Right = 10 },
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = color
                                                },
                                                seed = new TournamentSpriteText
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Colour = Colour4.FromHex("#262626"),
                                                    Text = team.Seed.Value ?? "???",
                                                    Shadow = false,
                                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                                                }
                                            }
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5),
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Left = 20, Top = 360 },
                                    Children = new Drawable[]
                                    {
                                        teamName = new TournamentSpriteText
                                        {
                                            Colour = color,
                                            Shadow = false,
                                            Text = team.FullName.Value ?? "???",
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 32),
                                        },
                                        avgRank = new TournamentSpriteText
                                        {
                                            Colour = Colour4.White,
                                            Shadow = false,
                                            Text = "#" + team.AverageRank.ToString(CultureInfo.InvariantCulture),
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 40),
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateBindings();
        }

        private void updateBindings()
        {
            if (team.FullName.Value == "???") return;

            team.Seed?.BindValueChanged(v => seed.Text = v.NewValue ?? "???", true);
            team.FullName?.BindValueChanged(v => teamName.Text = v.NewValue ?? "???", true);
            avgRank.Text = team.AverageRank > 0 ? $"#{team.AverageRank}" : "#???";
            TeamFlag.Flag!.Value = team.FlagName.Value;
        }
    }
}
