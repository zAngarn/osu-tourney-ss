// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TeamScore : CompositeDrawable
    {
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
        private readonly StarCounter counter;
        private readonly TeamColour counterColour;

        public TeamScore(Bindable<int?> score, TeamColour colour, int count)
        {
            bool flip = colour == TeamColour.Blue;
            var anchor = flip ? Anchor.TopRight : Anchor.TopLeft;

            counterColour = colour; //TeamColour.Red

            AutoSizeAxes = Axes.Both;

            InternalChild = counter = new TeamScoreStarCounter(count, colour) //SCORE COUNTER <<-----
            {
                Anchor = anchor,
                Scale = flip ? new Vector2(-1, 1) : Vector2.One,
            };

            currentTeamScore.BindValueChanged(scoreChanged);
            currentTeamScore.BindTo(score);
        }

        private void scoreChanged(ValueChangedEvent<int?> score)
        {
            counter.Current = score.NewValue ?? 0;
            PicksBansScreen.UpdateWinStateStatic(TournamentSceneManager.PicksBansScreenInstance, counterColour);
        }

        public partial class TeamScoreStarCounter : StarCounter
        {
            private readonly TeamColour teamColour;

            public TeamScoreStarCounter(int count, TeamColour colour)
                : base(count)
            {
                teamColour = colour;
            }

            public override Star CreateStar() => new LightSquare(teamColour);

            public partial class LightSquare : Star
            {
                private readonly Box box;

                public LightSquare(TeamColour teamColour)
                {
                    Size = new Vector2(22.5f);

                    Color4 boxColour = Color4.White;
                    if (teamColour == TeamColour.Red)
                    {
                         boxColour = Color4.DeepPink;
                    }
                    else
                    {
                        boxColour = Color4.Cyan;
                    }

                    InternalChildren = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderColour = OsuColour.Gray(0.5f),
                            BorderThickness = 3,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Transparent,
                                    RelativeSizeAxes = Axes.Both,
                                    AlwaysPresent = true,
                                },
                            }
                        },
                        box = new Box
                        {
                            Colour = boxColour,
                            RelativeSizeAxes = Axes.Both,
                        },
                    };

                    Masking = true;
                }

                public override void DisplayAt(float scale)
                {
                    box.FadeTo(scale, 500, Easing.OutQuint);
                    FadeEdgeEffectTo(0.2f * scale, 500, Easing.OutQuint);
                }
            }
        }
    }
}
