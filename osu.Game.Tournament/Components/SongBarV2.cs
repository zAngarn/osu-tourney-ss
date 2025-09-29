// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Models;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class SongBarV2 : CompositeDrawable
    {
        private IBeatmapInfo? beatmap;

        public IBeatmapInfo? Beatmap
        {
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                refreshContent();
            }
        }

        private LegacyMods mods;

        public LegacyMods Mods
        {
            get => mods;
            set
            {
                mods = value;
                refreshContent();
            }
        }

        private BeatmapChoice beatmapChoice = new BeatmapChoice
        {
            Slot = "???",
            TeamName = "???"
        };

        public BeatmapChoice BeatmapChoice
        {
            get => beatmapChoice;
            set
            {
                beatmapChoice = value;
                refreshContent();
            }
        }

        private FillFlowContainer flow = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            InternalChildren = new Drawable[]
            {
                flow = new FillFlowContainer
                {
                    Masking = true,
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                }
            };
        }

        private void refreshContent()
        {
            beatmap ??= new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = "unknown",
                    Title = "no beatmap selected",
                    Author = new RealmUser { Username = "unknown" },
                },
                DifficultyName = "unknown",
                BeatmapSet = new BeatmapSetInfo(),
                StarRating = 0,
                Difficulty = new BeatmapDifficulty
                {
                    CircleSize = 0,
                    DrainRate = 0,
                    OverallDifficulty = 0,
                    ApproachRate = 0,
                },
            };

            double bpm = beatmap.BPM;
            double length = beatmap.Length;
            string hardRockExtra = "";
            string srExtra = "";

            float ar = beatmap.Difficulty.ApproachRate;

            if ((mods & LegacyMods.HardRock) > 0)
            {
                hardRockExtra = "*";
                srExtra = "*";
            }

            if ((mods & LegacyMods.DoubleTime) > 0)
            {
                // temporary local calculation (taken from OsuDifficultyCalculator)
                double preempt = (int)IBeatmapDifficultyInfo.DifficultyRange(ar, 1800, 1200, 450) / 1.5;
                ar = (float)(preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5);

                bpm *= 1.5f;
                length /= 1.5f;
                srExtra = "*";
            }

            flow.Children = new Drawable[]
            {
                new Container
                {
                    Width = 992,
                    Height = 314,
                    Masking = true,
                    MaskingSmoothness = 0,
                    Children = new Drawable[]
                    {
                        new NoUnloadBeatmapSetCover
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            OnlineInfo = (beatmap as IBeatmapSetOnlineInfo),
                            Margin = new MarginPadding { Left = 1 }
                        },
                        new Box
                        {
                            Colour = Colour4.Black,
                            Alpha = 0.6f,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = 180, Top = 150 },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Masking = true,
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new TournamentSpriteText
                                        {
                                            Text = beatmap.Metadata.Artist ?? "unknown",
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 30),
                                            Colour = getColor(beatmapChoice.Slot),
                                        },
                                        new TournamentSpriteText
                                        {
                                            Text = beatmap.Metadata.Title ?? "unknown",
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 60),
                                            Colour = Colour4.White,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(14),
                                            Children = new Drawable[]
                                            {
                                                new TournamentSpriteText
                                                {
                                                    Colour = getColor(beatmapChoice.Slot),
                                                    Text = beatmap?.DifficultyName ?? "unknown",
                                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 30),
                                                },
                                                new TournamentSpriteText
                                                {
                                                    Colour = Colour4.White,
                                                    Text = "mapeado por " + (beatmap?.Metadata.Author.Username ?? "unknown"),
                                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 30),
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.312f,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Masking = true,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Colour4.FromHex("#282828"),
                                            Width = 0.7f,
                                        },
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = ColourInfo.GradientHorizontal(Colour4.FromHex("#282828"), Colour4.FromHex("#3d3d3d")),
                                            Width = 0.3f,
                                        },
                                        new TrianglesV2
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Thickness = 0.02f,
                                            ScaleAdjust = 1,
                                            SpawnRatio = 2f,
                                            Colour = ColourInfo.GradientVertical(Colour4.FromHex("#3d3d3d"), Colour4.FromHex("#282828")),
                                            Margin = new MarginPadding { Left = -200 }
                                        }
                                    }
                                },
                                new Container
                                {
                                    Masking = true,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Left = 40 }, //diffpieces
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            Masking = true,
                                            RelativeSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Margin = new MarginPadding { Left = 20, Top = 14 },
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    Masking = true,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Vertical,
                                                    Children = new Drawable[]
                                                    {
                                                        new DiffPiece(getColor(beatmapChoice.Slot), ("CS", $"{beatmap.Difficulty.CircleSize:0.#}{hardRockExtra}")),
                                                        new DiffPiece(getColor(beatmapChoice.Slot), ("AR", $"{ar:0.#}{hardRockExtra}")),
                                                        new DiffPiece(getColor(beatmapChoice.Slot), ("OD", $"{beatmap.Difficulty.OverallDifficulty:0.#}{hardRockExtra}")),
                                                    }
                                                },
                                                new FillFlowContainer
                                                {
                                                    Masking = true,
                                                    RelativeSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Vertical,
                                                    Margin = new MarginPadding { Left = -900 },
                                                    Children = new Drawable[]
                                                    {
                                                        new DiffPiece(getColor(beatmapChoice.Slot), ("Estrellas", $"{beatmap.StarRating.FormatStarRating()}{srExtra}")),
                                                        new DiffPiece(getColor(beatmapChoice.Slot), ("BPM", $"{bpm:0.#}")),
                                                        new DiffPiece(getColor(beatmapChoice.Slot), ("Duración", length.ToFormattedDuration().ToString())),
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.357f,
                            Width = 0.149f,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Margin = new MarginPadding { Bottom = 118 },
                            Children = new Drawable[]
                            {
                                new Box // slot
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#282828")
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding { Right = 20 },
                                    Text = beatmapChoice.Slot ?? "???",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 60),
                                    Colour = getColor(beatmapChoice.Slot!),
                                    Shadow = false,
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding { Right = 20, Top = 70 },
                                    Text = "Pickeado por:",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 16),
                                    Colour = Colour4.White,
                                    Shadow = false,
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding { Right = 20, Top = 85 },
                                    Text = beatmapChoice.TeamName ?? "???",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 16),
                                    Colour = getTeamColour(beatmapChoice.Team),
                                    Shadow = false,
                                },
                            }
                        },
                        new Container
                        {
                            Masking = false,
                            RelativeSizeAxes = Axes.Both,
                            Height = 0.07f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Bottom = 118 },
                            Children = new Drawable[]
                            {
                                new Box // barras de puntos
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("282828")
                                },
                                new TournamentMatchScoreDisplay
                                {
                                    Y = -22,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.TopCentre,
                                }
                            }
                        },
                    }
                }
            };
        }

        private Colour4 getTeamColour(TeamColour color)
        {
            Colour4 c = Colour4.FromHex(color switch
            {
                TeamColour.Red => "ed6dac",
                TeamColour.Blue => "6ddded",
                TeamColour.None => "757575",
                _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
            });

            return c;
        }

        private Colour4 getColor(string slot)
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

        private Colour4 getColor(TeamColour colour)
        {
            var color = Colour4.FromHex(colour == TeamColour.Red ? "ed6dac" : "6ddded");
            return color;
        }
    }

    public partial class DiffPiece : TextFlowContainer
    {
        public DiffPiece(Colour4 headerColour, params (string heading, string content)[] tuples)
        {
            Margin = new MarginPadding { Horizontal = 15, Vertical = 1 };
            AutoSizeAxes = Axes.Both;

            static void cp(SpriteText s, bool bold)
            {
                s.Font = OsuFont.Torus.With(weight: bold ? FontWeight.Bold : FontWeight.Regular, size: 20);
            }

            for (int i = 0; i < tuples.Length; i++)
            {
                (string heading, string content) = tuples[i];

                if (i > 0)
                {
                    AddText(" / ", s =>
                    {
                        cp(s, false);
                        s.Spacing = new Vector2(-2, 0);
                    });
                }

                AddText(new TournamentSpriteText { Text = heading, Colour = headerColour }, s => cp(s, true));
                AddText(" ", s => cp(s, false));
                AddText(new TournamentSpriteText { Text = content }, s => cp(s, true));
            }
        }
    }

    public partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
    {
        // As covers are displayed on stream, we want them to load as soon as possible.
        protected override double LoadDelay => 0;

        // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);
    }
}
