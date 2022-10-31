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
    public class Player : IEntity
    {
        private readonly Game1 _game;

        public int Velocity = 4;
        Vector2 move;
        Vector2 playerPos;
        public IShapeF Bounds { get; }

        private KeyboardState _currentKey;
        private KeyboardState _oldKey;

        bool isJumping;
        bool isGrounded;
        public int jumpSpeed;
        float force;

        public Player(Game1 game,IShapeF circleF)
        {
            _game = game;
            Bounds = circleF;

            jumpSpeed = -6;
            force = 20;
            isGrounded = false;
        }

        public virtual void Update(GameTime gameTime)
        {
            _currentKey = Keyboard.GetState();
            
            if(_currentKey.IsKeyDown(Keys.D) && Bounds.Position.X < _game.GetMapWidth() - ((RectangleF)Bounds).Width)
            {
                move = new Vector2(Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                if(Bounds.Position.X - _game.GetCameraPosX() >= 400 
                    && _game.GetCameraPosX() < _game.GetMapWidth() - 1728)
                {
                    _game.UpdateCamera(move);
                }
                Bounds.Position += move;
            }
            else if(_currentKey.IsKeyDown(Keys.A) && Bounds.Position.X > 0)
            {
                move = new Vector2(-Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                if(Bounds.Position.X - _game.GetCameraPosX() <= 300
                    && _game.GetCameraPosX()> 0)
                {
                    _game.UpdateCamera(move);
                }
                Bounds.Position += move;
            }

            if(isJumping && force < 0)
            {
                isJumping = false;
            }

            if(_currentKey.IsKeyDown(Keys.W) && _oldKey.IsKeyUp(Keys.W) && isGrounded)
            {
                isJumping = true;
                isGrounded = false;
            }

            if (isJumping)
            {
                jumpSpeed = -12;
             
                force -= gameTime.GetElapsedSeconds() * 32;
                
            }
            else
            {
                jumpSpeed = 8; 
            }

            Bounds.Position += new Vector2(0, jumpSpeed) * gameTime.GetElapsedSeconds() * 50;
            
            
            _oldKey = _currentKey;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3f);
        }
       
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other.ToString().Contains("PlatForm"))
            {
                if (!isJumping)
                {
                    if(((RectangleF)Bounds).Top<((RectangleF)collisionInfo.Other.Bounds).Top &&
                        ((RectangleF)Bounds).Bottom < ((RectangleF)collisionInfo.Other.Bounds).Bottom)
                    {
                        isGrounded = true;
                        force = 10;
                    }
                    Bounds.Position -= collisionInfo.PenetrationVector;
                }
            }
        }



    }
}
