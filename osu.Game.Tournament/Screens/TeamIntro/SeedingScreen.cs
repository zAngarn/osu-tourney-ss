// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
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
                new DrawablePlayerCard(currentTeam.Value, Colour4.White) { Position = new Vector2(20, 250), Scale = new Vector2(2f) },
                new RightInfo(currentTeam.Value) { Position = new Vector2(595, 30), },
            };
        });

        private partial class RightInfo : CompositeDrawable
        {
            public RightInfo(TournamentTeam team)
            {
                FillFlowContainer fill;

                Width = 600;

                InternalChildren = new Drawable[]
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(6),
                    },
                };

                foreach (var seeding in team.SeedingResults)
                {
                    int i = 1;

                    foreach (var beatmap in seeding.Beatmaps)
                    {
                        if (beatmap.Beatmap == null)
                            continue;

                        fill.Add(new BeatmapScoreRow(beatmap, seeding.Mod.Value, i));

                        i++;
                    }
                }
            }

            private partial class BeatmapScoreRow : CompositeDrawable
            {
                public BeatmapScoreRow(SeedingBeatmap beatmap, string mod, int index)
                {
                    Debug.Assert(beatmap.Beatmap != null);

                    Colour4 accentColour = getColor(mod);

                    Colour4 seedColour = beatmap.Seed.Value switch
                    {
                        1 => Colour4.Gold,
                        2 => Colour4.Silver,
                        3 => new Colour4(205, 127, 50, 255), // bronce
                        _ => TournamentGame.TEXT_COLOUR,
                    };

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
                                new TournamentBeatmapPanelV2(beatmap.Beatmap, mod, mod + index),
                                new Container
                                {
                                    Width = 200,
                                    Height = 64,
                                    CornerRadius = 15f,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = accentColour,
                                        },
                                        new TournamentSpriteText { Shadow = false, Text = "Score", Colour = TournamentGame.TEXT_COLOUR, Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 28), Margin = new MarginPadding { Top = 5, Left = 10 } },
                                        new TournamentSpriteText { Shadow = false, Text = beatmap.Score.ToString("#,0"), Colour = TournamentGame.TEXT_COLOUR, Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 42), Margin = new MarginPadding { Top = 25, Left = 10 } },
                                        new Container
                                        {
                                            Width = 60,
                                            Height = 60,
                                            CornerRadius = 15f,
                                            Masking = true,
                                            Origin = Anchor.CentreRight,
                                            Anchor = Anchor.CentreRight,
                                            Margin = new MarginPadding { Right = 2 },
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Colour4.FromHex("282828"),
                                                },
                                                new TournamentSpriteText
                                                {
                                                    Text = "#" + beatmap.Seed.Value.ToString("#,0"), Colour = seedColour, Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 45),
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Margin = new MarginPadding { Bottom = -4 },
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    };
                }
            }
        }

        private static Colour4 getColor(string slot)
        {
            Colour4 color = Colour4.FromHex("757575");

            if (slot == null) return color;

            string slotParsed = slot.Substring(0, 2);

            switch (slotParsed)
            {
                case "NM":
                    color = Colour4.FromHex("29a8f9");
                    break;

                case "HD":
                    color = Colour4.FromHex("fbba20");
                    break;

                case "HR":
                    color = Colour4.FromHex("f24141");
                    break;

                case "DT":
                    color = Colour4.FromHex("ca8cfb");
                    break;

                case "TB":
                    color = Colour4.FromHex("aeaeae");
                    break;
            }

            return color;
        }
    }
}

