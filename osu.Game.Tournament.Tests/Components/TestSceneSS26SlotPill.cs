// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneSS26SlotPill : TournamentTestScene
    {
        public TestSceneSS26SlotPill()
        {
            Children = new Drawable[]
            {
                new SS26SlotPill("NM1")
                {
                    Margin = new MarginPadding(20)
                },
                new SS26SlotPill("HR2")
                {
                    Margin = new MarginPadding(80)
                },
                new SS26SlotPill("DT2", Colour4.FromHex("262626"), Colour4.Aqua)
                {
                    Margin = new MarginPadding(140)
                },
            };
        }
    }
}
