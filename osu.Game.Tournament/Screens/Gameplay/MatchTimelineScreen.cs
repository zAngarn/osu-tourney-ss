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
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
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

        private string mapSlot = null!;

        private OsuButton redBanButton = null!;
        private OsuButton blueBanButton = null!;
        private OsuButton redPickButton = null!;
        private OsuButton bluePickButton = null!;
        private OsuButton deletionButton = null!;

        private RoundBeatmap lastPickedMap = null!;

        private ChoiceType currentPhase = ChoiceType.Ban;

        private TeamColour currentBan = TeamColour.None;
        private TeamColour currentPick = TeamColour.None;

        private TeamColour firstBan = TeamColour.None;
        private TeamColour firstPick = TeamColour.None;

        private SettingsCheckbox firstBanCheck = null!;
        private SettingsCheckbox firstPickCheck = null!;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            // Tienen que ser dos dummies distintos porque si no la instancia de TeamFlag es
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
                            Text = "Reset Match State",
                            Action = resetMatch,
                        },
                    },
                },
            };

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

                redPlayer.Team = t1;
                bluePlayer.Team = t2;
                computeCurrentState();
            }, true);

            ipc.Beatmap.BindValueChanged(beatmapChanged);
        }

        private void onScoreChanged(TeamColour team, int? oldScore, int? newScore)
        {
            // Si la puntuación es null, asumimos que es 0
            int oldVal = oldScore ?? 0;
            int newVal = newScore ?? 0;

            // Solo asignamos si el score ha SUBIDO (ignora si el ref resta puntos)
            if (newVal <= oldVal) return;

            // Buscamos el primer Pick cronológico que todavía no tenga un ganador asignado
            var activePick = CurrentMatch.Value?.PicksBans
                                         .FirstOrDefault(p => p.Type == ChoiceType.Pick && p.Winner.Value == null);

            if (activePick != null)
            {
                activePick.Winner.Value = team; // Esto dispara el evento visual automáticamente
            }
        }

        private void forceWinner(TeamColour team)
        {
            var activePick = CurrentMatch.Value?.PicksBans
                                         .FirstOrDefault(p => p.Type == ChoiceType.Pick && p.Winner.Value == null);

            if (activePick != null)
            {
                activePick.Winner.Value = team;
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

                switch (choiceType)
                {
                    // Bans ---------------------------------------------
                    case ChoiceType.Ban when colour == TeamColour.Red:
                    {
                        redActions.Add(new SS26BeatmapPanel(targetMap.Beatmap, targetMap.Slot, "0", choice)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                        });

                        currentBan = TeamColour.Blue;
                        LadderInfo.RedBans.Add(targetMap);
                        break;
                    }

                    case ChoiceType.Ban when colour == TeamColour.Blue:
                    {
                        blueActions.Add(new SS26BeatmapPanel(targetMap.Beatmap, targetMap.Slot, "0", choice)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                        });

                        currentBan = TeamColour.Red;
                        LadderInfo.BlueBans.Add(targetMap);
                        break;
                    }

                    // Picks ---------------------------------------------
                    case ChoiceType.Pick when colour == TeamColour.Red:

                        redActions.Add(new SS26BeatmapPanel(targetMap.Beatmap, targetMap.Slot, "0", choice)
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                        });

                        currentPick = TeamColour.Blue;
                        break;

                    case ChoiceType.Pick when colour == TeamColour.Blue:

                        blueActions.Add(new SS26BeatmapPanel(targetMap.Beatmap, targetMap.Slot, "0", choice)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
                        });

                        currentPick = TeamColour.Red;
                        break;
                }

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
                        Margin = new MarginPadding { Bottom = 40 },
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
        }
    }
}
