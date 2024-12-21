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

        protected readonly Box? Background;

        public TournamentSpriteTextWithBackground(string text = "")
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = TournamentGame.ELEMENT_BACKGROUND_COLOUR,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Colour = TournamentGame.ELEMENT_FOREGROUND_COLOUR,
                    Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 50),
                    Padding = new MarginPadding { Left = 10, Right = 20 },
                    Text = text,
                }
            };
        }

        public TournamentSpriteTextWithBackground(TeamColour colour, string text = "")
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Background = new Box
                {
                    Colour = TournamentGame.ELEMENT_BACKGROUND_COLOUR,
                    RelativeSizeAxes = Axes.Both,
                },
                Text = new TournamentSpriteText
                {
                    Colour = TournamentGame.GetTeamColour(colour),
                    Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 50),
                    Padding = new MarginPadding { Left = 10, Right = 20 },
                    Text = text,
                }
            };
        }

        public TournamentSpriteTextWithBackground(Colour4 bg, Colour4 colorTexto, bool bgtexto, string text = "")
        {
            AutoSizeAxes = Axes.Both;

            if (bgtexto)
            {
                InternalChildren = new Drawable[]
                {
                    Background = new Box
                    {
                        Colour = bg,
                        RelativeSizeAxes = Axes.Both
                    },
                    Text = new TournamentSpriteText
                    {
                        Colour = colorTexto,
                        Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 50),
                        Padding = new MarginPadding { Left = 10, Right = 10 },
                        Text = text,
                    }
                };
            }
            else
            {
                InternalChildren = new Drawable[]
                {
                    Text = new TournamentSpriteText
                    {
                        Colour = colorTexto,
                        Font = OsuFont.Futura.With(weight: FontWeight.Bold, size: 50),
                        Padding = new MarginPadding { Left = 10, Right = 10 },
                        Text = text,
                    }
                };
            }
        }
    }
}
