// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    public partial class TransitionScreen : TournamentMatchScreen
    {
        public TourneyVideo TransitionVideo = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                TransitionVideo = new TourneyVideo("transition")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            // base call intentionally omitted to not show match warning.
        }
    }
}
