// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneSS26MapStatPill : TournamentTestScene
    {
        public TestSceneSS26MapStatPill()
        {
            Children = new Drawable[]
            {
                new SS26MapStatPill(Colour4.AliceBlue, Colour4.Black, "565", OsuIcon.Metronome)
                {
                    Margin = new MarginPadding(20),
                },
                new SS26MapStatPill(Colour4.AliceBlue, Colour4.Black, "565", OsuIcon.Clock)
                {
                    Margin = new MarginPadding(80),
                },
            };
        }
    }
}
