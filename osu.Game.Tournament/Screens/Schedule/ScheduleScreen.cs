// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Schedule
{
    public partial class ScheduleScreen : TournamentScreen
    {
        private readonly BindableList<TournamentMatch> allMatches = new BindableList<TournamentMatch>();
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();
        private Container mainContainer = null!;
        private LadderInfo ladder = null!;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            this.ladder = ladder;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("schedule")
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = 50, Left = 20, Right = 20, Top = 100 },
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new DrawableTournamentHeaderText(),
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    mainContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                }
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            allMatches.BindTo(ladder.Matches);
            allMatches.BindCollectionChanged((_, _) => refresh());

            currentMatch.BindTo(ladder.CurrentMatch);
            currentMatch.BindValueChanged(_ => refresh(), true);
        }

        private void refresh()
        {
            const int days_for_displays = 4;

            IEnumerable<ConditionalTournamentMatch> conditionals =
                allMatches
                    .Where(m => !m.Completed.Value && (m.Team1.Value == null || m.Team2.Value == null) && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .SelectMany(m => m.ConditionalMatches.Where(cp => m.Acronyms.TrueForAll(a => cp.Acronyms.Contains(a))));

            IEnumerable<TournamentMatch> upcoming =
                allMatches
                    .Where(m => !m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .Concat(conditionals)
                    .OrderBy(m => m.Date.Value)
                    .Take(7);

            var recent =
                allMatches
                    .Where(m => m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .OrderByDescending(m => m.Date.Value)
                    .Take(7);

            FillFlowContainer comingUpNext;

            mainContainer.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.55f,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.75f,
                                },
                                comingUpNext = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.25f,
                                    Margin = new MarginPadding { Top = -90 }
                                },
                            }
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.45f,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ScheduleContainer("recent")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.25f,
                                    ChildrenEnumerable = recent.Select(p => new ScheduleMatch(p)),
                                },
                                new ScheduleContainer("upcoming")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.75f,
                                    ChildrenEnumerable = upcoming.Select(p => new ScheduleMatch(p))
                                },
                            }
                        }
                    }
                }
            };

            if (currentMatch.Value != null)
            {
                comingUpNext.Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = currentMatch.Value.Round.Value?.Name.Value.ToUpperInvariant() ?? string.Empty,
                            Font = OsuFont.Inter.With(size: 24, weight: FontWeight.Bold),
                            Colour = Colour4.FromHex("#F24998"),
                            Margin = new MarginPadding { Left = 30, Bottom = 30 }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(60),
                            Children = new Drawable[]
                            {
                                new TeamComingUpNextDisplay(currentMatch.Value.Team1.Value, true) { Margin = new MarginPadding { Left = -20 } },
                                new TournamentSpriteText
                                {
                                    Text = "vs.",
                                    Font = OsuFont.Inter.With(size: 24, weight: FontWeight.Bold),
                                    Colour = Colour4.FromHex("#F24998"),
                                    Margin = new MarginPadding { Left = 10 }
                                },
                                new TeamComingUpNextDisplay(currentMatch.Value.Team2.Value, false) { Margin = new MarginPadding { Left = -20 } },
                            }
                        },
                        new TournamentSpriteText
                        {
                            Text = "Empezando",
                            Font = OsuFont.Inter.With(size: 24, weight: FontWeight.Bold),
                            Colour = Colour4.Gray,
                            Margin = new MarginPadding { Top = 100, Left = 30 }
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ScheduleMatchDate(currentMatch.Value.Date.Value)
                                {
                                    Font = OsuFont.Inter.With(size: 36, weight: FontWeight.Bold),
                                    Colour = Colour4.FromHex("#F24998"),
                                    Shadow = false,
                                }
                            }
                        },
                    }
                };
            }
        }

        public partial class TeamComingUpNextDisplay : Container
        {
            public TeamComingUpNextDisplay(TournamentTeam? team, bool dir)
            {
                string teamName = team != null ? team.FullName.Value : "???";
                string teamSeed = team != null ? team.Seed.Value : "???";

                var anchor = dir ? Anchor.TopLeft : Anchor.TopRight;
                var color = dir ? Colour4.Aqua : Colour4.Pink;

                InternalChild = new FillFlowContainer
                {
                    Anchor = anchor,
                    Origin = anchor,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new DrawableTeamFlag(team)
                        {
                            Scale = new Vector2(0.8f)
                        },
                        new TournamentSpriteText
                        {
                            Text = teamName + " [#" + teamSeed + "]",
                            Colour = color,
                            Font = OsuFont.Inter.With(size: 18, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Left = -20 }
                        },
                    }
                };
            }
        }

        public partial class ScheduleMatch : DrawableTournamentMatch
        {
            public ScheduleMatch(TournamentMatch match, bool showTimestamp = true)
                : base(match)
            {
                Flow.Direction = FillDirection.Horizontal;

                Scale = new Vector2(0.8f);

                bool conditional = match is ConditionalTournamentMatch;

                if (conditional)
                    Colour = OsuColour.Gray(0.5f);

                /*if (showTimestamp)
                {
                    AddInternal(new DrawableDate(Match.Date.Value)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Font = OsuFont.Inter,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                    });
                    AddInternal(new TournamentSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Alpha = conditional ? 0.6f : 1,
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                        Text = match.Date.Value.ToUniversalTime().ToString("HH:mm UTC") + (conditional ? " (conditional)" : "")
                    });
                }*/
            }
        }

        public partial class ScheduleMatchDate : DrawableDate
        {
            public ScheduleMatchDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
                : base(date, textSize, italic)
            {
            }

            /*protected override string Format() => Date < DateTimeOffset.Now
                ? $"Started {base.Format()}"
                : $"Starting {base.Format()}";*/
        }

        public partial class ScheduleContainer : Container
        {
            protected override Container<Drawable> Content => content;

            private readonly DiagonalFlowContainer content;

            public ScheduleContainer(string title)
            {
                //Padding = new MarginPadding { Left = 60, Top = 10 };
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            content = new DiagonalFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Step = new Vector2(40)
                            }
                        }
                    },
                };
            }
        }

        public partial class DiagonalFlowContainer : Container<Drawable>
        {
            public Vector2 Step = new Vector2(20);

            protected override void Update()
            {
                base.Update();

                for (int i = 0; i < Children.Count; i++)
                    Children[i].Position = Step * i;
            }
        }
    }
}
