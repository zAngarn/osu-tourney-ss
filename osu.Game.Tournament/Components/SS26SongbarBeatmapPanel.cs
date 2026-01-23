// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class SS26SongbarBeatmapPanel : CompositeDrawable
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

        public SS26SongbarBeatmapPanel(IBeatmapInfo? beatmap, string slot)
        {
            Beatmap = beatmap;
            mod = slot[..2];
            this.slot = slot;

            Width = 520;
            Height = 520;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindTo(ladder.CurrentMatch);

            Masking = true;
            CornerRadius = 260f;

            AddRangeInternal(new Drawable[]
            {
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
                            Colour = Colour4.FromHex("#FF714D"),
                            Width = 1 / 2f
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#4DDBFF"),
                            Width = 1 / 2f
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    Width = 500 / 520f,
                    Height = 500 / 520f,
                    CornerRadius = 250f,
                    Children = new Drawable[]
                    {
                        new NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            OnlineInfo = Beatmap as IBeatmapSetOnlineInfo,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Masking = true,
                            Width = 400 / 500f,
                            Height = 400 / 500f,
                            CornerRadius = 200f,
                            Margin = new MarginPadding { Bottom = -40f },

                            Child = new BufferedContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(-100),  // importante para blur suave en bordes redondeados

                                Children = new Drawable[]
                                {
                                    // La portada VA DENTRO del buffer → esto es lo que se va a blurrear
                                    new NoUnloadBeatmapSetCover
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        OnlineInfo = Beatmap as IBeatmapSetOnlineInfo,
                                    },

                                    // El "vidrio" oscuro encima, semitransparente
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Colour4.FromHex("262626").Opacity(0.65f),  // baja opacity para que se vea más el blur debajo
                                    },
                                }
                            }.WithEffect(new BlurEffect
                            {
                                Sigma = new Vector2(8f), // prueba 5f, 7f, 9f y ve cuál te gusta más
                                Placement = EffectPlacement.InFront
                            })
                        },
                    }
                },
            });
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
