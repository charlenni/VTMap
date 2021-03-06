﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VTMap.Core.Utilities
{
    public class Animation
    {
        private static object sync = new object();
        private static readonly List<Animation> animations = new List<Animation>();

        /// <summary>
        /// Updates all animations that are running
        /// </summary>
        public static bool UpdateAnimations()
        {
            Animation[] localAnimations;

            if (animations.Count == 0)
                return false;

            lock (sync)
            {
                localAnimations = new Animation[animations.Count];
                animations.CopyTo(localAnimations);
            }

            // Sanity check
            if (localAnimations.Length == 0)
                return false;

            foreach (var animation in localAnimations)
                animation.Tick();

            return true;
        }

        private Stopwatch _stopwatch;
        private long _stopwatchStart;
        private long _durationTicks;

        public Animation(long duration)
        {
            Duration = duration;
        }

        public EventHandler<AnimationEventArgs> Started { get; set; }
        public EventHandler<AnimationEventArgs> Stopped { get; set; }
        public EventHandler<AnimationEventArgs> Ticked { get; set; }

        /// <summary>
        /// Duration of the whole animation cycle in milliseconds
        /// </summary>
        public long Duration { get; set; } = 300;

        /// <summary>
        /// Animations, that should be made
        /// </summary>
        public List<AnimationEntry> Entries { get; } = new List<AnimationEntry>();

        /// <summary>
        /// True, if an animation is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// If true, then the animation starts again and again
        /// </summary>
        public bool Loop { get; set; } = false;

        public void Start()
        {
            if (IsRunning)
            {
                Stop(false);
            }

            // Animation in ticks;
            _durationTicks = Duration * Stopwatch.Frequency / 1000;

            _stopwatch = Stopwatch.StartNew();
            _stopwatchStart = _stopwatch.ElapsedTicks;

            lock (sync)
            {
                animations.Add(this);
            }

            IsRunning = true;

            Started?.Invoke(this, new AnimationEventArgs(0));
        }

        /// <summary>
        /// Stop a running animation if there is one
        /// </summary>
        /// <param name="gotoEnd">Should final of each list entry be called</param>
        public void Stop(bool gotoEnd = true)
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            lock (sync)
            {
                animations.Remove(this);
            }

            _stopwatch.Stop();

            double ticks = _stopwatch.ElapsedTicks - _stopwatchStart;
            var value = ticks / _durationTicks;

            if (gotoEnd)
            {
                foreach (var entry in Entries)
                {
                    entry.Final();
                }
            }

            Stopped?.Invoke(this, new AnimationEventArgs(value));
        }

        internal void Tick()
        {
            double ticks = _stopwatch.ElapsedTicks - _stopwatchStart;
            var value = ticks / _durationTicks;

            if (value >= 1.0)
            {
                if (Loop)
                {
                    _stopwatchStart = _stopwatch.ElapsedTicks;
                    value = 0;
                }
                else
                {
                    Stop(true);
                    return;
                }
            }

            // Calc new values
            foreach (var entry in Entries)
            {
                if (value >= entry.AnimationStart && value <= entry.AnimationEnd)
                    entry.Tick(value);
            }
        }
    }
}
