// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class SS26BeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        private string slot;

        private readonly string mod;

        public string Slot
        {
            get => slot;
            set
            {
                slot = value;
                return;
            }
        }

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public SS26BeatmapPanel(IBeatmapInfo? beatmap, string slot)
        {
            Beatmap = beatmap;
            mod = slot[..2];
            this.slot = slot;

            Width = 350;
            Height = 50;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindTo(ladder.CurrentMatch);

            Masking = true;
            CornerRadius = 25f;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#282828"),
                },
                new NoUnloadBeatmapSetCover
                {
                    RelativeSizeAxes = Axes.Both,
                    OnlineInfo = Beatmap as IBeatmapSetOnlineInfo,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Colour4.FromHex("#262626ff"), Colour4.FromHex("#26262622")),
                            Width = 2 / 3f
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#26262622"),
                            Width = 1 / 3f
                        }
                    }
                },
                new Container
                {
                    Width = 44 / 350f,
                    Height = 44 / 50f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Masking = true,
                    CornerRadius = 22f,
                    Margin = new MarginPadding { Left = 3f },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = getColor(mod),
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Colour4.White,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                            Text = slot,
                        }
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 55f, Top = 10f },
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Colour = getColor(mod),
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 12),
                            Text = Beatmap?.Metadata?.Artist ?? "desconocido",
                        },
                        new TournamentSpriteText
                        {
                            Colour = Colour4.White,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                            Text = Beatmap?.Metadata?.Title ?? "desconocido",
                        },
                        new TournamentSpriteText
                        {
                            Colour = getColor(mod),
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 12),
                            Text = Beatmap?.DifficultyName ?? "desconocido",
                        }
                    }
                },
            });
        }

        private Colour4 getColor(string mod)
        {
            Colour4 color;

            switch (mod)
            {
                case "NM":
                    color = Colour4.FromHex("659EEB");
                    break;

                case "HR":
                    color = Colour4.FromHex("E06050");
                    break;

                case "HD":
                    color = Colour4.FromHex("FFB844");
                    break;

                case "DT":
                    color = Colour4.FromHex("8E7CBA");
                    break;

                case "TB":
                    color = Colour4.FromHex("AEAEAE");
                    break;

                default:
                    color = Colour4.FromHex("659EEB"); // Mismo que NM. ¿Por qué? Porque me sale del nabo
                    break;
            }

            return color;
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
}
