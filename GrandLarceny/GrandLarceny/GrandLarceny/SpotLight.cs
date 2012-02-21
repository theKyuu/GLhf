﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GrandLarceny
{
	[Serializable()]
	class SpotLight : Entity
	{
		Boolean m_lit;
		LightCone m_light;
		public SpotLight(Vector2 a_position, string a_sprite, float a_layer, float a_rotation, bool a_lit) :
			base(a_position, a_sprite, a_layer)
		{
			m_rotate = a_rotation;
			m_lit = a_lit;
			if (m_lit)
			{
				m_light = new LightCone(this, "Images//LightCone//Ljus",a_layer , 300f, 200f);

				(Game.getInstance().getState()).addObject(m_light);
			}
		}
		public override void loadContent()
		{
			base.loadContent();
			if (m_lit && m_light == null)
			{
				m_light = new LightCone(this, "Images//LightCone//Ljus", m_layer + 1, 500f, 100f);
				((GameState)(Game.getInstance().getState())).addObject(m_light);
			}
		}

		public LightCone getLightCone()
		{
			return m_light;
		}
		public override void update(GameTime a_gameTime)
		{
			base.update(a_gameTime);
			if (m_light != null)
			{
				m_light.setRotation(m_rotate);
			}
		}
	}
}
