// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentSpriteTextWithBackground : CompositeDrawable
    {
        public readonly TournamentSpriteText Text;

        protected readonly Box Background;

        public TournamentSpriteTextWithBackground(string text = "")
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = Colour4.Transparent,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Colour = Colour4.White,
                    Font = OsuFont.Torus.With(weight: FontWeight.SemiBold, size: 50),
                    //Padding = new MarginPadding { Left = 10, Right = 20 },
                    Text = text,
                }
            };
        }

        public TournamentSpriteTextWithBackground(TeamColour colour, string text = "")
        {
            AutoSizeAxes = Axes.Both;

            var textColour = colour == TeamColour.Red ? Colour4.FromHex("ed6dac") : Colour4.FromHex("6ddded");

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = Colour4.Transparent,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Colour = textColour,
                    Font = OsuFont.Poppins.With(weight: FontWeight.Bold, size: 70),
                    Text = text,
                }
            };
        }
    }
}
