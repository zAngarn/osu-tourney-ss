// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TeamScore : CompositeDrawable
    {
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
        private readonly StarCounter counter;

        public TeamScore(Bindable<int?> score, TeamColour colour, int count)
        {
            bool flip = colour == TeamColour.Blue;
            var anchor = flip ? Anchor.TopRight : Anchor.TopLeft;

            var counterColor1 = Colour4.FromHex(colour == TeamColour.Red ? "#FF714D" : "#4DDBFF");

            AutoSizeAxes = Axes.Both;

            InternalChild = counter = new StarCounter(counterColor1, count)
            {
                Anchor = anchor,
                Scale = flip ? new Vector2(-(1366 / 1920f), (1366 / 1920f)) : TournamentGame.FACTOR_DE_REESCALADO_1080,
            };

            currentTeamScore.BindValueChanged(scoreChanged);
            currentTeamScore.BindTo(score);
        }

        private void scoreChanged(ValueChangedEvent<int?> score) => counter.Current = score.NewValue ?? 0;
    }
}
