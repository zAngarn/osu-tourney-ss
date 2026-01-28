// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Tournament.Components
{
    public partial class SS26Songbar : CompositeDrawable
    {
        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private CancellationTokenSource? starDifficultyCancellationSource;

        //private TournamentSpriteText starRating = null!;

        private IBeatmapInfo? beatmap;

        public IBeatmapInfo? Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                refreshContent();
            }
        }

        private string slot = "NM2";

        public string Slot
        {
            get => slot;
            set
            {
                slot = value;
                refreshContent();
            }
        }

        private LegacyMods mods;

        public LegacyMods Mods
        {
            set
            {
                mods = value;
                refreshContent();
            }
        }

        private Container container = null!;

        private IBindable<StarDifficulty> starDifficultyBindable = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                container = new Container
                {
                    Height = 360,
                    Width = 1920,
                    Masking = true,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#FF714D"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#4DDBFF"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft
                        },
                        new Container
                        {
                            Height = 280,
                            Width = 1922,
                            Masking = true,
                            CornerRadius = 90,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = -140 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#282828"),
                                    Width = 1 / 2f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopRight
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#282828"),
                                    Width = 1 / 2f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopLeft
                                },
                            }
                        },
                        new Container
                        {
                            Height = 420,
                            Width = 1802,
                            Masking = true,
                            CornerRadius = 90,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = -210 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#282828"),
                                    Width = 1 / 2f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopRight
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#282828"),
                                    Width = 1 / 2f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopLeft
                                },
                            }
                        },
                        new Container
                        {
                            Height = 140,
                            Width = 722,
                            Masking = true,
                            CornerRadius = 90,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = -70 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#FF714D"),
                                    Width = 1 / 2f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopRight
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("#4DDBFF"),
                                    Width = 1 / 2f,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopLeft
                                },
                            }
                        }
                    }
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

            mods = convertSlotToMods(slot);
            var rulesetInstance = ruleset.Value.CreateInstance();
            var convertedMods = rulesetInstance.ConvertFromLegacyMods(mods).ToList();
            var adjustedDifficulty = rulesetInstance.GetAdjustedDisplayDifficulty(beatmap, convertedMods);
            computeStarRating(rulesetInstance.RulesetInfo, convertedMods);

            double rate = ModUtils.CalculateRateWithMods(convertedMods);
            double bpm = FormatUtils.RoundBPM(beatmap.BPM, rate);
            double length = beatmap.Length / rate;

            var apibeatmap = beatmap;

            if (beatmap.Metadata.Title != "no beatmap selected")
            {
                var localInfo = beatmapManager.QueryOnlineBeatmapId(beatmap.OnlineID);
                beatmap = localInfo;
            }

            container.Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#FF714D"),
                    Width = 1 / 2f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#4DDBFF"),
                    Width = 1 / 2f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft
                },
                new Container
                {
                    Height = 280,
                    Width = 1922,
                    Masking = true,
                    CornerRadius = 90,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = -140 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#282828"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#282828"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft
                        },
                    }
                },
                new Container
                {
                    Height = 420,
                    Width = 1802,
                    Masking = true,
                    CornerRadius = 90,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = -210 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#282828"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#282828"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft
                        },
                    }
                },
                new Container
                {
                    Height = 140,
                    Width = 722,
                    Masking = true,
                    CornerRadius = 90,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = -70 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#FF714D"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopRight
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#4DDBFF"),
                            Width = 1 / 2f,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopLeft
                        },
                    }
                },
                new SS26SongbarBeatmapPanel(apibeatmap, slot, adjustedDifficulty, bpm, length.ToFormattedDuration().ToString(), starDifficultyBindable)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Bottom = 140 },
                },
            };
        }

        private void computeStarRating(IRulesetInfo ruleset, List<Mod> mods)
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            if (beatmap == null)
                return;

            starDifficultyBindable = difficultyCache.GetBindableDifficultyArtesanal(beatmap, ruleset, mods, starDifficultyCancellationSource.Token);
        }

        private LegacyMods convertSlotToMods(string slot)
        {
            LegacyMods slotToMods = LegacyMods.None;

            if (slot[..2] == "NM")
            {
                slotToMods = LegacyMods.None;
            }
            else if (slot[..2] == "HD")
            {
                slotToMods = LegacyMods.Hidden;
            }
            else if (slot[..2] == "HR")
            {
                slotToMods = LegacyMods.HardRock;
            }
            else if (slot[..2] == "DT")
            {
                slotToMods = LegacyMods.DoubleTime;
            }
            else if (slot[..2] == "TB")
            {
                slotToMods = LegacyMods.None;
            }

            return slotToMods;
        }
    }
}
