// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class DrawableTeamRank : TournamentSpriteTextWithBackground
    {
        private readonly TournamentTeam? team;

        private IBindable<int> rank = null!;

        public DrawableTeamRank(TournamentTeam? team)
        {
            this.team = team;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Text.Font = Text.Font.With(size: 36);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (team == null) return;

            rank = new Bindable<int>((int)team.AverageRank);
            rank.BindValueChanged(s => Text.Text = s.NewValue.ToString(), true);
        }
    }
}
