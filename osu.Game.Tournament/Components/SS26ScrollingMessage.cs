// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Tournament.Components
{
    public partial class SS26ScrollingMessage : CompositeDrawable
    {
        private readonly Func<Drawable> createMessageContent;

        private readonly float spacing;
        private readonly float speed = 0.05f;

        private readonly Container messageContainer;

        private readonly List<Drawable> activeMessages = new List<Drawable>();

        public SS26ScrollingMessage(Func<Drawable> createMessageContent, float spacing = 50f)
        {
            this.createMessageContent = createMessageContent;
            this.spacing = spacing;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;

            InternalChild = messageContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeInFromZero(2000, Easing.OutQuint);

            addNewMessage(0);
        }

        protected override void Update()
        {
            base.Update();

            if (activeMessages.Count == 0) return;

            float delta = (float)Clock.ElapsedFrameTime * speed;

            foreach (var msg in activeMessages)
            {
                msg.X -= delta;
            }

            var first = activeMessages[0];

            if (first.IsLoaded && first.DrawWidth > 0 && first.X + first.DrawWidth < 0)
            {
                var last = activeMessages[^1];

                first.X = last.X + last.DrawWidth + spacing;

                activeMessages.RemoveAt(0);
                activeMessages.Add(first);
            }

            var currentLast = activeMessages[^1];

            if (currentLast.IsLoaded && currentLast.DrawWidth > 0)
            {
                float rightEdge = currentLast.X + currentLast.DrawWidth;

                if (rightEdge < DrawWidth)
                {
                    addNewMessage(rightEdge + spacing);
                }
            }
        }

        private void addNewMessage(float xPosition)
        {
            var newMessage = createMessageContent();
            newMessage.X = xPosition;

            activeMessages.Add(newMessage);
            messageContainer.Add(newMessage);
        }
    }
}
