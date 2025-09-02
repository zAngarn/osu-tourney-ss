// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
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
        private readonly Bindable<string> toDeletion = new Bindable<string>(string.Empty);

        private readonly Bindable<bool> firstProtectBindable = new Bindable<bool>(false);
        private readonly Bindable<bool> firstBanBindable = new Bindable<bool>(false);
        private readonly Bindable<bool> firstPickBindable = new Bindable<bool>(false);

        private DrawablePlayerCard redPlayer = null!;
        private DrawablePlayerCard bluePlayer = null!;

        private RoundDisplayV2 roundDisplay = null!;

        private string mapSlot = null!;

        private bool redSpacer = false;
        private bool blueSpacer = false;

        private OsuButton redBanButton = null!;
        private OsuButton blueBanButton = null!;
        private OsuButton redPickButton = null!;
        private OsuButton bluePickButton = null!;
        private OsuButton redProtectButton = null!;
        private OsuButton blueProtectButton = null!;
        private OsuButton deletionButton = null!;

        private RoundBeatmap lastPickedMap = null!;

        private ChoiceType currentPhase = ChoiceType.Protect;

        private TeamColour currentProtect = TeamColour.None;
        private TeamColour currentBan = TeamColour.None;
        private TeamColour currentPick = TeamColour.None;

        private TeamColour firstProtect = TeamColour.None;
        private TeamColour firstBan = TeamColour.None;
        private TeamColour firstPick = TeamColour.None;

        private BeatmapChoice lastPlayed = null!;

        private SettingsCheckbox firstProtectCheck = null!;
        private SettingsCheckbox firstBanCheck = null!;
        private SettingsCheckbox firstPickCheck = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            // Tienen que ser dos dummys distintos porque si no la instancia de TeamFlag es
            // compartida por ambos. 2H para darme cuenta de esto, soy imbécil.
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

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("mappoolV2")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both
                },
                roundDisplay = new RoundDisplayV2(dummyMatch.Round.Value)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Margin = new MarginPadding { Left = 160 }
                },
                redPlayer = new DrawablePlayerCard(dummyMatch.Team1.Value!, Color4Extensions.FromHex("#ed6dac"))
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Scale = new Vector2(1.4f),
                    Margin = new MarginPadding { Top = 100, Left = 20 }
                },
                bluePlayer = new DrawablePlayerCard(dummyMatch.Team2.Value!, Color4Extensions.FromHex("#6ddded"))
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
                        new SettingsTextBox
                        {
                            LabelText = "Enter map slot to add",
                            RelativeSizeAxes = Axes.X,
                            Current = slot,
                        },
                        new ControlPanel.HorizontalLine(),

                        // ----------- protects
                        blueProtectButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Protect",
                            Action = () => addMap(TeamColour.Blue, ChoiceType.Protect, mapSlot)
                        },
                        redProtectButton = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Protect",
                            BackgroundColour = Colour4.HotPink,
                            Action = () => addMap(TeamColour.Red, ChoiceType.Protect, mapSlot)
                        },
                        new ControlPanel.HorizontalLine(),

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
                        new ControlPanel.HorizontalLine(),

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
                        new ControlPanel.HorizontalLine(),
                        new TournamentSpriteText
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Set starting state (click checkboxes)",
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold)
                        },
                        new ControlPanel.Spacer(),
                        firstProtectCheck = new SettingsCheckbox
                        {
                            LabelText = "First protect",
                            RelativeSizeAxes = Axes.X,
                            Current = firstProtectBindable,
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
                    },
                },
            };

            firstProtectBindable.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    firstProtectCheck.Colour = Colour4.HotPink;
                    firstProtect = TeamColour.Red;
                }
                else
                {
                    firstProtectCheck.Colour = Colour4.FromHex("6ddded");
                    firstProtect = TeamColour.Blue;
                }

                computeCurrentState();
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

            // La lógica reside en primero se le da un dummy para que no crashee, después ese dummy lo
            // reemplazo por el team real. Es bastante peruano, pero qué se le va a hacer.
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
                computeCurrentState();
            }, true);
        }

        private bool deleteMap(string s)
        {
            // Se elimina primero el mapa visualmente (redActions, etc.) y después de la lista general (creo que es
            // la que se guarda después al bracket) (Dios quiera que si por favor)
            TournamentBeatmapPanelV2 panelToDelete = null!;
            string where = string.Empty;
            int beatmapID = 0;

            bool found = false;

            if (CurrentMatch.Value?.PicksBansProtects.Count == 0) return found;

            foreach (TournamentBeatmapPanelV2 b in redActions.OfType<TournamentBeatmapPanelV2>())
            {
                if (b is TournamentBeatmapPanelV2 panel && panel.Beatmap != null)
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

            foreach (TournamentBeatmapPanelV2 b in blueActions.OfType<TournamentBeatmapPanelV2>())
            {
                if (b is TournamentBeatmapPanelV2 panel && panel.Beatmap != null)
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
                var map = CurrentMatch.Value?.PicksBansProtects.FirstOrDefault(b => b.BeatmapID == beatmapID);
                CurrentMatch.Value?.PicksBansProtects.Remove(map!);
            }

            if (found)
            {
                if (where == "red")
                {
                    redActions.Remove(panelToDelete, true);
                }
                else
                {
                    blueActions.Remove(panelToDelete, true);
                }

                found = true;
            }

            switch (currentPhase)
            {
                case ChoiceType.Protect when CurrentMatch.Value?.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Protect) < 1:
                    currentProtect = firstProtect; // Invertir color sin cambiar fase
                    break;

                case ChoiceType.Ban when CurrentMatch.Value?.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Protect) == 1:
                    currentProtect = getOppositeColour(firstProtect); // Invertir color cambiando fase (condicion de contorno)
                    currentBan = TeamColour.None;
                    break;

                case ChoiceType.Ban when CurrentMatch.Value?.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Ban) < 3:
                    currentBan = firstBan;
                    break;

                case ChoiceType.Pick when CurrentMatch.Value?.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Ban) == 3:
                    currentBan = getOppositeColour(firstBan);
                    currentPick = TeamColour.None;
                    break;

                case ChoiceType.Pick when CurrentMatch.Value?.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Pick) < 12:
                    currentPick = getOppositeColour(currentPick);
                    break;
            }

            computeCurrentState();
            return found;
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

                // TODO borrar el comentario de esta línea
                /*if (CurrentMatch.Value.PicksBansProtects.Any(p => p.BeatmapID == targetMap.ID))
                    return;*/

                // Con esto debería ser compatible con la mappool antigua...
                CurrentMatch.Value.PicksBansProtects.Add(new BeatmapChoice
                {
                    Team = colour,
                    Type = choiceType,
                    BeatmapID = targetMap.ID,
                    Slot = map.ToUpper(CultureInfo.InvariantCulture)
                });

                lastPickedMap = targetMap;

                switch (choiceType)
                {
                    // Protects ---------------------------------------------
                    case ChoiceType.Protect when colour == TeamColour.Red:
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });

                        currentProtect = TeamColour.Blue;
                        break;
                    }

                    case ChoiceType.Protect when colour == TeamColour.Blue:
                    {
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = new Vector2(0.73f),
                        });

                        currentProtect = TeamColour.Red;
                        break;
                    }

                    // Bans ---------------------------------------------
                    case ChoiceType.Ban when colour == TeamColour.Red:
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });

                        currentBan = TeamColour.Blue;
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

                        currentBan = TeamColour.Red;
                        break;
                    }

                    // Picks ---------------------------------------------
                    case ChoiceType.Pick when colour == TeamColour.Red:

                        if (redActions.Children.Count >= 3 && !redSpacer)
                        {
                            redActions.Add(new Container
                            {
                                AutoSizeAxes = Axes.X,
                                Height = 100,
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                            });

                            redSpacer = true;
                        }

                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = new Vector2(0.73f),
                        });

                        currentPick = TeamColour.Blue;
                        break;

                    case ChoiceType.Pick when colour == TeamColour.Blue:

                        if (blueActions.Children.Count >= 3 && !blueSpacer)
                        {
                            blueActions.Add(new Container
                            {
                                AutoSizeAxes = Axes.X,
                                Height = 100,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            });

                            blueSpacer = true;
                        }

                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = new Vector2(0.73f),
                        });

                        currentPick = TeamColour.Red;
                        break;
                }

                computeCurrentState();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            computeCurrentState();
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);
            updateDisplay();
        }

        private void updateDisplay()
        {
            redActions.Clear();
            blueActions.Clear();

            firstProtectBindable.Value = false;
            firstBanBindable.Value = false;
            firstPickBindable.Value = false;

            firstProtect = TeamColour.None;
            firstBan = TeamColour.None;
            firstPick = TeamColour.None;

            firstProtectCheck.Colour = Colour4.White;
            firstBanCheck.Colour = Colour4.White;
            firstPickCheck.Colour = Colour4.White;

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

        private BeatmapChoice getLastPlayedMap()
        {
            BeatmapChoice beatmapChoice;

            if (CurrentMatch.Value?.Round.Value != null)
            {
                if (CurrentMatch.Value.PicksBansProtects.Count != 0)
                {
                    beatmapChoice = CurrentMatch.Value.PicksBansProtects.Last();
                    beatmapChoice.TeamName = getTeamNameFromColour(beatmapChoice.Team);
                }
                else
                {
                    beatmapChoice = new BeatmapChoice
                    {
                        TeamName = "fuera de match",
                        Slot = "warmup",
                        Team = TeamColour.None,
                    };
                }
            }
            else
            {
                beatmapChoice = new BeatmapChoice
                {
                    TeamName = "desconocido",
                    Slot = "???",
                    Team = TeamColour.None,
                };
            }

            return beatmapChoice;
        }

        public static BeatmapChoice GetLastPlayedMap(PicksBansScreen screen)
        {
            BeatmapChoice beatmapChoice = screen?.getLastPlayedMap()!;

            return beatmapChoice;
        }

        private string getTeamNameFromColour(TeamColour colour)
        {
            string s = string.Empty;

            s = colour == TeamColour.Red ? CurrentMatch.Value!.Team1.Value!.FullName.Value : CurrentMatch.Value!.Team2.Value!.FullName.Value;

            return s;
        }

        private void disableAllButtons()
        {
            blueProtectButton.Enabled.Value = false;
            redProtectButton.Enabled.Value = false;

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

            if (firstPick == TeamColour.None || firstBan == TeamColour.None || firstProtect == TeamColour.None) return;

            deletionButton.Enabled.Value = true;

            // cambia segun la ronda
            bool hasAllProtects = CurrentMatch.Value.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Protect) == 2;
            bool hasAllBans = CurrentMatch.Value.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Ban) == 4;
            bool hasAllPicks = CurrentMatch.Value.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Pick) == CurrentMatch.Value?.Round.Value.BestOf.Value - 1;

            if (!hasAllProtects)
            {
                currentPhase = ChoiceType.Pick;

                if (CurrentMatch.Value.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Protect) < 1)
                {
                    if (firstProtect == TeamColour.Blue)
                    {
                        blueProtectButton.Enabled.Value = true;
                    }
                    else if (firstProtect == TeamColour.Red)
                    {
                        redProtectButton.Enabled.Value = true;
                    }
                }
                else
                {
                    if (currentProtect == TeamColour.Blue)
                    {
                        blueProtectButton.Enabled.Value = true;
                    }
                    else if (currentProtect == TeamColour.Red)
                    {
                        redProtectButton.Enabled.Value = true;
                    }
                }
            }
            else if (!hasAllBans)
            {
                currentPhase = ChoiceType.Ban;

                if (CurrentMatch.Value.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Ban) < 1)
                {
                    if (firstBan == TeamColour.Blue)
                    {
                        blueBanButton.Enabled.Value = true;
                    }
                    else if (firstBan == TeamColour.Red)
                    {
                        redBanButton.Enabled.Value = true;
                    }
                }
                else
                {
                    if (currentBan == TeamColour.Blue)
                    {
                        blueBanButton.Enabled.Value = true;
                    }
                    else if (currentBan == TeamColour.Red)
                    {
                        redBanButton.Enabled.Value = true;
                    }
                }
            }
            else if (!hasAllPicks)
            {
                currentPhase = ChoiceType.Pick;

                if (CurrentMatch.Value.PicksBansProtects.Count(choice => choice.Type == ChoiceType.Pick) < 1)
                {
                    if (firstPick == TeamColour.Blue)
                    {
                        bluePickButton.Enabled.Value = true;
                    }
                    else if (firstPick == TeamColour.Red)
                    {
                        redPickButton.Enabled.Value = true;
                    }
                }
                else
                {
                    if (currentPick == TeamColour.Blue)
                    {
                        bluePickButton.Enabled.Value = true;
                    }
                    else if (currentPick == TeamColour.Red)
                    {
                        redPickButton.Enabled.Value = true;
                    }
                }
            }
        }
    }
}

