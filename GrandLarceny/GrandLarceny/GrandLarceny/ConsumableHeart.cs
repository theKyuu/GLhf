﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GrandLarceny
{
	public class ConsumableHeart : Consumable
	{
		public ConsumableHeart(Vector2 a_position, String a_sprite, int a_layer)
			: base(a_position, a_sprite, a_layer)
		{

		}
		protected override void collect()
		{
			throw new NotImplementedException();
		}
	}
}