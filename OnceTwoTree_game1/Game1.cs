using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;

using MonoGame.Extended.ViewportAdapters;

namespace OnceTwoTree_game1
{
    public class Game1 : Game
    {
        #region Every Var
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public GameTime gametimeUse;
        private const int MapWidth = 4536;
        private const int MapHeight = 4320;

        public static OrthographicCamera _camera1, _camera2;
        public static Vector2 _cameraPosition1, _cameraPosition2;
        public static Vector2 _bgPosition1, _bgPosition2;
        public Vector2 _panelPos1, _panelPos2;

        private readonly List<IEntity> _entities = new List<IEntity>();
        public readonly CollisionComponent _collisionComponent;
        public SpriteFont font;

        Texture2D block;
        Texture2D menu;

        TiledMap _tiledMap;
        TiledMapRenderer _tiledMapRenderer;

        TiledMapObjectLayer _platformTiledObj;
        TiledMapObjectLayer _climbObj;
        TiledMapObjectLayer _hookpointObj;
        TiledMapTileLayer _bgLayer;
        TiledMapTileLayer _towerLayer;
        TiledMapTileLayer _platformLayer;
        TiledMapTileLayer _hookLayer;

        bool openConfig1 = true;
        bool openConfig2 = true;


        PlayerOne playerInstance1;
        PlayerTwo playerInstance2;

        bool isGameplay;
        bool isMenu;

        private KeyboardState _currentKey;
        private KeyboardState _oldKey;

        Viewport defaultView;
        Viewport leftView;
        Viewport rightView;
        #endregion
        public Game1()
        {

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _collisionComponent = new CollisionComponent(new RectangleF(0, 0, MapWidth, MapHeight));
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1728;
            graphics.PreferredBackBufferHeight = 972;
        }

        protected override void Initialize()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //Set DefaultView
            defaultView = graphics.GraphicsDevice.Viewport;
            //Set leftView
            leftView = graphics.GraphicsDevice.Viewport;
            leftView.Width = graphics.PreferredBackBufferWidth / 2;
            //Set rightView
            rightView = graphics.GraphicsDevice.Viewport;
            rightView.Width = graphics.PreferredBackBufferWidth / 2;
            rightView.X = graphics.PreferredBackBufferWidth/2 ;


            var viewportadapter1 = new BoxingViewportAdapter(Window, GraphicsDevice, graphics.PreferredBackBufferWidth/2, 972);
            _camera1 = new OrthographicCamera(viewportadapter1);
            _bgPosition1 = new Vector2(graphics.PreferredBackBufferWidth / 4, MapHeight - (972 / 2));

            var viewportadapter2 = new BoxingViewportAdapter(Window, GraphicsDevice, graphics.PreferredBackBufferWidth / 2, 972);
            _camera2 = new OrthographicCamera(viewportadapter2);
            _bgPosition2 = new Vector2(MapWidth - graphics.PreferredBackBufferWidth/4, MapHeight - (972 / 2));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Font");
            block = Content.Load<Texture2D>("Resources\\TileSet\\block");

            //Load Tilemap
            _tiledMap = Content.Load<TiledMap>("TileManagement\\Platform_TileMap");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

            //Load Menu
            menu = Content.Load<Texture2D>("Resources\\Title\\TitleScene");

            #region find & Create OBJ
            //Getting
            //Get Obj layers
            foreach (TiledMapObjectLayer layer in _tiledMap.ObjectLayers)
            {
                if (layer.Name == "PlatformObject")
                {
                    _platformTiledObj = layer;
                }
                if (layer.Name == "ClimbObject")
                {
                    _climbObj = layer;
                }
                if(layer.Name == "HookPointObject")
                {
                    _hookpointObj = layer;
                }
            }

            foreach (TiledMapTileLayer tLayer in _tiledMap.TileLayers)
            {
                if (tLayer.Name == "Background")
                {
                    _bgLayer = tLayer;
                }
                if (tLayer.Name == "Midground")
                {
                    _towerLayer = tLayer;
                }
                if (tLayer.Name == "Platform")
                {
                    _platformLayer = tLayer;
                }
                if(tLayer.Name == "HookPoint")
                {
                    _hookLayer = tLayer;
                }
            }

