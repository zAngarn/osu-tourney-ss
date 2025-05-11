// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamIntro
{
    public partial class SeedingScreen : TournamentMatchScreen
    {
        private Container mainContainer = null!;

        private readonly Bindable<TournamentTeam?> currentTeam = new Bindable<TournamentTeam?>();

        private TourneyButton showFirstTeamButton = null!;
        private TourneyButton showSecondTeamButton = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("seeding")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        showFirstTeamButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Show first team",
                            Action = () => currentTeam.Value = CurrentMatch.Value?.Team1.Value,
                        },
                        showSecondTeamButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Show second team",
                            Action = () => currentTeam.Value = CurrentMatch.Value?.Team2.Value,
                        },
                        new SettingsTeamDropdown(LadderInfo.Teams)
                        {
                            LabelText = "Show specific team",
                            Current = currentTeam,
                        }
                    }
                }
            };

            currentTeam.BindValueChanged(teamChanged, true);
        }

        private void teamChanged(ValueChangedEvent<TournamentTeam?> team) => updateTeamDisplay();

        public override void Show()
        {
            base.Show();

            // Changes could have been made on editor screen.
            // Rather than trying to track all the possibilities (teams / players / scores) just force a full refresh.
            updateTeamDisplay();
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            if (match.NewValue == null)
            {
                showFirstTeamButton.Enabled.Value = false;
                showSecondTeamButton.Enabled.Value = false;
                return;
            }

            showFirstTeamButton.Enabled.Value = true;
            showSecondTeamButton.Enabled.Value = true;

            currentTeam.Value = match.NewValue.Team1.Value;
        }

        private void updateTeamDisplay() => Scheduler.AddOnce(() =>
        {
            if (currentTeam.Value == null)
            {
                mainContainer.Clear();
                return;
            }

            mainContainer.Children = new Drawable[]
            {
                new LeftInfo(currentTeam.Value) { Position = new Vector2(55, 150), },
                new RightInfo(currentTeam.Value) { Position = new Vector2(500, 150), },
            };
        });

        private partial class RightInfo : CompositeDrawable
        {
            public RightInfo(TournamentTeam team)
            {
                FillFlowContainer fill;

                Width = 400;

                InternalChildren = new Drawable[]
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Scale = new Vector2(1.3f),
                        Margin = new MarginPadding { Top = -70 }
                    },
                };

                foreach (var seeding in team.SeedingResults)
                {
                    fill.Add(new ModRow(seeding.Mod.Value, seeding.Seed.Value));

                    foreach (var beatmap in seeding.Beatmaps)
                    {
                        if (beatmap.Beatmap == null)
                            continue;

                        fill.Add(new BeatmapScoreRow(beatmap));
                    }
                }
            }

            private partial class BeatmapScoreRow : CompositeDrawable
            {
                public BeatmapScoreRow(SeedingBeatmap beatmap)
                {
                    Debug.Assert(beatmap.Beatmap != null);

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    Colour4 seedColour = beatmap.Seed.Value switch
                    {
                        1 => Colour4.Gold,
                        2 => Colour4.Silver,
                        3 => new Colour4(205, 127, 50, 255), // bronce
                        _ => TournamentGame.TEXT_COLOUR,
                    };

                    InternalChildren = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText { Text = beatmap.Beatmap.Metadata.Title, Colour = seedColour, Font = OsuFont.Inter.With(weight: FontWeight.Regular), Shadow = false },
                                new TournamentSpriteText { Text = "by", Colour = seedColour, Font = OsuFont.Inter.With(weight: FontWeight.Regular), Shadow = false },
                                new TournamentSpriteText { Text = beatmap.Beatmap.Metadata.Artist, Colour = seedColour, Font = OsuFont.Inter.With(weight: FontWeight.Regular), Shadow = false },
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(40),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText { Text = beatmap.Score.ToString("#,0"), Colour = seedColour, Width = 80, Font = OsuFont.Inter.With(weight: FontWeight.Regular) },
                                new TournamentSpriteText { Text = "#" + beatmap.Seed.Value.ToString("#,0"), Colour = seedColour, Font = OsuFont.Inter.With(weight: FontWeight.Regular) },
                            }
                        },
                    };
                }
            }

            private partial class ModRow : CompositeDrawable
            {
                private readonly string mods;
                private readonly int seeding;

                public ModRow(string mods, int seeding)
                {
                    this.mods = mods;
                    this.seeding = seeding;

                    Padding = new MarginPadding { Vertical = 10 };

                    AutoSizeAxes = Axes.Y;
                }

                [BackgroundDependencyLoader]
                private void load(TextureStore textures)
                {
                    FillFlowContainer row;

                    InternalChildren = new Drawable[]
                    {
                        row = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                        },
                    };

                    if (!string.IsNullOrEmpty(mods))
                    {
                        row.Add(new Sprite
                        {
                            Texture = textures.Get($"Mods/{mods.ToLowerInvariant()}"),
                            Scale = new Vector2(0.5f)
                        });
                    }

                    row.Add(new Container
                    {
                        Size = new Vector2(100, 16),
                        //CornerRadius = 10f,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.Inter.With(weight: FontWeight.Bold),
                                Text = mods.ToUpperInvariant() + " seed: " + seeding.ToString("#,0"),
                                Colour = Colour4.FromHex("#F24998")
                            },
                        }
                    });
                }
            }
        }

        private partial class LeftInfo : CompositeDrawable
        {
            public LeftInfo(TournamentTeam? team)
            {
                FillFlowContainer fill;

                Width = 200;

                if (team == null) return;

                InternalChildren = new Drawable[] //#f24998
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TeamDisplay(team)
                            {
                                Margin = new MarginPadding { Top = -130, Bottom = 30, Left = -30 }
                            },
                            new TournamentSpriteText
                            {
                                Text = $"{team.AverageRank:#,0}",
                                Colour = Colour4.FromHex("#F24998"),
                                Font = OsuFont.Inter.With(size: 28, weight: FontWeight.Bold),
                                Margin = new MarginPadding { Left = -30 },
                            },
                            new TournamentSpriteText
                            {
                                Text = $"{team.Seed.Value}",
                                Colour = Colour4.FromHex("#F24998"),
                                Font = OsuFont.Inter.With(size: 150, weight: FontWeight.Bold),
                                Margin = new MarginPadding { Left = -30, Top = 220 },
                            }
                            //new RowDisplay("Seed:", team.Seed.Value),
                            //new RowDisplay("Last year's placing:", team.LastYearPlacing.Value > 0 ? $"#{team.LastYearPlacing:#,0}" : "N/A"),
                            //new Container { Margin = new MarginPadding { Bottom = 30 } },
                        }
                    },
                };
            }

            internal partial class RowDisplay : CompositeDrawable
            {
                public RowDisplay(string left, string right)
                {
                    AutoSizeAxes = Axes.Y;
                    RelativeSizeAxes = Axes.X;

                    InternalChildren = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = left,
                            Colour = TournamentGame.TEXT_COLOUR,
                            Font = OsuFont.Inter.With(size: 22, weight: FontWeight.SemiBold),
                        },
                        new TournamentSpriteText
                        {
                            Text = right,
                            Colour = TournamentGame.TEXT_COLOUR,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopLeft,
                            Font = OsuFont.Inter.With(size: 22, weight: FontWeight.Regular),
                        },
                    };
                }
            }

            private partial class TeamDisplay : DrawableTournamentTeam
            {
                public TeamDisplay(TournamentTeam? team)
                    : base(team)
                {
                    AutoSizeAxes = Axes.Both;

                    Flag.RelativeSizeAxes = Axes.None;
                    Flag.Scale = new Vector2(2f);
                    Flag.CornerRadius = 50f;

                    InternalChild = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            Flag,
                            new OsuSpriteText
                            {
                                Text = team?.FullName.Value ?? "???",
                                Font = OsuFont.Inter.With(size: 40, weight: FontWeight.Bold),
                                Shadow = false,
                                Colour = Colour4.FromHex("#F24998")
                            },
                        }
                    };
                }
            }
        }
    }
}
