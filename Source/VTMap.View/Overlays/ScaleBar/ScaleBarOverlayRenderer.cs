using SkiaSharp;
using System;
using VTMap.Core;
using VTMap.Core.Enums;
using VTMap.Core.Interfaces;
using VTMap.Core.Layers;
using VTMap.Core.Utilities;
using VTMap.View;
using VTMap.View.Overlays.ScaleBar;

namespace VectorTilesView.Overlays.ScaleBar
{
    public class ScaleBarOverlayRenderer : IRenderer
    {
        ScaleBarOverlay _scaleBarOverlay;
        byte _opacity;
        SKPaint _paintScaleBar;
        SKPaint _paintScaleBarStroke;
        SKPaint _paintScaleText;
        SKPaint _paintScaleBarBox;
        SKPaint _paintScaleTextStroke;
        string _lastScaleBarText1;
        string _lastScaleBarText2;
        SKRect _textSize = SKRect.Empty;
        SKRect _textSize1 = SKRect.Empty;
        SKRect _textSize2 = SKRect.Empty;

        public ScaleBarOverlayRenderer(ScaleBarOverlay scaleBarOverlay, float layerOpacity)
        {
            _scaleBarOverlay = scaleBarOverlay;
            _opacity = (byte)(layerOpacity * 255);

            _paintScaleBar = CreateScaleBarPaint(SKPaintStyle.Fill);
            _paintScaleBarStroke = CreateScaleBarPaint(SKPaintStyle.Stroke);
            _paintScaleBarBox = CreateTextPaint(SKPaintStyle.Stroke);
            _paintScaleText = CreateTextPaint(SKPaintStyle.Fill);
            _paintScaleTextStroke = CreateTextPaint(SKPaintStyle.Stroke);

            UpdatePaints();
        }

