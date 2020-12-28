using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace Graphs
{
    public class Graphs : Game
    {
        // The renderer
        private readonly GraphicsDeviceManager _graphicsDevice;

        private SpriteBatch _spriteBatch;

        // The map texture
        private Texture2D _map;

        // An empty pixel texture
        private Texture2D _pixel;

        private SpriteFont _font;

        // The array for the traveling merchant problem
        private int[] _worldTravel;

        // The price and list of nodes for the brute force traveling merchant problem
        private (int, List<Flight>) _bruteWorldTravel;

        private KeyboardState _lastKeyboardState = Keyboard.GetState();

        private MouseState _lastMouseState = Mouse.GetState();

        // The path for the airports file
        private string _airportPath;

        // The file for the flight paths file
        private string _fligthPath;

        public Graphs(string[] args)
        {
            _graphicsDevice = new GraphicsDeviceManager(this);

            // Change the assets folder
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            // If the program was not launched with 2 arguments, exit the method
            if (args.Length != 2) return;

            // Change the file paths to the ones passed by the arguments
            _airportPath = args[0];
            _fligthPath = args[1];
        }

        protected override void Initialize()
        {
            // Sets the window size
            _graphicsDevice.PreferredBackBufferWidth = 1280;
            _graphicsDevice.PreferredBackBufferHeight = 720;

            _graphicsDevice.ApplyChanges();

            // Init the gui buttons hashmap
            Utils.GraphButtons = new Dictionary<string, GraphButton>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Loads the map and the font
            _map = Content.Load<Texture2D>("mundi");
            _font = Content.Load<SpriteFont>("Arial");

            // Creates the white pixel
            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Creates a button
            Utils.GraphButtons.Add("distance", new GraphButton("Distancia",
                10, 710, _font.MeasureString("Distancia"),
                Color.Aquamarine, Color.Aqua, Color.Black));

            // Activates the button by default
            Utils.GraphButtons["distance"].IsActive = true;
            // Declares a custom function to be executed when the button is clicked
            Utils.GraphButtons["distance"].OnPress = () =>
            {
                // Toggle the two buttons
                Utils.GraphButtons["distance"].IsActive = true;
                Utils.GraphButtons["price"].IsActive = false;

                // Selects the distance matrix as the default one
                GraphUtils.UsedGraph = GraphUtils.DistanceGraph;
                // Recalculates the shortest path
                GraphUtils.DesiredPath =
                    GraphUtils.GetPath(GraphUtils.UsedGraph, GraphUtils.StartAirport, GraphUtils.EndAirport);

                return Utils.GraphButtons["distance"].IsActive;
            };

            // Creates a button
            Utils.GraphButtons.Add("price", new GraphButton("Preco",
                10 + Utils.GraphButtons["distance"].Area.Width + Utils.GraphButtons["distance"].Area.X, 710, _font.MeasureString("Preco"),
                Color.Aquamarine, Color.Aqua, Color.Black));

            Utils.GraphButtons["price"].OnPress = () =>
            {
                Utils.GraphButtons["price"].IsActive = true;
                Utils.GraphButtons["distance"].IsActive = false;

                GraphUtils.UsedGraph = GraphUtils.PriceGraph;

                GraphUtils.DesiredPath =
                    GraphUtils.GetPath(GraphUtils.UsedGraph, GraphUtils.StartAirport, GraphUtils.EndAirport);

                return Utils.GraphButtons["price"].IsActive;
            };

            // Creates a button
            Utils.GraphButtons.Add("height", new GraphButton("Altitude",
                10 + Utils.GraphButtons["price"].Area.Width + +Utils.GraphButtons["price"].Area.X, 710, _font.MeasureString("Altitude"),
                Color.Aquamarine, Color.Aqua, Color.Black));

            // Creates a button
            Utils.GraphButtons.Add("tour", new GraphButton("Volta ao Mundo",
                10 + Utils.GraphButtons["height"].Area.Width + Utils.GraphButtons["height"].Area.X, 710, _font.MeasureString("Volta ao Mundo"),
                Color.Aquamarine, Color.Aqua, Color.Black));

            // Creates a button
            Utils.GraphButtons.Add("brute", new GraphButton("Forca Bruta",
                10 + Utils.GraphButtons["tour"].Area.Width + Utils.GraphButtons["tour"].Area.X, 710, _font.MeasureString("Forca Bruta"),
                Color.Aquamarine, Color.Aqua, Color.Black));

            GraphUtils.DesiredPath = new List<(int, int)>();

            // Read the files
            GraphUtils.ReadAirports(_airportPath ?? @"D:\Development\_Projects\Graphs\Graphs\Content\aer_teste.txt", _map);
            GraphUtils.ReadFlights(_fligthPath ?? @"D:\Development\_Projects\Graphs\Graphs\Content\voos_teste.txt");

            // Calculate the flight intersections
            GraphUtils.ComputeIntersections();

            // Run the traveling merchant problem with the first node
            _worldTravel = WorldTravel.Heuristic(GraphUtils.PriceGraph, GraphUtils.Airports.Count, 0);
            // Run it with the brute force method
            _bruteWorldTravel = WorldTravel.BruteForce(GraphUtils.PriceGraph, GraphUtils.Airports.Count);

            // Selects the distance matrix as the default one
            GraphUtils.UsedGraph = GraphUtils.DistanceGraph;
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Verify if the left or right mouse button was pressed
            var mouseState = Mouse.GetState();
            bool isLeftButtonPressed = _lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed;
            bool isRightButtonPressed = _lastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed;

            // For each airport that the mouse is hovering
            foreach (var (_, airport) in GraphUtils.Airports.Where(airport => airport.Value.Area.Contains(mouseState.Position)))
            {
                if (isLeftButtonPressed)
                {
                    // Sets the node as the starting airport
                    GraphUtils.StartAirport = GraphUtils.AirportIndex[airport.Name];
                    // Find the shortest path
                    GraphUtils.DesiredPath = GraphUtils.GetPath(GraphUtils.UsedGraph, GraphUtils.StartAirport,
                        GraphUtils.EndAirport);
                    // Run the traveling merchant problem
                    _worldTravel = WorldTravel.Heuristic(GraphUtils.PriceGraph,
                        GraphUtils.Airports.Count, GraphUtils.StartAirport);
                }
                else if (isRightButtonPressed)
                {
                    // Sets the node as the final airport
                    GraphUtils.EndAirport = GraphUtils.AirportIndex[airport.Name];
                    // Find the shortest path
                    GraphUtils.DesiredPath = GraphUtils.GetPath(GraphUtils.UsedGraph, GraphUtils.StartAirport,
                        GraphUtils.EndAirport);
                }
            }

            // Update the GUI buttons
            Utils.UpdateButtons(mouseState.Position, isLeftButtonPressed);

            // Update the state of the mouse and keboard
            _lastKeyboardState = Keyboard.GetState();
            _lastMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear the GPU and set the background to white
            GraphicsDevice.Clear(Color.White);

            // Begin drawing
            _spriteBatch.Begin();

            // Draw the map
            _spriteBatch.Draw(_map, new Vector2(0, 0), Color.White);

            // Draw the FPS
            _spriteBatch.DrawString(_font, $"FPS: {1 / gameTime.ElapsedGameTime.TotalSeconds:00.00}",
                new Vector2(0, _font.MeasureString("Based").Y), Color.Black);

            int totalPrice = 0;
            int totalDistance = 0;

            // For each flights
            foreach (Flight flight in GraphUtils.Flights)
            {
                // Verify if the current flight is on the shortest path
                if ((GraphUtils.DesiredPath.Contains((GraphUtils.AirportIndex[flight.End.Name],
                    GraphUtils.AirportIndex[flight.Start.Name])) || GraphUtils.DesiredPath.Contains((
                    GraphUtils.AirportIndex[flight.Start.Name], GraphUtils.AirportIndex[flight.End.Name]))) && (!Utils.GraphButtons["tour"].IsActive && !Utils.GraphButtons["brute"].IsActive))
                {
                    // Draw the line in red
                    Utils.DrawLine(_pixel, _spriteBatch, new Vector2(flight.Start.X, flight.Start.Y),
                        new Vector2(flight.End.X, flight.End.Y), Color.Red, 3);

                    // Calculates the total distance and price
                    totalPrice += flight.Price;
                    totalDistance += GraphUtils.DistanceGraph[GraphUtils.AirportIndex[flight.Start.Name],
                        GraphUtils.AirportIndex[flight.End.Name]];
                }
                else
                {
                   // Draw the line in white
                    Utils.DrawLine(_pixel, _spriteBatch, new Vector2(flight.Start.X, flight.Start.Y),
                        new Vector2(flight.End.X, flight.End.Y), Color.White, 3);
                }
            }

            // If the "Suggest Heights" buttons is active
            if (Utils.GraphButtons["height"].IsActive)
            {
                foreach (var flight in GraphUtils.Flights)
                {
                    foreach (var intersect in flight.Intersections)
                    {
                        // Draw the intersection
                        Utils.DrawLine(_pixel, _spriteBatch, new Vector2(intersect.Start.X, intersect.Start.Y),
                            new Vector2(intersect.End.X, intersect.End.Y), Color.BlueViolet, 2);
                    }
                }
            }

            // Draw the total distance or price
            _spriteBatch.DrawString(_font, Utils.GraphButtons["distance"].IsActive ? $"Distance Based : (Total Distance = {totalDistance})" : $"Price Based : (Total Price = ${totalPrice})",
                new Vector2(0, 0), Color.Black);

            foreach (var flight in GraphUtils.Flights)
            {
                // Get the middle of the line
                int posX = ((flight.Start.X + flight.End.X) / 2);
                int posY = ((flight.Start.Y + flight.End.Y) / 2);

                // Draw the price, the distance and the height
                _spriteBatch.DrawString(_font,
                    $"{flight.Price}\n{GraphUtils.DistanceGraph[GraphUtils.AirportIndex[flight.Start.Name], GraphUtils.AirportIndex[flight.End.Name]]} : {GraphUtils.Heights.GetValueOrDefault(flight, 0) + 10}k",
                    new Vector2(posX, posY), Color.Black, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 1f);
            }

            foreach ((string key, var airport) in GraphUtils.Airports)
            {
                Color curColor;

                // If this node is the starting, draw in blue
                if (GraphUtils.AirportIndex[key] == GraphUtils.StartAirport)
                {
                    curColor = Color.Aqua;
                }
                // If it is final, draw in orange, otherwise draw in black
                else
                {
                    curColor = GraphUtils.AirportIndex[key] == GraphUtils.EndAirport ? Color.Orange : Color.Black;
                }

                // Draw the airport and the name
                _spriteBatch.Draw(_pixel, airport.Area, curColor);
                _spriteBatch.DrawString(_font, key, new Vector2(airport.X + 5, airport.Y + 5), Color.Black);
            }

            var worldTourPrice = 0;
            if (Utils.GraphButtons["tour"].IsActive)
            {
                for (var i = 0; i < _worldTravel.Length; i++)
                {
                    // Gets the current airport
                    var currentAirport = GraphUtils.Airports[GraphUtils.ReverseAirportIndex[_worldTravel[i]]];

                    // Verify if the next airport exists
                    if (i + 1 < _worldTravel.Length)
                    {
                        // Gets the next one
                        var nextAirport = GraphUtils.Airports[GraphUtils.ReverseAirportIndex[_worldTravel[i + 1]]];
                        // Find the flight that connects the two
                        var flight = GraphUtils.Flights.First(value =>
                            (value.Start.Name == currentAirport.Name && value.End.Name == nextAirport.Name) ||
                            (value.End.Name == currentAirport.Name && value.Start.Name == nextAirport.Name));

                        // Draw the line between them
                        Utils.DrawLine(_pixel, _spriteBatch, new Vector2(flight.Start.X, flight.Start.Y),
                            new Vector2(flight.End.X, flight.End.Y), Color.Fuchsia, 2);

                        // Sums the world tour price
                        worldTourPrice += flight.Price;
                    }

                    // Draw the visited aiport
                    _spriteBatch.Draw(_pixel, currentAirport.Area, Color.Fuchsia);
                }

                // Draw the world tour price
                _spriteBatch.DrawString(_font, $"World Tour Price : ${worldTourPrice}", new Vector2(0, _font.MeasureString("AAA\n000").Y),
                    Color.Black);
            }

            // If the "Brute Force World Tour" is active
            if (Utils.GraphButtons["brute"].IsActive)
            {
                // For each flight in the brute force world tour
                foreach (Flight t in _bruteWorldTravel.Item2)
                {
                    // Draw the flight line
                    Utils.DrawLine(_pixel, _spriteBatch, new Vector2(t.Start.X, t.Start.Y),
                        new Vector2(t.End.X, t.End.Y), Color.Chocolate, 2);

                    // Draw the airports
                    _spriteBatch.Draw(_pixel, t.Start.Area, Color.Chocolate);
                    _spriteBatch.Draw(_pixel, t.End.Area, Color.Chocolate);
                }

                // Draw the price of it
                _spriteBatch.DrawString(_font, $"Brute Force World Tour Price : ${_bruteWorldTravel.Item1}", new Vector2(0, _font.MeasureString("AAA\n000\n000").Y),
                    Color.Black);
            }

            // Draw the GUI
            Utils.DrawButtons(_spriteBatch, _pixel, _font);

            // End drawing
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}