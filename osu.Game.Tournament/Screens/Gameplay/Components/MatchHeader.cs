// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class MatchHeader : Container
    {
        private TeamScoreDisplayV2 teamDisplay1 = null!;
        private TeamScoreDisplayV2 teamDisplay2 = null!;

        private bool showScores = true;

        public bool ShowScores
        {
            get => showScores;
            set
            {
                if (value == showScores)
                    return;

                showScores = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            Height = 95;
            Children = new Drawable[]
            {
                teamDisplay1 = new TeamScoreDisplayV2(TeamColour.Red)
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                teamDisplay2 = new TeamScoreDisplayV2(TeamColour.Blue)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            teamDisplay1.ShowScore = showScores;
            teamDisplay2.ShowScore = showScores;
        }
    }
}
