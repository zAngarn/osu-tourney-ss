// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class SS26SongbarBeatmapPanel : CompositeDrawable
    {
        public readonly IBeatmapInfo? beatmap;

        private string slot;
        private readonly string mod;
        private readonly string length;
        private readonly double bpm;

        private readonly BeatmapDifficulty adjustedDifficulty;

        private MarqueeContainer artista = null!;

        private MarqueeContainer titulo = null!;
        //private MarqueeContainer dificultad = null!;

        private StarRatingDisplay starRatingDisplay = null!;

        public string Slot
        {
            get => slot;
            set
            {
                slot = value;
                return;
            }
        }

        private readonly IBindable<StarDifficulty> sr;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        public SS26SongbarBeatmapPanel(IBeatmapInfo? beatmap, string slot, BeatmapDifficulty adjustedDifficulty, double bpm, string length, IBindable<StarDifficulty> sr)
        {
            this.beatmap = beatmap;
            mod = slot[..2];
            this.slot = slot;
            this.adjustedDifficulty = adjustedDifficulty;
            this.sr = sr;
            this.length = length;
            this.bpm = bpm;

            Width = 520;
            Height = 520;
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindTo(ladder.CurrentMatch);

            Masking = true;
            CornerRadius = 260f;

            InternalChildren = new Drawable[]
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
                        new SS26BeatmapPanel.NoUnloadBeatmapSetCover
                        {
                            RelativeSizeAxes = Axes.Both,
                            OnlineInfo = beatmap as IBeatmapSetOnlineInfo,
                            FillMode = FillMode.Fill,
                            FillAspectRatio = 1f,
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

                            Children = new Drawable[]
                            {
                                new BufferedContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(-10),

                                    Child = new SS26BeatmapPanel.NoUnloadBeatmapSetCover
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        OnlineInfo = beatmap as IBeatmapSetOnlineInfo,
                                        FillMode = FillMode.Fill,
                                        FillAspectRatio = 1f,
                                    },
                                }.WithEffect(new BlurEffect
                                {
                                    Sigma = new Vector2(16f),
                                    Placement = EffectPlacement.Behind
                                }),
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex("262626").Opacity(0.65f),
                                },
                                new CircularContainer
                                {
                                    Size = new Vector2(400),
                                    BorderThickness = 4,
                                    BorderColour = TournamentGameBase.GetColor(mod),
                                    Masking = true,
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Colour4.White.Opacity(0)
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Height = 100 / 500f,
                            Width = 140 / 500f,
                            Masking = true,
                            Margin = new MarginPadding { Top = -140f },
                            Children = new Drawable[]
                            {
                                artista = new MarqueeContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.TopCentre,
                                    OverflowSpacing = 50,
                                },
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Height = 200 / 500f,
                            Width = 280 / 500f,
                            Masking = true,
                            Margin = new MarginPadding { Top = -180f },
                            Children = new Drawable[]
                            {
                                titulo = new MarqueeContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.TopCentre,
                                    OverflowSpacing = 50,
                                },
                            }
                        },
                        new BurbujitaEstadistica("CS", (adjustedDifficulty.CircleSize.ToString(CultureInfo.InvariantCulture)) ?? "0", TournamentGameBase.GetColor(mod))
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = -80, Left = -190 },
                        },
                        new BurbujitaEstadistica("HP", (adjustedDifficulty.DrainRate.ToString(CultureInfo.InvariantCulture)) ?? "0", TournamentGameBase.GetColor(mod))
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = 0, Left = -350 },
                        },
                        new BurbujitaEstadistica("AR", (adjustedDifficulty.ApproachRate.ToString(CultureInfo.InvariantCulture)) ?? "0", TournamentGameBase.GetColor(mod))
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = -80, Right = -190 },
                        },
                        new BurbujitaEstadistica("OD", (adjustedDifficulty.OverallDifficulty.ToString(CultureInfo.InvariantCulture)) ?? "0", TournamentGameBase.GetColor(mod))
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = 0, Right = -350 },
                        },
                        new SS26SlotPill(beatmap!.DifficultyName, Colour4.FromHex("262626"), TournamentGameBase.GetColor(slot[..2]))
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = 20f },
                        },
                        new SS26SlotPill(slot)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Margin = new MarginPadding { Top = -105f },
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Margin = new MarginPadding { Top = 70f, Left = -180f },
                            Children = new Drawable[]
                            {
                                new SS26MapStatPill(Colour4.FromHex("#282828"), TournamentGameBase.GetColor(mod), length, OsuIcon.Clock),
                                starRatingDisplay = new StarRatingDisplay(default),
                                new SS26MapStatPill(Colour4.FromHex("#282828"), TournamentGameBase.GetColor(mod), bpm.ToString(CultureInfo.InvariantCulture), OsuIcon.Metronome)
                            }
                        }
                    }
                },
            };

            artista.CreateContent = () => new TournamentSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Colour = TournamentGameBase.GetColor(mod),
                Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                Text = beatmap?.Metadata?.Artist ?? "desconocido",
            };

            titulo.CreateContent = () => new TournamentSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Colour = Colour4.White,
                Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 36),
                Text = beatmap?.Metadata?.Title ?? "desconocido",
            };

            /*dificultad.CreateContent = () => new TournamentSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Colour = getColor(mod),
                Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                Text = Beatmap?.DifficultyName ?? "desconocido",
            };*/

            sr.BindValueChanged(starDifficulty =>
            {
                starRatingDisplay.Current.Value = starDifficulty.NewValue;
            }, true);
        }
    }

    public partial class BurbujitaEstadistica : CompositeDrawable
    {
        public BurbujitaEstadistica(string encabezado, string contexto, Colour4 colour)
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    Height = 44,
                    Width = 44,
                    CornerRadius = 22f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colour
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Colour4.White,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                            Text = encabezado,
                            Margin = new MarginPadding { Bottom = 18 },
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Colour4.White,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                            Text = contexto,
                            Margin = new MarginPadding { Top = 18 },
                        }
                    }
                }
            };
        }
    }
}
