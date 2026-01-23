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
            var bgColor1 = getColor(slot[..2]);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Width = 43,
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
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                        }
                    }
                }
            };
        }

        public SS26SlotPill(string slot, Colour4 txtColor)
        {
            var bgColor1 = getColor(slot[..2]);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Width = 43,
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
                            Colour = txtColor,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 16),
                        }
                    }
                }
            };
        }

        private Colour4 getColor(string mod)
        {
            Colour4 color;

            switch (mod)
            {
                case "NM":
                    color = Colour4.FromHex("659EEB");
                    break;

                case "HR":
                    color = Colour4.FromHex("E06050");
                    break;

                case "HD":
                    color = Colour4.FromHex("FFB844");
                    break;

                case "DT":
                    color = Colour4.FromHex("8E7CBA");
                    break;

                case "TB":
                    color = Colour4.FromHex("AEAEAE");
                    break;

                default:
                    color = Colour4.FromHex("659EEB"); // Mismo que NM. ¿Por qué? Porque me sale del nabo
                    break;
            }

            return color;
        }
    }
}
