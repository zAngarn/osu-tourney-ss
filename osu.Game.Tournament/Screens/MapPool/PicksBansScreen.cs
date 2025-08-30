// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
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

namespace osu.Game.Tournament.Screens.MapPool
{
    public partial class PicksBansScreen : TournamentMatchScreen
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

        private string currentProtect = "blue";
        private string currentBan = null!;
        private string currentPick = null!;

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
                            RelativeSizeAxes = Axes.X,
                            Text = "Map Selection Panel",
                        },
                        new SettingsTextBox
                        {
                            LabelText = "Enter map slot (NM1, HR3...)",
                            RelativeSizeAxes = Axes.X,
                            Current = slot,
                        },
                        new ControlPanel.HorizontalLine(),

                        // ----------- protects
                        new TournamentSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Protects",
                        },
                        redProtectButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Protect",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Protect, mapSlot)
                        },
                        blueProtectButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Protect",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Protect, mapSlot)
                        },
                        new ControlPanel.HorizontalLine(),

                        // ----------- bans
                        new TournamentSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Bans",
                        },
                        redBanButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Ban",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Ban, mapSlot)
                        },
                        blueBanButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Ban",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Ban, mapSlot)
                        },
                        new ControlPanel.HorizontalLine(),

                        // ----------- picks
                        new TournamentSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Picks",
                        },
                        redPickButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Pick",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Pick, mapSlot)
                        },
                        bluePickButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Pick",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Pick, mapSlot)
                        },
                    },
                },
            };

            slot.BindValueChanged(slotString => mapSlot = slotString.NewValue.ToUpper(CultureInfo.InvariantCulture));

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
                    // Protects ---------------------------------------------
                    case ChoiceType.Protect:
                    default:
                    {
                        if (redActions.Count == 1 && blueActions.Count == 1)
                        {
                            currentProtect = null!;
                            break;
                        }

                        if (colour == TeamColour.Red)
                        {
                            redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                Scale = new Vector2(0.73f),
                            });

                            redProtectButton.BackgroundColour = Colour4.Gray;
                            blueProtectButton.Enabled.Value = false;
                            currentProtect = "red";
                        }
                        else
                        {
                            blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Scale = new Vector2(0.73f),
                            });

                            blueProtectButton.BackgroundColour = Colour4.Gray;
                            blueProtectButton.Enabled.Value = false;
                            currentProtect = "blue";
                        }

                        break;
                    }

                    // Bans
                    case ChoiceType.Ban when colour == TeamColour.Red:
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });

                        currentBan = "red";
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

                        currentBan = "blue";
                        break;
                    }

                    // Picks ---------------------------------------------
                    case ChoiceType.Pick when colour == TeamColour.Red:
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });

                        currentPick = "red";
                        break;

                    case ChoiceType.Pick when colour == TeamColour.Blue:
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = new Vector2(0.73f),
                        });

                        currentPick = "blue";
                        break;
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
                    if (panel.Beatmap.OnlineID == lastPickedMap.ID)
                    {
                        panel.UpdateState(colour);
                    }
                }
            }
        }

        public static void UpdateWinStateStatic(PicksBansScreen screen, TeamColour colour)
        {
            screen?.updateWinState(colour);
        }

        private void computeCurrentState()
        {
            blueProtectButton.Enabled.Value = false;
            redProtectButton.Enabled.Value = false;

            blueBanButton.Enabled.Value = false;
            redBanButton.Enabled.Value = false;

            bluePickButton.Enabled.Value = false;
            redPickButton.Enabled.Value = false;

            //PROTECTS
            if (redActions.Children.Count == 1 && blueActions.Children.Count == 1)
            {
                currentProtect = null!;
                currentBan = "blue";
            }
            else
            {
                switch (currentProtect)
                {
                    case "blue":
                        redProtectButton.Enabled.Value = true;
                        break;

                    case "red":
                        blueProtectButton.Enabled.Value = true;
                        break;

                    default:
                        blueProtectButton.Enabled.Value = false;
                        redProtectButton.Enabled.Value = false;
                        break;
                }
            }

            //BANS
            if (redActions.Children.Count == 3 && blueActions.Children.Count == 3)
            {
                currentBan = null!;
                currentPick = "blue";

                blueBanButton.BackgroundColour = Colour4.Gray;
                redBanButton.BackgroundColour = Colour4.Gray;

                redActions.Add(new Container
                {
                    AutoSizeAxes = Axes.X,
                    Height = 100,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                });
                blueActions.Add(new Container
                {
                    AutoSizeAxes = Axes.X,
                    Height = 100,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                });
            }
            else
            {
                switch (currentBan)
                {
                    case "blue":
                        redBanButton.Enabled.Value = true;
                        break;

                    case "red":
                        blueBanButton.Enabled.Value = true;
                        break;

                    default:
                        blueBanButton.Enabled.Value = false;
                        redBanButton.Enabled.Value = false;
                        break;
                }
            }

            if (redActions.Children.Count == 10 && blueActions.Children.Count == 10)
            {
                currentPick = "tiebreaker"; // <- a lo mejor puedes usar esto para algo
                bluePickButton.BackgroundColour = Colour4.Gray;
                redPickButton.BackgroundColour = Colour4.Gray;
            }
            else
            {
                switch (currentPick)
                {
                    case "blue":
                        redPickButton.Enabled.Value = true;
                        break;

                    case "red":
                        bluePickButton.Enabled.Value = true;
                        break;

                    default:
                        bluePickButton.Enabled.Value = false;
                        redPickButton.Enabled.Value = false;
                        break;
                }
            }
        }
    }
}

