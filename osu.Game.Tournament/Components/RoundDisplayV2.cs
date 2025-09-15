// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class RoundDisplayV2 : TournamentSpriteText
    {
        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 30);
            Colour = Colour4.White;
            Shadow = false;
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match) =>
            Text = match.NewValue?.Round.Value?.Name.Value ?? "Unknown Round";
    }
}
