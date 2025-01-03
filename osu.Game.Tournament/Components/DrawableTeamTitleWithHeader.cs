// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamTitleWithHeader : CompositeDrawable
    {
        private readonly TournamentSpriteTextWithBackground teamNameText;

        public DrawableTeamTitleWithHeader(TournamentTeam? team, TeamColour colour)
        {
            AutoSizeAxes = Axes.Both;
            string texto = team != null ? team.FullName.ToString() : "???";
            Colour4 colortexto = colour == TeamColour.Red ? Colour4.FromHex("#ec675d") : Colour4.White;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    teamNameText = new TournamentSpriteTextWithBackground(Colour4.White, colortexto, false, texto) // Nombre
                }
            };
        }

        public DrawableTeamTitleWithHeader(TournamentTeam? team)
        {
            AutoSizeAxes = Axes.Both;
            string texto = team != null ? team.FullName.ToString() : "???";
            Colour4 colortexto = Colour4.Black;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    teamNameText = new TournamentSpriteTextWithBackground(Colour4.White, colortexto, false, texto) // Nombre
                }
            };
        }
    }
}
