﻿using SkiaSharp;
using VTMap.Core.Interfaces;
using VTMap.Core.Primitives;

namespace VTMap.Common.Primitives
{
    public class Drawable : SKDrawable, IDrawable
    {
        /// <summary>
        /// Context to use to draw this tile 
        /// </summary>
        public EvaluationContext Context { get; } = new EvaluationContext(0);

        /// <summary>
        /// Zoom factor for this tile
        /// </summary>
        public int Zoom { get; internal set; } = 0;

        /// <summary>
        /// Correction factor for zoom because of overzoom
        /// </summary>
        public int Overzoom { get; internal set; } = 0;
    }
}
