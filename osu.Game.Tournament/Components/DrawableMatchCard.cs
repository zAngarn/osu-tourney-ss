// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableMatchCard : CompositeDrawable
    {
        private readonly TournamentMatch match;

        private const int ancho_tarjeta = 218;
        private const int altura_tarjeta = 64;

        private float alphaScores = 0f;
        private float alphaVersus = 1f;

        private bool started;

        public bool Started
        {
            get => started;
            set
            {
                if (started == value) return;

                started = value;
                switchScoreCardView();
            }
        }

        private Colour4 team1ScoreText = Colour4.White;
        private Colour4 team2ScoreText = Colour4.White;

        private readonly FillFlowContainer flow;

        [Resolved]
        private LadderInfo? ladderInfo { get; set; }

        public DrawableMatchCard(TournamentMatch match)
        {
            if(match == null)

            {
                this.match = new TournamentMatch
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
            }
            else
            {
                this.match = match;
            }

            Margin = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                flow = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                }
            };

            boundReference(match.Team1).BindValueChanged(_ => updateContents());
            boundReference(match.Team2).BindValueChanged(_ => updateContents());
            boundReference(match.Team1Score).BindValueChanged(_ => updateContents());
            boundReference(match.Team2Score).BindValueChanged(_ => updateContents());
            boundReference(match.Started).BindValueChanged(_ => Started = true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateContents();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button != MouseButton.Left) return false;

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
                    ladderInfo.CurrentMatch.Value = match;
                }
            }
        }

        private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

        private T boundReference<T>(T obj)
            where T : IBindable
        {
            obj = (T)obj.GetBoundCopy();
            refBindables.Add(obj);
            return obj;
        }

        private void switchScoreCardView()
        {
            if (started)
            {
                alphaScores = 1f;
                alphaVersus = 0f;
            }
            else
            {
                alphaScores = 0f;
                alphaVersus = 1f;
            }

            updateContents();
        }

        private void updateContents()
        {
            if (match.Round.Value != null && match.Round.Value.BestOf.Value != 0)
            {
                int toWin = (match.Round.Value.BestOf.Value + 1) / 2;

                if (match.Team1Score.Value == toWin)
                {
                    team1ScoreText = Colour4.FromHex("ed6dac");
                }
                else if (match.Team2Score.Value == toWin)
                {
                    team2ScoreText = Colour4.FromHex("6ddded");
                }
            }

            flow.Children = new Drawable[]
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
                                    Width = ancho_tarjeta,
                                    Height = altura_tarjeta,
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
                                            Colour = Color4Extensions.FromHex("#ed6dac"),
                                            Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 34),
                                            Margin = new MarginPadding { Top = 5 },
                                        },
                                    }
                                },
                                new Container
                                {
                                    Width = ancho_tarjeta,
                                    Height = altura_tarjeta,
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
                                            Colour = Color4Extensions.FromHex("#6ddded"),
                                            Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 34),
                                            Margin = new MarginPadding { Top = 5 },
                                        },
                                    }
                                },
                            }
                        }
                    }
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Top = 24, Left = 198 * 2 },
                    Alpha = alphaScores,
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Origin = Anchor.Centre,
                            Text = match.Team1Score.Value.ToString() ?? "?",
                            Font = OsuFont.Poppins.With(size: 34, weight: FontWeight.Bold, fixedWidth: true),
                            Margin = new MarginPadding { Top = 20 },
                            Colour = team1ScoreText
                        },
                        new TournamentSpriteText
                        {
                            Origin = Anchor.Centre,
                            Text = " - ",
                            Font = OsuFont.Poppins.With(size: 34, weight: FontWeight.Bold, fixedWidth: true),
                            Margin = new MarginPadding { Top = 20 }
                        },
                        new TournamentSpriteText
                        {
                            Origin = Anchor.Centre,
                            Text = match.Team2Score.Value.ToString() ?? "?",
                            Font = OsuFont.Poppins.With(size: 34, weight: FontWeight.Bold, fixedWidth: true),
                            Margin = new MarginPadding { Top = 20 },
                            Colour = team2ScoreText,
                        }
                    }
                },
                new TournamentSpriteText
                {
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Top = 18, Left = 217 * 2 },
                    Alpha = alphaVersus,
                    Font = OsuFont.Poppins.With(size: 34, weight: FontWeight.Bold),
                    Text = "vs"
                }
            };
        }
    }
}
