// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
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
                new LeftInfo(currentTeam.Value) { Position = new Vector2(55, 30), },
                new RightInfo(currentTeam.Value) { Position = new Vector2(470, 137), },
            };
        });

        private partial class RightInfo : CompositeDrawable
        {
            public RightInfo(TournamentTeam team)
            {
                FillFlowContainer fillCompleto, fillNmHrEz, fillHdDtDr;

                Width = 400;

                InternalChildren = new Drawable[]
                {
                    fillCompleto = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            fillNmHrEz = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                            },
                            fillHdDtDr = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                Margin = new MarginPadding { Left = 460 }
                            },
                        }
                    },
                };

                foreach (var seeding in team.SeedingResults)
                {
                    if (seeding.Mod.Value is "NM" or "HR" or "EZ")
                    {
                        fillNmHrEz.Add(new ModRow(seeding.Mod.Value, seeding.Seed.Value));

                        foreach (var beatmap in seeding.Beatmaps)
                        {
                            if (beatmap.Beatmap == null) continue;

                            fillNmHrEz.Add(new BeatmapScoreRow(beatmap));
                        }
                    }
                    else
                    {
                        fillHdDtDr.Add(new ModRow(seeding.Mod.Value, seeding.Seed.Value));

                        foreach (var beatmap in seeding.Beatmaps)
                        {
                            if (beatmap.Beatmap == null) continue;

                            fillHdDtDr.Add(new BeatmapScoreRow(beatmap));
                        }

                        if (seeding.Mod.Value == "HD")
                        {
                            fillHdDtDr.Add(new BeatmapScoreRow(null!)
                            {
                                Margin = new MarginPadding { Top = 12, Bottom = 0 }
                            });
                        }
                    }
                }
            }

            private partial class BeatmapScoreRow : CompositeDrawable
            {
                public BeatmapScoreRow(SeedingBeatmap beatmap)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    if (beatmap == null)
                    {
                        // fila vacía para margen.
                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;
                        InternalChildren = Array.Empty<Drawable>();
                        return;
                    }

                    Colour4 seedColour = beatmap.Seed.Value switch
                    {
                        1 => Colour4.Gold,
                        2 => Colour4.Silver,
                        3 => new Colour4(205, 127, 50, 255), // bronce
                        _ => TournamentGame.TEXT_COLOUR,
                    };

                    Debug.Assert(beatmap.Beatmap != null);

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

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
                                new TournamentSpriteText { Text = beatmap.Beatmap.Metadata.Title, Colour = TournamentGame.TEXT_COLOUR, Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 14) },
                                new TournamentSpriteText { Text = "by", Colour = TournamentGame.TEXT_COLOUR, Font = OsuFont.Futura.With(weight: FontWeight.Regular, size: 14) },
                                new TournamentSpriteText { Text = beatmap.Beatmap.Metadata.Artist, Colour = TournamentGame.TEXT_COLOUR, Font = OsuFont.Futura.With(weight: FontWeight.Regular, size: 14) },
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Position = new Vector2(300, 0),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText { Text = beatmap.Score.ToString("#,0"), Colour = TournamentGame.TEXT_COLOUR, Width = 57, Font = OsuFont.Futura.With(weight: FontWeight.Regular, size: 13) },
                                new TournamentSpriteText
                                    { Text = "#" + beatmap.Seed.Value.ToString("#,0"), Colour = seedColour, Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 13) },
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

                    Padding = new MarginPadding { Top = 83, Bottom = 10, Left = 345 };

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

                    /*if (!string.IsNullOrEmpty(mods))
                    {
                        row.Add(new Sprite
                        {
                            Texture = textures.Get($"Mods/{mods.ToLowerInvariant()}"),
                            Scale = new Vector2(0.5f)
                        });
                    }*/

                    row.Add(new Container
                    {
                        Size = new Vector2(30, 16),
                        Masking = true,
                        Shear = new Vector2(0.3f, 0),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = TournamentGame.ELEMENT_BACKGROUND_COLOUR,
                            },
                            new TournamentSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "#" + seeding.ToString("#,0"),
                                Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 13),
                                Shear = new Vector2(-0.3f, 0),
                                Colour = TournamentGame.ELEMENT_FOREGROUND_COLOUR
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

                Width = 100;

                if (team == null) return;

                InternalChildren = new Drawable[]
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TeamDisplay(team) { Margin = new MarginPadding { Bottom = 80 } },
                            new TournamentSpriteText
                            {
                                Text = $"#{team.AverageRank:#,0}",
                                Colour = Colour4.FromHex("#ec675d"),
                                Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 80),
                                Shadow = false,
                                Shear = new Vector2(0.2f, 0),
                                Rotation = -9.5f,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Width = 500,
                            },
                            new TournamentSpriteText
                            {
                                Text = team.Seed.Value,
                                Colour = Colour4.FromHex("#ec675d"),
                                Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 80),
                                Shadow = false,
                                Shear = new Vector2(0.2f, 0),
                                Rotation = -9.5f,
                                Margin = new MarginPadding { Left = 225, Top = -30 },
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Width = 300,
                            },
                            //new RowDisplay("Average Rank:", $"#{team.AverageRank:#,0}"),
                            //new RowDisplay("Seed:", team.Seed.Value),
                            //new RowDisplay("Last year's placing:", team.LastYearPlacing.Value > 0 ? $"#{team.LastYearPlacing:#,0}" : "N/A"),
                            //new Container { Margin = new MarginPadding { Bottom = -10 } },
                        }
                    },
                };

                bool isFirst = true;

                foreach (var p in team.Players)
                {
                    var row = new RowDisplay(p.Username, p.Rank?.ToString("\\##,0") ?? "-");

                    if (isFirst)
                    {
                        row.Margin = new MarginPadding { Top = -30 }; // margen solo para el primero.
                        isFirst = false;
                    }

                    fill.Add(row);
                }
            }

            internal partial class RowDisplay : CompositeDrawable
            {
                public RowDisplay(string left, string right)
                {
                    AutoSizeAxes = Axes.Y;
                    RelativeSizeAxes = Axes.X;
                    Margin = new MarginPadding { Top = -10, };

                    InternalChildren = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = left,
                            Colour = Colour4.Black,
                            Font = OsuFont.Futura.With(size: 20, weight: FontWeight.Bold),
                            RelativeSizeAxes = Axes.None,
                            Width = 150,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Left = -20 },
                            Shadow = false,
                        },
                        new TournamentSpriteText
                        {
                            Text = right,
                            Colour = Colour4.White,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopLeft,
                            Font = OsuFont.Futura.With(size: 20, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Left = 20 },
                            RelativeSizeAxes = Axes.None,
                            Width = 100,
                            Shadow = false,
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
                    Flag.Scale = new Vector2(2.4f);
                    Flag.Margin = new MarginPadding { Bottom = -4, Left = 20 };

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
                                Font = OsuFont.Futura.With(size: 42, weight: FontWeight.Bold),
                                Colour = Colour4.Black,
                                Shadow = false,
                                Margin = new MarginPadding { Left = -25 }
                            },
                        }
                    };
                }
            }
        }
    }
}
