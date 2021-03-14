using System;
using System.IO;
using System.Linq;
using System.Reflection;
using VTMap.MapboxGL;
using VTMap.View.Layers;
using VTMap.View.Overlays.ScaleBar;
using Xamarin.Forms;
using Xamarin.Essentials;
using VTMap.Core.Map;
using VectorTilesView.Layers;

namespace VTMap.App
{
    public partial class MainPage : ContentPage
    {
        Random rand = new Random();

        public MainPage()
        {
            InitializeComponent();

            LoadMapboxGL(Assembly.GetAssembly(GetType()), mapView.Map);

            mapView.Map.Layers.Add(new CrossLayer());
            //var tileIdLayer = new TileIdLayer();
            //mapView.Map.Layers.Add(tileIdLayer);
            var scaleBarLayer = new ScaleBarOverlay(mapView.Map.Layers[1]) { SecondaryUnitConverter = ImperialUnitConverter.Instance, ScaleBarMode = ScaleBarMode.Both, ShowBoundingBox = true, };
            mapView.Map.Overlays.Add(scaleBarLayer);
            //var demoLayer = new DemoLayer();
            //mapView.Map.Layers.Add(demoLayer);

            mapView.Viewport.PropertyChanged += (s,e) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var tiles = mapView.Viewport.Tiles;
                    labelCenter.Text = $"Center {mapView.Viewport.Center.X:0.000000}/{mapView.Viewport.Center.Y:0.000000}";// $"Box {mapView.Viewport.BoundingBox}"; // $"Center {mapView.Viewport.Center.X:0.000000}/{mapView.Viewport.Center.Y:0.000000}";
                    labelScale.Text = $"Scale {mapView.Viewport.Scale:#0.0000} / ZoomLevel {mapView.Viewport.ZoomLevel:0.0} / Zoom {mapView.Viewport.Zoom:#0.0000} / ZoomScale {mapView.Viewport.ZoomScale:#0.0000}";
                });
            };
            mapView.TouchEventHandler.TouchMove += (s, e) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    labelPosition.Text = $"Map {mapView.Viewport.FromScreenToView(e.Location).X}/{mapView.Viewport.FromScreenToView(e.Location).Y}";
                });
            };

            buttonCenter.Command = new Command(() =>
            {
                mapView.Navigator.MoveTo(new Core.Primitives.Point(0, 0), 2000);
                mapView.Navigator.RotateTo(0f, Core.Primitives.Point.Empty, 2000);
                mapView.Navigator.ScaleTo(1f, Core.Primitives.Point.Empty, 2000);
            });
            buttonRotate.Command = new Command(() => { mapView.Navigator.RotateTo(mapView.Viewport.Rotation + 10, Core.Primitives.Point.Empty, 1000); });
            buttonZoomIn.Command = new Command(() => mapView.Navigator.ScaleTo(mapView.Viewport.Scale * 2f, Core.Primitives.Point.Empty, 1000));
            buttonZoomOut.Command = new Command(() => mapView.Navigator.ScaleTo(mapView.Viewport.Scale / 2f, Core.Primitives.Point.Empty, 1000));
        }

        public void RotateMap()
        {
            var angle = mapView.Viewport.Rotation - 30; // -180.0f + 360.0f * (float)rand.NextDouble();
            mapView.Navigator.RotateTo(angle, Core.Primitives.Point.Empty, 300);
        }

        public void LoadMapboxGL(Assembly assemblyToUse, MapData map)
        {
            var filename = "monaco.mbtiles";
            MGLStyleLoader.DirectoryForFiles = FileSystem.AppDataDirectory;

            CheckForMBTilesFile(filename, MGLStyleLoader.DirectoryForFiles);

            // Get Mapbox GL Style File
            var mglStyleFile = CreateMGLStyleFile(assemblyToUse);

            if (mglStyleFile == null)
                return;

            // Ok, we have a valid style file, so get the tile sources, which contain the style file
            foreach (var tileSource in mglStyleFile.TileSources)
            {
                if (tileSource is MGLBackgroundTileSource)
                {
                    var layer = new BackgroundLayer(tileSource);
                    // This is a background layer, so it should always the first layer to draw
                    map.Layers.Add(layer);
                }

                if (tileSource is MGLVectorTileSource)
                {
                    var layer = new VectorTileLayer(tileSource);
                    map.Layers.Add(layer);
                    //var layer = new TileLayer(tileSource, fetchStrategy: new FetchStrategy(30), fetchToFeature: DrawableTile.DrawableTileToFeature, fetchGetTile: tileSource.GetVectorTile);
                    //layer.MinVisible = 30.ToResolution();
                    //layer.MaxVisible = 0.ToResolution();
                    //layer.Style = new DrawableTileStyle();
                    //layer.DataChanged += (s, args) =>
                    //{
                    //    if (layer.Busy)
                    //        return;

                    //    // TODO: All tiles are loaded, so create buckets for symbols
                    //    var features = layer.GetFeaturesInView(mapView.Viewport.Extent, mapView.Viewport.Resolution);
                    //    var tiles = features.Select(f => (VectorTile)((DrawableTile)f.Geometry).Data).ToArray();

                    //    int numBuckets = 0;
                    //    int numSymbols = 0;

                    //    var symbols = new List<Symbol>();
                    //    var pathSymbols = new List<MGLPathSymbol>();
                    //    var rbush = new RBush<Symbol>();

                    //    // Check symbols, if they are visible or not. For this,
                    //    // we use the same layer for all tiles and set as much as
                    //    // possible symbols to visible. Than we take the next
                    //    // layer and begin the next check.
                    //    for (int i = 0; i < tiles[0].Buckets.Length; i++)
                    //    {
                    //        if (tiles[0].StyleLayers[i].Type != StyleType.Symbol)
                    //            continue;

                    //        numBuckets++;

                    //        symbols.Clear();

                    //        foreach (var tile in tiles)
                    //        {
                    //            if (tile.Buckets[i] == null || ((SymbolBucket)tile.Buckets[i]).Symbols.Count == 0)
                    //                continue;

                    //            //symbols.AddRange(((SymbolBucket)tile.Buckets[i]).Symbols.Where((symbol) => !(symbol is MGLTextSymbol)));
                    //            symbols.AddRange(((SymbolBucket)tile.Buckets[i]).Symbols);

                    //            numSymbols += ((SymbolBucket)tile.Buckets[i]).Symbols.Count;
                    //        }

                    //        if (symbols.Count == 0)
                    //            continue;

                    //        // If we have path symbols, then check, if there are path over tile borders
                    //        if (symbols[0] is MGLPathSymbol)
                    //        {
                    //            int pos = 0;
                    //            var sameSymbols = new List<MGLPathSymbol>();

                    //            while (pos < symbols.Count)
                    //            {
                    //                var pathSymbol = symbols[pos] as MGLPathSymbol;
                    //                var id = symbols[pos].Id;

                    //                sameSymbols.Add(pathSymbol);

                    //                var next = pos + 1;

                    //                while (next < symbols.Count)
                    //                {
                    //                    if (symbols[next].Id == id)
                    //                    {
                    //                        // We have a symbol, which belongs to another symbol on another tile
                    //                        sameSymbols.Add((MGLPathSymbol)symbols[next]);
                    //                    }

                    //                    next++;
                    //                }

                    //                // Now we have all tiles for this symbol ID
                    //                if (sameSymbols.Count > 1)
                    //                {
                    //                    var found = true;
                    //                }

                    //                pos++;
                    //            }
                    //        }

                    //        // Now we have all symbols of one style layer, that should be 
                    //        // visible. So we could check, which symbol should be visible 
                    //        // and which not.
                    //        foreach (var symbol in symbols)
                    //        {
                    //            //if (symbol.Envelope != SKRect.Empty && rbush.Search(symbol.Envelope).Count == 0)
                    //            {
                    //                symbol.IsVisible = true;
                    //                rbush.Insert(symbol);
                    //            }
                    //        }

                    //        //System.Diagnostics.Debug.WriteLine($"Found {numSymbols} symbols in {numBuckets} buckets of {tiles.Length} tiles");
                    //    };



                    //};

                    //map.Layers.Add(layer);
                }

                //if (tileSource is MGLRasterTileSource)
                //{
                //    var layer = new TileLayer(tileSource, fetchStrategy: new FetchStrategy(3), fetchToFeature: DrawableTile.DrawableTileToFeature, fetchGetTile: tileSource.GetVectorTile);
                //    layer.MinVisible = tileSource.Schema.Resolutions.Last().Value.UnitsPerPixel;
                //    layer.MaxVisible = tileSource.Schema.Resolutions.First().Value.UnitsPerPixel;
                //    layer.Style = new DrawableTileStyle();
                //    map.Layers.Add(layer);
                //}
            }
        }

        private static string CheckForMBTilesFile(string filename, string dataDir)
        {
            filename = Path.Combine(dataDir, filename);
            if (!File.Exists(filename))
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var resourceName = resourceNames.FirstOrDefault(s => s.ToLower().EndsWith("monaco.mbtiles") == true);
                if (resourceName != null)
                {
                    var stream = assembly.GetManifestResourceStream(resourceName);
                    using (var file = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(file);
                    }
                }
            }

            return filename;
        }

        public MGLStyleFile CreateMGLStyleFile(Assembly assemblyToUse)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assemblyToUse.GetManifestResourceNames();
            var resourceName = resourceNames.FirstOrDefault(s => s.ToLower().EndsWith("styles.osm-liberty.json") == true);

            MGLStyleFile result;

            if (string.IsNullOrEmpty(resourceName))
                return null;

            // Open JSON style files and read contents
            using (var stream = assemblyToUse.GetManifestResourceStream(resourceName))
            {
                // Open JSON style files and read contents
                result = MGLStyleLoader.Load(stream, assemblyToUse);
            }

            return result;
        }
    }
}
