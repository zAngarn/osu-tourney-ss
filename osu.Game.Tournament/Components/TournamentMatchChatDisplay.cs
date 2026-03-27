// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public partial class TournamentMatchChatDisplay : CompositeDrawable
    {
        private readonly Bindable<string> channelName = new Bindable<string>();

        private ChannelManager? manager;
        private Channel? channel;
        private BubbleChatHistory chatHistory = null!;

        [Resolved]
        private LadderInfo ladderInfo { get; set; } = null!;

        private static readonly string[] FilteredPrefixes =
        [
            "!mp",
            ">",
            "Bans",
            "Disponibles",
            "Timeout",
            "El equipo",
            "Activando",
            "first",
        ];

        public TournamentMatchChatDisplay()
        {
            RelativeSizeAxes = Axes.X;
            Height = 300;
            Width = 0.27f;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc, IAPIProvider api)
        {
            AddInternal(manager = new ChannelManager(api));

            AddInternal(chatHistory = new BubbleChatHistory
            {
                RelativeSizeAxes = Axes.Both,
            });

            channelName.BindTo(ipc.ChatChannel);

            channelName.BindValueChanged(c =>
            {
                if (int.TryParse(c.OldValue, out int oldChannelId) && oldChannelId > 0)
                {
                    var joined = manager.JoinedChannels.SingleOrDefault(ch => ch.Id == oldChannelId);

                    if (joined != null)
                    {
                        joined.NewMessagesArrived -= onNewMessagesArrived;
                        manager.LeaveChannel(joined);
                    }
                }

                chatHistory.Clear();

                if (int.TryParse(c.NewValue, out int newChannelId) && newChannelId > 0)
                {
                    channel = new Channel
                    {
                        Id = newChannelId,
                        Type = ChannelType.Public
                    };

                    manager.JoinChannel(channel);
                    manager.CurrentChannel.Value = channel;
                    channel.NewMessagesArrived += onNewMessagesArrived;
                }
            }, true);
        }

        private void onNewMessagesArrived(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                string content = message.Content;

                if (FilteredPrefixes.Any(p => content.StartsWith(p, StringComparison.Ordinal)))
                    continue;

                if (content.StartsWith("[DISCORD", StringComparison.Ordinal))
                    content = Regex.Replace(content, @"^\[DISCORD.*?\]\s*", "");

                Color4? teamColour = null;

                if (ladderInfo.CurrentMatch.Value is TournamentMatch match)
                {
                    if (match.Team1.Value?.Players.Any(u => u.OnlineID == message.Sender.OnlineID) == true)
                        teamColour = Color4Extensions.FromHex("#FF714D");
                    else if (match.Team2.Value?.Players.Any(u => u.OnlineID == message.Sender.OnlineID) == true)
                        teamColour = Color4Extensions.FromHex("#4DDBFF");
                }

                chatHistory.PostMessage(message.Sender, content, teamColour);
            }
        }

        public void Expand() => this.FadeIn(300);

        public void Contract() => this.FadeOut(200);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (channel != null)
                channel.NewMessagesArrived -= onNewMessagesArrived;
        }

        public partial class BubbleChatHistory : CompositeDrawable
        {
            private const float message_spacing = 4f;

            private readonly ChatScrollContainer scroll;
            private readonly FillFlowContainer<MessageBubble> flow;

            public BubbleChatHistory()
            {
                InternalChild = scroll = new ChatScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = flow = new FillFlowContainer<MessageBubble>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, message_spacing),
                        Padding = new MarginPadding { Horizontal = 80, Bottom = 120 },
                    }
                };
            }

            public new void Clear()
            {
                flow.Clear();
                scroll.ScrollToStart(false);
            }

            public void PostMessage(APIUser user, string content, Color4? teamColour = null)
            {
                flow.Add(new MessageBubble(user, content, teamColour)
                {
                    RelativeSizeAxes = Axes.X,
                });

                ScheduleAfterChildren(() => scroll.ScrollToEndIfAppropriate());
            }

            private partial class MessageBubble : CompositeDrawable
            {
                private const float avatar_size = 20f;
                private const float accent_width = 3f;
                private const float padding_h = 10f;
                private const float padding_v = 6f;
                private const float content_spacing = 6f;

                public MessageBubble(APIUser user, string message, Color4? teamColour)
                {
                    AutoSizeAxes = Axes.Y;

                    var textFlow = new OsuTextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    };

                    textFlow.AddText(message, t =>
                    {
                        t.Colour = Color4.White;
                        t.Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 12);
                        t.UseFullGlyphHeight = true;
                    });

                    InternalChild = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        CornerRadius = 8,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.FromHex("303030"),
                            },
                            new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = accent_width,
                                Colour = teamColour.HasValue
                                    ? teamColour.Value
                                    : Color4.Transparent,
                            },

                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Left = accent_width + padding_h,
                                    Right = padding_h,
                                    Top = padding_v,
                                    Bottom = padding_v,
                                },
                                Children = new Drawable[]
                                {
                                    new CircularContainer
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Size = new Vector2(avatar_size),
                                        Masking = true,
                                        Child = new UpdateableAvatar(user)
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Padding = new MarginPadding { Left = avatar_size + content_spacing },
                                        Margin = new MarginPadding { Top = 3 },
                                        Spacing = new Vector2(5, 0),
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = user.Username,
                                                Font = OsuFont.BalooDa.With(weight: FontWeight.Bold, size: 12),
                                                Colour = teamColour ?? Colour4.Gray,
                                                UseFullGlyphHeight = true,
                                            },
                                            textFlow
                                        }
                                    }
                                }
                            }
                        }
                    };
                }
            }

            private partial class ChatScrollContainer : OsuScrollContainer
            {
                private bool userScrolledUp;

                public ChatScrollContainer()
                    : base(Direction.Vertical)
                {
                    ScrollbarVisible = true;
                    ScrollbarOverlapsContent = true;
                }

                protected override bool OnScroll(ScrollEvent e)
                {
                    bool result = base.OnScroll(e);

                    userScrolledUp = !IsScrolledToEnd();
                    return result;
                }

                public void ScrollToEndIfAppropriate()
                {
                    if (!userScrolledUp)
                        ScrollToEnd(true);

                    if (IsScrolledToEnd())
                        userScrolledUp = false;
                }
            }
        }
    }
}
