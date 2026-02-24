// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Utils;
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
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#282828")
                        }
                    }
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
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Next team (Seed reveal)",
                            Action = advanceToNextSeedTeam
                        },
                    }
                }
            };

            currentTeam.BindValueChanged(teamChanged, true);
        }

        private Colour4 getColourForSeed(int currentSeed, int minSeed, int maxSeed)
        {
            if (minSeed == maxSeed)
                return OsuColour.STAR_DIFFICULTY_SPECTRUM[0].Item2;

            int clampedSeed = Math.Clamp(currentSeed, minSeed, maxSeed);

            float t = 1f - ((float)(clampedSeed - minSeed) / (maxSeed - minSeed));

            const float min_spectrum = 0.1f;
            const float max_spectrum = 7.0f;
            float mappedValue = min_spectrum + t * (max_spectrum - min_spectrum);

            return ColourUtils.SampleFromLinearGradient(OsuColour.STAR_DIFFICULTY_SPECTRUM, mappedValue);
        }

        private void advanceToNextSeedTeam()
        {
            var sortedTeams = LadderInfo.Teams
                                        .Where(t => !string.IsNullOrEmpty(t.Seed?.Value))
                                        .OrderByDescending(t => int.TryParse(t.Seed.Value, out int seedNumber) ? seedNumber : 0)
                                        .ToList();

            if (!sortedTeams.Any())
                return;

            if (currentTeam.Value == null)
            {
                currentTeam.Value = sortedTeams.First();
                return;
            }

            int currentIndex = sortedTeams.IndexOf(currentTeam.Value);

            if (currentIndex == -1 || currentIndex == sortedTeams.Count - 1)
            {
                currentTeam.Value = sortedTeams.First();
            }
            else
            {
                currentTeam.Value = sortedTeams[currentIndex + 1];
            }
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

            int currentSeedValue = int.Parse(currentTeam.Value.Seed.Value);
            int totalTeams = LadderInfo.Teams.Count > 1 ? LadderInfo.Teams.Count : 32;

            Colour4 accentColor = getColourForSeed(currentSeedValue, 1, totalTeams);

            var bestBeatmap = currentTeam.Value.SeedingResults
                                         .SelectMany(seeding => seeding.Beatmaps)
                                         .Where(beatmap => beatmap.Beatmap != null)
                                         .MaxBy(beatmap => beatmap.Score);

            string bestBeatmapSlot = "??";

            if (CurrentMatch.Value?.Round.Value != null && bestBeatmap?.Beatmap != null)
            {
                var map = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(m => m.ID == bestBeatmap.Beatmap.OnlineID);
                if (map != null) bestBeatmapSlot = map.Slot;
            }

            mainContainer.Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#282828")
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1275f / 1920f,
                    Height = 615f / 1080f,
                    CornerRadius = 96f,
                    Masking = true,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = -135, Top = -135 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = accentColor,
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 735f / 1920f,
                    Height = 405f / 1080f,
                    CornerRadius = 96f,
                    Masking = true,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = -135, Top = -135 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = accentColor,
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1525f / 1920f,
                    Height = 405f / 1080f,
                    CornerRadius = 96f,
                    Masking = true,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Right = -135, Bottom = -135 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = accentColor,
                        }
                    }
                },
                new DrawableTeamCard(currentTeam.Value, accentColor, 100)
                {
                    Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                    Margin = new MarginPadding { Top = 70, Left = 70 },
                },
                new DrawableSeedingMap(currentTeam.Value)
                {
                    Margin = new MarginPadding { Top = 380, Left = 57 },
                },
                new TournamentSpriteText
                {
                    Colour = Colour4.FromHex("#282828"),
                    Text = "Posición",
                    Shadow = false,
                    Font = OsuFont.BalooDa.With(weight: FontWeight.Black, size: 32),
                    Margin = new MarginPadding { Left = 649, Top = 25 },
                },
                new TournamentSpriteText
                {
                    Colour = Colour4.FromHex("#282828"),
                    Text = $"#{currentTeam.Value.Seed.Value}",
                    Shadow = false,
                    Font = OsuFont.BalooDa.With(weight: FontWeight.Black, size: 100),
                    Margin = new MarginPadding { Left = 676, Top = 28 },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 700f / 1920f,
                    Height = 135f / 1080f,
                    CornerRadius = 36f,
                    Masking = true,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 650, Top = 155 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#282828"),
                        }
                    }
                },
                new TournamentSpriteText
                {
                    Colour = Colour4.FromHex("#ffffff"),
                    Text = "Mejor Score:",
                    Shadow = false,
                    Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 16),
                    Margin = new MarginPadding { Left = 676, Top = 160 },
                },
                new DrawableSeedingMap.BeatmapScoreRow(bestBeatmap!, bestBeatmapSlot)
                {
                    Margin = new MarginPadding { Left = 700, Top = 190 },
                }
            };
        });

        private partial class DrawableSeedingMap : CompositeDrawable
        {
            public DrawableSeedingMap(TournamentTeam team)
            {
                FillFlowContainer seed;

                InternalChildren = new Drawable[]
                {
                    seed = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                    },
                };

                foreach (var seeding in team.SeedingResults)
                {
                    string title = string.Empty;

                    switch (seeding.Mod.Value)
                    {
                        case "NM":
                            title = "NoMod";
                            break;

                        case "HD":
                            title = "Hidden";
                            break;

                        case "HR":
                            title = "HardRock";
                            break;

                        case "DT":
                            title = "DoubleTime";
                            break;
                    }

                    var columnContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(5),
                        Width = 320,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Colour = TournamentGameBase.GetColor(seeding.Mod.Value),
                                Text = title,
                                Shadow = false,
                                Font = OsuFont.BalooDa.With(weight: FontWeight.Black, size: 32),
                                Margin = new MarginPadding { Bottom = 6 },
                            }
                        }
                    };

                    seed.Add(columnContainer);

                    int j = 1;

                    foreach (var beatmap in seeding.Beatmaps)
                    {
                        if (beatmap.Beatmap == null)
                            continue;

                        columnContainer.Add(new BeatmapScoreRow(beatmap, $"{seeding.Mod.Value}{j}")
                        {
                            Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                        });

                        j++;
                    }
                }
            }

            public partial class BeatmapScoreRow : CompositeDrawable
            {
                public BeatmapScoreRow(SeedingBeatmap beatmap, string slot)
                {
                    Debug.Assert(beatmap.Beatmap != null);

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    Colour4 seedColour = beatmap.Seed.Value switch
                    {
                        1 => Colour4.Gold,
                        2 => Colour4.Silver,
                        3 => new Colour4(205, 127, 50, 255), // bronce
                        _ => Colour4.FromHex("#595959"),
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
                                new SS26BeatmapPanel(beatmap.Beatmap, slot, beatmap.Score.ToString()),
                                new Container
                                {
                                    Width = 44f,
                                    Height = 44f,
                                    Masking = true,
                                    CornerRadius = 22f,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Glow,
                                        Colour = seedColour,
                                        Radius = 8,
                                        Hollow = true,
                                    },
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = seedColour,
                                        },
                                        new TournamentSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = Colour4.FromHex("#262626"),
                                            Text = beatmap.Seed.Value.ToString() ?? "???",
                                            Shadow = false,
                                            Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 24),
                                            Margin = new MarginPadding { Bottom = 6 },
                                        }
                                    }
                                }
                            }
                        }
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

                    row.Add(new Container
                    {
                        Size = new Vector2(50, 16),
                        CornerRadius = 10,
                        Masking = true,
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
                                Text = seeding.ToString("#,0"),
                                Colour = TournamentGame.ELEMENT_FOREGROUND_COLOUR
                            },
                        }
                    });
                }
            }
        }
    }
}
