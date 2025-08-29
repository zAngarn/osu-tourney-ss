// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.MapPool
{
    public partial class MapPoolScreenV2 : TournamentMatchScreen
    {
        private FillFlowContainer redActions = null!;
        private FillFlowContainer blueActions = null!;

        private readonly Bindable<string> slot = new Bindable<string>(string.Empty);

        private DrawablePlayerCard redPlayer = null!;
        private DrawablePlayerCard bluePlayer = null!;

        private RoundDisplayV2 roundDisplay = null!;

        private string mapSlot = null!;

        private OsuButton redBanButton = null!;
        private OsuButton blueBanButton = null!;
        private OsuButton redPickButton = null!;
        private OsuButton bluePickButton = null!;
        private OsuButton redProtectButton = null!;
        private OsuButton blueProtectButton = null!;

        private RoundBeatmap lastPickedMap = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            // TODO meter en dummyMatch los dummyTeams
            // Tienen que ser dos dummys distintos porque si no la instancia de TeamFlag es
            // compartida por ambos. 2H para darme cuenta de esto, soy imbécil.
            var dummyTeam1 = new TournamentTeam { FullName = { Value = "???" } };
            var dummyTeam2 = new TournamentTeam { FullName = { Value = "???" } };
            var dummyRound = new TournamentRound { Name = { Value = "???" } };

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("mappoolV2")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both
                },
                roundDisplay = new RoundDisplayV2(dummyRound)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 160 }
                },
                redPlayer = new DrawablePlayerCard(dummyTeam1, Color4Extensions.FromHex("#6ddded"))
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Scale = new Vector2(1.4f),
                    Margin = new MarginPadding { Top = 100, Left = 20 }
                },
                bluePlayer = new DrawablePlayerCard(dummyTeam2, Color4Extensions.FromHex("#ed6dac"))
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Scale = new Vector2(1.4f),
                    Margin = new MarginPadding { Top = 100, Right = 220 }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Y = 95,
                    Width = 0.5f,
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
                        new TournamentSpriteText
                        {
                            Text = "Map Selection Panel"
                        },
                        new SettingsTextBox
                        {
                            LabelText = "Enter map slot (NM1, HR3...)",
                            RelativeSizeAxes = Axes.X,
                            Current = slot,
                        },
                        new ControlPanel.Spacer(),
                        redBanButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Ban",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Ban, mapSlot)
                        },
                        blueBanButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Ban",
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Ban, mapSlot)
                        },
                        new ControlPanel.Spacer(),
                        redPickButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Pick",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Pick, mapSlot)
                        },
                        bluePickButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Pick",
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Pick, mapSlot)
                        },
                        new ControlPanel.Spacer(),
                        redProtectButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Protect",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Protect, mapSlot)
                        },
                        blueProtectButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Protect",
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Protect, mapSlot)
                        },
                    }
                }
            };

            slot.BindValueChanged(slotString => mapSlot = slotString.NewValue);

            // La lógica reside en primero se le da un dummy para que no crashee, despues ese dummy lo
            // reemplazo por el team real. Es bastante peruano pero qué se le va a hacer.
            LadderInfo.CurrentMatch.BindValueChanged(match =>
            {
                TournamentTeam t1 = match.NewValue?.Team1?.Value
                                    ?? new TournamentTeam { FullName = { Value = "???" } };

                TournamentTeam t2 = match.NewValue?.Team2?.Value
                                    ?? new TournamentTeam { FullName = { Value = "???" } };

                TournamentRound round = match.NewValue?.Round.Value
                                        ?? new TournamentRound { Name = { Value = "???" } };

                redPlayer.Team = t1;
                bluePlayer.Team = t2;
                roundDisplay.Round = round;
            }, true);
        }

        private void executeAction(TeamColour colour, ChoiceType choiceType, string map)
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

                /*if (CurrentMatch.Value.PicksBansProtects.Any(p => p.BeatmapID == targetMap.ID))
                    return;*/

                // Con esto debería ser compatible con la mappool antigua...
                CurrentMatch.Value.PicksBansProtects.Add(new BeatmapChoice
                {
                    Team = colour,
                    Type = choiceType,
                    BeatmapID = targetMap.ID
                });

                lastPickedMap = targetMap;

                switch (choiceType)
                {
                    // Picks
                    case ChoiceType.Pick when colour == TeamColour.Red:
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });
                        break;

                    case ChoiceType.Pick when colour == TeamColour.Blue:
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = new Vector2(0.73f),
                        });
                        break;

                    // Bans
                    case ChoiceType.Ban when colour == TeamColour.Red:
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });

                        if (redActions.Children.Count >= 3)
                        {
                            redActions.Add(new Container
                            {
                                AutoSizeAxes = Axes.X,
                                Height = 100,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                            });
                            redBanButton.Enabled.Value = false;
                            redBanButton.Colour = Colour4.Gray;
                        }

                        break;
                    }

                    case ChoiceType.Ban when colour == TeamColour.Blue:
                    {
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = new Vector2(0.73f),
                        });

                        if (blueActions.Children.Count >= 3)
                        {
                            blueActions.Add(new Container
                            {
                                AutoSizeAxes = Axes.X,
                                Height = 100,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            });
                            blueBanButton.Enabled.Value = false;
                            blueBanButton.Colour = Colour4.Gray;
                        }

                        break;
                    }

                    // Protects
                    case ChoiceType.Protect:
                    default:
                    {
                        if (colour == TeamColour.Red)
                        {
                            redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Scale = new Vector2(0.73f),
                            });

                            redProtectButton.Enabled.Value = false;
                            redProtectButton.Colour = Colour4.Gray;
                        }
                        else
                        {
                            blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Scale = new Vector2(0.73f),
                            });

                            blueProtectButton.Enabled.Value = false;
                            blueProtectButton.Colour = Colour4.Gray;
                        }

                        break;
                    }
                }
            }

            computeCurrentState();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            computeCurrentState();
        }

        private void updateWinState(TeamColour colour)
        {
            if (lastPickedMap == null) return;

            foreach (TournamentBeatmapPanelV2 b in redActions.OfType<TournamentBeatmapPanelV2>())
            {
                if (b is TournamentBeatmapPanelV2 panel && panel.Beatmap != null)
                {
                    int panelID1 = panel.Beatmap.OnlineID;

                    if (panel.Beatmap.OnlineID == lastPickedMap.ID)
                    {
                        panel.UpdateState(colour);
                    }
                }
            }

            foreach (TournamentBeatmapPanelV2 b in blueActions.OfType<TournamentBeatmapPanelV2>())
            {
                if (b is TournamentBeatmapPanelV2 panel && panel.Beatmap != null)
                {
                    int panelID2 = panel.Beatmap.OnlineID;

                    if (panel.Beatmap.OnlineID == lastPickedMap.ID)
                    {
                        panel.UpdateState(colour);
                    }
                }
            }
        }

        public static void UpdateWinStateStatic(MapPoolScreenV2 screen, TeamColour colour)
        {
            screen?.updateWinState(colour);
        }

        private void computeCurrentState()
        {
            if (blueActions.Children.Count < 1 || redActions.Children.Count < 1)
            {
                redPickButton.Colour = Color4.Gray;
                redPickButton.Enabled.Value = false;

                bluePickButton.Colour = Color4.Gray;
                bluePickButton.Enabled.Value = false;

                redBanButton.Colour = Color4.Gray;
                redBanButton.Enabled.Value = false;

                blueBanButton.Colour = Color4.Gray;
                blueBanButton.Enabled.Value = false;

                redProtectButton.Colour = Color4.White;
                redProtectButton.Enabled.Value = true;

                blueProtectButton.Colour = Color4.White;
                blueProtectButton.Enabled.Value = true;
            }
            else if ((blueActions.Children.Count < 3 || redActions.Children.Count < 3) && (blueActions.Children.Count >= 1 && redActions.Children.Count >= 1))
            {
                redBanButton.Colour = Color4.White;
                redBanButton.Enabled.Value = true;

                blueBanButton.Colour = Color4.White;
                blueBanButton.Enabled.Value = true;

                redProtectButton.Colour = Colour4.Gray;
                redProtectButton.Enabled.Value = false;

                blueProtectButton.Colour = Colour4.Gray;
                blueProtectButton.Enabled.Value = false;
            }
            else if (blueActions.Children.Count >= 3 && redActions.Children.Count >= 3)
            {
                redPickButton.Colour = Color4.White;
                redPickButton.Enabled.Value = true;

                bluePickButton.Colour = Color4.White;
                bluePickButton.Enabled.Value = true;

                redBanButton.Colour = Colour4.Gray;
                redBanButton.Enabled.Value = false;

                blueBanButton.Colour = Colour4.Gray;
                blueBanButton.Enabled.Value = false;
            }
        }
    }
}

