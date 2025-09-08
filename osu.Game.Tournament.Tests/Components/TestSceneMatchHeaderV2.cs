// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneMatchHeaderV2 : TournamentTestScene
    {
        public TestSceneMatchHeaderV2()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(50),
                Children = new Drawable[]
                {
                    new TournamentSpriteText { Text = "with logo", Font = OsuFont.Torus.With(size: 30) },
                    new MatchHeaderV2(),
                    new TournamentSpriteText { Text = "without logo", Font = OsuFont.Torus.With(size: 30) },
                    new MatchHeaderV2 { ShowLogo = false },
                    new TournamentSpriteText { Text = "without scores", Font = OsuFont.Torus.With(size: 30) },
                    new MatchHeaderV2 { ShowScores = false },
                }
            };
        }
    }
}
