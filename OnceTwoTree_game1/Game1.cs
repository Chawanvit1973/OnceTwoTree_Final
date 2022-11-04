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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private const int MapWidth = 4536;
        private const int MapHeight= 4320;

        public static OrthographicCamera _camera;
        public static Vector2 _cameraPosition;
        public static Vector2 _bgPosition;
        public Vector2 _panelPos;
        
        private readonly List<IEntity> _entities = new List<IEntity>();
        public readonly CollisionComponent _collisionComponent;
        public SpriteFont font;
        Texture2D block; 

        TiledMap _tiledMap;
        TiledMapRenderer _tiledMapRenderer;

        TiledMapObjectLayer _platformTiledObj;
        TiledMapObjectLayer _climbObj;

        bool openConfig = true;
        bool isKeyDown = false;
        
        Vector2 oldPosCam;

        PlayerOne PlayerOnePos;
        PlayerTwo PlayerTwoPos;

        #region ViewPort

        Viewport defaultView;
        Viewport leftView;
        Viewport rightView;

        Camera cameraOne;
        Camera cameraTwo;

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
            var viewportadapter = new BoxingViewportAdapter(Window,GraphicsDevice, 1728, 972);
            _camera = new OrthographicCamera(viewportadapter);
            _bgPosition = new Vector2(1728/2, MapHeight-(972/2));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Font");
            block = Content.Load<Texture2D>("Resources\\TileSet\\block");

            //Load Tilemap
            _tiledMap = Content.Load<TiledMap>("TileManagement\\Platform_TileMap");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

            

            //Get Obj layers
            foreach (TiledMapObjectLayer layer in _tiledMap.ObjectLayers)
            {
                if (layer.Name == "PlatformObject")
                {
                    _platformTiledObj = layer;
                }
            }

            //Get Climb Obj
            foreach(TiledMapObjectLayer layer in _tiledMap.ObjectLayers)
            {
                if(layer.Name == "ClimbObject")
                {
                    _climbObj = layer;
                }
            }

            //Create entities from Map
            foreach(TiledMapObject obj in _platformTiledObj.Objects)
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

            //Setup Player
            _entities.Add(new PlayerOne(this, new RectangleF(new Point2(276, MapHeight - 600), new Size2(108, 138))));
            _entities.Add(new PlayerTwo(this, new RectangleF(new Point2(476, MapHeight - 600), new Size2(108, 138))));

            //Start look at
            foreach (IEntity entity in _entities)
            {
                _collisionComponent.Insert(entity);
                if(entity.GetType().Name == "PlayerOne")
                {
                    PlayerOnePos = (PlayerOne)entity;
                }
                if (entity.GetType().Name == "PlayerTwo")
                {
                    PlayerTwoPos = (PlayerTwo)entity;
                }
            }

            //_camera.LookAt(PlayerPos.Bounds.Position + _bgPosition);
            //_camera.LookAt(_bgPosition + _camera.Position);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            //ViewPort
            defaultView = GraphicsDevice.Viewport;
            leftView = defaultView;
            rightView = defaultView;
            leftView.Width = leftView.Width / 2;
            rightView.Width = rightView.Width / 2;
            rightView.X = leftView.Width;

            cameraOne = new Camera();
            cameraTwo = new Camera();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

           

            //Open Panel
            if (!isKeyDown && Keyboard.GetState().IsKeyDown(Keys.NumPad1))
            {
                isKeyDown = true;
                if (openConfig)
                {
                    openConfig = false;

                }
                else if (!openConfig)
                {
                    openConfig = true;

                }
            }
            else if(Keyboard.GetState().IsKeyUp(Keys.NumPad1) && isKeyDown){ isKeyDown = false; }

            foreach(IEntity entity in _entities)
            {
                entity.Update(gameTime);
                if (entity.GetType().Name == "Player")
                {
                    
                    if (oldPosCam.Y < entity.Bounds.Position.Y - _cameraPosition.Y)
                    {
                        oldPosCam.Y = entity.Bounds.Position.Y - _cameraPosition.Y;
                    }
                    if(oldPosCam.X < entity.Bounds.Position.X - _cameraPosition.X)
                    {
                        oldPosCam.X = entity.Bounds.Position.X - _cameraPosition.X;
                    }
                    
                }

            }
            
            _tiledMapRenderer.Update(gameTime);

            _collisionComponent.Update(gameTime);
            //follow look at
            _camera.LookAt(_bgPosition + _cameraPosition);
            _panelPos = new Vector2(_camera.Position.X + Window.ClientBounds.Width-540, _camera.Position.Y+Window.ClientBounds.Height-270);

            //update camera viewport
            cameraOne.Update(PlayerOnePos.Bounds.Position);
            cameraTwo.Update(PlayerTwoPos.Bounds.Position);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            var transformMatrix = _camera.GetViewMatrix();
            _tiledMapRenderer.Draw(transformMatrix);
            spriteBatch.Begin(transformMatrix: transformMatrix);

            foreach(IEntity entity in _entities)
            {
                entity.Draw(spriteBatch);
                if(entity.GetType().Name == "Player")
                {
                    PlayerOne _player1 = (PlayerOne)entity;
                    if (openConfig)
                    {
                        spriteBatch.Draw(block, _panelPos, null, Color.Brown, 0f, Vector2.Zero, new Vector2(5, 2.5f), SpriteEffects.None, 0);
                        spriteBatch.DrawString(font, "Player Pos = " + _player1.Bounds.Position, new Vector2(_panelPos.X + 10, _panelPos.Y + 10), Color.Black);
                        spriteBatch.DrawString(font, "Camera Pos = " + _camera.Position, new Vector2(_panelPos.X + 10, _panelPos.Y + 30), Color.Black);
                        spriteBatch.DrawString(font, "Camera move = " + _cameraPosition, new Vector2(_panelPos.X + 10, _panelPos.Y + 50), Color.Black);
                        spriteBatch.DrawString(font, "DPos X = " + (_player1.Bounds.Position.X - _cameraPosition.X), new Vector2(_panelPos.X + 10, _panelPos.Y + 70), Color.Black);
                        spriteBatch.DrawString(font, "DPos Y = " + (_player1.Bounds.Position.Y - _cameraPosition.Y), new Vector2(_panelPos.X + 10, _panelPos.Y + 90), Color.Black);
                        spriteBatch.DrawString(font, "Wall Check R= " + (_player1.wallCheckRight), new Vector2(_panelPos.X + 10, _panelPos.Y + 110), Color.Black);
                        spriteBatch.DrawString(font, "Wall Check L = " + (_player1.wallCheckLeft), new Vector2(_panelPos.X + 10, _panelPos.Y + 130), Color.Black);
                        spriteBatch.DrawString(font, "Climb = " + (_player1.onClimb), new Vector2(_panelPos.X + 10, _panelPos.Y + 150), Color.Black);
                        spriteBatch.DrawString(font, "CountG = " + (_player1.countG), new Vector2(_panelPos.X + 10, _panelPos.Y + 170), Color.Black);
                        spriteBatch.DrawString(font, "CountW = " + (_player1.countW), new Vector2(_panelPos.X + 10, _panelPos.Y + 190), Color.Black);
                    
                    }
                }
            }

            spriteBatch.End();

            GraphicsDevice.Viewport = leftView;
            DrawSprites(cameraOne);

            GraphicsDevice.Viewport = rightView;
            DrawSprites(cameraTwo);

            GraphicsDevice.Viewport = defaultView;

            base.Draw(gameTime);
        }

        public int GetMapWidth()
        {
            return MapWidth;
        }
        public int GetMapHeight()
        {
            return MapHeight;
        }

        #region Camera One
        public void UpdateCameraXOne(Vector2 move)
        {
            _cameraPosition += move;
        }

        public void UpdateCameraYOne(Vector2 move)
        {
            _cameraPosition -= move;
        }

        public float GetCameraPosXOne()
        {
            return _cameraPosition.X;
        }

        public float GetCameraPosYOne()
        {
            return _cameraPosition.Y;
        }
        #endregion

        #region Camera Two
        public void UpdateCameraXTwo(Vector2 move)
        {
            _cameraPosition += move;
        }

        public void UpdateCameraYTwo(Vector2 move)
        {
            _cameraPosition -= move;
        }

        public float GetCameraPosXTwo()
        {
            return _cameraPosition.X;
        }

        public float GetCameraPosYTwo()
        {
            return _cameraPosition.Y;
        }
        #endregion

        void DrawSprites(Camera camera)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null, null, null, null,
                camera.Tranform);
            PlayerOnePos.Draw(spriteBatch);
            PlayerTwoPos.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
