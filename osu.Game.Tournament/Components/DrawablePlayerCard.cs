// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawablePlayerCard : CompositeDrawable
    {
        private Container trianglesBox;
        protected TrianglesV2? Triangles { get; private set; }

        private readonly TournamentSpriteText seed;
        private readonly TournamentSpriteText teamName;
        private readonly TournamentSpriteText avgRank;
        private readonly TournamentSpriteText countryRank;
        private DrawableTeamFlag teamFlag;

        private TournamentTeam team = new TournamentTeam { FullName = { Value = "???" } };

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

        public DrawablePlayerCard(TournamentTeam team, Colour4 color)
        {
            Margin = new MarginPadding(10);

            if(team != null) this.team = team;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Width = 200,
                            Height = 225,
                            Masking = true,
                            CornerRadius = 21f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = color
                                },
                                trianglesBox = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    CornerRadius = 21f,
                                    Width = 0.93f,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4Extensions.FromHex("#3d3d3d"),
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Height = 0.7f,
                                        },
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#3d3d3d"), Color4Extensions.FromHex("#282828")),
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            Height = 0.3f,
                                        },
                                        new TrianglesV2
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Thickness = 0.02f,
                                            ScaleAdjust = 1,
                                            SpawnRatio = 2f,
                                            Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#3d3d3d"), Color4Extensions.FromHex("#282828")),
                                        }
                                    }
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = color,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Height = 0.22f,
                                    Width = 0.22f,
                                    Margin = new MarginPadding { Right = 22 }
                                },
                                teamFlag = new DrawableTeamFlag(team, new Vector2(87, 87), 17)
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Margin = new MarginPadding { Left = 28, Top = 11 }
                                },
                                seed = new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding { Right = 25, Top = 28 },
                                    Text = "#" + team.Seed.Value ?? "#?",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, fixedWidth: true, size: 20),
                                    Colour = Color4Extensions.FromHex("3d3d3d")
                                },
                                teamName = new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Margin = new MarginPadding { Left = 28, Top = 100 },
                                    Text = team.FullName.Value ?? "???",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 20),
                                    Colour = color,
                                },
                                avgRank = new TournamentSpriteText
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Margin = new MarginPadding { Right = 10 },
                                    Text = team.AverageRank > 0 ? $"#{team.AverageRank}" : "#???",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 40),
                                    Colour = Color4Extensions.FromHex("3d3d3d")
                                },
                                countryRank = new TournamentSpriteText
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Margin = new MarginPadding { Right = 10, Bottom = 30 },
                                    Text = team.Players?.FirstOrDefault() != null
                                        ? $"#{team.Players.First().CountryRank} ES"
                                        : "#???",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 20),
                                    Colour = color
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateColours();
            updateBindings();
        }

        private void updateColours()
        {
            if (Triangles == null) return;

            Triangles.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#76b4e4"), Color4Extensions.FromHex("#6ddded"));
        }

        private void updateBindings()
        {
            if (team.FullName.Value == "???") return;

            team.Seed?.BindValueChanged(v => seed.Text = v.NewValue ?? "???", true);
            team.FullName?.BindValueChanged(v => teamName.Text = v.NewValue ?? "???", true);

            avgRank.Text = team.AverageRank > 0 ? $"#{team.AverageRank}" : "#???";

            var firstPlayer = team.Players?.FirstOrDefault();
            countryRank.Text = firstPlayer != null
                ? $"#{firstPlayer.CountryRank} ES"
                : "#???";

            teamFlag.flag!.Value = team.FlagName.Value;
        }
    }
}
