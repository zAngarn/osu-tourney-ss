// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay.Components;

namespace osu.Game.Tournament.Screens.MapPool
{
    public partial class MapPoolScreenV2 : TournamentMatchScreen
    {
        private FillFlowContainer<TournamentBeatmapPanel> redActions = null!;
        private FillFlowContainer<TournamentBeatmapPanel> blueActions = null!;

        private readonly Bindable<string> slot = new Bindable<string>(string.Empty);

        private string mapSlot = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            InternalChildren = new Drawable[]
            {
                new TourneyVideo("mappoolV2")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both
                },
                new MatchHeader()
                {
                    ShowScores = true,
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
                                redActions = new FillFlowContainer<TournamentBeatmapPanel>()
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
                                blueActions = new FillFlowContainer<TournamentBeatmapPanel>()
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

                if (choiceType == ChoiceType.Pick)
                {
                    if (colour == TeamColour.Red)
                    {
                        redActions.Add(new TournamentBeatmapPanel(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 42,
                        });
                    }
                    else
                    {
                        blueActions.Add(new TournamentBeatmapPanel(targetMap.Beatmap, targetMap.Mods, targetMap.Slot)
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
