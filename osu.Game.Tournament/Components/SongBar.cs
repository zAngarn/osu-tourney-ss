// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select.Details;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class SongBar : CompositeDrawable
    {
        private IBeatmapInfo? beatmap;

        public const float HEIGHT = 145 / 2f;

        protected AdvancedStats.StatisticRow? ApproachRate, CircleSize, OverrallDifficulty;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

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

        private FillFlowContainer flow = null!;

        private bool expanded;

        public bool Expanded
        {
            get => expanded;
            set
            {
                expanded = value;
                flow.Direction = expanded ? FillDirection.Full : FillDirection.Vertical;
            }
        }

        // Todo: This is a hack for https://github.com/ppy/osu-framework/issues/3617 since this container is at the very edge of the screen and potentially initially masked away.
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                }
            };

            Expanded = true;
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

            CircleSize ??= new AdvancedStats.StatisticRow(11f, false);
            ApproachRate ??= new AdvancedStats.StatisticRow(11f, false);
            OverrallDifficulty ??= new AdvancedStats.StatisticRow(11f, false);

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

            ApproachRate!.Value = (ar, null);

            //(string heading, string content)[] stats;

            flow.Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = HEIGHT,
                    Width = 0.5f,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,

                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,

                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Direction = FillDirection.Horizontal,
                                        Children = new Drawable[]
                                        {
                                            CircleSize = new AdvancedStats.StatisticRow(maxValue: 11)
                                            {
                                                Padding = new MarginPadding { Left = 231, Top = 135, Bottom = 2.5f },
                                                Rotation = -9.5f,
                                            },
                                            ApproachRate = new AdvancedStats.StatisticRow(maxValue: 11)
                                            {
                                                Padding = new MarginPadding { Left = 127, Top = 78, Bottom = 2.5f },
                                                Rotation = -9.5f,
                                            },
                                            OverrallDifficulty = new AdvancedStats.StatisticRow(maxValue: 11)
                                            {
                                                Padding = new MarginPadding { Left = -11, Top = 96, Bottom = 2.5f },
                                                Rotation = -9.5f,
                                            },
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Direction = FillDirection.Horizontal,
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                Padding = new MarginPadding { Left = -190, Top = 15, Bottom = 2.5f },
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new DiffPiece(true, ("SR", $"{beatmap.StarRating:0.#}{srExtra}"))
                                                    {
                                                        Padding = new MarginPadding { Left = 0, Top = 0, Bottom = 2.5f }
                                                    },
                                                    new DiffPiece(true, ("Length", length.ToFormattedDuration().ToString()))
                                                    {
                                                        Padding = new MarginPadding { Left = 35, Top = 5, Bottom = 2.5f }
                                                    },
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                Padding = new MarginPadding { Left = -80, Top = 15, Bottom = 2.5f },
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new DiffPiece(true, ("BPM", $"{bpm:0.#}"))
                                                    {
                                                        Padding = new MarginPadding { Left = 0, Top = 0, Bottom = 2.5f }
                                                    },
                                                    new DiffPiece(false, ("CS", $"{beatmap.Difficulty.CircleSize:0.#}{hardRockExtra}"))
                                                    {
                                                        Padding = new MarginPadding { Left = 120, Top = -16, Bottom = 2.5f }
                                                    },
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                Padding = new MarginPadding { Left = 165, Top = 20, Bottom = 2.5f },
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new DiffPiece(false, ("AR", $"{beatmap.Difficulty.ApproachRate:0.#}{hardRockExtra}"))
                                                    {
                                                        Padding = new MarginPadding { Left = 0, Top = 0, Bottom = 2.5f }
                                                    },
                                                    new DiffPiece(false, ("OD", $"{beatmap.Difficulty.OverallDifficulty:0.#}{hardRockExtra}"))
                                                    {
                                                        Padding = new MarginPadding { Left = 117, Top = -32, Bottom = 2.5f }
                                                    },
                                                }
                                            },
                                        }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new OsuLogo
                                            {
                                                Triangles = false,
                                                Scale = new Vector2(0.08f),
                                                Margin = new MarginPadding(50),
                                                X = -10,
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                            },
                                        }
                                    },
                                },
                            }
                        }
                    }
                },
                new TournamentBeatmapPanel(beatmap, 30)
                {
                    //Shear = new Vector2(0.2f, 0),
                    RelativeSizeAxes = Axes.X,
                    Width = 0.5f,
                    Height = HEIGHT,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                }
            };

            CircleSize!.Value = (beatmap.Difficulty.CircleSize, null);
            ApproachRate!.Value = (ar, null);
            OverrallDifficulty!.Value = (beatmap.Difficulty.OverallDifficulty, null);

            /*switch (ruleset.Value.OnlineID)
            {
                default:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{beatmap.Difficulty.CircleSize:0.#}{hardRockExtra}"),
                        ("AR", $"{ar:0.#}{hardRockExtra}"),
                        ("OD", $"{beatmap.Difficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                    };
                    break;

                case 1:
                case 3:
                    stats = new (string heading, string content)[]
                    {
                        ("OD", $"{beatmap.Difficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                        ("HP", $"{beatmap.Difficulty.DrainRate:0.#}{hardRockExtra}")
                    };
                    break;

                case 2:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{beatmap.Difficulty.CircleSize:0.#}{hardRockExtra}"),
                        ("AR", $"{ar:0.#}"),
                    };
                    break;
            }*/
        }

        public partial class DiffPiece : TextFlowContainer
        {
            public DiffPiece(bool color, params (string heading, string content)[] tuples)
            {
                Margin = new MarginPadding { Horizontal = 15, Vertical = 1 };
                AutoSizeAxes = Axes.Both;

                static void cp(SpriteText s, bool color)
                {
                    int sizeTexto = color ? 20 : 30;
                    s.Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: sizeTexto);
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

                    //AddText(new TournamentSpriteText { Text = heading }, s => cp(s, false));
                    //AddText(" ", s => cp(s, false));
                    var textoColor = color ? Colour4.FromHex("#ec675d") : Colour4.White;
                    AddText(new TournamentSpriteText { Text = content, Colour = textoColor, Rotation = -9, Shear = new Vector2(0.25f, 0), Shadow = false }, s => cp(s, color));
                }
            }
        }
    }
}
