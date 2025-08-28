// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Showcase;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawablePlayerCard : CompositeDrawable
    {
        private Container trianglesBox;
        protected TrianglesV2? Triangles { get; private set; }

        public DrawablePlayerCard(TournamentTeam tournamentTeam)
        {
            Margin = new MarginPadding(10);

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
                                    Colour = Color4Extensions.FromHex("#6ddded")
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
                                    Colour = Color4Extensions.FromHex("#6ddded"),
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Height = 0.22f,
                                    Width = 0.22f,
                                    Margin = new MarginPadding { Right = 22 }
                                },
                                new DrawableTeamFlag(tournamentTeam, new Vector2(87, 87), 17)
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Margin = new MarginPadding { Left = 28, Top = 11 }
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding { Right = 38, Top = 28 },
                                    Text = tournamentTeam.Seed.Value,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, fixedWidth: true, size: 20),
                                    Colour = Color4Extensions.FromHex("3d3d3d")
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Margin = new MarginPadding { Left = 28, Top = 100 },
                                    Text = tournamentTeam.FullName.Value,
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 20),
                                    Colour = Color4Extensions.FromHex("6ddded"),
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Margin = new MarginPadding { Right = 10 },
                                    Text = "#" + tournamentTeam.AverageRank.ToString(CultureInfo.InvariantCulture),
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 40),
                                    Colour = Color4Extensions.FromHex("3d3d3d")
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Margin = new MarginPadding { Right = 10, Bottom = 30 },
                                    Text = "#" + tournamentTeam.Players[0].CountryRank + " ES",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 20),
                                    Colour = Color4Extensions.FromHex("6ddded")
                                },
                                new TournamentLogo
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Margin = new MarginPadding { Left = 0 }
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

            updateColours();
        }

        private void updateColours()
        {
            if (Triangles == null) return;

            Triangles.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#76b4e4"), Color4Extensions.FromHex("#6ddded"));
        }
    }
}