            //Create
            //Create entities from Map
            foreach (TiledMapObject obj in _platformTiledObj.Objects)
            {
                Point2 position = new Point2(obj.Position.X, obj.Position.Y);
                _entities.Add(new PlatForm(this, new RectangleF(position, obj.Size)));
            }
            //Create ClimbObj from Map
            foreach (TiledMapObject obj in _climbObj.Objects)
            {
                Point2 position = new Point2(obj.Position.X, obj.Position.Y);
                _entities.Add(new ClimbOBJ(this, new RectangleF(position, obj.Size)));
            }
            foreach(TiledMapObject obj in _hookpointObj.Objects)
            {
                Point2 position = new Point2(obj.Position.X, obj.Position.Y);
                _entities.Add(new HookPointObject(this, new RectangleF(position, obj.Size)));
            }

            #endregion

            //Setup Player
            _entities.Add(new PlayerOne(this, new RectangleF(new Point2(276, MapHeight - 600), new Size2(108, 138))));
            _entities.Add(new PlayerTwo(this, new RectangleF(new Point2(MapWidth - 276, MapHeight - 600), new Size2(108, 138))));

            //Start look at
            foreach (IEntity entity in _entities)
            {
                _collisionComponent.Insert(entity);
                if (entity.GetType().Name == "PlayerOne")
                {
                    playerInstance1 = (PlayerOne)entity;
                }
                if (entity.GetType().Name == "PlayerTwo")
                {
                    playerInstance2 = (PlayerTwo)entity;
                }
            }

            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Set Scene
            isMenu = true;
            isGameplay = false;
        }

