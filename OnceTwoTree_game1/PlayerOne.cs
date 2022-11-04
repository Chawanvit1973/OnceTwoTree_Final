using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace OnceTwoTree_game1
{
    public class PlayerOne : IEntity
    {
        private readonly Game1 _game;
        
        public int Velocity = 10;
        public Vector2 move;
        public Vector2 positionOne;

        public IShapeF Bounds { get; }

        protected Texture2D player1Texture;
        protected Texture2D UI_skillcheck;
        protected Texture2D UI_staminabar;

        private KeyboardState _currentKey;
        private KeyboardState _oldKey;

        public int Gfroce = 10;
        public int countG = 0;
        public int countW = 0;
        public bool isFlip;
        public bool wallCheckLeft, wallCheckRight;
        public bool onClimb;

        #region SkillCheck var
        public Vector2 barPos, leftHandPos, rightHandPos,triggerPos;

        Viewport defaultView;
        Viewport leftView;
        Viewport rightView;

        Camera cameraOne;
        Camera cameraTwo;

        private SpriteBatch spriteBatch;

        #endregion
        //public CollisionEventArgs _currentBlock;
        public PlayerOne(Game1 game,IShapeF circleF)
        {
            _game = game;
            Bounds = circleF;

            player1Texture = _game.Content.Load<Texture2D>("Resources\\Character\\S_player");
            UI_skillcheck = _game.Content.Load<Texture2D>("Resources\\UI\\UI_skillcheck");
            UI_staminabar = _game.Content.Load<Texture2D>("Resources\\UI\\UI_stamina");

            wallCheckLeft = wallCheckRight = false;
            isFlip = false;

            defaultView = GraphicsDevice.Viewport;
            leftView = defaultView;
            rightView = defaultView;
            leftView.Width = leftView.Width / 2;
            rightView.Width = rightView.Width / 2;
            rightView.X = leftView.Width;

            cameraOne = new Camera();
            cameraTwo = new Camera();
        }

        
        public virtual void Update(GameTime gameTime)
        {
            _currentKey = Keyboard.GetState();

            #region A D (Left Right)
            /*if (_currentKey.IsKeyDown(Keys.D) && Bounds.Position.X < _game.GetMapWidth() - ((RectangleF)Bounds).Width)
            {
                isFlip = false;
                move = new Vector2(Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                if (Bounds.Position.X - _game.GetCameraPosXOne() >= 400
                    && _game.GetCameraPosXOne() < _game.GetMapWidth() - 1728)
                {
                    _game.UpdateCameraXOne(move);
                }
                Bounds.Position += move;
            }
            else if (_currentKey.IsKeyDown(Keys.A) && Bounds.Position.X > 0)
            {
                isFlip = true;
                move = new Vector2(-Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                if (Bounds.Position.X - _game.GetCameraPosXOne() <= 300
                    && _game.GetCameraPosXOne() > 0)
                {
                    _game.UpdateCameraXOne(move);
                }
                Bounds.Position += move;
            }*/
            #endregion

            if (_currentKey.IsKeyDown(Keys.D))
            {
                isFlip = false;
                move = new Vector2(Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                /*if (Bounds.Position.X - _game.GetCameraPosXOne() >= 400
                    && _game.GetCameraPosXTwo() < _game.GetMapWidth() - 1728)
                {
                    _game.UpdateCameraXTwo(move);
                }*/
                Bounds.Position += move;
            }
            else if (_currentKey.IsKeyDown(Keys.A))
            {
                isFlip = true;
                move = new Vector2(-Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                /*if (Bounds.Position.X - _game.GetCameraPosXTwo() <= 300
                    && _game.GetCameraPosXTwo() > 0)
                {
                    _game.UpdateCameraXTwo(move);
                }*/
                Bounds.Position += move;
            }

            if (_currentKey.IsKeyDown(Keys.W)  && onClimb == true)
            {
                move = new Vector2(0, 10) * gameTime.GetElapsedSeconds() * 60;
                if (_game.GetCameraPosYOne() - Bounds.Position.Y <= 864)
                {
                    _game.UpdateCameraYOne(move);
                }

            }
            else if (Bounds.Position.Y + 108 - _game.GetCameraPosYOne() >= (_game.GetMapHeight()-216))
            {
                move = new Vector2(0, (-Gfroce * 2)) * gameTime.GetElapsedSeconds() * 50;
                _game.UpdateCameraYOne(move);
            }

            if (_currentKey.IsKeyDown(Keys.W) && onClimb == true)
            {
                Bounds.Position -= new Vector2(0, 10);
            }

            if (_currentKey.IsKeyDown(Keys.W) && _oldKey.IsKeyUp(Keys.W) && (wallCheckRight == true || wallCheckLeft == true))
            {
                if (!onClimb) { onClimb = true; }
                else if (onClimb) { onClimb = false; }
            }

            if (wallCheckRight == false && wallCheckLeft == false)
            {
                onClimb = false;
            }

            /*if (_currentKey.IsKeyDown(Keys.D1))
            {
                Bounds.Position -= new Vector2(0,10);
            }*/

            //Gfroce
            if (!onClimb)
            {
                Bounds.Position += new Vector2(0, (Gfroce * 2)) * gameTime.GetElapsedSeconds() * 50;
            }

            if (countG > 10) { countG = 0; }
            if (countW > 10) { countW = 0; }
            _oldKey = _currentKey;

            positionOne = Bounds.Position;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!isFlip)
            {
                spriteBatch.Draw(player1Texture, Bounds.Position, new Rectangle(0, 3, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            }
            else if (isFlip)
            {
                spriteBatch.Draw(player1Texture, Bounds.Position, new Rectangle(0, 3, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);

            }
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3f);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            //Platform Collision
            if (collisionInfo.Other.ToString().Contains("PlatForm"))
            {
                if (!onClimb)
                {
                    countG++;
                    Bounds.Position -= collisionInfo.PenetrationVector;
                }

            }
            //Wall Collision
            if (collisionInfo.Other.ToString().Contains("ClimbOBJ"))
            {
                //Check wall on Right side of Character
                if (((RectangleF)Bounds).Right >= ((RectangleF)collisionInfo.Other.Bounds).Left &&
                    ((RectangleF)Bounds).Left < ((RectangleF)collisionInfo.Other.Bounds).Left &&
                    ((RectangleF)Bounds).Bottom > ((RectangleF)collisionInfo.Other.Bounds).Top)
                {
                    countW++;
                    wallCheckRight = true;
                    if (onClimb)
                    {
                        Bounds.Position = new Vector2(((RectangleF)collisionInfo.Other.Bounds).Position.X - ((RectangleF)collisionInfo.Other.Bounds).Size.Width - 15, Bounds.Position.Y);
                        //End of Climbing
                        if (((RectangleF)Bounds).Top + (((RectangleF)Bounds).Size.Height * 3 / 4) < ((RectangleF)collisionInfo.Other.Bounds).Top)
                        {
                            Bounds.Position = new Vector2(((RectangleF)collisionInfo.Other.Bounds).Position.X - ((RectangleF)collisionInfo.Other.Bounds).Size.Width,
                                                          ((RectangleF)collisionInfo.Other.Bounds).Top - ((RectangleF)Bounds).Size.Height);
                            onClimb = false;
                        }
                    }
                }
                else { wallCheckRight = false;}
                //Check wall on Left side of Character
                if (((RectangleF)Bounds).Left <= ((RectangleF)collisionInfo.Other.Bounds).Right &&
                    ((RectangleF)Bounds).Right > ((RectangleF)collisionInfo.Other.Bounds).Right &&
                    ((RectangleF)Bounds).Bottom > ((RectangleF)collisionInfo.Other.Bounds).Top)
                {
                    countW++;
                    wallCheckLeft = true;
                    if (onClimb)
                    {
                        Bounds.Position = new Vector2(((RectangleF)collisionInfo.Other.Bounds).Position.X + (((RectangleF)collisionInfo.Other.Bounds).Size.Width*1/3), Bounds.Position.Y);
                        //End of Climbing
                        if(((RectangleF)Bounds).Top + (((RectangleF)Bounds).Size.Height * 3 / 4) < ((RectangleF)collisionInfo.Other.Bounds).Top)
                        {
                            Bounds.Position = new Vector2(((RectangleF)collisionInfo.Other.Bounds).Position.X + (((RectangleF)collisionInfo.Other.Bounds).Size.Width * 1 / 3),
                                                          ((RectangleF)collisionInfo.Other.Bounds).Top - ((RectangleF)Bounds).Size.Height);
                            onClimb = false;
                        }
                    }
                }
                else { wallCheckLeft = false;}
            }
            else { wallCheckRight = false; wallCheckLeft = false;  }
        }


        public void SetSkillcheckUI()
        {

        }
        public void SkillCheck()
        {

        }
    }
}
