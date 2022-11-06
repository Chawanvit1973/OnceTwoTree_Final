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
    public class PlayerOne : Player 
    {
        private readonly Game1 _game;

        public int Velocity = 10;
        Vector2 move;

        public override IShapeF Bounds { get; }

        protected Texture2D P_idle,P_climb,P_walk,P_fall;
        protected Texture2D UI_stamina;
        protected Texture2D UI_bar;
        protected Texture2D UI_insidebar;
        protected Texture2D UI_hand;
        protected Texture2D UI_check;
        protected Texture2D UI_popup;
        protected Texture2D UI_energy;
        protected Texture2D UI_half;

        private KeyboardState _currentKey;
        private KeyboardState _oldKey;
        //time Count
        float totalElapsed;
        float timePerFrame;
        #region Player Animate var
        public float p_timePerframe;
        float p_totalElapsed;
        int p_frame;
        public int p_stateNum;
        int p_climbFrame;
        #endregion
        #region StaminaVar
        float tiredZone;
        float staminaCost;
        public float energy;
        public float maxEnergy;
        public float energyBar;
        public float maxEnergyBar;
        float totalTime;
        float maxTime;
        #endregion


        internal HookBox myhook;


        public int Gfroce = 10;

        public float timeCount = 0;
        public float countG = 0;
        public int countW = 0;
        public int countC = 0;

        public bool onAir;
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

        #region SkillCheck var & Staminabar
        public Vector2 barPos, leftHandPos, rightHandPos,triggerPos,insideBarPos,dropBarPos,checkBarPosL,checkBarPosR, halfPos;
        public Vector2 dropBarScale, checkBarScaleL, checkBarScaleR;
        public Vector2 staminaPos,energyPos;
        #endregion
        
        //public CollisionEventArgs _currentBlock;
        public PlayerOne(Game1 game,IShapeF circleF)
        {
            _game = game;
            Bounds = circleF;

            P_idle = _game.Content.Load<Texture2D>("Resources\\Character\\P_idle");
            P_climb = _game.Content.Load<Texture2D>("Resources\\Character\\P_climb");
            P_fall = _game.Content.Load<Texture2D>("Resources\\Character\\P_fall");
            P_walk = _game.Content.Load<Texture2D>("Resources\\Character\\P_walk");

            UI_stamina = _game.Content.Load<Texture2D>("Resources\\UI\\UI_stamina");
            UI_bar = _game.Content.Load<Texture2D>("Resources\\UI\\UI_bar");
            UI_insidebar = _game.Content.Load<Texture2D>("Resources\\UI\\UI_insidebar");
            UI_hand = _game.Content.Load<Texture2D>("Resources\\UI\\UI_hand");
            UI_check = _game.Content.Load<Texture2D>("Resources\\UI\\UI_skillcheck_trigger");
            UI_popup = _game.Content.Load<Texture2D>("Resources\\UI\\UI_Popup");
            UI_energy = _game.Content.Load<Texture2D>("Resources\\UI\\UI_energy");
            UI_half = _game.Content.Load<Texture2D>("Resources\\UI\\UI_separateScene");

            totalElapsed = 0;
            timePerFrame = 0.025f;
            //energy config
            totalTime = 0;
            maxTime = 0.5f;
            maxEnergy = 100;
            energy = maxEnergy;
            tiredZone = 15;
            staminaCost = 5;
            energyBar = UI_energy.Height;
            maxEnergyBar = UI_energy.Height;
            //Player Animate
            p_stateNum = 0;
            p_totalElapsed = 0;
            p_timePerframe = 1;
            p_frame = 0;
            p_climbFrame = 0;
            //Basic Setting
            wallCheckLeft = wallCheckRight = false;
            isFlip = false;
            leftHand = false;
            onAir = false;

            this.myhook = HookBox.LoadHookToPlayer(this, this._game);
           
        }

        
        public override void Update(GameTime gameTime)
        {
            PlayerControl(gameTime);

            //Rectangle
            triggerRec = new Rectangle((int)triggerPos.X, (int)triggerPos.Y, (int)(UI_check.Width / 3), (int)(UI_check.Height));
            barRec = new Rectangle((int)insideBarPos.X, (int)insideBarPos.Y, (int)(UI_insidebar.Width), (int)(UI_insidebar.Height/3));
            dropRec = new Rectangle((int)dropBarPos.X, (int)dropBarPos.Y, (int)(UI_check.Width / 3 * dropBarScale.X), (int)(UI_check.Height * dropBarScale.Y));
            scLRec = new Rectangle((int)checkBarPosL.X, (int)checkBarPosL.Y, (int)(UI_check.Width/3 * checkBarScaleL.X), (int)(UI_check.Height * checkBarScaleL.Y));
            scRRec = new Rectangle((int)checkBarPosR.X, (int)checkBarPosR.Y, (int)(UI_check.Width / 3 * checkBarScaleR.X), (int)(UI_check.Height * checkBarScaleR.Y));

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
            if (timeCount - countG >= 1f)
            {
                onAir = true;
            }
            else { onAir = false; }
            

            if (onAir)
            {
                if (wallCheckLeft) { wallCheckLeft = false; }
                if (wallCheckRight) { wallCheckRight = false; }
                p_stateNum = 3;
            }
            
            //Debug
            if (countW > 10) { countW = 0; }
            if (countC > 10) { countC = 0; }

            //Energy
            StaminaSystem((float)gameTime.ElapsedGameTime.TotalSeconds);
            energyBar = maxEnergyBar - (((energy / maxEnergy)) * maxEnergyBar );
            if(energyBar > maxEnergyBar)
            {
                energyBar = maxEnergyBar;
            }

            //Animate
            switch (p_stateNum)
            {
                case 0:
                    {
                        p_timePerframe = 0.5f;
                        break;
                    }
                case 1:
                    {
                        p_timePerframe = 0.1f;
                        break;
                    }
            }
            UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds);

            //hook
            myhook.Update(gameTime);
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!isFlip)
            {
                switch (p_stateNum)
                {
                    case 0:
                        {
                            spriteBatch.Draw(P_idle, Bounds.Position, new Rectangle(108*p_frame, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
                            break;
                        }
                    case 1:
                        {
                            spriteBatch.Draw(P_walk, Bounds.Position, new Rectangle(108*p_frame, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
                            break;
                        }
                    case 2:
                        {
                            spriteBatch.Draw(P_climb, Bounds.Position, new Rectangle(108*p_climbFrame, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
                            break;
                        }
                    case 3:
                        {
                            spriteBatch.Draw(P_fall, Bounds.Position, new Rectangle(0, 0, 144, 108), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);
                            break;
                        }
                }

            }
            else if (isFlip)
            {
                switch (p_stateNum)
                {
                    case 0:
                        {
                            spriteBatch.Draw(P_idle, Bounds.Position, new Rectangle(108 * p_frame, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                            break;
                        }
                    case 1:
                        {
                            spriteBatch.Draw(P_walk, Bounds.Position, new Rectangle(108 * p_frame, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                            break;
                        }
                    case 2:
                        {
                            spriteBatch.Draw(P_climb, Bounds.Position, new Rectangle(108 * p_climbFrame, 0, 108, 138), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                            break;
                        }
                    case 3:
                        {
                            spriteBatch.Draw(P_fall, Bounds.Position, new Rectangle(0, 0, 144, 108), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0);
                            break;
                        }
                }

            }

            //Draw InteractPopUp
            if((wallCheckLeft || wallCheckRight) && !onClimb)
            {
                spriteBatch.Draw(UI_popup, new Vector2(((RectangleF)Bounds).Position.X+(((RectangleF)Bounds).Size.Width/2-UI_popup.Width/4), ((RectangleF)Bounds).Top - UI_popup.Height), 
                                           new Rectangle(framePopup*UI_popup.Width, 0, UI_popup.Width/2, UI_popup.Height), Color.White);
            }
            DrawSkilCheck(spriteBatch);
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3f);

            //HookDraw
            myhook.Draw(spriteBatch);
        }
        
        public override void OnCollision(CollisionEventArgs collisionInfo)
        {
            countC++;
            //Platform Collision
            if (collisionInfo.Other.ToString().Contains("PlatForm"))
            {
                countG = timeCount;
                if (!onClimb)
                {
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
                timeCount = timeCount + 0.5f;
                totalElapsed -= timePerFrame;
            }

        }

        public void UpdateFrame(float elapsed)
        {
            p_totalElapsed += elapsed;
            if(p_totalElapsed > p_timePerframe)
            {
                switch (p_stateNum)
                {
                    case 0:
                        {
                            if (p_totalElapsed > p_timePerframe)
                            {
                                p_frame = (p_frame + 1) % 4;
                            }
                            p_totalElapsed -= p_timePerframe;
                            break;
                        }
                    case 1:
                        {
                            if (p_totalElapsed > p_timePerframe)
                            {
                                p_frame = (p_frame + 1) % 4;
                            }

                            p_totalElapsed -= p_timePerframe;
                            break;
                        } 
                }
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
                p_stateNum = 2;
                if (_currentKey.IsKeyDown(Keys.A) && _oldKey.IsKeyUp(Keys.A))
                {
                    if (leftHand == true && triggerRec.Intersects(scLRec) && energy / maxEnergy * 100 > tiredZone )
                    {
                        energy -= staminaCost;
                        leftHand = false;
                        if(p_climbFrame == 0) { p_climbFrame = 1; }
                        else if (p_climbFrame ==1) { p_climbFrame = 0; }
                        move = new Vector2(0, Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPos1Y() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCamera1Y(move);
                        }
                        Bounds.Position -= move;
                    }
                    if (triggerRec.Intersects(dropRec))
                    {
                        energy -= staminaCost;
                        move = new Vector2(0, -Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPos1Y() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCamera1Y(move);
                        }
                        Bounds.Position -= move;
                    }
                }
                else if (_currentKey.IsKeyDown(Keys.D) && _oldKey.IsKeyUp(Keys.D))
                {
                    if (leftHand == false && triggerRec.Intersects(scRRec) && energy / maxEnergy * 100 > tiredZone)
                    {
                        energy -= staminaCost;
                        leftHand = true;
                        if (p_climbFrame == 0) { p_climbFrame = 1; }
                        else if (p_climbFrame == 1) { p_climbFrame = 0; }
                        move = new Vector2(0, Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPos1Y() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCamera1Y(move);
                        }
                        Bounds.Position -= move;
                    }
                    if (triggerRec.Intersects(dropRec))
                    {
                        energy -= staminaCost;
                        move = new Vector2(0, -Velocity) * gameTime.GetElapsedSeconds() * 100;
                        if (_game.GetCameraPos1Y() - Bounds.Position.Y <= 864)
                        {
                            _game.UpdateCamera1Y(move);
                        }
                        Bounds.Position -= move;
                    }
                }
            }
            else if (onClimb == false && onAir == false)
            {
                if (_currentKey.IsKeyDown(Keys.D) && Bounds.Position.X < _game.GetMapWidth() - ((RectangleF)Bounds).Width)
                {
                    p_stateNum = 1;
                    isFlip = false;
                    move = new Vector2(Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                    if (Bounds.Position.X - _game.GetCameraPos1X() >= 400 &&
                        _game.GetCamera1X() + 868 < _game.GetMapWidth() / 2)
                    {
                            _game.UpdateCamera1X(move);

                    }
                    Bounds.Position += move;
                }
                else if (_currentKey.IsKeyDown(Keys.A) && Bounds.Position.X > 0)
                {
                    p_stateNum = 1;
                    isFlip = true;
                    move = new Vector2(-Velocity, 0) * gameTime.GetElapsedSeconds() * 50;
                    if (Bounds.Position.X - _game.GetCameraPos1X() <= 300
                        && _game.GetCameraPos1X() > 0)
                    {
                        _game.UpdateCamera1X(move);
                    }
                    Bounds.Position += move;
                }
                else if (_currentKey.IsKeyUp(Keys.A) && _currentKey.IsKeyUp(Keys.D))
                {
                        p_stateNum = 0;
                }
            }
            #endregion
            //For camera
            if (_currentKey.IsKeyDown(Keys.W) && onClimb == true)
            {
                move = new Vector2(0, 10) * gameTime.GetElapsedSeconds() * 60;
                if (_game.GetCameraPos1Y() - Bounds.Position.Y <= 864)
                {
                    _game.UpdateCamera1Y(move);
                }

            }
            else if (Bounds.Position.Y + 108 - _game.GetCameraPos1Y() >= (_game.GetMapHeight() - 216))
            {
                move = new Vector2(0, (-Gfroce * 2)) * gameTime.GetElapsedSeconds() * 50;
                _game.UpdateCamera1Y(move);
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
            //Stamina
            staminaPos = new Vector2(target.X + 9, target.Y + 486 - UI_stamina.Height / 2);
            energyPos = new Vector2(target.X + 32.5f, (target.Y + 486 - UI_stamina.Height / 2)+23.5f);
            halfPos = new Vector2(target.X + 864 - UI_half.Width /2 , target.Y);
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
            //Stamina
            spriteBatch.Draw(UI_stamina, staminaPos, new Rectangle(0, 0, UI_stamina.Width, UI_stamina.Height), Color.White);
            spriteBatch.Draw(UI_energy,new Rectangle((int)energyPos.X,(int)energyPos.Y,UI_energy.Width,(int)energyBar), new Rectangle(0, 0, UI_energy.Width, UI_energy.Height),Color.White);
            //Web Status
            //spriteBatch.Draw(UI_bar, barPos, new Rectangle(0, UI_bar.Height/2, UI_bar.Width, UI_bar.Height / 2), Color.White);
            //Inside
            //spriteBatch.Draw(UI_insidebar, insideBarPos, new Rectangle(0, UI_insidebar.Height * frameInsideBar / 3, UI_insidebar.Width, UI_insidebar.Height / 3), Color.White);
            //Half
            spriteBatch.Draw(UI_half, halfPos, new Rectangle(0, 0, UI_half.Width / 2, UI_half.Height), Color.White);

        }
        public void SkillCheck(GameTime gameTime)
        {
            //Energy((float)gameTime.ElapsedGameTime.TotalSeconds);
            //energyBar = energyBar - ((energy / maxEnergy) * maxEnergyBar);

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
                if( leftHand == true && triggerRec.Intersects(scLRec) && energy / maxEnergy * 100 > tiredZone)
                {
                    leftHandOn = 1;
                }
                else if (leftHand == false && triggerRec.Intersects(scRRec) && energy / maxEnergy * 100 > tiredZone)
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
        public void StaminaSystem(float time)
        {
            totalTime += time;
            if (totalTime > maxTime)
            {
                energy = energy + 2.5f;
                totalTime -= maxTime;
            }
            if (energy > maxEnergy)
            {
                energy = maxEnergy;
            } 
            else if (energy/maxEnergy * 100 > 50)
            {
                p_timePerframe = 0.5f;
                maxTime = 0.5f;
            }
            else if (energy / maxEnergy * 100 < 50)
            {
                p_timePerframe = 0.25f;
                maxTime = 0.8f;
            }
            else if (energy / maxEnergy * 100 < tiredZone)
            {
                p_timePerframe = 0.1f;
                maxTime = 2f;
            }
            else if (energy <= 0)
            {
                energy = 0;
            }

        }
        #endregion
    }

    
}