        public void UpdatePaints()
        {
            // Update paints with new values
            _paintScaleBar.Color = _scaleBarOverlay.TextColor.WithAlpha(_opacity);
            _paintScaleBar.StrokeWidth = _scaleBarOverlay.StrokeWidth * _scaleBarOverlay.Scale;
            _paintScaleBarStroke.Color = _scaleBarOverlay.HaloColor.WithAlpha(_opacity);
            _paintScaleBarStroke.StrokeWidth = _scaleBarOverlay.StrokeWidthHalo * _scaleBarOverlay.Scale;
            _paintScaleText.Color = _scaleBarOverlay.TextColor.WithAlpha(_opacity);
            _paintScaleText.StrokeWidth = _scaleBarOverlay.StrokeWidth * _scaleBarOverlay.Scale;
            _paintScaleText.Typeface = SKTypeface.FromFamilyName(_scaleBarOverlay.Font.Typeface.FamilyName,
                SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            _paintScaleText.TextSize = (float)_scaleBarOverlay.Font.Size * _scaleBarOverlay.Scale;
            _paintScaleTextStroke.Color = _scaleBarOverlay.HaloColor.WithAlpha(_opacity);
            _paintScaleTextStroke.StrokeWidth = _scaleBarOverlay.StrokeWidthHalo / 2 * _scaleBarOverlay.Scale;
            _paintScaleTextStroke.Typeface = SKTypeface.FromFamilyName(_scaleBarOverlay.Font.Typeface.FamilyName,
                SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            _paintScaleTextStroke.TextSize = (float)_scaleBarOverlay.Font.Size * _scaleBarOverlay.Scale;

            // Do this, because height of text changes sometimes (e.g. from 2 m to 1 m)
            _paintScaleTextStroke.MeasureText("9999 m", ref _textSize);
        }

        public void Draw(object canvasObj, object viewportObj)
        {
            var canvas = (SKCanvas)canvasObj;
            var viewport = (Viewport)viewportObj;

            if (!_scaleBarOverlay.CanTransform()) 
                return;

            float scaleBarLength1;
            string scaleBarText1;
            float scaleBarLength2;
            string scaleBarText2;

            (scaleBarLength1, scaleBarText1, scaleBarLength2, scaleBarText2) = GetScaleBarLengthAndText(viewport);

            var scaleBarHeight = _textSize.Height + (_scaleBarOverlay.TickLength + _scaleBarOverlay.StrokeWidthHalo * 0.5f + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale;

            if (_scaleBarOverlay.ScaleBarMode == ScaleBarMode.Both && _scaleBarOverlay.SecondaryUnitConverter != null)
            {
                scaleBarHeight *= 2;
            }
            else
            {
                scaleBarHeight += _scaleBarOverlay.StrokeWidthHalo * 0.5f * _scaleBarOverlay.Scale;
            }

            _scaleBarOverlay.Height = scaleBarHeight;

            // Draw lines

            // Get lines for scale bar
            var points = GetScaleBarLinePositions(viewport, scaleBarLength1, scaleBarLength2, _scaleBarOverlay.StrokeWidthHalo);

            // BoundingBox for scale bar
            Box boundingBox = new Box();

            if (points != null)
            {
                // Draw outline of scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, _paintScaleBarStroke);
                }

                // Draw scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, _paintScaleBar);
                }

                for (int i = 0; i < points.Length; i++)
                {
                    boundingBox.Add(points[i].X, points[i].Y);
                }

                boundingBox.Expand(_scaleBarOverlay.StrokeWidthHalo * 0.5f * _scaleBarOverlay.Scale);
            }

            // Draw text

            // Calc text height
            scaleBarText1 = scaleBarText1 ?? string.Empty;
            if (scaleBarText1 != _lastScaleBarText1)
            {
                _paintScaleTextStroke.MeasureText(scaleBarText1, ref _textSize1);
                _lastScaleBarText1 = scaleBarText1;
            }

            float posX1, posY1, posX2, posY2;

            if (_scaleBarOverlay.ScaleBarMode == ScaleBarMode.Both && _scaleBarOverlay.SecondaryUnitConverter != null)
            {
                // Draw text of second unit converter
                scaleBarText2 = scaleBarText2 ?? string.Empty;
                if (scaleBarText2 != _lastScaleBarText2)
                {
                    _paintScaleTextStroke.MeasureText(scaleBarText2, ref _textSize2);
                    _lastScaleBarText2 = scaleBarText2;
                }

                (posX1, posY1, posX2, posY2) = GetScaleBarTextPositions(viewport, _textSize, _textSize1, _textSize2, _scaleBarOverlay.StrokeWidthHalo);

                // TODO: Save bitmaps of text in a dictionary and reuse them, because
                // TODO: DrawText is time consuming.
                // Draw text of second unit converter
                canvas.DrawText(scaleBarText2, posX2, posY2 - _textSize2.Top, _paintScaleTextStroke);
                canvas.DrawText(scaleBarText2, posX2, posY2 - _textSize2.Top, _paintScaleText);

                boundingBox.Add(new Box(posX2, posY2, posX2 + _textSize2.Width, posY2 + _textSize2.Height));
            }
            else
            {
                (posX1, posY1, _, _) = GetScaleBarTextPositions(viewport, _textSize, _textSize1, SKRect.Empty, _scaleBarOverlay.StrokeWidthHalo);
            }

            // TODO: Save bitmaps of text in a dictionary and reuse them, because
            // TODO: DrawText is time consuming.
            // Draw text of first unit converter
            canvas.DrawText(scaleBarText1, posX1, posY1 - _textSize1.Top, _paintScaleTextStroke);
            canvas.DrawText(scaleBarText1, posX1, posY1 - _textSize1.Top, _paintScaleText);

            boundingBox.Add(new Box(posX1, posY1, posX1 + _textSize1.Width, posY1 + _textSize1.Height));

            _scaleBarOverlay.BoundingBox = boundingBox;

            if (!_scaleBarOverlay.ShowBoundingBox)
                return;

            // Draw a rect around the scale bar for testing
            canvas.DrawRect(new SKRect(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY), _paintScaleBarBox);
        }

        SKPaint CreateScaleBarPaint(SKPaintStyle style)
        {
            return new SKPaint
            {
                LcdRenderText = true,
                Style = style,
                StrokeCap = SKStrokeCap.Square
            };
        }

        SKPaint CreateTextPaint(SKPaintStyle style)
        {
            return new SKPaint
            {
                LcdRenderText = true,
                Style = style,
                IsAntialias = true
            };
        }

        /// <summary>
        /// Calculates the length and text for both scalebars
        /// </summary>
        /// <returns>
        /// Length of upper scalebar
        /// Text of upper scalebar
        /// Length of lower scalebar
        /// Text of lower scalebar
        /// </returns>
        public (float scaleBarLength1, string scaleBarText1, float scaleBarLength2, string scaleBarText2)
            GetScaleBarLengthAndText(Viewport viewport)
        {
            if (_scaleBarOverlay.Layer == null) return (0, null, 0, null);

            float length1;
            string text1;

            (length1, text1) = CalculateScaleBarLengthAndValue(_scaleBarOverlay.Layer, viewport, _scaleBarOverlay.MaxWidth, _scaleBarOverlay.UnitConverter);

            float length2;
            string text2;

            if (_scaleBarOverlay.SecondaryUnitConverter != null)
                (length2, text2) = CalculateScaleBarLengthAndValue(_scaleBarOverlay.Layer, viewport, _scaleBarOverlay.MaxWidth, _scaleBarOverlay.SecondaryUnitConverter);
            else
                (length2, text2) = (0, null);

            return (length1, text1, length2, text2);
        }

        /// <summary>
        /// Get pairs of points, which determin start and stop of the lines used to draw the scalebar
        /// </summary>
        /// <param name="viewport">The viewport of the map</param>
        /// <param name="scaleBarLength1">Length of upper scalebar</param>
        /// <param name="scaleBarLength2">Length of lower scalebar</param>
        /// <param name="stroke">Width of line</param>
        /// <returns>Array with pairs of Points. First is always the start point, the second is the end point.</returns>
        public Point[] GetScaleBarLinePositions(Viewport viewport, float scaleBarLength1, float scaleBarLength2, float stroke)
        {
            Point[] points = null;

            bool drawNoSecondScaleBar = _scaleBarOverlay.ScaleBarMode == ScaleBarMode.Single || _scaleBarOverlay.ScaleBarMode == ScaleBarMode.Both && _scaleBarOverlay.SecondaryUnitConverter == null;

            float maxScaleBarLength = Math.Max(scaleBarLength1, scaleBarLength2);

            var posX = _scaleBarOverlay.CalculatePositionX(0, (int)viewport.Width, _scaleBarOverlay.MaxWidth);
            var posY = _scaleBarOverlay.CalculatePositionY(0, (int)viewport.Height, _scaleBarOverlay.Height);

            float left = posX + stroke * 0.5f * _scaleBarOverlay.Scale;
            float right = posX + _scaleBarOverlay.MaxWidth - stroke * 0.5f * _scaleBarOverlay.Scale;
            float center1 = posX + (_scaleBarOverlay.MaxWidth - scaleBarLength1) / 2;
            float center2 = posX + (_scaleBarOverlay.MaxWidth - scaleBarLength2) / 2;
            // Top position is Y in the middle of scale bar line
            float top = posY + (drawNoSecondScaleBar ? _scaleBarOverlay.Height - stroke * 0.5f * _scaleBarOverlay.Scale : _scaleBarOverlay.Height * 0.5f);

            switch (_scaleBarOverlay.TextAlignment)
            {
                case Alignment.Center:
                    if (drawNoSecondScaleBar)
                    {
                        points = new Point[6];
                        points[0] = new Point(center1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[1] = new Point(center1, top);
                        points[2] = new Point(center1, top);
                        points[3] = new Point(center1 + maxScaleBarLength, top);
                        points[4] = new Point(center1 + maxScaleBarLength, top);
                        points[5] = new Point(center1 + scaleBarLength1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                    }
                    else
                    {
                        points = new Point[10];
                        points[0] = new Point(Math.Min(center1, center2), top);
                        points[1] = new Point(Math.Min(center1, center2) + maxScaleBarLength, top);
                        points[2] = new Point(center1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[3] = new Point(center1, top);
                        points[4] = new Point(center1 + scaleBarLength1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[5] = new Point(center1 + scaleBarLength1, top);
                        points[6] = new Point(center2, top + _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[7] = new Point(center2, top);
                        points[8] = new Point(center2 + scaleBarLength2, top + _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[9] = new Point(center2 + scaleBarLength2, top);
                    }
                    break;
                case Alignment.Left:
                    if (drawNoSecondScaleBar)
                    {
                        points = new Point[6];
                        points[0] = new Point(left, top);
                        points[1] = new Point(left + scaleBarLength1, top);
                        points[2] = new Point(left, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[3] = new Point(left, top);
                        points[4] = new Point(left + scaleBarLength1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[5] = new Point(left + scaleBarLength1, top);
                    }
                    else
                    {
                        points = new Point[8];
                        points[0] = new Point(left, top);
                        points[1] = new Point(left + maxScaleBarLength, top);
                        points[2] = new Point(left, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[3] = new Point(left, top + _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[4] = new Point(left + scaleBarLength1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[5] = new Point(left + scaleBarLength1, top);
                        points[6] = new Point(left + scaleBarLength2, top + _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[7] = new Point(left + scaleBarLength2, top);
                    }
                    break;
                case Alignment.Right:
                    if (drawNoSecondScaleBar)
                    {
                        points = new Point[6];
                        points[0] = new Point(right, top);
                        points[1] = new Point(right - maxScaleBarLength, top);
                        points[2] = new Point(right, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[3] = new Point(right, top);
                        points[4] = new Point(right - scaleBarLength1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[5] = new Point(right - scaleBarLength1, top);
                    }
                    else
                    {
                        points = new Point[8];
                        points[0] = new Point(right, top);
                        points[1] = new Point(right - maxScaleBarLength, top);
                        points[2] = new Point(right, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[3] = new Point(right, top + _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[4] = new Point(right - scaleBarLength1, top - _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[5] = new Point(right - scaleBarLength1, top);
                        points[6] = new Point(right - scaleBarLength2, top + _scaleBarOverlay.TickLength * _scaleBarOverlay.Scale);
                        points[7] = new Point(right - scaleBarLength2, top);
                    }
                    break;
            }

            return points;
        }

        /// <summary>
        /// Calculates the top-left-position of upper and lower text
        /// </summary>
        /// <param name="viewport">The viewport</param>
        /// <param name="textSize">Default textsize for the string "9999 m"</param>
        /// <param name="textSize1">Size of upper text of scalebar</param>
        /// <param name="textSize2">Size of lower text of scalebar</param>
        /// <param name="stroke">Width of line</param>
        /// <returns>
        /// posX1 as left position of upper scalebar text
        /// posY1 as top position of upper scalebar text
        /// posX2 as left position of lower scalebar text
        /// posY2 as top position of lower scalebar text
        /// </returns>
        (float posX1, float posY1, float posX2, float posY2) GetScaleBarTextPositions(Viewport viewport,
            SKRect textSize, SKRect textSize1, SKRect textSize2, float stroke)
        {
            bool drawNoSecondScaleBar = _scaleBarOverlay.ScaleBarMode == ScaleBarMode.Single || (_scaleBarOverlay.ScaleBarMode == ScaleBarMode.Both && _scaleBarOverlay.SecondaryUnitConverter == null);

            float posX = _scaleBarOverlay.CalculatePositionX(0, (int)viewport.Width, _scaleBarOverlay.MaxWidth);
            float posY = _scaleBarOverlay.CalculatePositionY(0, (int)viewport.Height, _scaleBarOverlay.Height);

            float left = posX + (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale;
            float right1 = posX + _scaleBarOverlay.MaxWidth - (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale - (float)textSize1.Width;
            float right2 = posX + _scaleBarOverlay.MaxWidth - (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale - (float)textSize2.Width;
            float top = posY;
            float bottom = posY + _scaleBarOverlay.Height - (float)textSize2.Height;

            switch (_scaleBarOverlay.TextAlignment)
            {
                case Alignment.Center:
                    if (drawNoSecondScaleBar)
                    {
                        return (posX + (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale + (_scaleBarOverlay.MaxWidth - 2.0f * (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale - (float)textSize1.Width) / 2.0f,
                            top,
                            0,
                            0);
                    }
                    else
                    {
                        return (posX + (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale + (_scaleBarOverlay.MaxWidth - 2.0f * (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale - (float)textSize1.Width) / 2.0f,
                                top,
                                posX + (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale + (_scaleBarOverlay.MaxWidth - 2.0f * (stroke + _scaleBarOverlay.TextMargin) * _scaleBarOverlay.Scale - (float)textSize2.Width) / 2.0f,
                                bottom);
                    }
                case Alignment.Left:
                    if (drawNoSecondScaleBar)
                    {
                        return (left, top, 0, 0);
                    }
                    else
                    {
                        return (left, top, left, bottom);
                    }
                case Alignment.Right:
                    if (drawNoSecondScaleBar)
                    {
                        return (right1, top, 0, 0);
                    }
                    else
                    {
                        return (right1, top, right2, bottom);
                    }
                default:
                    return (0, 0, 0, 0);
            }
        }

        /// Calculates the required length and value of a scalebar
        ///
        /// @param viewport the Viewport to calculate for
        /// @param width of the scale bar in pixel to calculate for
        /// @param unitConverter the DistanceUnitConverter to calculate for
        /// @return scaleBarLength and scaleBarText
        private static (float scaleBarLength, string scaleBarText) CalculateScaleBarLengthAndValue(
            Layer layer, Viewport viewport, float width, IUnitConverter unitConverter)
        {
            // We have to calc the angle difference to the equator (angle = 0), 
            // because EPSG:3857 is only there 1 m. At othere angles, we
            // should calculate the correct length.
            var position = new Point(layer.Projection.XToLongitude(viewport.Center.X), layer.Projection.YToLatitude(viewport.Center.Y));

            // Calc ground resolution in meters per pixel of viewport for this latitude
            double groundResolution = Globals.EarthCircumference / viewport.TileScaleFactor * Math.Cos(position.Y / 180.0 * Math.PI);

            // Convert in units of UnitConverter
            groundResolution = groundResolution / unitConverter.MeterRatio;

            var scaleBarValues = unitConverter.ScaleBarValues;

            float scaleBarLength = 0;
            int scaleBarValue = 0;

            foreach (int value in scaleBarValues)
            {
                scaleBarValue = value;
                scaleBarLength = (float)(scaleBarValue / groundResolution);
                if (scaleBarLength < width - 10)
                {
                    break;
                }
            }

            var scaleBarText = unitConverter.GetScaleText(scaleBarValue);

            return (scaleBarLength, scaleBarText);
        }
    }
}
