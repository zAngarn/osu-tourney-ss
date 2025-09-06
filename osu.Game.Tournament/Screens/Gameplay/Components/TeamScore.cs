// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;
using osuTK;

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

            counterColour = colour;

            var starColour = colour == TeamColour.Red ? Colour4.FromHex("ed6dac") : Colour4.FromHex("6ddded");

            AutoSizeAxes = Axes.Both;

            InternalChild = counter = new StarCounter(starColour, count) //SCORE COUNTER <<-----
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
    }
}
