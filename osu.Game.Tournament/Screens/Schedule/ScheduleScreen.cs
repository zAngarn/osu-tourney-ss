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
                    Padding = new MarginPadding(100) { Bottom = 50 },
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
                    .Take(8);

            var recent =
                allMatches
                    .Where(m => m.Completed.Value && m.Team1.Value != null && m.Team2.Value != null && Math.Abs(m.Date.Value.DayOfYear - DateTimeOffset.UtcNow.DayOfYear) < days_for_displays)
                    .OrderByDescending(m => m.Date.Value)
                    .Take(8);

            ScheduleContainer comingUpNext;

            mainContainer.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.74f,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new ScheduleContainer("")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.33f,
                                    ChildrenEnumerable = recent.Select(p => new ScheduleMatch(p)),
                                    Margin = new MarginPadding { Top = 225, Left = -55 }
                                },
                                new ScheduleContainer("")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.33f,
                                    ChildrenEnumerable = upcoming.Select(p => new ScheduleMatch(p)),
                                    Margin = new MarginPadding { Top = 225, Left = 12 }
                                },
                                comingUpNext = new ScheduleContainer("")
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.34f,
                                }
                            }
                        }
                    },
                }
            };

            if (currentMatch.Value != null)
            {
                comingUpNext.Child = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new DrawablePlayerCard(currentMatch.Value.Team1.Value ?? new TournamentTeam { FullName = { Value = "???" } }, Colour4.FromHex("ed6dac"))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Scale = new Vector2(0.72f),
                            Margin = new MarginPadding { Bottom = 220, Left = 60 },
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = "vs",
                            Font = OsuFont.Poppins.With(size: 56, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Left = 200 },
                        },
                        new DrawablePlayerCard(currentMatch.Value.Team2.Value ?? new TournamentTeam { FullName = { Value = "???" } }, Colour4.FromHex("6ddded"))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Scale = new Vector2(0.72f),
                            Margin = new MarginPadding { Bottom = 220, Left = 345 },
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new ScheduleMatchDate(currentMatch.Value.Date.Value)
                                {
                                    Font = OsuFont.Poppins.With(size: 34, weight: FontWeight.Bold),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Margin = new MarginPadding { Top = 233, Left = 100 }
                                }
                            }
                        },
                    }
                };
            }
        }

        public partial class ScheduleMatch : CompositeDrawable
        {
            public ScheduleMatch(TournamentMatch match, bool showTimestamp = true)
            {
                AddInternal(new DrawableMatchCard(match));
                Scale = new Vector2(0.75f);

                if (showTimestamp)
                {
                    AddInternal(new TournamentSpriteText
                    {
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomLeft,
                        Colour = OsuColour.Gray(0.7f),
                        Margin = new MarginPadding { Left = 200, Bottom = -70 },
                        Text = match.Date.Value.ToUniversalTime().ToString("HH:mm UTC")
                    });
                }
            }
        }

        public partial class ScheduleMatchDate : DrawableDate
        {
            public ScheduleMatchDate(DateTimeOffset date, float textSize = OsuFont.DEFAULT_FONT_SIZE, bool italic = true)
                : base(date, textSize, italic)
            {
            }

            protected override string Format() => Date < DateTimeOffset.Now
                ? $"Comenzó {base.Format()}"
                : $"Comenzando {base.Format()}";
        }

        public partial class ScheduleContainer : Container
        {
            protected override Container<Drawable> Content => content;

            private readonly FillFlowContainer content;

            public ScheduleContainer(string title)
            {
                Padding = new MarginPadding { Left = 60, Top = 10 };
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TournamentSpriteTextWithBackground(title.ToUpperInvariant())
                            {
                                Scale = new Vector2(0.5f)
                            },
                            content = new FillFlowContainer
                            {
                                Direction = FillDirection.Vertical,
                                RelativeSizeAxes = Axes.Both,
                                Spacing = new Vector2(0, 60),
                                Margin = new MarginPadding(10)
                            },
                        }
                    },
                };
            }
        }
    }
}
