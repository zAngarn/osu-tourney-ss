// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    public abstract partial class BeatmapInfoScreen : TournamentMatchScreen
    {
        protected readonly SS26Songbar SongBar;

        protected BeatmapInfoScreen()
        {
            AddInternal(SongBar = new SS26Songbar
            {
                Y = -128,
                X = -393,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Scale = TournamentGame.FACTOR_DE_REESCALADO_1080,
            });
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            ipc.Beatmap.BindValueChanged(beatmapChanged, true);
            ipc.Mods.BindValueChanged(modsChanged, true);
        }

        private void modsChanged(ValueChangedEvent<LegacyMods> mods)
        {
            SongBar.Mods = mods.NewValue;
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap?> beatmap)
        {
            SongBar.FadeInFromZero(300, Easing.OutQuint);
            SongBar.Beatmap = beatmap.NewValue;
        }
    }
}
