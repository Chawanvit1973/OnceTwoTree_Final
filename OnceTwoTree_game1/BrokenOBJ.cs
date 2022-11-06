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
    public class BrokenOBJ: IEntity
    {
        private readonly Game1 _game;
        public IShapeF Bounds { get; }

        public BrokenOBJ(Game1 game, RectangleF rectangleF)
        {
            _game = game;
            Bounds = rectangleF;
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Purple, 3);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {

        }
    }
}
