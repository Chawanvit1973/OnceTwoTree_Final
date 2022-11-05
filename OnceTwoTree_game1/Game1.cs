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

        private const int MapWidth = 4536;
        private const int MapHeight = 4320;

        public static OrthographicCamera _camera;
        public static Vector2 _cameraPosition;
        public static Vector2 _bgPosition;
        public Vector2 _panelPos;

        private readonly List<IEntity> _entities = new List<IEntity>();
        public readonly CollisionComponent _collisionComponent;
        public SpriteFont font;

        Texture2D block;
        Texture2D menu;

        TiledMap _tiledMap;
        TiledMapRenderer _tiledMapRenderer;

        TiledMapObjectLayer _platformTiledObj;
        TiledMapObjectLayer _climbObj;

        bool openConfig = true;
        bool isKeyDown = false;

        Vector2 oldPosCam;

        Player playerInstance;
        
        bool isGameplay;
        bool isMenu;
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
            }
            //Get Climb Obj
            foreach (TiledMapObjectLayer layer in _tiledMap.ObjectLayers)
            {
                if (layer.Name == "ClimbObject")
                {
                    _climbObj = layer;
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
            
            #endregion

            //Setup Player
            _entities.Add(new Player(this, new RectangleF(new Point2(276, MapHeight - 600), new Size2(108, 138))));

            //Start look at
            foreach (IEntity entity in _entities)
            {
                _collisionComponent.Insert(entity);
                if(entity.GetType().Name == "Player")
                {
                    playerInstance = (Player)entity;
                }
            }
            
            //_camera.LookAt(PlayerPos.Bounds.Position + _bgPosition);
            //_camera.LookAt(_bgPosition + _camera.Position);

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

            if(isMenu == true)
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

        public float GetCameraX()
        {
            return _camera.Position.X;
        }
        public float GetCameraY()
        {
            return _camera.Position.Y;
        }

        public Vector2 GetCamera()
        {
            return _camera.Position;
        }
        #endregion

        public void InGameUpdate(GameTime gameTime)
        {
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
            else if (Keyboard.GetState().IsKeyUp(Keys.NumPad1) && isKeyDown) { isKeyDown = false; }

            foreach (IEntity entity in _entities)
            {
                entity.Update(gameTime);
            }

            _tiledMapRenderer.Update(gameTime);

            _collisionComponent.Update(gameTime);
            //follow look at

            _camera.LookAt(_bgPosition + _cameraPosition);
            playerInstance.SetSkillCheckPos(_camera.Position);
            playerInstance.SkillCheck(gameTime);
            _panelPos = new Vector2(_camera.Position.X + Window.ClientBounds.Width - 540, _camera.Position.Y + Window.ClientBounds.Height - 324);

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
        }
        public void UpdateGameplay()
        {
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                isMenu = true;
                isGameplay = false;
            }
        }
        public void DrawMenu()
        {
            spriteBatch.Begin();

            spriteBatch.Draw(menu, Vector2.Zero, new Rectangle(0, 0, menu.Width / 2, menu.Height), Color.White);

            spriteBatch.End();
        }
        public void DrawGameplay()
        {
            var transformMatrix = _camera.GetViewMatrix();
            _tiledMapRenderer.Draw(transformMatrix);
            spriteBatch.Begin(transformMatrix: transformMatrix);

            foreach (IEntity entity in _entities)
            {
                entity.Draw(spriteBatch);
                if (entity.GetType().Name == "Player")
                {
                    
                    if (openConfig)
                    {
                        spriteBatch.Draw(block, _panelPos, null, Color.Brown, 0f, Vector2.Zero, new Vector2(5, 3f), SpriteEffects.None, 0);
                        spriteBatch.DrawString(font, "Player Pos = " + playerInstance.Bounds.Position, new Vector2(_panelPos.X + 10, _panelPos.Y + 10), Color.Black);
                        spriteBatch.DrawString(font, "Camera.Pos = " + _camera.Position, new Vector2(_panelPos.X + 10, _panelPos.Y + 30), Color.Black);
                        spriteBatch.DrawString(font, "CameraPosition = " + _cameraPosition, new Vector2(_panelPos.X + 10, _panelPos.Y + 50), Color.Black);
                        spriteBatch.DrawString(font, "DPos X = " + (playerInstance.Bounds.Position.X - _cameraPosition.X), new Vector2(_panelPos.X + 10, _panelPos.Y + 70), Color.Black);
                        spriteBatch.DrawString(font, "DPos Y = " + (playerInstance.Bounds.Position.Y - _cameraPosition.Y), new Vector2(_panelPos.X + 10, _panelPos.Y + 90), Color.Black);
                        spriteBatch.DrawString(font, "Wall Check R= " + (playerInstance.wallCheckRight), new Vector2(_panelPos.X + 10, _panelPos.Y + 110), Color.Black);
                        spriteBatch.DrawString(font, "Wall Check L = " + (playerInstance.wallCheckLeft), new Vector2(_panelPos.X + 10, _panelPos.Y + 130), Color.Black);
                        spriteBatch.DrawString(font, "Climb = " + (playerInstance.onClimb), new Vector2(_panelPos.X + 10, _panelPos.Y + 150), Color.Black);
                        spriteBatch.DrawString(font, "CountG = " + (playerInstance.countG), new Vector2(_panelPos.X + 10, _panelPos.Y + 170), Color.Black);
                        spriteBatch.DrawString(font, "CountW = " + (playerInstance.countW), new Vector2(_panelPos.X + 10, _panelPos.Y + 190), Color.Black);
                        spriteBatch.DrawString(font, "CountC = " + (playerInstance.countC), new Vector2(_panelPos.X + 10, _panelPos.Y + 210), Color.Black);
                        spriteBatch.DrawString(font, "oldCount = " + (playerInstance.timeCount), new Vector2(_panelPos.X + 10, _panelPos.Y + 230), Color.Black);
                        spriteBatch.DrawString(font, "DCount = " + (playerInstance.timeCount-playerInstance.countG), new Vector2(_panelPos.X + 10, _panelPos.Y + 250), Color.Black);
                        spriteBatch.DrawString(font, "FirstCheck = " + (playerInstance.firstCheck), new Vector2(_panelPos.X + 10, _panelPos.Y + 270), Color.Black);
                        spriteBatch.DrawString(font, "Energy = " + (playerInstance.energy), new Vector2(_panelPos.X + 10, _panelPos.Y + 290), Color.Black);
                        spriteBatch.DrawString(font, "Throwstate = " + (_entities.Count), new Vector2(_panelPos.X + 10, _panelPos.Y + 310), Color.Black);
                    
                    }
                }
            }
            spriteBatch.End();
        } 
        #endregion
    }
}
