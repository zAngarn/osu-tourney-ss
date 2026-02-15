// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class SS26SlotPill : CompositeDrawable
    {
        public SS26SlotPill(string slot)
        {
            var bgColor1 = TournamentGameBase.GetColor(slot[..2]);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Width = 43,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Height = 24,
                    CornerRadius = 12,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = bgColor1
                        },
                        new TournamentSpriteText
                        {
                            Text = slot,
                            Colour = Colour4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 16),
                            Margin = new MarginPadding { Bottom = 6f },
                        }
                    }
                }
            };
        }

        // un poco multiusos porque se usa ese elemento a lo largo del cliente (creo)
        // TournamentSpriteTextWithBackground si fuese bueno
        public SS26SlotPill(string slot, Colour4 txtColor, Colour4 bgColor)
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 12,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = bgColor
                        },
                        new TournamentSpriteText
                        {
                            Text = slot,
                            Colour = txtColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding { Left = 8, Right = 8, Top = 0, Bottom = 0 },
                            Margin = new MarginPadding { Bottom = 6f },
                            Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 16),
                        }
                    }
                }
            };
        }
    }
}
