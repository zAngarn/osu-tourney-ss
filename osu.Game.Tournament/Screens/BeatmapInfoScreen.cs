// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;
using osuTK;

namespace osu.Game.Tournament.Screens
{
    public abstract partial class BeatmapInfoScreen : TournamentMatchScreen
    {
        public SongBarV2 SongBar;

        protected BeatmapInfoScreen()
        {
            AddInternal(SongBar = new SongBarV2
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Depth = float.MinValue,
                Scale = new Vector2(0.72f),
                Margin = new MarginPadding { Left = 369, Bottom = -5 }
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

            int debug = 1 + 1;
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap?> beatmap)
        {
            SongBar.FadeInFromZero(300, Easing.OutQuint);
            SongBar.Beatmap = beatmap.NewValue;
            if (!TournamentSceneManager.IsShowcaseScreen) SongBar.BeatmapChoice = PicksBansScreen.GetLastPlayedMap(TournamentSceneManager.PicksBansScreenInstance);

            if (CurrentMatch.Value?.Round.Value == null) return;

            RoundBeatmap slot = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.ID == (beatmap.NewValue?.OnlineID ?? default(int))) ?? new RoundBeatmap
            {
                Slot = "???"
            };

            SongBar.BeatmapChoice = new BeatmapChoice
            {
                Slot = slot.Slot,
                TeamName = "Showcase",
                Team = TeamColour.None,
            };
        }
    }
}