        protected override void Update(GameTime gameTime)
        {
            InGameUpdate(gameTime);

            if (isMenu == true)
            {
                UpdateMenu();
            }
            else if (isGameplay == true)
            {
                UpdateGameplay();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (isMenu == true)
            {
                DrawMenu();
            }
            if (isGameplay == true)
            {
                DrawGameplay();
            }

            //InGameDraw();

            base.Draw(gameTime);
        }

        #region Get Weight,Height
        public int GetMapWidth()
        {
            return MapWidth;
        }
        public int GetMapHeight()
        {
            return MapHeight;
        }
        #endregion

        #region Camera
        //Camera1
        public void UpdateCamera1X(Vector2 move)
        {
            _cameraPosition1 += move;
        }

        public void UpdateCamera1Y(Vector2 move)
        {
            _cameraPosition1 -= move;
        }

        public float GetCameraPos1X()
        {
            return _cameraPosition1.X;
        }

        public float GetCameraPos1Y()
        {
            return _cameraPosition1.Y;
        }

        public float GetCamera1X()
        {
            return _camera1.Position.X;
        }
        public float GetCamera1Y()
        {
            return _camera1.Position.Y;
        }

        public Vector2 GetCamera1()
        {
            return _camera1.Position;
        }
        // Camera2
        public void UpdateCamera2X(Vector2 move)
        {
            _cameraPosition2 += move;
        }

        public void UpdateCamera2Y(Vector2 move)
        {
            _cameraPosition2 -= move;
        }

        public float GetCameraPos2X()
        {
            return _cameraPosition2.X;
        }

        public float GetCameraPos2Y()
        {
            return _cameraPosition2.Y;
        }

        public float GetCamera2X()
        {
            return _camera2.Position.X;
        }
        public float GetCamera2Y()
        {
            return _camera2.Position.Y;
        }

        public Vector2 GetCamera2()
        {
            return _camera2.Position;
        }
        #endregion

        public void InGameUpdate(GameTime gameTime)
        {
            _currentKey = Keyboard.GetState();
            gametimeUse = gameTime;
            //Open Panel
            if (_currentKey.IsKeyDown(Keys.NumPad1) && _oldKey.IsKeyUp(Keys.NumPad1))
            {

                if (openConfig1)
                {
                    openConfig1 = false;
                }
                else if (!openConfig1)
                {
                    openConfig1 = true;
                }
            }

            if (_currentKey.IsKeyDown(Keys.NumPad2) && _oldKey.IsKeyUp(Keys.NumPad2))
            {

                if (openConfig2)
                {
                    openConfig2 = false;
                }
                else if (!openConfig2)
                {
                    openConfig2 = true;
                }
            }


            foreach (IEntity entity in _entities)
            {
                entity.Update(gameTime);
            }

            _tiledMapRenderer.Update(gameTime);

            _collisionComponent.Update(gameTime);
            //follow look at

            _camera1.LookAt(_bgPosition1 + _cameraPosition1);
            playerInstance1.SetSkillCheckPos(_camera1.Position);
            playerInstance1.SkillCheck(gameTime);
            _panelPos1 = new Vector2(_camera1.Position.X , _camera1.Position.Y + graphics.PreferredBackBufferHeight - 324);

            _camera2.LookAt(_bgPosition2 + _cameraPosition2);
            playerInstance2.SetSkillCheckPos(_camera2.Position);
            playerInstance2.SkillCheck(gameTime);
            _panelPos2 = new Vector2(_camera2.Position.X +50, _camera2.Position.Y +50);
            _oldKey = _currentKey;

            //hook update
            HookBox.Update_Hook_Hitblock(_collisionComponent);
        }


        #region Scene Manage
        public void UpdateMenu()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                isMenu = false;
                isGameplay = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                isMenu = false;
                isGameplay = true;
            }
        }
        public void UpdateGameplay()
        {
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                isMenu = true;
                isGameplay = false;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                isMenu = true;
                isGameplay = false;
            }
        }
        public void DrawMenu()
        {
            GraphicsDevice.Viewport = defaultView;
            spriteBatch.Begin();

            spriteBatch.Draw(menu, Vector2.Zero, new Rectangle(0, 0, menu.Width / 2, menu.Height), Color.White);

            spriteBatch.End();
        }
        public void DrawGameplay()
        {
            GraphicsDevice.Viewport = leftView;
            var transformMatrix1 = _camera1.GetViewMatrix();
            _tiledMapRenderer.Draw(_bgLayer, transformMatrix1);
            _tiledMapRenderer.Draw(_towerLayer, transformMatrix1);
            _tiledMapRenderer.Draw(_platformLayer, transformMatrix1);
            _tiledMapRenderer.Draw(_hookLayer, transformMatrix1);
            spriteBatch.Begin(transformMatrix: transformMatrix1);

            foreach (IEntity entity in _entities)
            {
                entity.Draw(spriteBatch);
                if (openConfig1)
                {
                    spriteBatch.Draw(block, _panelPos1, null, Color.Brown, 0f, Vector2.Zero, new Vector2(5, 3f), SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Player Pos = " + playerInstance1.Bounds.Position, new Vector2(_panelPos1.X + 10, _panelPos1.Y + 10), Color.Black);
                    spriteBatch.DrawString(font, "Camera.Pos = " + _camera1.Position, new Vector2(_panelPos1.X + 10, _panelPos1.Y + 30), Color.Black);
                    spriteBatch.DrawString(font, "CameraPosition = " + _cameraPosition1, new Vector2(_panelPos1.X + 10, _panelPos1.Y + 50), Color.Black);
                    spriteBatch.DrawString(font, "DcamX = " + (playerInstance1.Bounds.Position.X - _cameraPosition1.X), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 70), Color.Black);
                    spriteBatch.DrawString(font, "Player Stage= " + (playerInstance1.p_stateNum), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 90), Color.Black);
                    spriteBatch.DrawString(font, "Wall Check R= " + (playerInstance1.wallCheckRight), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 110), Color.Black);
                    spriteBatch.DrawString(font, "Wall Check L = " + (playerInstance1.wallCheckLeft), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 130), Color.Black);
                    spriteBatch.DrawString(font, "Climb = " + (playerInstance1.onClimb), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 150), Color.Black);
                    spriteBatch.DrawString(font, "CountG = " + (playerInstance1.countG), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 170), Color.Black);
                    spriteBatch.DrawString(font, "CountW = " + (playerInstance1.countW), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 190), Color.Black);
                    spriteBatch.DrawString(font, "CountC = " + (playerInstance1.countC), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 210), Color.Black);
                    spriteBatch.DrawString(font, "oldCount = " + (playerInstance1.timeCount), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 230), Color.Black);
                    spriteBatch.DrawString(font, "Dcount = " + (playerInstance1.timeCount - playerInstance1.countG), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 250), Color.Black);
                    spriteBatch.DrawString(font, "FirstCheck = " + (playerInstance1.firstCheck), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 270), Color.Black);
                    spriteBatch.DrawString(font, "Energy = " + (playerInstance1.energy), new Vector2(_panelPos1.X + 10, _panelPos1.Y + 290), Color.Black);
                    spriteBatch.DrawString(font, "Throwstate = " + (HookBox._Hookboxes[0].Bounds.Position), new Vector2(_panelPos1.X + 270, _panelPos1.Y + 290), Color.Black);
                    spriteBatch.DrawString(font, "Throwstate = " + (HookBox._Hookboxes[0].throw_state), new Vector2(_panelPos1.X + 270, _panelPos1.Y + 270), Color.Black);

                }

            }
            spriteBatch.End();

            GraphicsDevice.Viewport = rightView;
            var transformMatrix2 = _camera2.GetViewMatrix();
            _tiledMapRenderer.Draw(_bgLayer, transformMatrix2);
            _tiledMapRenderer.Draw(_towerLayer, transformMatrix2);
            _tiledMapRenderer.Draw(_platformLayer, transformMatrix2);
            _tiledMapRenderer.Draw(_hookLayer, transformMatrix2);
            spriteBatch.Begin(transformMatrix: transformMatrix2);

            foreach (IEntity entity in _entities)
            {
                entity.Draw(spriteBatch);
                if (openConfig2)
                {

                    
                    spriteBatch.Draw(block, _panelPos2, null, Color.Brown, 0f, Vector2.Zero, new Vector2(5, 3f), SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Player Pos = " + playerInstance2.Bounds.Position, new Vector2(_panelPos2.X + 10, _panelPos2.Y + 10), Color.Black);
                    spriteBatch.DrawString(font, "Camera.Pos = " + _camera2.Position, new Vector2(_panelPos2.X + 10, _panelPos2.Y + 30), Color.Black);
                    spriteBatch.DrawString(font, "CameraPosition = " + _cameraPosition2, new Vector2(_panelPos2.X + 10, _panelPos2.Y + 50), Color.Black);
                    spriteBatch.DrawString(font, "DcamX = " + (playerInstance2.Bounds.Position.X - _cameraPosition2.X), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 70), Color.Black);
                    spriteBatch.DrawString(font, "DcamY= " + (playerInstance2.Bounds.Position.Y), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 90), Color.Black);
                    spriteBatch.DrawString(font, "Wall Check R= " + (playerInstance2.wallCheckRight), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 110), Color.Black);
                    spriteBatch.DrawString(font, "Wall Check L = " + (playerInstance2.wallCheckLeft), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 130), Color.Black);
                    spriteBatch.DrawString(font, "Climb = " + (playerInstance2.onClimb), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 150), Color.Black);
                    spriteBatch.DrawString(font, "CountG = " + (playerInstance2.countG), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 170), Color.Black);
                    spriteBatch.DrawString(font, "CountW = " + (playerInstance2.countW), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 190), Color.Black);
                    spriteBatch.DrawString(font, "CountC = " + (playerInstance2.countC), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 210), Color.Black);
                    spriteBatch.DrawString(font, "oldCount = " + (playerInstance2.timeCount), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 230), Color.Black);
                    spriteBatch.DrawString(font, "Dcount = " + (playerInstance2.timeCount - playerInstance2.countG), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 250), Color.Black);
                    spriteBatch.DrawString(font, "FirstCheck = " + (playerInstance2.firstCheck), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 270), Color.Black);
                    spriteBatch.DrawString(font, "Energy = " + (playerInstance2.energy), new Vector2(_panelPos2.X + 10, _panelPos2.Y + 290), Color.Black);
                    spriteBatch.DrawString(font, "Throwstate = " + (HookBox._Hookboxes[1].Bounds.Position), new Vector2(_panelPos2.X + 270, _panelPos2.Y + 290), Color.Black);
                    spriteBatch.DrawString(font, "Throwstate = " + (HookBox._Hookboxes[1].throw_state), new Vector2(_panelPos2.X + 270, _panelPos2.Y + 270), Color.Black);

                }
            }

            spriteBatch.End();

        }
        #endregion
    }
}
