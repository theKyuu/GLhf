﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GrandLarceny
{
	[Serializable()]
	public class Player : Entity
	{
		private Vector2 m_cameraPoint = new Vector2(0,0);

		private const float CAMERASPEED = 0.1f;

		private const int CLIMBINGSPEED = 200;
		private const int PLAYERSPEED = 200;
		private const int JUMPSTRENGTH = 600;
		private const int CAMERAMAXDISTANCE = 100;
		private const int ACCELERATION = 2000;
		private const int DEACCELERATION = 800;
		private const int AIRDEACCELERATION = 300;
		private const int SLIDESPEED = 25;

		private float m_rollTimer;

		[NonSerialized]
		private KeyboardState m_currentKeyInput;
		[NonSerialized]
		private KeyboardState m_previousKeyInput;
		private State m_currentState = State.Stop;

		private bool m_facingRight = false;

		enum State
		{
			Stop,
			Walking,
			Jumping,
			RightSlide,
			LeftSlide,
			Climbing, 
			Rolling
		}

		public Player(Vector2 a_posV2, String a_sprite) : base(a_posV2, a_sprite)
		{
			m_currentState = State.Jumping;
		}

		public override void update(GameTime a_gameTime)
		{
			m_gravity = 1000f;
			m_previousKeyInput = m_currentKeyInput;
			m_currentKeyInput = Keyboard.GetState();
			float t_deltaTime = ((float) a_gameTime.ElapsedGameTime.Milliseconds) / 1000f;
			switch (m_currentState)
			{
				case State.Stop:
				{
					updateStop(t_deltaTime);
					break;
				}
				case State.Walking:
				{
					updateWalking(t_deltaTime);
					break;
				}
				case State.Jumping:
				{
					updateJumping(t_deltaTime);
					break;
				}
				case State.RightSlide:
				{
					updateRightSliding(t_deltaTime);
					break;
				}
				case State.LeftSlide:
				{
					updateLeftSliding(t_deltaTime);
					break;
				}
				case State.Climbing:
				{
					updateClimbing();
					break;
				}
				case State.Rolling:
				{
					updateRolling();
					break;
				}
			}
			m_previousKeyInput = m_currentKeyInput;

			base.update(a_gameTime);
			Game.getInstance().m_camera.getPosition().smoothStep(m_cameraPoint, CAMERASPEED);
		}

		private void updateStop(float a_deltaTime)
		{
			if (m_previousKeyInput.IsKeyUp(Keys.Down) && m_currentKeyInput.IsKeyDown(Keys.Down))
			{
				m_currentState = State.Rolling;
				m_rollTimer = a_deltaTime + 15;
				return;
			}
			if (m_currentKeyInput.IsKeyDown(Keys.Left) || m_currentKeyInput.IsKeyDown(Keys.Right))
			{
				m_currentState = State.Walking;
				changeAnimation();
				if (m_currentKeyInput.IsKeyDown(Keys.Left))
				{
					m_facingRight = false;
					m_spriteEffects = SpriteEffects.FlipHorizontally;
				}
				else
				{
					m_facingRight = true;
					m_spriteEffects = SpriteEffects.None;
				}
			}
			if (m_previousKeyInput.IsKeyUp(Keys.Up) && m_currentKeyInput.IsKeyDown(Keys.Up))
			{
				m_speed.Y -= JUMPSTRENGTH;
				m_currentState = State.Jumping;
				changeAnimation();
			}

			//Game.getInstance().m_camera.getPosition().smoothStep(Vector2.Zero, CAMERASPEED);
		}

		private void updateWalking(float a_deltaTime)
		{
			if (m_previousKeyInput.IsKeyUp(Keys.Down) && m_currentKeyInput.IsKeyDown(Keys.Down))
			{
				m_currentState = State.Rolling;
				m_rollTimer = a_deltaTime + 15;
				return;
			}

			if (m_currentKeyInput.IsKeyDown(Keys.Right))
			{
				if (m_speed.X > PLAYERSPEED)
				{
					m_speed.X = m_speed.X - (DEACCELERATION * a_deltaTime);
				}
				else
				{
					m_speed.X = Math.Min(m_speed.X + (ACCELERATION * a_deltaTime), PLAYERSPEED);
				}
			}
			if (m_currentKeyInput.IsKeyDown(Keys.Left))
			{
				if (m_speed.X < -PLAYERSPEED)
				{
					m_speed.X = m_speed.X + (DEACCELERATION * a_deltaTime);
				}
				else
				{
					m_speed.X = Math.Max(m_speed.X - (ACCELERATION * a_deltaTime), -PLAYERSPEED);
				}
			}
			if (m_speed.X > 0)
			{
				m_speed.X = Math.Max(m_speed.X - (DEACCELERATION * a_deltaTime), 0);
				m_facingRight = true;
				m_spriteEffects = SpriteEffects.None;
			}
			else if (m_speed.X < 0)
			{
				m_speed.X = Math.Min(m_speed.X + (DEACCELERATION * a_deltaTime), 0);
				m_facingRight = false;
				m_spriteEffects = SpriteEffects.FlipHorizontally;
			}
			if (m_speed.X == 0)
			{
				m_currentState = State.Stop;
				changeAnimation();
			}
			if (m_previousKeyInput.IsKeyUp(Keys.Up) && m_currentKeyInput.IsKeyDown(Keys.Up))
			{
				m_speed.Y -= JUMPSTRENGTH;
				m_currentState = State.Jumping;
				changeAnimation();
			}

			m_cameraPoint.X = Math.Max(Math.Min(m_cameraPoint.X + (m_speed.X * 1.5f * a_deltaTime), CAMERAMAXDISTANCE), -CAMERAMAXDISTANCE);
		
			m_img.setAnimationSpeed(Math.Abs(m_speed.X / 10f));
			if (m_position.getY() != getLastPosition().Y)
			{
				m_currentState = State.Jumping;
			}		
		}

		private void updateJumping(float a_deltaTime)
		{
			if (m_currentKeyInput.IsKeyUp(Keys.Left) && m_currentKeyInput.IsKeyUp(Keys.Right))
			{
				if (m_speed.X > 0)
					m_speed.X = Math.Max(m_speed.X - (AIRDEACCELERATION * a_deltaTime), 0);
				else if (m_speed.X < 0)
					m_speed.X = Math.Min(m_speed.X + (AIRDEACCELERATION * a_deltaTime), 0);
			}
			else if (m_currentKeyInput.IsKeyDown(Keys.Left))
			{
				if (m_speed.X < -PLAYERSPEED)
				{
					m_speed.X += AIRDEACCELERATION * a_deltaTime;
				}
				else
				{
					m_speed.X = Math.Max(-PLAYERSPEED, m_speed.X - AIRDEACCELERATION * a_deltaTime);
				}
			}
			else if (m_currentKeyInput.IsKeyDown(Keys.Right))
			{
				if (m_speed.X > PLAYERSPEED)
				{
					m_speed.X -= AIRDEACCELERATION * a_deltaTime;
				}
				else
				{
					m_speed.X = Math.Min(PLAYERSPEED, m_speed.X + AIRDEACCELERATION * a_deltaTime);
				}
			}
			m_cameraPoint.X = Math.Max(Math.Min(m_cameraPoint.X + (m_speed.X * 1.5f * a_deltaTime), CAMERAMAXDISTANCE), -CAMERAMAXDISTANCE);
        }

		//TODO Byta animation :3
		private void updateRightSliding(float a_deltaTime)
		{
			if (m_lastPosition.Y != m_position.getY())
			{
				if (m_currentKeyInput.IsKeyDown(Keys.Right))
				{
					if (m_previousKeyInput.IsKeyUp(Keys.Up) && m_currentKeyInput.IsKeyDown(Keys.Up))
					{
						m_speed.Y = 0;
						m_speed.Y -= JUMPSTRENGTH;
						m_speed.X -= JUMPSTRENGTH;
						m_currentState = State.Jumping;
						return;
					}
					if (m_speed.Y > SLIDESPEED)
						m_speed.Y = SLIDESPEED;
					return;
				}
				m_currentState = State.Jumping;
				return;
			}
			m_currentState = State.Walking;
		}

		//TODO Byta animation
		private void updateLeftSliding(float a_deltaTime)
		{
			if (m_lastPosition.Y != m_position.getY())
			{
				if (m_currentKeyInput.IsKeyDown(Keys.Left))
				{
					if (m_previousKeyInput.IsKeyUp(Keys.Up) && m_currentKeyInput.IsKeyDown(Keys.Up))
					{
						m_speed.Y = 0;
						m_speed.Y -= JUMPSTRENGTH;
						m_speed.X += JUMPSTRENGTH;
						m_currentState = State.Jumping;
						return;
					}
					if (m_speed.Y > SLIDESPEED)
						m_speed.Y = SLIDESPEED;
					return;
				}
				m_currentState = State.Jumping;
				return;
			}
			m_currentState = State.Walking;
		}

		//TODO Implement :3
		private void updateClimbing()
		{
			if (m_currentKeyInput.IsKeyDown(Keys.Up))
			{
				m_speed.Y = -CLIMBINGSPEED;
			}
			else if (m_currentKeyInput.IsKeyDown(Keys.Down))
			{
				m_speed.Y = CLIMBINGSPEED;
			}
			else
			{
				m_gravity = 0;
				m_speed.Y = 0;
			}
		}

		private void updateRolling()
		{
			if (m_previousKeyInput.IsKeyUp(Keys.Up) && m_currentKeyInput.IsKeyDown(Keys.Up))
			{
				m_speed.Y -= JUMPSTRENGTH;
				m_currentState = State.Jumping;
				return;
			}
			if (--m_rollTimer <= 0)
			{
				m_currentState = State.Walking;
			}
			else
			{
				if (m_facingRight)
					m_speed.X = 500;
				else
					m_speed.X = -500;
			}
		}

		//TODO, titta sin state och ändra till rätt animation
		private void changeAnimation()
		{
			if (m_currentState == State.Stop)
			{
				m_img.setSprite("Images//hero_idle");
				//m_img.setSprite("Images//WalkingSquareStand");
			}
			else if (m_currentState == State.Walking)
			{
				m_img.setSprite("Images//hero_idle");
				//m_img.setSprite("Images//WalkingSquareWalking");
			}

			else if (m_currentState == State.Jumping)
			{
				if (m_speed.Y < 0)
				{
					m_img.setSprite("Images//hero_idle");
					//m_img.setSprite("Images//WalkingSquareJumping");
				}
				else
				{
					m_img.setSprite("Images//hero_idle");
					//m_img.setSprite("Images//WalkingSquareFalling");
				}
			}
        }

		public override void draw(GameTime a_gameTime)
		{
			base.draw(a_gameTime);
		}

		internal override void collisionCheck(List<Entity> a_collisionList)
		{
			if (a_collisionList.Count == 0)
				m_currentState = State.Jumping;
			foreach (Entity t_collider in a_collisionList)
			{
				if (GameState.checkBoxCollision(this, t_collider))
				{
					if (t_collider is Platform)
					{
						//Colliding with ze floor
						if ((int)(m_lastPosition.Y + (m_img.getSize().Y / 2)) - 2 <= (int)(t_collider.getLastPosition().Y - (t_collider.getImg().getSize().Y / 2)))
						{
							m_position.setY(t_collider.getBox().Y - (m_img.getSize().Y / 2));
							m_speed.Y = 0;
							if (m_currentState == State.Jumping)
							{
								if (m_speed.X == 0)
								{
									m_currentState = State.Stop;
									changeAnimation();
								}
								else
								{
									m_currentState = State.Walking;
									changeAnimation();
								}
							}
							continue;
						}

						//Colliding with ze zeeling
						if ((int)(m_lastPosition.Y - (m_img.getSize().Y / 2)) + 2 >= (int)(t_collider.getLastPosition().Y + (t_collider.getImg().getSize().Y / 2)))
						{
							m_position.setY(t_collider.getBox().Y + t_collider.getBox().Height + (m_img.getSize().Y / 2));
							m_speed.Y = 0;
							continue;
						}
						//Colliding with ze left wall
						if ((int)(m_lastPosition.X - (m_img.getSize().X / 2)) + 2 >= (int)(t_collider.getLastPosition().X + (t_collider.getImg().getSize().X / 2)))
						{
							setLeftPoint(t_collider.getTopLeftPoint().X + t_collider.getImg().getSize().X);
							if(m_currentState == State.Jumping)
								m_currentState = State.LeftSlide;
							m_speed.X = 0;
						}
						//Colliding with ze right wall
						if ((int)(m_lastPosition.X + (m_img.getSize().X / 2)) - 2 <= (int)(t_collider.getLastPosition().X - (t_collider.getImg().getSize().X / 2)))
						{
							setLeftPoint(t_collider.getTopLeftPoint().X - (m_img.getSize().X));
							if (m_currentState == State.Jumping)
								m_currentState = State.RightSlide;
							m_speed.X = 0;
						}
					}
					else if (t_collider is Ladder)
					{
						//Colliding with ze ladd0rz
						Rectangle t_rect = new Rectangle(t_collider.getBox().X + ((t_collider.getBox().Width / 2) - 2),
							t_collider.getBox().Y, 4, t_collider.getBox().Height);
						if (t_rect.Contains((int)m_lastPosition.X, (int)m_lastPosition.Y) && (m_currentKeyInput.IsKeyDown(Keys.Up) || m_currentKeyInput.IsKeyDown(Keys.Down)))
						{
							m_speed.X = 0;
							m_currentState = State.Climbing;
							if (m_speed.Y < -CLIMBINGSPEED || m_speed.Y > CLIMBINGSPEED)
								m_speed.Y = 0;
							setLeftPoint(t_collider.getLeftPoint());
						}
					}
				}
			}
			//m_currentState = State.Stop;
		}
	}
}

