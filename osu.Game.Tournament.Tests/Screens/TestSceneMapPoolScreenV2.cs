// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.MapPool;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneMapPoolScreenV2 : TournamentScreenTestScene
    {
        private PicksBansScreen screen = null!;
        private TournamentMatchChatDisplay chat { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            // Dependency error (arreglao): The type PicksBanScreen has a dependency on TournamentMatchChatDisplay, but the dependency is not registered.
            /*
            Vale resulta que en los tests hay que poner las dependencias manualmente.
            - https://discord.com/channels/188630481301012481/1097318920991559880/1411431057773035721
            - https://github.com/ppy/osu-framework/wiki/Dependency-Injection
            */
            Dependencies.CacheAs(new MatchIPCInfo()); // para un futuro??
            Dependencies.CacheAs(new TournamentMatchChatDisplay());
            Add(screen = new PicksBansScreen { Width = 1.3f });
        }

        [Test]
        public void TestFewMaps()
        {
            AddStep("load few maps", () =>
            {
                Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Clear();

                for (int i = 0; i < 6; i++)
                    addBeatmap("NM", "NM" + i);

                for (int i = 0; i < 4; i++)
                    addBeatmap("HD", "HD" + i);

                for (int i = 0; i < 4; i++)
                    addBeatmap("HR", "HR" + i);

                for (int i = 0; i < 4; i++)
                    addBeatmap("DT", "DT" + i);
            });
        }

        private void addBeatmap(string mods, string slot)
        {
            Ladder.CurrentMatch.Value!.Round.Value!.Beatmaps.Add(new RoundBeatmap
            {
                Beatmap = CreateSampleBeatmap(),
                Mods = mods,
                Slot = slot,
            });
        }

        [Test]
        public void TestChat()
        {
            AddStep("ensure chat is loaded", () => Assert.IsTrue(screen.IsLoaded, "PicksBansScreen should be loaded"));

            AddStep("get chat component", () =>
            {
                var chat = screen.ChildrenOfType<TournamentMatchChatDisplay>().FirstOrDefault();
                Assert.IsNotNull(chat, "TournamentMatchChatDisplay should be present");
            });

            AddStep("hide chat", () =>
            {
                var chat = screen.ChildrenOfType<TournamentMatchChatDisplay>().First();
                chat.Contract();
            });

            AddAssert("chat is hidden", () =>
            {
                var chat = screen.ChildrenOfType<TournamentMatchChatDisplay>().First();
                return chat.Alpha == 0 || !chat.IsPresent;
            });

            AddStep("show chat", () =>
            {
                var chat = screen.ChildrenOfType<TournamentMatchChatDisplay>().First();
                chat.Expand();
            });

            AddAssert("chat is visible", () =>
            {
                var chat = screen.ChildrenOfType<TournamentMatchChatDisplay>().First();
                return chat.Alpha > 0 && chat.IsPresent;
            });
        }
    }
}
