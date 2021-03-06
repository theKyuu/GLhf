﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GrandLarceny.AI
{
	[Serializable()]
	public class AIStateGoingToTheSwitch : AIState
	{
		private AIStateGoingToTheSwitch()
		{
		}
		private static AIStateGoingToTheSwitch instance;
		public static AIStateGoingToTheSwitch getInstance()
		{
			if (instance == null)
			{
				instance = new AIStateGoingToTheSwitch();
			}
			return instance;
		}

		public override AIState execute(NPE a_agent, GameTime a_gameTime)
		{
			if(a_agent==null)
            {
                throw new ArgumentNullException("The Agent cannot be null");
            }
			if (a_agent is Guard)
			{
				Guard t_guard = (Guard)a_agent;
				if (t_guard.canHearPlayer())
				{
					t_guard.setChaseTarget(Game.getInstance().getState().getPlayer());
					if (t_guard.isFacingRight())
					{
						Game.getInstance().getState().addObject(new Particle(
							t_guard.getPosition().getGlobalCartesian() + new Vector2(60, -20),
							"Images//Sprite//Guard//qmark",
							20f,
							t_guard.getLayer()));
					}
					else
					{
						Game.getInstance().getState().addObject(new Particle(
							t_guard.getPosition().getGlobalCartesian() + new Vector2(-20, -10),
							"Images//Sprite//Guard//qmark",
							20f,
							t_guard.getLayer()));
					}
					return new AIStateObserving(((float)a_gameTime.TotalGameTime.TotalMilliseconds) + 2000f, t_guard.isFacingRight());
				}
				if (t_guard.hasNoLampSwitchTargets())
				{
					return AIStatepatroling.getInstance();
				}
				else
				{
					if (!t_guard.isCarryingFlash() && t_guard.hasFlash())
					{
						t_guard.toggleFlashLight();
					}
					else
					{
						LampSwitch t_lampSwitch = t_guard.getFirstLampSwitchTarget();
						while (Math.Abs(t_lampSwitch.getPosition().getGlobalX() - a_agent.getPosition().getGlobalX()) < 10)
						{
							if (!t_lampSwitch.isOn())
							{
								t_lampSwitch.toggleSwitch();
							}
							foreach (GameObject t_g in Game.getInstance().getState().getCurrentList())
							{
								if (t_g is Guard)
								{
									((Guard)t_guard).removeLampSwitchTarget(t_lampSwitch);
								}
							}
							if (t_guard.hasNoLampSwitchTargets())
							{
								return AIStatepatroling.getInstance();
							}
							t_lampSwitch = t_guard.getFirstLampSwitchTarget();
						}
						if (t_guard.isRunning())
						{
							t_guard.setRunning(false);
						}
						else if (t_guard.getPosition().getGlobalX() < t_lampSwitch.getPosition().getGlobalX())
						{
							if (t_guard.getHorizontalSpeed() <= 0)
							{
								t_guard.goRight();
							}
						}
						else if (t_guard.getPosition().getGlobalX() > t_lampSwitch.getPosition().getGlobalX())
						{
							if (t_guard.getHorizontalSpeed() >= 0)
							{
								t_guard.goLeft();
							}
						}
					}
					return this;
				}
			}
			else
			{
				throw new ArgumentException("Only guards can go to the switch");
			}
		}
	}
}
