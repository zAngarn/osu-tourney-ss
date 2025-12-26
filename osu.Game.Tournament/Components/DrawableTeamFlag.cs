// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamFlag : Container
    {
        private readonly TournamentTeam? team;

        [UsedImplicitly]
        public Bindable<string>? Flag;

        private Sprite? flagSprite;
        private readonly Vector2 size;
        private readonly float cornerRadius;

        public DrawableTeamFlag(TournamentTeam? team, Vector2 size, float cornerRadius)
        {
            this.team = team;
            this.size = size;
            this.cornerRadius = cornerRadius;
        }

        public DrawableTeamFlag(TournamentTeam? team) // por compatibilidad y tal
        {
            this.team = team;
            size = new Vector2(75, 54);
            cornerRadius = 5;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (team == null) return;

            Size = size;
            Masking = true;
            CornerRadius = cornerRadius;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("333"),
                },
                flagSprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit
                },
            };

            (Flag = team.FlagName.GetBoundCopy()).BindValueChanged(_ => flagSprite.Texture = textures.Get($@"Flags/{team.FlagName}"), true);
        }
    }
}
