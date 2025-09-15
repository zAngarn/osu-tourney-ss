// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osu.Game.Tournament.Screens.MapPool;
using osu.Game.Tournament.Screens.TeamWin;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay
{
    public partial class GameplayScreen : BeatmapInfoScreen
    {
        private readonly BindableBool warmup = new BindableBool();

        public readonly Bindable<TourneyState> State = new Bindable<TourneyState>();
        private OsuButton warmupButton = null!;
        private MatchIPCInfo ipc = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private TournamentMatchChatDisplay chat { get; set; } = null!;

        private Drawable chroma = null!;

        private const int distancia_x = 374;
        private const int distancia_y = -130; //protect como referencia base

        public static BindableList<RoundBeatmap?> BlueProtects = LadderInfo.BlueProtects;
        public static BindableList<RoundBeatmap?> BlueBans = LadderInfo.BlueBans;
        public static BindableList<RoundBeatmap?> RedProtects = LadderInfo.RedProtects;
        public static BindableList<RoundBeatmap?> RedBans = LadderInfo.RedBans;

        private static Container containerPicksBansProtects = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            this.ipc = ipc;

            var dummyMatch = new TournamentMatch
            {
                Round =
                {
                    Value = new TournamentRound { Name = { Value = "???" } }
                },
                Team1 =
                {
                    Value = new TournamentTeam { FullName = { Value = "???" } }
                },
                Team2 =
                {
                    Value = new TournamentTeam { FullName = { Value = "???" } }
                },
            };

            AddRangeInternal(new Drawable[]
            {
                new TourneyVideo("gameplay")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new RoundDisplayV2
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 103 }
                },
                header = new MatchHeaderV2
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    ShowLogo = false,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Y = 34,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new[]
                    {
                        chroma = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 512,
                            Children = new Drawable[]
                            {
                                new ChromaArea
                                {
                                    Name = "Left chroma",
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                new ChromaArea
                                {
                                    Name = "Right chroma",
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = 0.5f,
                                }
                            }
                        },
                    }
                },
                containerPicksBansProtects = new Container
                {
                    AutoSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        warmupButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle warmup",
                            Action = () => warmup.Toggle()
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Toggle chat",
                            Action = () => { State.Value = State.Value == TourneyState.Idle ? TourneyState.Playing : TourneyState.Idle; }
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "Chroma width",
                            Current = LadderInfo.ChromaKeyWidth,
                            KeyboardStep = 1,
                        },
                        new SettingsSlider<int>
                        {
                            LabelText = "Players per team",
                            Current = LadderInfo.PlayersPerTeam,
                            KeyboardStep = 1,
                        },
                    }
                }
            });
            //SongBar.MoveToOffset(new Vector2(300, 0), 1000, Easing.Out);

            LadderInfo.ChromaKeyWidth.BindValueChanged(width => chroma.Width = width.NewValue, true);

            warmup.BindValueChanged(w =>
            {
                warmupButton.Alpha = !w.NewValue ? 0.5f : 1;
            }, true);

            LadderInfo.BlueProtects.BindCollectionChanged((sender, args) =>
            {
                UpdateDisplayPicksBansProtects();
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            State.BindTo(ipc.State);
            State.BindValueChanged(_ => updateState(), true);
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            if (match.NewValue == null)
                return;

            warmup.Value = match.NewValue.Team1Score.Value + match.NewValue.Team2Score.Value == 0;
            scheduledScreenChange?.Cancel();
        }

        private ScheduledDelegate? scheduledScreenChange;
        private ScheduledDelegate? scheduledContract;

        private TourneyState lastState;
        private MatchHeaderV2 header = null!;

        private void contract()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();
            using (chat.BeginDelayedSequence(500))
                chat.Expand();
        }

        private void expand()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            chat.Contract();
        }

        private void updateState()
        {
            try
            {
                scheduledScreenChange?.Cancel();

                if (State.Value == TourneyState.Ranking)
                {
                    if (warmup.Value || CurrentMatch.Value == null) return;

                    if (ipc.Score1.Value > ipc.Score2.Value)
                        CurrentMatch.Value.Team1Score.Value++;
                    else
                        CurrentMatch.Value.Team2Score.Value++;
                }

                switch (State.Value)
                {
                    case TourneyState.Idle:
                        contract();

                        if (LadderInfo.AutoProgressScreens.Value)
                        {
                            const float delay_before_progression = 4000;

                            // if we've returned to idle and the last screen was ranking
                            // we should automatically proceed after a short delay
                            if (lastState == TourneyState.Ranking && !warmup.Value)
                            {
                                if (CurrentMatch.Value?.Completed.Value == true)
                                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(TeamWinScreen)); }, delay_before_progression);
                                else if (CurrentMatch.Value?.Completed.Value == false)
                                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(MapPoolScreen)); }, delay_before_progression);
                            }
                        }

                        break;

                    case TourneyState.Ranking:
                        scheduledContract = Scheduler.AddDelayed(contract, 10000);
                        break;

                    default:
                        expand();
                        break;
                }
            }
            finally
            {
                lastState = State.Value;
            }
        }

        public override void Hide()
        {
            scheduledScreenChange?.Cancel();
            base.Hide();
        }

        public override void Show()
        {
            updateState();
            base.Show();
        }

        public static void UpdateDisplayPicksBansProtects()
        {
            if (BlueProtects.Count < 1 || RedProtects.Count < 1 || BlueBans.Count < 2 || RedBans.Count < 2) return;
            //containerPicksBansProtects.Clear();

            containerPicksBansProtects.Children = new Drawable[]
            {
                // ------------------- RED
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = -distancia_x, Top = distancia_y },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            new TournamentSpriteTextWithBackground(RedProtects[0]!.Slot)
                            {
                                Scale = new Vector2(0.4f),
                                Origin = Anchor.TopRight,
                                Anchor = Anchor.TopRight,
                            },
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = -distancia_x, Top = distancia_y + 74 },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            new TournamentSpriteTextWithBackground(RedBans[0]!.Slot)
                            {
                                Scale = new Vector2(0.4f),
                                Origin = Anchor.TopRight,
                                Anchor = Anchor.TopRight,
                            },
                            new TournamentSpriteTextWithBackground(RedBans[1]!.Slot)
                            {
                                Scale = new Vector2(0.4f),
                                Origin = Anchor.TopRight,
                                Anchor = Anchor.TopRight,
                            },
                        }
                    }
                },
                // ------------------- BLUE
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = distancia_x, Top = distancia_y },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            new TournamentSpriteTextWithBackground(BlueProtects[0]!.Slot)
                            {
                                Scale = new Vector2(0.4f),
                                Origin = Anchor.TopLeft,
                                Anchor = Anchor.TopLeft,
                            },
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Left = distancia_x, Top = distancia_y + 74 },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            new TournamentSpriteTextWithBackground(BlueBans[0]!.Slot)
                            {
                                Scale = new Vector2(0.4f),
                                Origin = Anchor.TopLeft,
                                Anchor = Anchor.TopLeft,
                            },
                            new TournamentSpriteTextWithBackground(BlueBans[1]!.Slot)
                            {
                                Scale = new Vector2(0.4f),
                                Origin = Anchor.TopLeft,
                                Anchor = Anchor.TopLeft,
                            },
                        }
                    }
                },
            };
        }

        private partial class ChromaArea : CompositeDrawable
        {
            [Resolved]
            private LadderInfo ladder { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                // chroma key area for stable gameplay
                Colour = new Color4(0, 255, 0, 255);

                ladder.PlayersPerTeam.BindValueChanged(performLayout, true);
            }

            private void performLayout(ValueChangedEvent<int> playerCount)
            {
                switch (playerCount.NewValue)
                {
                    case 3:
                        InternalChildren = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.5f,
                                Height = 0.5f,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Height = 0.5f,
                            },
                        };
                        break;

                    default:
                        InternalChild = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        };
                        break;
                }
            }
        }
    }
}
