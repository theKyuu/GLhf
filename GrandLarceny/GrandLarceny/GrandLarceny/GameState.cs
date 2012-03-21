﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GrandLarceny.Events;

namespace GrandLarceny
{
	public class GameState : States
	{
		private LinkedList<GameObject>[] m_gameObjectList;
		private Stack<GameObject>[] m_removeList;
		private Stack<GameObject>[] m_addList;
		private LinkedList<GuiObject> m_guiObject;
		private LinkedList<Event> m_events;
		private string m_currentLevel;
		private int m_currentList;

		private static Keys m_upKey;
		private static Keys m_downKey;
		private static Keys m_leftKey;
		private static Keys m_rightKey;
		private static Keys m_jumpKey;
		private static Keys m_rollKey;
		private static Keys m_actionKey;
		private static Keys m_sneakKey;

		private Player player;

		public GameState(string a_levelToLoad)
		{
			m_currentLevel = a_levelToLoad;
		}

		public override void load()
		{
			Game.getInstance().m_camera.setZoom(1.0f);
			m_guiObject = new LinkedList<GuiObject>();
			m_gameObjectList = Loader.getInstance().loadLevel(m_currentLevel);

			//TODO
			//Load Events
			m_events = new LinkedList<Event>();

			m_removeList = new Stack<GameObject>[m_gameObjectList.Length];
			m_addList = new Stack<GameObject>[m_gameObjectList.Length];
			for (int i = 0; i < m_gameObjectList.Length; ++i)
			{
				m_removeList[i] = new Stack<GameObject>();
				m_addList[i] = new Stack<GameObject>();
			}
			
			foreach (LinkedList<GameObject> t_ll in m_gameObjectList)
			{
				foreach (GameObject t_go in t_ll)
				{
					t_go.loadContent();

					if (t_go is Player)
					{
						Game.getInstance().getState().setPlayer((Player)t_go);
					}
				}
			}
			
			if (player != null)
			{
				Game.getInstance().m_camera.setPosition(Vector2.Zero);
				Game.getInstance().m_camera.setParentPosition(player.getPosition());
			}

			string[] t_loadedFile = System.IO.File.ReadAllLines("Content//wtf//input.ini");
			foreach (string t_currentLine in t_loadedFile)
			{
				string[] t_keybinding = t_currentLine.Split('=');
				if (t_keybinding[0].Equals("Up"))
					m_upKey		= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1]);
				else if (t_keybinding[0].Equals("Down"))
					m_downKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1]);
				else if (t_keybinding[0].Equals("Left"))
					m_leftKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1]);
				else if (t_keybinding[0].Equals("Right"))
					m_rightKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1]);
				else if (t_keybinding[0].Equals("Jump"))
					m_jumpKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1].ToUpper());
				else if (t_keybinding[0].Equals("Roll"))
					m_rollKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1].ToUpper());
				else if (t_keybinding[0].Equals("Action"))
					m_actionKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1].ToUpper());
				else if (t_keybinding[0].Equals("Sneak"))
					m_sneakKey	= (Keys)Enum.Parse(typeof(Keys), t_keybinding[1]);
				else
					System.Console.WriteLine("Unknown keybinding found!");
			}
		}

		public override void setPlayer(Player a_player)
		{
			player = a_player;
		}

		/*
		Update-metod, går igenom alla objekt i scenen och kallas på deras update
		och kollar sedan om de ska dö och läggs därefter i dödslistan.
		Dödslistan loopas sedan igenom och tar bort de objekt som ska dö ifrån
		objektlistan.
		*/
		public override void update(GameTime a_gameTime)
		{
			m_currentList = -1;
			

			foreach (LinkedList<GameObject> t_list in m_gameObjectList)
			{

				m_currentList++;
				foreach (GameObject t_gameObject in t_list)
				{
					t_gameObject.update(a_gameTime);
				}
			}

			if (Game.isKeyPressed(Keys.Q))
			{
				Game.getInstance().setState(new DevelopmentState(m_currentLevel));
			}

			if (Game.isKeyPressed(Keys.R))
			{
				Game.getInstance().setState(new GameState(m_currentLevel));
				Game.getInstance().m_camera.setLayer(0);
			}
			m_currentList = -1;
			foreach (LinkedList<GameObject> t_list in m_gameObjectList)
			{
				++m_currentList;
				foreach (GameObject t_firstGameObject in t_list)
				{
					if (t_firstGameObject is MovingObject)
					{
						List<Entity> t_collided = new List<Entity>();
						foreach (GameObject t_secondGameObject in t_list)
						{
							if (t_secondGameObject is Entity && t_firstGameObject != t_secondGameObject
								&& ((Entity)t_firstGameObject).getHitBox() != null && ((Entity)t_secondGameObject).getHitBox() != null
								&& checkBigBoxCollision(((Entity)t_firstGameObject).getHitBox().getOutBox(), ((Entity)t_secondGameObject).getHitBox().getOutBox()))
							{
								t_collided.Add((Entity)t_secondGameObject);
							}
						}
						((MovingObject)t_firstGameObject).collisionCheck(t_collided);
						((Entity)t_firstGameObject).updatePosition();

					}

					if (t_firstGameObject.isDead() && !m_removeList[m_currentList].Contains(t_firstGameObject))
					{
						m_removeList[m_currentList].Push(t_firstGameObject);
					}
				}
				while (m_addList[m_currentList].Count > 0)
				{
					GameObject t_goToAdd = m_addList[m_currentList].Pop();
					if(! t_list.Contains(t_goToAdd))
					{
						t_list.AddLast(t_goToAdd);
					}
				}
				while (m_removeList[m_currentList].Count > 0)
				{
					t_list.Remove(m_removeList[m_currentList].Pop());
				}
				LinkedListNode<Event> t_eventNode = m_events.First;
				while(t_eventNode != null)
				{
					LinkedListNode<Event> t_next = t_eventNode.Next;
					if (t_eventNode.Value.Execute())
					{
						m_events.Remove(t_eventNode);
					}
					t_eventNode = t_next;
				}
			}
		}
		/*
		Draw-metod, loopar igenom alla objekt och ber dem ritas ut på skärmen 
		*/
		public override void draw(GameTime a_gameTime, SpriteBatch a_spriteBatch)
		{
			foreach (GameObject t_gameObject in m_gameObjectList[Game.getInstance().m_camera.getLayer()])
			{
				t_gameObject.draw(a_gameTime);
			}
			foreach (GuiObject t_go in m_guiObject)
			{
				if (!t_go.isDead())
				{
					t_go.draw(a_gameTime);
				}
			}
		}

		public static bool checkBigBoxCollision(Rectangle a_first, Rectangle a_second)
		{
			return (a_first.X - 1 < a_second.X + a_second.Width &&
				a_first.X + a_first.Width + 1 > a_second.X &&
				a_first.Y - 1 < a_second.Y + a_second.Height &&
				a_first.Y + a_first.Height + 1 > a_second.Y);
		}

		public override void addObject(GameObject a_object)
		{
			m_addList[m_currentList].Push(a_object);
		}
		public override void removeObject(GameObject a_object)
		{
			m_removeList[m_currentList].Push(a_object);
		}
		public override void addObject(GameObject a_object, int a_layer)
		{
			m_addList[a_layer].Push(a_object);
		}
		public override void removeObject(GameObject a_object, int a_layer)
		{
			m_removeList[a_layer].Push(a_object);
		}
		public override Player getPlayer()
		{
			return player;
		}
		public override LinkedList<GameObject>[] getObjectList()
		{
			return m_gameObjectList;
		}
		public override LinkedList<GameObject> getCurrentList() {
			return m_gameObjectList[m_currentList];
		}
		public override void changeLayer(int a_newLayer)
		{
			Player t_player = null;
			foreach (GameObject t_go in m_gameObjectList[Game.getInstance().m_camera.getLayer()])
			{
				if (t_go is Player)
				{
					t_player = (Player)t_go;
				}
			}
			if (t_player != null)
			{
				addObject(t_player, a_newLayer);
				removeObject(t_player, Game.getInstance().m_camera.getLayer());
			}
			Game.getInstance().m_camera.setLayer(a_newLayer);
		}
		public override void addGuiObject(GuiObject a_go)
		{
			m_guiObject.AddLast(a_go);
		}

		internal override GameObject getObjectById(int a_id)
		{
			foreach (LinkedList<GameObject> t_goList in m_gameObjectList)
			{
				foreach (GameObject t_go in t_goList)
				{
					if (a_id == t_go.getId())
					{
						return t_go;
					}
				}
			}
			return null;
		}

		public static Keys getUpKey()
		{
			return m_upKey;
		}

		public static Keys getDownKey()
		{
			return m_downKey;
		}

		public static Keys getLeftKey()
		{
			return m_leftKey;
		}

		public static Keys getRightKey()
		{
			return m_rightKey;
		}

		public static Keys getJumpKey()
		{
			return m_jumpKey;
		}

		public static Keys getRollKey()
		{
			return m_rollKey;
		}

		public static Keys getActionKey()
		{
			return m_actionKey;
		}

		public static Keys getSneakKey()
		{
			return m_sneakKey;
		}
	}
}
