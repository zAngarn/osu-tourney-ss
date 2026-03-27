// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
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
        private MatchIPCInfo ipc = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private TournamentMatchChatDisplay chat { get; set; } = null!;

        private Drawable chroma = null!;

        private FillFlowContainer redBansFlow = null!;
        private FillFlowContainer redPicksFlow = null!;

        private FillFlowContainer blueBansFlow = null!;
        private FillFlowContainer bluePicksFlow = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            this.ipc = ipc;

            LabelledSwitchButton chatToggle;

            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                scoreDisplay = new TournamentMatchScoreDisplay
                {
                    Y = -265,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                },
                header = new MatchHeader
                {
                    Y = -280,
                    ShowScores = true,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new LabelledSwitchButton
                        {
                            Label = "Warmup",
                            Current = warmup,
                        },
                        chatToggle = new LabelledSwitchButton
                        {
                            Label = "Show chat",
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

            AddInternal(new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, -10),
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Left = 40, Bottom = 25 },
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(20, 0),
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Text = "BANS",
                                Colour = Color4Extensions.FromHex("#FF714D"),
                                Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 20),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft
                            },
                            redBansFlow = new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1, 0),
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Top = 10 },
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(20, 0),
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Text = "PICKS",
                                Colour = Color4Extensions.FromHex("#FF714D"),
                                Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 20),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft
                            },
                            redPicksFlow = new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1, 0),
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Top = 10 },
                            }
                        }
                    }
                }
            });

            AddInternal(new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, -10),
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Right = 40, Bottom = 25 },
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(20, 0),
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Children = new Drawable[]
                        {
                            blueBansFlow = new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1, 0),
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Top = 10 },
                            },
                            new TournamentSpriteText
                            {
                                Text = "BANS",
                                Colour = Color4Extensions.FromHex("#4DDBFF"),
                                Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 20),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(20, 0),
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Children = new Drawable[]
                        {
                            bluePicksFlow = new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(1, 0),
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Top = 10 },
                            },
                            new TournamentSpriteText
                            {
                                Text = "PICKS",
                                Colour = Color4Extensions.FromHex("#4DDBFF"),
                                Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 20),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft
                            },
                        }
                    }
                }
            });

            State.BindValueChanged(state => chatToggle.Current.Value = State.Value == TourneyState.Idle, true);
            chatToggle.Current.BindValueChanged(v => State.Value = v.NewValue ? TourneyState.Idle : TourneyState.Playing);

            LadderInfo.ChromaKeyWidth.BindValueChanged(width => chroma.Width = width.NewValue, true);

            //warmup.BindValueChanged(w => header.ShowScores = !w.NewValue, true);
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

            if (match.OldValue != null)
            {
                match.OldValue.PicksBans.CollectionChanged -= onPicksBansChanged;
            }

            if (match.NewValue != null)
            {
                match.NewValue.PicksBans.CollectionChanged += onPicksBansChanged;
            }

            rebuildChoices();
        }

        private void onPicksBansChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            rebuildChoices();
        }

        private void rebuildChoices() // Mejor llamarlo así ahora
        {
            redBansFlow.Clear();
            redPicksFlow.Clear();
            blueBansFlow.Clear();
            bluePicksFlow.Clear();

            if (CurrentMatch.Value == null) return;

            foreach (var choice in CurrentMatch.Value.PicksBans)
            {
                var choiceItem = new GameplayPickItem(choice);

                choiceItem.FadeInFromZero(400, Easing.OutQuint);
                choiceItem.MoveToY(10).Then().MoveToY(0, 400, Easing.OutQuint);

                if (choice.Type == ChoiceType.Ban)
                {
                    switch (choice.Team)
                    {
                        case TeamColour.Red:
                            redBansFlow.Add(choiceItem);
                            break;

                        case TeamColour.Blue:
                            blueBansFlow.Add(choiceItem);
                            break;
                    }
                }
                else if (choice.Type == ChoiceType.Pick)
                {
                    switch (choice.Team)
                    {
                        case TeamColour.Red:
                            redPicksFlow.Add(choiceItem);
                            break;

                        case TeamColour.Blue:
                            bluePicksFlow.Add(choiceItem);
                            break;

                        default:
                            redPicksFlow.Add(new GameplayPickItem(choice));
                            bluePicksFlow.Add(new GameplayPickItem(choice));
                            break;
                    }
                }
            }
        }

        private ScheduledDelegate? scheduledScreenChange;
        private ScheduledDelegate? scheduledContract;

        private TournamentMatchScoreDisplay scoreDisplay = null!;

        private TourneyState lastState;
        private MatchHeader header = null!;

        private void contract()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            //scoreDisplay.FadeOut(100);
            using (chat.BeginDelayedSequence(500))
                chat.Expand();
        }

        private void expand()
        {
            if (!IsLoaded)
                return;

            scheduledContract?.Cancel();

            chat.Contract();

            using (BeginDelayedSequence(300))
            {
                scoreDisplay.FadeIn(100);
            }
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

                    scoreDisplay.AnimateWin();
                }

                switch (State.Value)
                {
                    case TourneyState.Idle:
                        contract();

                        if (LadderInfo.AutoProgressScreens.Value)
                        {
                            const float delay_before_progression = 2000;

                            // if we've returned to idle and the last screen was ranking
                            // we should automatically proceed after a short delay
                            if (lastState == TourneyState.Ranking && !warmup.Value)
                            {
                                if (CurrentMatch.Value?.Completed.Value == true)
                                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(TeamWinScreen)); }, delay_before_progression * 2);

                                if (CurrentMatch.Value?.Completed.Value == false)
                                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(MatchTimelineScreen)); }, delay_before_progression * 2);

                                using (BeginDelayedSequence(delay_before_progression))
                                {
                                    scoreDisplay.ResetWinTransforms();
                                }
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

        public partial class GameplayPickItem : CompositeDrawable
        {
            public readonly BeatmapChoice Choice;

            private Box winnerBackground;
            private Container pulseContainer;

            public GameplayPickItem(BeatmapChoice choice)
            {
                Choice = choice;
                Size = new Vector2(51, 32);

                InternalChild = pulseContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = 16,
                            Masking = true,
                            BorderThickness = 3,
                            BorderColour = Colour4.Transparent,
                            Children = new Drawable[]
                            {
                                winnerBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0f
                                }
                            }
                        },
                        new SS26SlotPill(choice.Slot)
                        {
                            Margin = new MarginPadding(4),
                            Size = new Vector2(43, 24),
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Choice.Winner.BindValueChanged(onWinnerChanged, true);
            }

            private void onWinnerChanged(ValueChangedEvent<TeamColour?> winner)
            {
                var winnerColor = Color4Extensions.FromHex("#4DDBFF");

                if (winner.NewValue == TeamColour.Red)
                {
                    winnerColor = Color4Extensions.FromHex("#FF714D");
                }

                pulseContainer.ClearTransforms();

                if (Choice.Type == ChoiceType.Ban)
                {
                    pulseContainer.ScaleTo(0.85f);
                    pulseContainer.Alpha = 0.5f;
                    winnerBackground.Alpha = 0f;
                    pulseContainer.BorderColour = Colour4.Transparent;
                    return;
                }

                if (winner.NewValue == null)
                {
                    pulseContainer.Alpha = 1f;
                    pulseContainer.BorderColour = Colour4.Transparent;

                    pulseContainer.ScaleTo(1f)
                                  .Then()
                                  .Loop(p => p.ScaleTo(1.08f, 600, Easing.InOutSine)
                                              .Then()
                                              .ScaleTo(1f, 600, Easing.InOutSine));

                    winnerBackground.Alpha = 0f;
                }
                else
                {
                    pulseContainer.Alpha = 1f;
                    pulseContainer.ScaleTo(1f, 400, Easing.OutElastic);

                    pulseContainer.BorderColour = winnerColor;
                    winnerBackground.Colour = winnerColor;

                    bool isBreakpoint = winner.NewValue != Choice.Team && Choice.Team != TeamColour.None;
                    winnerBackground.FadeTo(isBreakpoint ? 0.3f : 0.8f, 400, Easing.OutQuint);
                }
            }
        }
    }
}
