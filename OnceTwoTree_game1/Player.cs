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
        
        public int Velocity = 10;
        Vector2 move;

        public IShapeF Bounds { get; }

        protected Texture2D playerTexture;
        protected Texture2D UI_stamina;
        protected Texture2D UI_bar;
        protected Texture2D UI_insidebar;
        protected Texture2D UI_hand;
        protected Texture2D UI_check;
        protected Texture2D UI_popup;

        private KeyboardState _currentKey;
        private KeyboardState _oldKey;
        //time Count
        float totalElapsed;
        float timePerFrame;

        public float energy;
        float totalTime;
        float maxTime;

        internal HookBox myhook;


        public int Gfroce = 10;

        public int timeCount = 0;
        public int countG = 0;
        public int countW = 0;
        public int countC = 0;
        
        public bool isFlip;
        public bool wallCheckLeft, wallCheckRight;
        public bool onClimb;
        public bool leftHand;
        public bool firstCheck = false;
        public int triggerSpeed = 10;
        int framePopup = 0;
        int frameInsideBar = 0;
        //Hand stage var 0 = แบมือ 1 = กำมือ
        int leftHandStage = 0;
        int rightHandStage = 0;
        //0 = can't press 1 = can press
        int leftHandOn = 0;
        int rightHandOn = 0;

        Rectangle triggerRec, barRec, dropRec, scLRec, scRRec;

        #region SkillCheck var
        public Vector2 barPos, leftHandPos, rightHandPos,triggerPos,insideBarPos,dropBarPos,checkBarPosL,checkBarPosR;
        public Vector2 dropBarScale, checkBarScaleL, checkBarScaleR;

        #endregion
        
        //public CollisionEventArgs _currentBlock;
        public Player(Game1 game,IShapeF circleF)
        {
            _game = game;
            Bounds = circleF;

            playerTexture = _game.Content.Load<Texture2D>("Resources\\Character\\S_player");
            UI_stamina = _game.Content.Load<Texture2D>("Resources\\UI\\UI_stamina");
            UI_bar = _game.Content.Load<Texture2D>("Resources\\UI\\UI_bar");
            UI_insidebar = _game.Content.Load<Texture2D>("Resources\\UI\\UI_insidebar");
            UI_hand = _game.Content.Load<Texture2D>("Resources\\UI\\UI_hand");
            UI_check = _game.Content.Load<Texture2D>("Resources\\UI\\UI_skillcheck_trigger");
            UI_popup = _game.Content.Load<Texture2D>("Resources\\UI\\UI_Popup");
            
            totalElapsed = 0;
            timePerFrame = 1;
            //energy config
            totalTime = 0;
            maxTime = 0.5f;
            energy = 100;

            wallCheckLeft = wallCheckRight = false;
            isFlip = false;
            leftHand = false;

            HookBox.LoadHookToPlayer(this, this._game);
           
        }

        
        public virtual void Update(GameTime gameTime)
        {
            PlayerControl(gameTime);

            //Rectangle
            triggerRec = new Rectangle((int)triggerPos.X, (int)triggerPos.Y, (int)(UI_check.Width / 3), (int)(UI_check.Height));
            barRec = new Rectangle((int)insideBarPos.X, (int)insideBarPos.Y, (int)(UI_insidebar.Width), (int)(UI_insidebar.Height/3));
            dropRec = new Rectangle((int)dropBarPos.X, (int)dropBarPos.Y, (int)(UI_check.Width / 3 * dropBarScale.X), (int)(UI_check.Height * dropBarScale.Y));
            scLRec = new Rectangle((int)checkBarPosL.X, (int)checkBarPosL.Y, (int)(UI_check.Width/3 * checkBarScaleL.X), (int)(UI_check.Height * checkBarScaleL.Y));
            scRRec = new Rectangle((int)checkBarPosR.X, (int)checkBarPosR.Y, (int)(UI_check.Width / 3 * checkBarScaleR.X), (int)(UI_check.Height * checkBarScaleR.Y));

            //CoreGame
            //SkillCheck(gameTime,barRec,triggerRec);

            //Gfroce
            if (onClimb == false)
            {
                Bounds.Position += new Vector2(0, (Gfroce * 2)) * gameTime.GetElapsedSeconds() * 50;

            }

            //Check out of wall
            if (wallCheckRight == false && wallCheckLeft == false)
            {
                onClimb = false;
            }

            //Time Count
            TimeCount((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (timeCount - countG >= 0)
            {
                if (wallCheckLeft) { wallCheckLeft = false; }
                if (wallCheckRight) { wallCheckRight = false; }

            }
            //Debug
            if (countW > 10) { countW = 0; }
            if (countC > 10) { countC = 0; }

            //hook
            myhook.Update(gameTime);
            
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!isFlip)
            {
                spriteBatch.Draw(playerTexture, Bounds.Position, new Rectangle(0, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

            }
            else if (isFlip)
            {
                spriteBatch.Draw(playerTexture, Bounds.Position, new Rectangle(0, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);

            }
            //Draw InteractPopUp
            if((wallCheckLeft || wallCheckRight) && !onClimb)
            {
                spriteBatch.Draw(UI_popup, new Vector2(((RectangleF)Bounds).Position.X+(((RectangleF)Bounds).Size.Width/2-UI_popup.Width/4), ((RectangleF)Bounds).Top - UI_popup.Height), 
                                           new Rectangle(framePopup*UI_popup.Width, 0, UI_popup.Width/2, UI_popup.Height), Color.White);
            }
            DrawSkilCheck(spriteBatch);
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3f);
        }
       
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            countC++;
            //Platform Collision
            if (collisionInfo.Other.ToString().Contains("PlatForm"))
            {
                if (!onClimb)
                {
                    countG = timeCount;
                    Bounds.Position -= collisionInfo.PenetrationVector;
                }

            }

            #region WallCollision
            if (collisionInfo.Other.ToString().Contains("ClimbOBJ"))
            {
                //Check wall on Right side of Character
                if (((RectangleF)Bounds).Right >= ((RectangleF)collisionInfo.Other.Bounds).Left &&
                    ((RectangleF)Bounds).Left < ((RectangleF)collisionInfo.Other.Bounds).Left &&
                    ((RectangleF)Bounds).Bottom - 10 >= ((RectangleF)collisionInfo.Other.Bounds).Top &&
                    ((RectangleF)Bounds).Bottom - 10 <= ((RectangleF)collisionInfo.Other.Bounds).Bottom)
                {
                    
                    wallCheckRight = true;
                    if (onClimb)
                    {
                        isFlip = false;
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
                else { wallCheckRight = false; }
                //Check wall on Left side of Character
                if (((RectangleF)Bounds).Left <= ((RectangleF)collisionInfo.Other.Bounds).Right &&
                    ((RectangleF)Bounds).Right > ((RectangleF)collisionInfo.Other.Bounds).Right &&
                    ((RectangleF)Bounds).Bottom - 10 >= ((RectangleF)collisionInfo.Other.Bounds).Top &&
                    ((RectangleF)Bounds).Bottom - 10d <= ((RectangleF)collisionInfo.Other.Bounds).Bottom)
                {
                    countW++;
                    wallCheckLeft = true;
                    if (onClimb)
                    {
                        isFlip = true;
                        Bounds.Position = new Vector2(((RectangleF)collisionInfo.Other.Bounds).Position.X + (((RectangleF)collisionInfo.Other.Bounds).Size.Width * 1 / 3), Bounds.Position.Y);
                        //End of Climbing
                        if (((RectangleF)Bounds).Top + (((RectangleF)Bounds).Size.Height * 3 / 4) < ((RectangleF)collisionInfo.Other.Bounds).Top)
                        {
                            Bounds.Position = new Vector2(((RectangleF)collisionInfo.Other.Bounds).Position.X + (((RectangleF)collisionInfo.Other.Bounds).Size.Width * 1 / 3),
                                                          ((RectangleF)collisionInfo.Other.Bounds).Top - ((RectangleF)Bounds).Size.Height);
                            onClimb = false;
                        }
                    }
                }
                else { wallCheckLeft = false; }
            }
            else { wallCheckRight = false; wallCheckLeft = false; } 
            #endregion

        }

        public void TimeCount(float elapsed)
        {
            totalElapsed += elapsed;
            if(totalElapsed > timePerFrame)
            {
                timeCount = timeCount + 1;
                totalElapsed -= timePerFrame;
            }

        }

        #region PlayerControl
        public void PlayerControl(GameTime gameTime)
        {
            _currentKey = Keyboard.GetState();

            //framePopup = (framePopup+1) % 2;
            #region A D (Left Right)
            if (onClimb == true)
            {
                if (_currentKey.IsKeyDown(Keys.A) && _oldKey.IsKeyUp(Keys.A))
                {
                    if (leftHand == true && triggerRec.Intersects(scLRec) && energy >0)
                    {
                        leftHand = false;
                        move = new Vector2(0, Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPosY() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCameraY(move);
                        }
                        Bounds.Position -= move;
                    }
                    if (triggerRec.Intersects(dropRec))
                    {
                        move = new Vector2(0, -Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPosY() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCameraY(move);
                        }
                        Bounds.Position -= move;
                    }
                }
                else if (_currentKey.IsKeyDown(Keys.D) && _oldKey.IsKeyUp(Keys.D))
                {
                    if (leftHand == false && triggerRec.Intersects(scRRec) && energy > 0)
                    {
                        leftHand = true;
                        move = new Vector2(0, Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPosY() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCameraY(move);
                        }
                        Bounds.Position -= move;
                    }
                    if (triggerRec.Intersects(dropRec))
                    {
                        move = new Vector2(0, -Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPosY() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCameraY(move);
                        }
                        Bounds.Position -= move;
                    }
                }
            }
            else if (onClimb == false)
            {
                if (_currentKey.IsKeyDown(Keys.D) && Bounds.Position.X < _game.GetMapWidth() - ((RectangleF)Bounds).Width)
                {
                    isFlip = false;
                    move = new Vector2(Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                    if (Bounds.Position.X - _game.GetCameraPosX() >= 400
                        && _game.GetCameraPosX() < _game.GetMapWidth() - 1728)
                    {
                        _game.UpdateCameraX(move);
                    }
                    Bounds.Position += move;
                }
                else if (_currentKey.IsKeyDown(Keys.A) && Bounds.Position.X > 0)
                {
                    isFlip = true;
                    move = new Vector2(-Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                    if (Bounds.Position.X - _game.GetCameraPosX() <= 300
                        && _game.GetCameraPosX() > 0)
                    {
                        _game.UpdateCameraX(move);
                    }
                    Bounds.Position += move;
                }
            }
            #endregion
            //For camera
            if (_currentKey.IsKeyDown(Keys.W) && onClimb == true)
            {
                move = new Vector2(0, 10) * gameTime.GetElapsedSeconds() * 60;
                if (_game.GetCameraPosY() - Bounds.Position.Y <= 864)
                {
                    _game.UpdateCameraY(move);
                }

            }
            else if (Bounds.Position.Y + 108 - _game.GetCameraPosY() >= (_game.GetMapHeight() - 216))
            {
                move = new Vector2(0, (-Gfroce * 2)) * gameTime.GetElapsedSeconds() * 50;
                _game.UpdateCameraY(move);
            }
            //For test
            if (_currentKey.IsKeyDown(Keys.W) && onClimb == true)
            {
                Bounds.Position -= new Vector2(0, 10);
            }

            if (_currentKey.IsKeyDown(Keys.W) && _oldKey.IsKeyUp(Keys.W) && (wallCheckRight == true || wallCheckLeft == true))
            {
                if (!onClimb) { onClimb = true; }
                else if (onClimb) { onClimb = false; }
            }



            if (_currentKey.IsKeyDown(Keys.D1))
            {
                Bounds.Position -= new Vector2(0, 20);
            }

            _oldKey = _currentKey;
        } 
        #endregion

        #region SkillCheck
        public void SetSkillCheckPos(Vector2 target)
        {
            //LeftHand
            leftHandPos = new Vector2(target.X + 9,target.Y +27);
            //Bar
            barPos = new Vector2(leftHandPos.X+(UI_hand.Width/2), leftHandPos.Y);
            //Righthand
            rightHandPos = new Vector2(barPos.X + UI_bar.Width, barPos.Y);
            //InsideBar
            insideBarPos = new Vector2(barPos.X + 18, barPos.Y + 18);
            //DropCheck
            dropBarScale = new Vector2(2, 1);
            dropBarPos = new Vector2(insideBarPos.X+UI_insidebar.Width/2-(((UI_check.Width/3)*dropBarScale.X)/2), insideBarPos.Y + 4.6f);
            //LeftCheck
            checkBarScaleL = new Vector2(2, 1);
            checkBarPosL = new Vector2(insideBarPos.X +4.6f, insideBarPos.Y + 4.6f);
            //RightChek
            checkBarScaleR = new Vector2(2, 1);
            checkBarPosR = new Vector2(insideBarPos.X + UI_insidebar.Width - (UI_check.Width / 3 * checkBarScaleR.X)  - 4.6f, insideBarPos.Y + 4.6f);
            //Trigger
            //triggerPos = new Vector2(insideBarPos.X + UI_insidebar.Width / 2 - (((UI_check.Width / 3)/ 2), insideBarPos.Y + 4.6f);
        }

        public void DrawSkilCheck(SpriteBatch spriteBatch)
        {
            //LeftHand
            spriteBatch.Draw(UI_hand, leftHandPos, new Rectangle(UI_hand.Width/2 * leftHandOn, UI_hand.Height/2 * leftHandStage, UI_hand.Width/2, UI_hand.Height/2), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
            //RightHand
            spriteBatch.Draw(UI_hand, rightHandPos, new Rectangle(UI_hand.Width / 2 * rightHandOn, UI_hand.Height / 2 * rightHandStage, UI_hand.Width / 2, UI_hand.Height / 2), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            //Bar
            spriteBatch.Draw(UI_bar, barPos, new Rectangle(0, 0, UI_bar.Width, UI_bar.Height / 2), Color.White);
            if (onClimb)
            {
                //Check left
                spriteBatch.Draw(UI_check, checkBarPosL, new Rectangle(UI_check.Width * 1 / 3, 0, UI_check.Width / 3, UI_check.Height), Color.White, 0, Vector2.Zero, checkBarScaleL, SpriteEffects.None, 0);
                //Check Right
                spriteBatch.Draw(UI_check, checkBarPosR, new Rectangle(UI_check.Width * 1 / 3, 0, UI_check.Width / 3, UI_check.Height), Color.White, 0, Vector2.Zero, checkBarScaleR, SpriteEffects.None, 0);
                //Check Drop
                spriteBatch.Draw(UI_check, dropBarPos, new Rectangle(UI_check.Width * 0 / 3, 0, UI_check.Width / 3, UI_check.Height), Color.White, 0, Vector2.Zero, dropBarScale, SpriteEffects.None, 0);
                //Trigger
                spriteBatch.Draw(UI_check, triggerPos, new Rectangle(UI_check.Width * 2 / 3, 0, UI_check.Width / 3, UI_check.Height), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
            }
            //Web Status
            //spriteBatch.Draw(UI_bar, barPos, new Rectangle(0, UI_bar.Height/2, UI_bar.Width, UI_bar.Height / 2), Color.White);
            //Inside
            //spriteBatch.Draw(UI_insidebar, insideBarPos, new Rectangle(0, UI_insidebar.Height * frameInsideBar / 3, UI_insidebar.Width, UI_insidebar.Height / 3), Color.White);

        }

        public void SkillCheck(GameTime gameTime)
        {
            if (onClimb)
            {
                countW++;
                triggerPos.Y = insideBarPos.Y + 4.6f;

                if ((triggerPos.X < insideBarPos.X || triggerPos.X > insideBarPos.X + UI_insidebar.Width) && firstCheck == false)
                {
                    triggerPos.X = insideBarPos.X + UI_insidebar.Width / 2 - (((UI_check.Width / 3) / 2));
                    firstCheck = true;
                }
                else
                {
                    triggerPos.X += triggerSpeed * gameTime.GetElapsedSeconds() * 50;
                }

                if (triggerRec.Intersects(barRec))
                {
                    if (triggerRec.Left + triggerSpeed <= barRec.Left ||
                        triggerRec.Right + triggerSpeed >= barRec.Right)
                    {
                        triggerSpeed *= -1;
                    }

                }
                //Hand Stage Set
                if (leftHand == true)
                {
                    leftHandStage = 0;
                    rightHandStage = 1;
                }
                else if (leftHand == false)
                {
                    leftHandStage = 1;
                    rightHandStage = 0;
                }
                
                //Hand ON Set
                if( leftHand == true && triggerRec.Intersects(scLRec))
                {
                    leftHandOn = 1;
                }
                else if (leftHand == false && triggerRec.Intersects(scRRec))
                {
                    rightHandOn = 1;
                }
                else
                {
                    rightHandOn = 0;
                    leftHandOn = 0;
                }
            }
            else if (!onClimb)
            {
                firstCheck = false;
                leftHandStage = 0;
                rightHandStage = 0;
                leftHandOn = 0;
                rightHandOn = 0;
            }
   
        }
        public void Energy(float time)
        {
            totalTime += time;
            if (totalTime > maxTime)
            {
                energy = energy + 1;
                totalTime -= maxTime;
            }

            if (energy > 100)
            {
                energy--;
            }
            else if (energy <= 0)
            {
                energy = 0;
            }
            else if (energy >= 50)
            {
                maxTime = 0.5f;
            }
            else if (energy < 50)
            {
                maxTime = 0.1f;
            }

        }
        #endregion
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

        CollisionComponent _myBox;


        internal HookBox(Game1 game, IShapeF myShape)
        {
            this._game = game;
            Bounds = myShape;

            //สำหรับ Test
            _RopeTexture_Test = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _RopeTexture_Test.SetData(new[] { Color.Aqua });

            //สำหรับ Load Texture เชือก
            //_RopeTexture.Load();

        }


        public static void LoadHookToPlayer(Player _mPlayer, Game1 game)
        {
            _PlayerControl.Add(_mPlayer);
            _Hookboxes.Add(new HookBox(game, new RectangleF(Point2.Zero, new Size2(108, 108))));
            _mPlayer.myhook = _Hookboxes[_Hookboxes.Count-1];
        }

        enum throwing_state { throwing, startthrow, finalthrow, idle }

        throwing_state throw_state = throwing_state.idle;

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

            }
            #endregion
                        
            _oldkey = keyboard;
        }
                

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3f);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {

        }


        public void ThrowHook(GameTime _mTimimg)
        {
            if (throw_state == throwing_state.startthrow)
            {
                this.Bounds.Position = new Vector2(this.myPlayer.Bounds.Position.X, this.myPlayer.Bounds.Position.Y);
                _myTime += _mTimimg.GetElapsedSeconds();
            }

            if (throw_state == throwing_state.throwing)
            {
                _myTime += _mTimimg.GetElapsedSeconds();
            }

            if (throw_state == throwing_state.finalthrow)
            {

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

                }
            }
        }


    }
}
