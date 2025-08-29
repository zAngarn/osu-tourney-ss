// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class RoundDisplayV2 : CompositeDrawable
    {
        private TournamentRound round;

        private readonly TournamentSpriteText roundText;

        public TournamentRound Round
        {
            get => round;
            set
            {
                if (round == value) return;

                round = value;
                updateBindings();
            }
        }

        public RoundDisplayV2(TournamentRound round)
        {
            this.round = round;

            InternalChildren = new Drawable[]
            {
                roundText = new TournamentSpriteText
                {
                    Text = round.Name.Value ?? "Ronda desconocida",
                    Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 30),
                    Colour = Colour4.White,
                    Shadow = false,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateBindings();
        }

        private void updateBindings()
        {
            round.Name.BindValueChanged(e =>
            {
                if (e.NewValue == "???")
                    roundText.Text = "Ronda desconocida";
                else
                    roundText.Text = e.NewValue ?? "HOLA";
            }, true);
        }
    }
}
