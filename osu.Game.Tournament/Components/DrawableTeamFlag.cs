// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        private Bindable<string>? flag;

        private Sprite? flagSprite;

        private bool teamIntro = false;

        public DrawableTeamFlag(TournamentTeam? team)
        {
            this.team = team;
        }
        public DrawableTeamFlag(TournamentTeam? team, bool isTeamIntro)
        {
            this.team = team;
            teamIntro = isTeamIntro;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            float size = 75;
            if (team == null) return;

            if (teamIntro) size = 175;

            Size = new Vector2(size, size);
            Masking = true;
            CornerRadius = 0;
            Child = flagSprite = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill
            };

            (flag = team.FlagName.GetBoundCopy()).BindValueChanged(_ => flagSprite.Texture = textures.Get($@"Flags/{team.FlagName}"), true);
        }
    }
}
