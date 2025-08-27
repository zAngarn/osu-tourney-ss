// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableMatchCard : CompositeDrawable
    {
        private readonly TournamentMatch Match;
        private const int ANCHO_TARJETA = 218;
        private const int ALTURA_TARJETA = 64;
        private bool matchStarted = false;
        private FillFlowContainer Scores;
        private readonly Bindable<int?> scoreTeam1 = new Bindable<int?>();
        private readonly Bindable<int?> scoreTeam2 = new Bindable<int?>();
        private TournamentSpriteText team1ScoreText;
        private TournamentSpriteText team2ScoreText;
        private int alpha = 1;

        [Resolved]
        private LadderInfo? ladderInfo { get; set; }

        public DrawableMatchCard(TournamentMatch match)
        {
            Match = match;
            Margin = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            Width = ANCHO_TARJETA,
                                            Height = ALTURA_TARJETA,
                                            Masking = true,
                                            Children = new Drawable[]
                                            {
                                                new DrawableTeamFlag(match.Team1.Value, new Vector2(109, 64), 10)
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Width = 0.5f,
                                                    Colour = Color4Extensions.FromHex("#3d3d3d"),
                                                },
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Width = 0.5f,
                                                    Colour = ColourInfo.GradientHorizontal(Color4Extensions.FromHex("#00000000"), Color4Extensions.FromHex("#3d3d3d")),
                                                },
                                                new TournamentSpriteText
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Text = match.Team1.Value!.Acronym.ToString() ?? string.Empty,
                                                    Colour = Color4Extensions.FromHex("#6ddded"),
                                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                                                },
                                            }
                                        },
                                        new Container
                                        {
                                            Width = ANCHO_TARJETA,
                                            Height = ALTURA_TARJETA,
                                            Masking = true,
                                            Children = new Drawable[]
                                            {
                                                new DrawableTeamFlag(match.Team2.Value, new Vector2(109, 64), 10)
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                },
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                    Width = 0.5f,
                                                    Colour = Color4Extensions.FromHex("#3d3d3d"),
                                                },
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                    Width = 0.5f,
                                                    Colour = ColourInfo.GradientHorizontal(Color4Extensions.FromHex("#3d3d3d"), Color4Extensions.FromHex("#00000000")),
                                                },
                                                new TournamentSpriteText
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Text = match.Team2.Value!.Acronym.ToString() ?? string.Empty,
                                                    Colour = Color4Extensions.FromHex("#ed6dac"),
                                                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                                                },
                                            }
                                        },
                                    }
                                }
                            }
                        },
                        Scores = new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            Origin = Anchor.Centre,
                            Margin = new MarginPadding { Top = 24, Left = 198 * 2 },
                            Alpha = alpha,
                            Children = new Drawable[]
                            {
                                team1ScoreText = new TournamentSpriteText
                                {
                                    Origin = Anchor.Centre,
                                    Text = match.Team1Score.Value.ToString()!,
                                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold, fixedWidth: true),
                                    Margin = new MarginPadding { Top = 20 }
                                },
                                new TournamentSpriteText
                                {
                                    Origin = Anchor.Centre,
                                    Text = " - ",
                                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold, fixedWidth: true),
                                    Margin = new MarginPadding { Top = 20 }
                                },
                                team2ScoreText = new TournamentSpriteText
                                {
                                    Origin = Anchor.Centre,
                                    Text = match.Team1Score.Value.ToString()!,
                                    Font = OsuFont.Torus.With(size: 24, weight: FontWeight.Bold, fixedWidth: true),
                                    Margin = new MarginPadding { Top = 20 }
                                }
                            }
                        }
                    }
                }
            };

            boundReference(match.Team1).BindValueChanged(_ => updateTeams());
            boundReference(match.Team2).BindValueChanged(_ => updateTeams());
            boundReference(match.Team1Score).BindValueChanged(_ => updateWinConditions());
            boundReference(match.Team2Score).BindValueChanged(_ => updateWinConditions());
            boundReference(match.Round).BindValueChanged(_ =>
            {
                updateWinConditions();
                Changed?.Invoke();
            });
            boundReference(match.Completed).BindValueChanged(_ => updateProgression());
            boundReference(match.Progression).BindValueChanged(_ => updateProgression());
            boundReference(match.LosersProgression).BindValueChanged(_ => updateProgression());
            boundReference(match.Losers).BindValueChanged(_ =>
            {
                updateTeams();
                Changed?.Invoke();
            });
            boundReference(match.Current).BindValueChanged(_ => updateCurrentMatch(), true);

            if (match.Team1 != null && match.Team2 != null)
            {
                scoreTeam1.BindTo(match.Team1Score);
                scoreTeam2.BindTo(match.Team2Score);
            }
        }

        public Action? Changed;

        private void updateTeams()
        {
            if (LoadState != LoadState.Loaded)
                return;

            // todo: teams may need to be bindable for transitions at a later point.

            if (Match.Team1.Value == null || Match.Team2.Value == null)
                Match.CancelMatchStart();

            if (Match.ConditionalMatches.Count > 0)
            {
                foreach (var conditional in Match.ConditionalMatches)
                {
                    bool team1Match = Match.Team1Acronym != null && conditional.Acronyms.Contains(Match.Team1Acronym);
                    bool team2Match = Match.Team2Acronym != null && conditional.Acronyms.Contains(Match.Team2Acronym);

                    if (team1Match && team2Match)
                        Match.Date.Value = conditional.Date.Value;
                }
            }

            /*Match.Started.BindValueChanged(val =>
            {
                alpha = val.NewValue ? 1 : 0;
            }, true);*/

            scoreTeam1.BindValueChanged(val =>
            {
                team1ScoreText.Text = val.NewValue?.ToString() ?? string.Empty;
            }, true);

            scoreTeam2.BindValueChanged(val =>
            {
                team2ScoreText.Text = val.NewValue?.ToString() ?? string.Empty;
            }, true);

            SchedulerAfterChildren.Add(() => Scheduler.Add(updateProgression));
            updateWinConditions();
        }

        private void updateProgression()
        {
            if (!Match.Completed.Value)
            {
                // ensure we clear any of our teams from our progression.
                // this is not pretty logic but should suffice for now.
                if (Match.Progression.Value != null && Match.Progression.Value.Team1.Value == Match.Team1.Value)
                    Match.Progression.Value.Team1.Value = null;

                if (Match.Progression.Value != null && Match.Progression.Value.Team2.Value == Match.Team2.Value)
                    Match.Progression.Value.Team2.Value = null;

                if (Match.LosersProgression.Value != null && Match.LosersProgression.Value.Team1.Value == Match.Team1.Value)
                    Match.LosersProgression.Value.Team1.Value = null;

                if (Match.LosersProgression.Value != null && Match.LosersProgression.Value.Team2.Value == Match.Team2.Value)
                    Match.LosersProgression.Value.Team2.Value = null;
            }
            else
            {
                Debug.Assert(Match.Winner != null);
                transferProgression(Match.Progression.Value, Match.Winner);
                Debug.Assert(Match.Loser != null);
                transferProgression(Match.LosersProgression.Value, Match.Loser);
            }

            Changed?.Invoke();
        }

        private void updateWinConditions()
        {
            if (Match.Round.Value == null) return;

            int instantWinAmount = Match.Round.Value.BestOf.Value / 2;

            Match.Completed.Value = Match.Round.Value.BestOf.Value > 0
                                    && (Match.Team1Score.Value + Match.Team2Score.Value >= Match.Round.Value.BestOf.Value || Match.Team1Score.Value > instantWinAmount
                                                                                                                          || Match.Team2Score.Value > instantWinAmount);
        }

        private void transferProgression(TournamentMatch? destination, TournamentTeam team)
        {
            if (destination == null) return;

            bool progressionAbove = destination.ID < Match.ID;

            Bindable<TournamentTeam?> destinationTeam;

            // check for the case where we have already transferred out value
            if (destination.Team1.Value == team)
                destinationTeam = destination.Team1;
            else if (destination.Team2.Value == team)
                destinationTeam = destination.Team2;
            else
            {
                destinationTeam = progressionAbove ? destination.Team2 : destination.Team1;
                if (destinationTeam.Value != null)
                    destinationTeam = progressionAbove ? destination.Team1 : destination.Team2;
            }

            destinationTeam.Value = team;
        }

        private void updateCurrentMatch()
        {
            Logger.Log("Match focus changed");
        }

        private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

        private T boundReference<T>(T obj)
            where T : IBindable
        {
            obj = (T)obj.GetBoundCopy();
            refBindables.Add(obj);
            return obj;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            foreach (var b in refBindables)
                b.UnbindAll();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateTeams();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Match is ConditionalTournamentMatch || e.Button != MouseButton.Left)
                return false;

            Selected = true;
            return true;
        }

        private bool selected;

        public bool Selected
        {
            get => selected;

            set
            {
                if (value == selected) return;

                selected = value;

                if (selected)
                {
                    ladderInfo.CurrentMatch.Value = Match;
                }
            }
        }
    }
}
