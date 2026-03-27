// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.MapPool
{
    public partial class MatchTimelineScreen : TournamentMatchScreen
    {
        private FillFlowContainer redActions = null!;
        private FillFlowContainer blueActions = null!;

        private Container tiebreakerCardContainer = null!;

        private readonly Bindable<string> slot = new Bindable<string>(string.Empty);

        private readonly Bindable<bool> firstBanBindable = new Bindable<bool>(false);
        private readonly Bindable<bool> firstPickBindable = new Bindable<bool>(false);

        private readonly Bindable<int?> team1Score = new Bindable<int?>();
        private readonly Bindable<int?> team2Score = new Bindable<int?>();

        private DrawableTeamCard redPlayer = null!;
        private DrawableTeamCard bluePlayer = null!;
        private ScheduledDelegate? scheduledScreenChange;

        private string mapSlot = null!;

        private OsuButton redBanButton = null!;
        private OsuButton blueBanButton = null!;
        private OsuButton redPickButton = null!;
        private OsuButton bluePickButton = null!;
        private OsuButton deletionButton = null!;

        private readonly Bindable<string> scrollingRoundText = new Bindable<string>("Ronda desconocida - Spanish Showdown 2026");

        private RoundBeatmap lastPickedMap = null!;

        private ChoiceType currentPhase = ChoiceType.Ban;

        private TeamColour currentBan = TeamColour.None;
        private TeamColour currentPick = TeamColour.None;

        private TeamColour firstBan = TeamColour.None;
        private TeamColour firstPick = TeamColour.None;

        private SettingsCheckbox firstBanCheck = null!;
        private SettingsCheckbox firstPickCheck = null!;

        private Box redTurnGlow = null!;
        private Box blueTurnGlow = null!;
        private Box redTurnGlow2 = null!;
        private Box blueTurnGlow2 = null!;

        private Container centerStatusContainer = null!;
        private TournamentSpriteText centerStatusText = null!;
        private Box centerStatusBackground = null!;

        private Container redStatusContainer = null!;
        private TournamentSpriteText redStatusText = null!;

        private Container blueStatusContainer = null!;
        private TournamentSpriteText blueStatusText = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            // Tienen que ser dos dummies distintos porque si no la instancia de TeamFlag es
            // compartida por ambos. 2H para darme cuenta de esto, soy imbécil.
            var dummyMatch = new TournamentMatch
            {
                Round =
                {
                    Value = new TournamentRound { Description = { Value = "???" } }
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

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("picksbans")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both
                },
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
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.37f,
                    TriangleScale = 2,
                    Blending = BlendingParameters.Additive,
                    Colour = Colour4.White,
                },
                redTurnGlow = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White,
                    Width = 1 / 2f,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight,
                    Alpha = 0,
                },
                blueTurnGlow = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White,
                    Width = 1 / 2f,
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft,
                    Alpha = 0,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 800 / 1920f,
                    Height = 835 / 1080f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 28, Top = 96 },
                    Masking = true,
                    CornerRadius = 28f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#262626"),
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 750 / 1920f,
                    Height = 835 / 1080f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 28, Top = 96 },
                    Masking = true,
                    CornerRadius = 28f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#262626"),
                        },
                    }
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#262626"),
                    Width = 400 / 1920f,
                    Height = 810 / 1080f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Top = 192 },
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("#262626"),
                    Width = 400 / 1920f,
                    Height = 810 / 1080f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Top = 192 },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 839 / 1920f,
                    Height = 245 / 1080f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 28, Top = 689 },
                    Masking = true,
                    CornerRadius = 65f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#FF714D"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.37f,
                            TriangleScale = 2,
                            Blending = BlendingParameters.Additive,
                            Colour = Colour4.White,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 839 / 1920f,
                    Height = 245 / 1080f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding { Right = 28, Top = 689 },
                    Masking = true,
                    CornerRadius = 65f,
                    Children = new Drawable[]
                    {
                        redTurnGlow2 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 839 / 1920f,
                    Height = 245 / 1080f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 28, Top = 689 },
                    Masking = true,
                    CornerRadius = 65f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#4DDBFF"),
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.37f,
                            TriangleScale = 2,
                            Blending = BlendingParameters.Additive,
                            Colour = Colour4.White,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 839 / 1920f,
                    Height = 245 / 1080f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 28, Top = 689 },
                    Masking = true,
                    CornerRadius = 65f,
                    Children = new Drawable[]
                    {
                        blueTurnGlow2 = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 500 / 1920f,
                    Height = 164 / 1080f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding { Bottom = -58 },
                    Masking = true,
                    CornerRadius = 65f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#262626"),
                        },
                    }
                },
                redPlayer = new DrawableTeamCard(dummyMatch.Team1.Value!, Color4Extensions.FromHex("#FF714D"))
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                    Margin = new MarginPadding { Top = 70, Left = 70 }
                },
                bluePlayer = new DrawableTeamCard(dummyMatch.Team2.Value!, Color4Extensions.FromHex("#4DDBFF"), 0, true)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                    Margin = new MarginPadding { Top = 70, Right = 430 }
                },
                tiebreakerCardContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                new SS26ScrollingMessage(() =>
                {
                    var text = new TournamentSpriteText
                    {
                        Font = OsuFont.BalooDa.With(size: 24, weight: FontWeight.Black),
                        Colour = Colour4.White,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0.2f,
                    };

                    scrollingRoundText.BindValueChanged(v => text.Text = v.NewValue, true);

                    return text;
                })
                {
                    Y = 16,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                new ScoreOnlyMatchHeader(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Y = 140,
                    Width = 0.49f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                redActions = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Width = 0.5f,
                                    RelativeSizeAxes = Axes.X,
                                    Spacing = new Vector2(8),
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                blueActions = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = 0.5f,
                                    RelativeSizeAxes = Axes.X,
                                    Spacing = new Vector2(8),
                                }
                            }
                        },
                    }
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new SettingsTextBox
                        {
                            LabelText = "Enter map slot to add",
                            RelativeSizeAxes = Axes.X,
                            Current = slot,
                        },
                        // ----------- bans
                        blueBanButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Ban",
                            Action = () => addMap(TeamColour.Blue, ChoiceType.Ban, mapSlot)
                        },
                        redBanButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Ban",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => addMap(TeamColour.Red, ChoiceType.Ban, mapSlot)
                        },
                        new ControlPanel.Spacer(),

                        // ----------- picks
                        bluePickButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Pick",
                            Action = () => addMap(TeamColour.Blue, ChoiceType.Pick, mapSlot)
                        },
                        redPickButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Pick",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => addMap(TeamColour.Red, ChoiceType.Pick, mapSlot)
                        },
                        new ControlPanel.Spacer(),
                        deletionButton = new TourneyButton()
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Delete last added beatmap",
                            Action = () => deleteMap(lastPickedMap.Slot),
                        },
                        new ControlPanel.Spacer(),
                        new TournamentSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Set starting state (click checkboxes)",
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold)
                        },
                        firstBanCheck = new SettingsCheckbox
                        {
                            LabelText = "First ban",
                            RelativeSizeAxes = Axes.X,
                            Current = firstBanBindable,
                        },
                        firstPickCheck = new SettingsCheckbox
                        {
                            LabelText = "First pick",
                            RelativeSizeAxes = Axes.X,
                            Current = firstPickBindable,
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Force Red Win",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => forceWinner(TeamColour.Red)
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Force Blue Win",
                            Action = () => forceWinner(TeamColour.Blue)
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Undo last win",
                            Action = undoLastWinner
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Force TB",
                            Action = forceTiebreaker
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Refresh Internal Automaton",
                            Action = computeCurrentState
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Reset Match State",
                            Action = resetMatch,
                        },
                    },
                },
            };

            AddInternal(centerStatusContainer = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Y = 30,
                Height = 40,
                Margin = new MarginPadding { Top = 600 },
                AutoSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = 20,
                Alpha = 0,
                Children = new Drawable[]
                {
                    centerStatusBackground = new Box { RelativeSizeAxes = Axes.Both, Colour = Colour4.Black.Opacity(0.8f) },
                    centerStatusText = new TournamentSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 20),
                        Margin = new MarginPadding { Horizontal = 20, Top = -10 },
                        Colour = Colour4.White,
                    }
                }
            });

            AddInternal(redStatusContainer = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Position = new Vector2(-500, 40),
                Margin = new MarginPadding { Top = 380 },
                AutoSizeAxes = Axes.Both,
                Alpha = 0,
                Child = redStatusText = new TournamentSpriteText
                {
                    Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 24),
                    Colour = Color4Extensions.FromHex("#FF714D"),
                }
            });

            AddInternal(blueStatusContainer = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Position = new Vector2(500, 40),
                Margin = new MarginPadding { Top = 380 },
                AutoSizeAxes = Axes.Both,
                Alpha = 0,
                Child = blueStatusText = new TournamentSpriteText
                {
                    Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 24),
                    Colour = Color4Extensions.FromHex("#4DDBFF"),
                }
            });

            firstBanBindable.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    firstBanCheck.Colour = Colour4.HotPink;
                    firstBan = TeamColour.Red;
                }
                else
                {
                    firstBanCheck.Colour = Colour4.FromHex("6ddded");
                    firstBan = TeamColour.Blue;
                }

                computeCurrentState();
            });

            firstPickBindable.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    firstPickCheck.Colour = Colour4.HotPink;
                    firstPick = TeamColour.Red;
                }
                else
                {
                    firstPickCheck.Colour = Colour4.FromHex("6ddded");
                    firstPick = TeamColour.Blue;
                }

                computeCurrentState();
            });

            slot.BindValueChanged(slotString => mapSlot = slotString.NewValue.ToUpper(CultureInfo.InvariantCulture));

            //LadderInfo.BlueBans.BindCollectionChanged((_, _) => GameplayScreen.UpdateDisplayPicksBansProtects(), true);
            //LadderInfo.RedBans.BindCollectionChanged((_, _) => GameplayScreen.UpdateDisplayPicksBansProtects(), true);

            // La lógica reside en primero se le da un dummy para que no crashee, después ese dummy lo
            // reemplazo por el team real. Es bastante peruano, pero qué se le va a hacer.
            LadderInfo.CurrentMatch.BindValueChanged(match =>
            {
                TournamentTeam t1 = match.NewValue?.Team1?.Value
                                    ?? new TournamentTeam { FullName = { Value = "???" } };

                TournamentTeam t2 = match.NewValue?.Team2?.Value
                                    ?? new TournamentTeam { FullName = { Value = "???" } };

                var round = match.NewValue?.Round.Value
                            ?? new TournamentRound { Description = { Value = "???" } };

                redPlayer.Team = t1;
                bluePlayer.Team = t2;
                scrollingRoundText.Value = $"{round.Description.Value} - Spanish Showdown 2026";
                computeCurrentState();
            }, true);

            ipc.Beatmap.BindValueChanged(beatmapChanged);
        }

        private void forceTiebreaker()
        {
            var tbMap = CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(x => x.Slot == "TB1");

            if (tbMap != null)
            {
                if (tiebreakerCardContainer.Children.Count > 1)
                {
                    foreach (var panel in tiebreakerCardContainer.Children.OfType<SS26BeatmapPanel>())
                    {
                        tiebreakerCardContainer.Remove(panel, true);
                    }
                }
                else
                {
                    var tbPanel = new SS26BeatmapPanel(tbMap.Beatmap, "TB1")
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding { Bottom = 10 },
                        Y = 50,
                        Alpha = 0,
                    };

                    tiebreakerCardContainer.Add(tbPanel);
                    tbPanel.FadeIn(600, Easing.OutQuint);
                    tbPanel.MoveToY(0, 800, Easing.OutElastic);

                    tbPanel.ScaleTo(TournamentGame.FACTOR_DE_REESCALADO_1080 + new Vector2(0.05f), 400, Easing.OutQuint)
                           .Then()
                           .ScaleTo(TournamentGame.FACTOR_DE_REESCALADO_1080, 800, Easing.OutElastic);

                    CurrentMatch.Value!.PicksBans.Add(new BeatmapChoice
                    {
                        Team = TeamColour.None,
                        Type = ChoiceType.Pick,
                        BeatmapID = tbMap.Beatmap!.OnlineID,
                        Slot = tbMap.Slot,
                    });
                }
            }
        }

        private void onScoreChanged(TeamColour team, int? oldScore, int? newScore)
        {
            int oldVal = oldScore ?? 0;
            int newVal = newScore ?? 0;

            if (newVal <= oldVal) return;

            var activePick = CurrentMatch.Value?.PicksBans
                                         .FirstOrDefault(p => p.Type == ChoiceType.Pick && p.Winner.Value == null);

            if (activePick != null)
            {
                activePick.Winner.Value = team;
                computeCurrentState();
            }
        }

        private void forceWinner(TeamColour team)
        {
            var activePick = CurrentMatch.Value?.PicksBans
                                         .FirstOrDefault(p => p.Type == ChoiceType.Pick && p.Winner.Value == null);

            if (activePick != null)
            {
                activePick.Winner.Value = team;
                computeCurrentState();
            }
        }

        private void undoLastWinner()
        {
            // Buscamos el ÚLTIMO pick que YA tenga un ganador asignado
            var lastResolvedPick = CurrentMatch.Value?.PicksBans
                                               .LastOrDefault(p => p.Type == ChoiceType.Pick && p.Winner.Value != null);

            if (lastResolvedPick != null)
            {
                lastResolvedPick.Winner.Value = null; // Quita el ganador y oculta el cartel visual
            }
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap?> beatmap)
        {
            bool found = false;
            string map = string.Empty;

            if (!LadderInfo.AutoProgressScreens.Value) return;

            if (CurrentMatch.Value?.Round.Value == null || beatmap.NewValue == null) return;

            // esperamos a que esten los protects y los bans TODO el número de bans debe ser extraido de la ronda
            if (CurrentMatch.Value?.PicksBans.Count(choice => choice.Type == ChoiceType.Ban) < 2) return;

            // si lo que toca es un tb hacemos skip también
            if (CurrentMatch.Value?.PicksBans.Count(choice => choice.Type == ChoiceType.Pick) == CurrentMatch.Value?.Round.Value.BestOf.Value - 1) return;

            foreach (var b in CurrentMatch.Value?.Round.Value.Beatmaps!)
            {
                if (beatmap.NewValue.OnlineID == b.ID)
                {
                    found = true;
                    map = b.Slot;
                }
            }

            if (found)
            {
                addMap(currentPick, ChoiceType.Pick, map);
                scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(GameplayScreen)); }, 4000);
            }
        }

        private void addMap(TeamColour colour, ChoiceType choiceType, string map)
        {
            RoundBeatmap targetMap = null!;
            if (CurrentMatch.Value == null!) return;

            if (CurrentMatch.Value.Round.Value != null)
            {
                foreach (RoundBeatmap b in CurrentMatch.Value.Round.Value.Beatmaps.Where(b => b.Slot == map))
                {
                    targetMap = b;
                }

                if (targetMap == null!) return;

                if (CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == targetMap.ID))
                    return;

                var choice = new BeatmapChoice
                {
                    Team = colour,
                    Type = choiceType,
                    BeatmapID = targetMap.ID,
                    Slot = map.ToUpper(CultureInfo.InvariantCulture)
                };

                CurrentMatch.Value.PicksBans.Add(choice);

                Console.WriteLine($"Team {colour} [{choiceType} {targetMap.Slot}]: {targetMap.ID}");

                lastPickedMap = targetMap;

                var panel = new SS26BeatmapPanel(targetMap.Beatmap, targetMap.Slot, "0", choice)
                {
                    Anchor = colour == TeamColour.Red ? Anchor.TopLeft : Anchor.TopRight,
                    Origin = colour == TeamColour.Red ? Anchor.TopLeft : Anchor.TopRight,
                };

                if (colour == TeamColour.Red)
                {
                    redActions.Add(panel);

                    if (choiceType == ChoiceType.Ban)
                    {
                        currentBan = TeamColour.Blue;
                        LadderInfo.RedBans.Add(targetMap);
                    }
                    else { currentPick = TeamColour.Blue; }

                    redActions.ScaleTo(1.03f, 50, Easing.OutQuint).Then().ScaleTo(1f, 400, Easing.OutElastic);
                }
                else
                {
                    blueActions.Add(panel);

                    if (choiceType == ChoiceType.Ban)
                    {
                        currentBan = TeamColour.Red;
                        LadderInfo.BlueBans.Add(targetMap);
                    }
                    else { currentPick = TeamColour.Red; }

                    blueActions.ScaleTo(1.03f, 50, Easing.OutQuint).Then().ScaleTo(1f, 400, Easing.OutElastic);
                }

                panel.ScaleTo(0)
                     .Then()
                     .ScaleTo(TournamentGame.FACTOR_DE_REESCALADO_1080, 800, Easing.OutQuint);

                panel.FadeInFromZero(800);

                computeCurrentState();
            }
        }

        private void deleteFromCollections(BeatmapChoice beatmap)
        {
            switch (beatmap.Team)
            {
                case TeamColour.Red:
                {
                    if (beatmap.Type == ChoiceType.Ban)
                    {
                        LadderInfo.RedBans.Remove(LadderInfo.RedBans.FirstOrDefault(map => map!.Slot == beatmap.Slot));
                    }

                    break;
                }

                case TeamColour.Blue:
                {
                    if (beatmap.Type == ChoiceType.Ban)
                    {
                        LadderInfo.BlueBans.Remove(LadderInfo.BlueBans.FirstOrDefault(map => map!.Slot == beatmap.Slot));
                    }

                    break;
                }
            }
        }

        private bool deleteMap(string s)
        {
            // Se elimina primero el mapa visualmente (redActions, etc.) y después de la lista general (creo que es
            // la que se guarda después al bracket) (Dios quiera que si por favor)
            SS26BeatmapPanel panelToDelete = null!;
            string where = string.Empty;
            int beatmapID = 0;

            bool found = false;

            if (CurrentMatch.Value?.PicksBans.Count == 0) return found;

            foreach (SS26BeatmapPanel b in redActions.OfType<SS26BeatmapPanel>())
            {
                if (b is SS26BeatmapPanel panel && panel.Beatmap != null)
                {
                    if (panel.Slot == s)
                    {
                        panelToDelete = panel;
                        where = "red";
                        beatmapID = panel.Beatmap.OnlineID;
                        found = true;
                    }
                }
            }

            foreach (SS26BeatmapPanel b in blueActions.OfType<SS26BeatmapPanel>())
            {
                if (b is SS26BeatmapPanel panel && panel.Beatmap != null)
                {
                    if (panel.Slot == s)
                    {
                        panelToDelete = panel;
                        where = "blue";
                        beatmapID = panel.Beatmap.OnlineID;
                        found = true;
                    }
                }
            }

            if (found)
            {
                var map = CurrentMatch.Value?.PicksBans.FirstOrDefault(b => b.BeatmapID == beatmapID);
                CurrentMatch.Value?.PicksBans.Remove(map!);
                deleteFromCollections(map!);

                if (where == "red")
                {
                    redActions.Remove(panelToDelete, true);
                }
                else
                {
                    blueActions.Remove(panelToDelete, true);
                }
            }

            computeCurrentState();
            return found;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            computeCurrentState();
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);

            if (match.NewValue == null) return;

            team1Score.UnbindBindings();
            team2Score.UnbindBindings();

            team1Score.BindTo(match.NewValue.Team1Score);
            team2Score.BindTo(match.NewValue.Team2Score);

            team1Score.BindValueChanged(score => onScoreChanged(TeamColour.Red, score.OldValue, score.NewValue));
            team2Score.BindValueChanged(score => onScoreChanged(TeamColour.Blue, score.OldValue, score.NewValue));

            updateDisplay();

            match.NewValue?.PicksBans.Clear(); // Limpio la lista porque es lo más facil
        }

        private void resetMatch()
        {
            LadderInfo.CurrentMatch.Value?.PicksBans.Clear();
            updateDisplay();
        }

        private void updateDisplay()
        {
            redActions.Clear();
            blueActions.Clear();

            LadderInfo.RedBans.Clear();
            LadderInfo.BlueBans.Clear();

            firstBanBindable.Value = false;
            firstPickBindable.Value = false;

            firstBan = TeamColour.None;
            firstPick = TeamColour.None;

            firstBanCheck.Colour = Colour4.White;
            firstPickCheck.Colour = Colour4.White;

            computeCurrentState();
        }

        private void updateWinState(TeamColour colour)
        {
            foreach (SS26BeatmapPanel b in redActions.OfType<SS26BeatmapPanel>())
            {
                if (b is SS26BeatmapPanel panel && panel.Beatmap != null)
                {
                    if (panel.Beatmap.OnlineID == lastPickedMap.ID)
                    {
                        panel.SetWinState(colour);
                    }
                }
            }

            foreach (SS26BeatmapPanel b in blueActions.OfType<SS26BeatmapPanel>())
            {
                if (b is SS26BeatmapPanel panel && panel.Beatmap != null)
                {
                    if (panel.Beatmap.OnlineID == lastPickedMap.ID)
                    {
                        panel.SetWinState(colour);
                    }
                }
            }

            computeCurrentState();
        }

        private void disableAllButtons()
        {
            blueBanButton.Enabled.Value = false;
            redBanButton.Enabled.Value = false;

            bluePickButton.Enabled.Value = false;
            redPickButton.Enabled.Value = false;

            deletionButton.Enabled.Value = false;
        }

        private TeamColour getOppositeColour(TeamColour c)
        {
            TeamColour color = c switch
            {
                TeamColour.Blue => TeamColour.Red,
                TeamColour.Red => TeamColour.Blue,
                _ => TeamColour.Red // default por si aca
            };

            return color;
        }

        private void computeCurrentState()
        {
            if (CurrentMatch.Value?.Round.Value == null) return;

            disableAllButtons();

            if (firstPick == TeamColour.None || firstBan == TeamColour.None) return;

            deletionButton.Enabled.Value = true;

            int bansPerTeam = CurrentMatch.Value.Round.Value.BanCount.Value;
            int totalBansNeeded = bansPerTeam * 2;

            var picksBansList = CurrentMatch.Value.PicksBans;

            int bansRealizados = picksBansList.Count(choice => choice.Type == ChoiceType.Ban);
            int picksRealizados = picksBansList.Count(choice => choice.Type == ChoiceType.Pick && choice.Slot != "TB1");

            bool isMapInPlay = picksBansList.Any(choice => choice.Type == ChoiceType.Pick && choice.Winner.Value == null && choice.Slot != "TB1");

            bool isTiebreaker = CurrentMatch.Value?.Team1Score.Value == (CurrentMatch.Value?.Round.Value.BestOf.Value - 1) / 2
                                && CurrentMatch.Value?.Team2Score.Value == (CurrentMatch.Value?.Round.Value.BestOf.Value - 1) / 2;

            bool hasAllPicks = picksRealizados == CurrentMatch.Value?.Round.Value.BestOf.Value - 1;

            if (bansRealizados < 2 && bansRealizados < totalBansNeeded)
            {
                currentPhase = ChoiceType.Ban;
            }
            else if (bansPerTeam == 2 && bansRealizados >= 2 && picksRealizados < 4)
            {
                currentPhase = ChoiceType.Pick;
            }
            else if (bansPerTeam == 2 && picksRealizados >= 4 && bansRealizados < totalBansNeeded)
            {
                currentPhase = ChoiceType.Ban;
            }
            else if (!hasAllPicks && !isTiebreaker)
            {
                currentPhase = ChoiceType.Pick;
            }
            else
            {
                currentPhase = ChoiceType.None;
            }

            if (currentPhase == ChoiceType.Ban)
            {
                if (bansRealizados == 0)
                {
                    currentBan = firstBan;
                }
                else if (bansRealizados == 1)
                {
                    currentBan = getOppositeColour(firstBan);
                }
                else if (bansRealizados == 2)
                {
                    currentBan = getOppositeColour(firstBan);
                }
                else if (bansRealizados == 3)
                {
                    currentBan = firstBan;
                }

                if (currentBan == TeamColour.Blue) blueBanButton.Enabled.Value = true;
                else if (currentBan == TeamColour.Red) redBanButton.Enabled.Value = true;
            }
            else if (currentPhase == ChoiceType.Pick)
            {
                if (picksRealizados % 2 == 0)
                {
                    currentPick = firstPick;
                }
                else
                {
                    currentPick = getOppositeColour(firstPick);
                }

                if (currentPick == TeamColour.Blue) bluePickButton.Enabled.Value = true;
                else if (currentPick == TeamColour.Red) redPickButton.Enabled.Value = true;
            }

            if (hasAllPicks && isTiebreaker && picksBansList.All(x => x.Slot != "TB1"))
            {
                var tbMap = CurrentMatch.Value?.Round.Value.Beatmaps.FirstOrDefault(x => x.Slot == "TB1");

                if (tbMap != null)
                {
                    tiebreakerCardContainer.Add(new SS26BeatmapPanel(tbMap.Beatmap, "TB1")
                    {
                        Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding { Bottom = 10 },
                    });

                    CurrentMatch.Value!.PicksBans.Add(new BeatmapChoice
                    {
                        Team = TeamColour.None,
                        Type = ChoiceType.Pick,
                        BeatmapID = tbMap.Beatmap!.OnlineID,
                        Slot = tbMap.Slot,
                    });
                }
            }

            redTurnGlow.ClearTransforms();
            blueTurnGlow.ClearTransforms();

            redTurnGlow2.ClearTransforms();
            blueTurnGlow2.ClearTransforms();

            redTurnGlow.FadeOut(300, Easing.OutQuint);
            blueTurnGlow.FadeOut(300, Easing.OutQuint);

            redTurnGlow2.FadeOut(300, Easing.OutQuint);
            blueTurnGlow2.FadeOut(300, Easing.OutQuint);

            bool isRedTurn = (currentPhase == ChoiceType.Ban && currentBan == TeamColour.Red) ||
                             (currentPhase == ChoiceType.Pick && currentPick == TeamColour.Red);

            bool isBlueTurn = (currentPhase == ChoiceType.Ban && currentBan == TeamColour.Blue) ||
                              (currentPhase == ChoiceType.Pick && currentPick == TeamColour.Blue);

            if (!isMapInPlay)
            {
                if (isRedTurn)
                {
                    redTurnGlow.FadeTo(0.12f, 800, Easing.InOutSine).Then().FadeTo(0.02f, 800, Easing.InOutSine).Loop();
                    redTurnGlow2.FadeTo(0.12f, 800, Easing.InOutSine).Then().FadeTo(0.02f, 800, Easing.InOutSine).Loop();
                }
                else if (isBlueTurn)
                {
                    blueTurnGlow.FadeTo(0.12f, 800, Easing.InOutSine).Then().FadeTo(0.02f, 800, Easing.InOutSine).Loop();
                    blueTurnGlow2.FadeTo(0.12f, 800, Easing.InOutSine).Then().FadeTo(0.02f, 800, Easing.InOutSine).Loop();
                }
            }

            string newCenterText = "";
            Colour4 newCenterColor = Colour4.Black.Opacity(0.8f);
            string newRedText = "";
            string newBlueText = "";

            if (isMapInPlay)
            {
                newCenterText = "¡A JUGAR!";
                newCenterColor = Color4Extensions.FromHex("#FFD700"); // Dorado
            }
            else if (isRedTurn)
            {
                newRedText = currentPhase == ChoiceType.Ban ? "Baneando mapa..." : "Pickeando mapa...";
            }
            else if (isBlueTurn)
            {
                newBlueText = currentPhase == ChoiceType.Ban ? "Baneando mapa..." : "Pickeando mapa...";
            }
            else
            {
                newCenterText = "Esperando...";
            }

            if (string.IsNullOrEmpty(newCenterText))
            {
                centerStatusContainer.FadeOut(300, Easing.OutQuint);
            }
            else
            {
                centerStatusContainer.FadeIn(300, Easing.OutQuint);

                if (centerStatusText.Text != newCenterText)
                {
                    centerStatusBackground.FadeColour(newCenterColor, 300, Easing.OutQuint);
                    centerStatusText.Text = newCenterText;

                    centerStatusText.ClearTransforms();
                    centerStatusContainer.ScaleTo(1.15f, 150, Easing.OutQuint).Then().ScaleTo(1f, 400, Easing.OutElastic);

                    if (newCenterText == "¡A JUGAR!")
                    {
                        centerStatusText.Delay(550)
                                        .Loop(t => t.ScaleTo(1.05f, 800, Easing.InOutSine).RotateTo(1, 800, Easing.InOutSine)
                                                    .Then()
                                                    .ScaleTo(1f, 800, Easing.InOutSine).RotateTo(-1, 800, Easing.InOutSine));
                    }
                    else
                    {
                        centerStatusText.ScaleTo(1f, 300).RotateTo(0, 300);
                    }
                }
            }

            if (string.IsNullOrEmpty(newRedText))
            {
                redStatusContainer.FadeOut(300, Easing.OutQuint);
            }
            else
            {
                redStatusContainer.FadeIn(300, Easing.OutQuint);

                if (redStatusText.Text != newRedText)
                {
                    redStatusText.Text = newRedText;
                    redStatusContainer.MoveToY(30).Then().MoveToY(40, 500, Easing.OutElastic);

                    redStatusText.ClearTransforms();

                    redStatusText.ScaleTo(1f).RotateTo(0)
                                 .Loop(t => t.ScaleTo(1.08f, 1100, Easing.InOutSine).RotateTo(2, 1100, Easing.InOutSine)
                                             .Then()
                                             .ScaleTo(1f, 1100, Easing.InOutSine).RotateTo(-2, 1100, Easing.InOutSine));
                }
            }

            if (string.IsNullOrEmpty(newBlueText))
            {
                blueStatusContainer.FadeOut(300, Easing.OutQuint);
            }
            else
            {
                blueStatusContainer.FadeIn(300, Easing.OutQuint);

                if (blueStatusText.Text != newBlueText)
                {
                    blueStatusText.Text = newBlueText;
                    blueStatusContainer.MoveToY(30).Then().MoveToY(40, 500, Easing.OutElastic);

                    blueStatusText.ClearTransforms();

                    blueStatusText.ScaleTo(1f).RotateTo(0)
                                  .Loop(t => t.ScaleTo(1.08f, 1150, Easing.InOutSine).RotateTo(-2, 1150, Easing.InOutSine)
                                              .Then()
                                              .ScaleTo(1f, 1150, Easing.InOutSine).RotateTo(2, 1150, Easing.InOutSine));
                }
            }
        }
    }
}
