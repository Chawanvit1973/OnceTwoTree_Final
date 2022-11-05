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
    public abstract class Player : IEntity
    {

        internal HookBox myhook;

        public abstract IShapeF Bounds { get; }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void OnCollision(CollisionEventArgs collisionInfo);
        public abstract void Update(GameTime gameTime);
    }

    internal class HookBox : IEntity
    {

        private readonly Game1 _game;

        public IShapeF Bounds { get; }

        Texture2D _RopeTexture; // รอimportรูปเชือก
        Texture2D _RopeTexture_Test;

        bool _ishooking = false;
        public double _myTime = 0.0;

        Player myPlayer;

        static List<Player> _PlayerControl = new List<Player>();
        public static List<HookBox> _Hookboxes = new List<HookBox>();



        internal HookBox(Game1 game, IShapeF myShape)
        {
            this._game = game;
            Bounds = myShape;

            //สำหรับ Test
            _RopeTexture_Test = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _RopeTexture_Test.SetData(new[] { Color.Aqua });

            //สำหรับ Load Texture เชือก
            _RopeTexture = _game.Content.Load<Texture2D>("Resources\\UI\\UI_Rope");

        }


        public static HookBox LoadHookToPlayer(Player _mPlayer, Game1 game)
        {
            _PlayerControl.Add(_mPlayer);
            _Hookboxes.Add(new HookBox(game, new RectangleF(Point2.Zero, new Size2(54, 54))));
            return _Hookboxes[_Hookboxes.Count - 1];
        }

        public enum throwing_state { throwing, startthrow, finalthrow, idle }

        public throwing_state throw_state = throwing_state.idle;

        KeyboardState _oldkey;
        public virtual void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            #region ThrowingPlayer1
            if ((keyboard.IsKeyDown(Keys.E) && (_Hookboxes[0].throw_state == throwing_state.idle)) || _Hookboxes[0].throw_state == throwing_state.startthrow) // สำหรับ player 1 แม้จะมี player แค่คนเดียว
            {
                if (_Hookboxes[0].throw_state != throwing_state.throwing)
                {
                    //อาจจะ set Animate ไว้ตรงนี้ก็ได้
                }

                if (_oldkey.IsKeyUp(Keys.E) && (_Hookboxes[0].throw_state == throwing_state.idle))
                {
                    myPlayer = _PlayerControl[0];
                    _Hookboxes[0].throw_state = throwing_state.startthrow;
                    _Hookboxes[0].ThrowHook(gameTime);
                }

                else if (_Hookboxes[0].throw_state == throwing_state.startthrow)
                {
                    _Hookboxes[0].throw_state = throwing_state.throwing;
                    _Hookboxes[0].ThrowHook(gameTime);
                }

            }

            else if (_Hookboxes[0].throw_state == throwing_state.throwing)
            {
                _Hookboxes[0].ThrowHook(gameTime);
            }

            else if (_Hookboxes[0].throw_state == throwing_state.finalthrow)
            {
                _Hookboxes[0].ThrowHook(gameTime);
                throw_state = throwing_state.idle;
            }
            #endregion

            _oldkey = keyboard;
        }


        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (this.throw_state == throwing_state.idle)
            {

            }
            else
            {
                spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Yellow, 3f);

                double distancerope = Vector2.Distance(myPlayer.Bounds.Position, this.Bounds.Position);
                float ropeThickness = 10;
                float ropeJoint = 10;

                for (int i = 0; i <= (int)(distancerope - ropeJoint); i += (int)ropeJoint)
                {
                    Rectangle Start = new Rectangle((int)(myPlayer.Bounds.Position.X + ((108 + ropeJoint) * 0.5)), (int)(myPlayer.Bounds.Position.Y + (i * Math.Sin(-Math.PI / 2.0)) /*+ ((168 + ropeThickness) * 0.5)*/), (int)ropeJoint, (int)ropeThickness);
                    spriteBatch.Draw(this._RopeTexture, Start, null, Color.White, (float)(Math.PI / 2.0), new Vector2(0, ropeThickness * 0.5f), 0, 0);
                }
            }
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {

        }

        double distance = 108 * 5;

        public void ThrowHook(GameTime _mTimimg)
        {
            if (throw_state == throwing_state.startthrow)
            {
                this.Bounds.Position = new Vector2(this.myPlayer.Bounds.Position.X, this.myPlayer.Bounds.Position.Y);
                _myTime += _mTimimg.GetElapsedSeconds();
            }

            if (throw_state == throwing_state.throwing)
            {
                double distance = 0, startValo = 0, time = _myTime, gavity = 3000;

                startValo = -Math.Sqrt(2 * gavity * this.distance);
                distance = (startValo * time) + ((0.5) * gavity * Math.Pow(time, 2));
                this.Bounds.Position = new Vector2(this.myPlayer.Bounds.Position.X, this.myPlayer.Bounds.Position.Y + (float)distance);

                if (distance > -10)
                {
                    throw_state = throwing_state.finalthrow;
                }

                _myTime += _mTimimg.GetElapsedSeconds();
            }

            if (throw_state == throwing_state.finalthrow)
            {
                _myTime = 0;
            }
        }

        public static void Update_Hook_Hitblock(CollisionComponent _myCollide)
        {
            foreach (HookBox x in _Hookboxes)
            {
                if (x.throw_state == throwing_state.startthrow)
                {
                    _myCollide.Insert(x);
                }
                else if (x.throw_state == throwing_state.throwing)
                {
                    _myCollide.Remove(x);
                    _myCollide.Insert(x);
                }
                else if (x.throw_state == throwing_state.finalthrow)
                {
                    _myCollide.Remove(x);
                }
                else if (x.throw_state == throwing_state.idle)
                {
                    _myCollide.Remove(x);
                }
            }
        }


    }
}
