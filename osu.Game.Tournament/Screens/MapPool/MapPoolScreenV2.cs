// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.MapPool
{
    public partial class MapPoolScreenV2 : TournamentMatchScreen
    {
        private FillFlowContainer<TournamentBeatmapPanelV2> redActions = null!;
        private FillFlowContainer<TournamentBeatmapPanelV2> blueActions = null!;

        private readonly Bindable<string> slot = new Bindable<string>(string.Empty);

        private DrawablePlayerCard redPlayer = null!;
        private DrawablePlayerCard bluePlayer = null!;

        private RoundDisplayV2 roundDisplay = null!;

        private string mapSlot = null!;

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
                    Y = 160,
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
                                redActions = new FillFlowContainer<TournamentBeatmapPanelV2>()
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Width = 0.5f,
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                blueActions = new FillFlowContainer<TournamentBeatmapPanelV2>()
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = 0.5f,
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
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Ban",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Ban, mapSlot)
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Ban",
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Ban, mapSlot)
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Pick",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Pick, mapSlot)
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Pick",
                            Action = () => executeAction(TeamColour.Blue, ChoiceType.Pick, mapSlot)
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Protect",
                            Action = () => executeAction(TeamColour.Red, ChoiceType.Protect, mapSlot)
                        },
                        new TourneyButton
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
                foreach (var b in CurrentMatch.Value.Round.Value.Beatmaps)
                {
                    if (b.Slot == map)
                    {
                        targetMap = b;
                    }
                }

                if (targetMap == null!) return;

                if (choiceType == ChoiceType.Pick) // Pick
                {
                    if (colour == TeamColour.Red)
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                    else
                    {
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                }
                else if (choiceType == ChoiceType.Ban) // Bans
                {
                    if (colour == TeamColour.Red)
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                    else
                    {
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                }
                else // Protects
                {
                    if (colour == TeamColour.Red)
                    {
                        redActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                    else
                    {
                        blueActions.Add(new TournamentBeatmapPanelV2(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                }
            }
        }
    }
}
