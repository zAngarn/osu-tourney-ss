// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentBeatmapPanelV2 : CompositeDrawable
    {
        public readonly IBeatmapInfo? Beatmap;

        private readonly string mod;

        private readonly string slot;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private Container content;

        public TournamentBeatmapPanelV2(IBeatmapInfo? beatmap, string mod, string slot)
        {
            Beatmap = beatmap;
            this.mod = mod;
            this.slot = slot;

            Width = 420;
            Height = 64;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            Masking = true;
            CornerRadius = 15f;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = getColor(mod),
                },
                new TournamentSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = slot,
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 30),
                    Shadow = false,
                    Margin = new MarginPadding { Left = 10 }
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    CornerRadius = 15f,
                    Width = 0.8452f, // 355/420
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("282828"),
                        },
                        new NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            OnlineInfo = (Beatmap as IBeatmapSetOnlineInfo),
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Width = 0.33f,
                                    Colour = Colour4.FromHex("282828"),
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Width = 0.67f,
                                    Colour = ColourInfo.GradientHorizontal(Colour4.FromHex("282828"), Colour4.FromHex("00000000"))
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Margin = new MarginPadding { Left = 15, Top = 25 },
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Colour = getColor(mod),
                                    Text = Beatmap?.Metadata.Artist ?? "unknown",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 10),
                                },
                                new TournamentSpriteText
                                {
                                    Colour = Colour4.White,
                                    Text = Beatmap?.Metadata.Title ?? (LocalisableString)@"unknown",
                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5),
                                    Children = new Drawable[]
                                    {
                                        new TournamentSpriteText
                                        {
                                            Colour = getColor(mod),
                                            Text = Beatmap?.DifficultyName ?? "unknown",
                                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 10),
                                        },
                                        new TournamentSpriteText
                                        {
                                            Colour = Colour4.White,
                                            Text = "mapeado por " + (Beatmap?.Metadata.Author.Username ?? "unknown"),
                                            Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 10),
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private Colour4 getColor(string mod)
        {
            Colour4 color;

            switch (mod)
            {
                case "NM":
                    color = Colour4.FromHex("29a8f9");
                    break;

                case "HR":
                    color = Colour4.FromHex("f24141");
                    break;

                case "HD":
                    color = Colour4.FromHex("fbba20");
                    break;

                case "DT":
                    color = Colour4.FromHex("ca8cfb");
                    break;

                case "TB":
                    color = Colour4.FromHex("aeaeae");
                    break;

                default:
                    color = Colour4.FromHex("29a8f9"); // Mismo que NM. Por qué? Porque me sale del nabo
                    break;
            }

            return color;
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
                match.OldValue.PicksBansProtects.CollectionChanged -= picksBansProtectsOnCollectionChanged;
            if (match.NewValue != null)
                match.NewValue.PicksBansProtects.CollectionChanged += picksBansProtectsOnCollectionChanged;

            //Scheduler.AddOnce(UpdateState);
        }

        private void picksBansProtectsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Logger.Log("d"); //Scheduler.AddOnce(UpdateState);

        public void UpdateState(TeamColour colour)
        {
            Colour4 colorGradiente = Colour4.FromHex(colour == TeamColour.Blue ? "ed6dac" : "6ddded");

            content.Add(new Box
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Colour = ColourInfo.GradientHorizontal(Colour4.FromHex("00000000"), colorGradiente),
            });
        }

        private partial class NoUnloadBeatmapSetCover : UpdateableOnlineBeatmapSetCover
        {
            // As covers are displayed on stream, we want them to load as soon as possible.
            protected override double LoadDelay => 0;

            // Use DelayedLoadWrapper to avoid content unloading when switching away to another screen.
            protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
                => new DelayedLoadWrapper(createContentFunc(), timeBeforeLoad);
        }
    }
}
