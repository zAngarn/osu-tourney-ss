// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamHeader : TournamentSpriteTextWithBackground
    {
        public DrawableTeamHeader(TeamColour colour)
        {
            // cutre pero funciona, cambiar el nombre en el enum rompe 28 referencias, cosa con la que no me apetece lidiar
            string localisedTeamDisplay;

            if (colour.ToString().Equals("Red", System.StringComparison.OrdinalIgnoreCase))
            {
                localisedTeamDisplay = "Equipo Rojo";
            }
            else
            {
                localisedTeamDisplay = "Equipo Azul";
            }

            Text.Colour = TournamentGame.GetTeamColour(colour);
            Text.Text = localisedTeamDisplay;
            Text.Scale = new Vector2(1f);
        }
    }
}
