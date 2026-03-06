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

        private IBeatmapInfo? lastCalculatedBeatmap;
        private List<Mod>? lastCalculatedMods;

        private CancellationTokenSource? starDifficultyCancellationSource;

        //private TournamentSpriteText starRating = null!;

        private SS26SongbarBeatmapPanel panel = null!;

        private IBeatmapInfo? beatmap;

        public IBeatmapInfo? Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value || beatmap?.MD5Hash == null)
                    return;

                beatmap = value;
                Scheduler.AddOnce(refreshContent);
            }
        }

        private string slot = "???";

        public string Slot
        {
            get => slot;
            set
            {
                slot = value;
                Scheduler.AddOnce(refreshContent);
            }
        }

        private LegacyMods mods;

        public LegacyMods Mods
        {
            set
            {
                mods = value;
                Scheduler.AddOnce(refreshContent);
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
                        },
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

            double rate = ModUtils.CalculateRateWithMods(convertedMods);
            double bpm = FormatUtils.RoundBPM(beatmap.BPM, rate);
            double length = beatmap.Length / rate;

            var apibeatmap = beatmap;

            if (beatmap.Metadata.Title != "no beatmap selected")
            {
                var localInfo = beatmapManager.QueryOnlineBeatmapId(beatmap.OnlineID);
                beatmap = localInfo;
            }

            computeStarRating(rulesetInstance.RulesetInfo, convertedMods);

            if (container.Children[^1] is SS26SongbarBeatmapPanel)
            {
                container.Children[^1].RemoveAndDisposeImmediately();
            }

            container.Add(new SS26SongbarBeatmapPanel(apibeatmap, slot, adjustedDifficulty, bpm, length.ToFormattedDuration().ToString(), starDifficultyBindable)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.Centre,
                Margin = new MarginPadding { Bottom = 140 },
            });
        }

        private void computeStarRating(IRulesetInfo ruleset, List<Mod> mods)
        {
            if (beatmap == null)
                return;

            if (lastCalculatedBeatmap == beatmap && lastCalculatedMods != null && mods.SequenceEqual(lastCalculatedMods))
                return;

            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            lastCalculatedBeatmap = beatmap;
            lastCalculatedMods = mods.ToList();

            starDifficultyBindable = difficultyCache.GetBindableDifficultyArtesanal(beatmap, ruleset, mods, starDifficultyCancellationSource.Token);
        }

        private LegacyMods convertSlotToMods(string slot)
        {
            LegacyMods slotToMods = slot[..2] switch
            {
                "NM" => LegacyMods.None,
                "HD" => LegacyMods.Hidden,
                "HR" => LegacyMods.HardRock,
                "DT" => LegacyMods.DoubleTime,
                "TB" => LegacyMods.None,
                _ => LegacyMods.None
            };

            return slotToMods;
        }
    }
}
