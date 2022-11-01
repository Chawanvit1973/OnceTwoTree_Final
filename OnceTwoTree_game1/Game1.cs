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

        private const int MapWidth = 4320;
        private const int MapHeight= 2160;

        public static OrthographicCamera _camera;
        public static Vector2 _cameraPosition;
        public static Vector2 _bgPosition;

        private readonly List<IEntity> _entities = new List<IEntity>();
        public readonly CollisionComponent _collisionComponent;
        public SpriteFont font;
        Texture2D block; 

        TiledMap _tiledMap;
        TiledMapRenderer _tiledMapRenderer;

        TiledMapObjectLayer _platformTiledObj;
        TiledMapObjectLayer _climbObj;

        bool openConfig = false;
        bool isKeyDown = false;
        
        Vector2 oldPosCam;
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
            _bgPosition = new Vector2(1728/2, 972/2);
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
                if(layer.Name == "ClimbObj")
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
            _entities.Add(new Player(this, new RectangleF(new Point2(0, MapHeight-108), new Size2(108, 108))));
            

            foreach(IEntity entity in _entities)
            {
                _collisionComponent.Insert(entity);
            }
            spriteBatch = new SpriteBatch(GraphicsDevice);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            if(!isKeyDown && Keyboard.GetState().IsKeyDown(Keys.NumPad1))
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
            //Start look at
            _camera.LookAt(_bgPosition + _cameraPosition);
            
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
                    Player _player1 = (Player)entity;
                    if (openConfig)
                    {
                        spriteBatch.Draw(block, new Vector2(_cameraPosition.X, _cameraPosition.Y), null, Color.Brown, 0f, Vector2.Zero, new Vector2(4, 2), SpriteEffects.None, 0);
                        spriteBatch.DrawString(font, "Player Pos = " + _player1.Bounds.Position, new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 10), Color.Black);
                        spriteBatch.DrawString(font, "Camera Pos = " + _camera.Position, new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 30), Color.Black);
                        spriteBatch.DrawString(font, "DPos X = " + (_player1.Bounds.Position.X - _cameraPosition.X), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 50), Color.Black);
                        spriteBatch.DrawString(font, "DPos Y = " + (_player1.Bounds.Position.Y - _cameraPosition.Y), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 70), Color.Black);
                        //spriteBatch.DrawString(font, "MapWidth = " + (GetMapWidth()), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 90), Color.Black);
                        //spriteBatch.DrawString(font, "MaxHight = " + (oldPosCam.Y), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 110), Color.Black);
                        //spriteBatch.DrawString(font, "MaxWidth = " + (oldPosCam.X), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 130), Color.Black);
                        spriteBatch.DrawString(font, "IsGround = " + (_player1.isGrounded), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 90), Color.Black);
                        spriteBatch.DrawString(font, "Wall Check = " + (_player1.wallCheck), new Vector2(_cameraPosition.X + 10, _cameraPosition.Y + 110), Color.Black);
                    
                    }
                }
                

            }

            

            spriteBatch.End();
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

        public void UpdateCameraX(Vector2 move)
        {
            _cameraPosition += move;
        }

        public void UpdateCameraY(Vector2 move)
        {
            _cameraPosition -= move;
        }

        public float GetCameraPosX()
        {
            return _cameraPosition.X;
        }

        public float GetCameraPosY()
        {
            return _cameraPosition.Y;
        }
    }
}
