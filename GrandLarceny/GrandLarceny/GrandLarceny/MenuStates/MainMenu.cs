﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace GrandLarceny
{
	public class MainMenu : MenuState
	{
		private int m_currentButton = 0;

		#region Constructor & Load
		public override void load()
		{
			base.load();
			Game.getInstance().m_camera.setPosition(Vector2.Zero);

			if (!Directory.Exists("Content//levels//"))
			{
				Directory.CreateDirectory("Content//levels//");
			}

			TextButton t_newGame = new TextButton(Vector2.Zero, "Start Game", "MotorwerkLarge");
			t_newGame.m_clickEvent += new TextButton.clickDelegate(loadGameClick);
			m_buttons.AddFirst(t_newGame);

			TextButton t_levelSelect = new TextButton(Vector2.Zero, "User Levels", "MotorwerkLarge", m_normal, m_hover, m_pressed, Color.Red);
			t_levelSelect.m_clickEvent += new TextButton.clickDelegate(levelSelectClick);
			m_buttons.AddFirst(t_levelSelect);

			TextButton t_settings = new TextButton(Vector2.Zero, "Settings", "MotorwerkLarge", m_normal, m_hover, m_pressed, Color.Red);
			t_settings.m_clickEvent += new TextButton.clickDelegate(settingsClick);
			m_buttons.AddFirst(t_settings);

			TextButton t_credit = new TextButton(Vector2.Zero, "Credits", "MotorwerkLarge", m_normal, m_hover, m_pressed, Color.Red);
			t_credit.m_clickEvent += new TextButton.clickDelegate(creditsClick);
			m_buttons.AddFirst(t_credit);
			
			TextButton t_exitButton = new TextButton(Vector2.Zero, "Exit", "MotorwerkLarge", m_normal, m_hover, m_pressed, Color.Red);
			t_exitButton.m_clickEvent += new TextButton.clickDelegate(exitClick);
			m_buttons.AddFirst(t_exitButton);
			GuiListFactory.setListPosition(m_buttons, new Vector2(20, Game.getInstance().getResolution().Y - 120));
			GuiListFactory.setButtonDistance(m_buttons, new Vector2(0, -60));

			m_buttons.Last().setState(Button.State.Hover);

			if (Music.getInstance().musicIsPlaying())
			{
				Music.getInstance().stop();
			}
			Music.getInstance().loadSong("MenuSong");
			Music.getInstance().play("MenuSong");
		}
		#endregion

		#region Update & Draw
		public override void update(GameTime a_gameTime)
		{
			base.update(a_gameTime);
			if (KeyboardHandler.keyClicked(Keys.Up))
			{
				moveCurrentHover(-1);
			}
			else if (KeyboardHandler.keyClicked(Keys.Down))
			{
				moveCurrentHover(+1);
			}
			else if (KeyboardHandler.keyClicked(Keys.Enter))
			{
				m_buttons.ElementAt(m_currentButton).setState(Button.State.Pressed);
				m_buttons.ElementAt(m_currentButton).invokeClickEvent();
			}
		}
		#endregion

		#region Main Menu Methods (MMM...Bio)
		public void moveCurrentHover(int a_move)
		{
			m_buttons.ElementAt(m_currentButton).setState(Button.State.Normal);
			m_currentButton += a_move;
			if (m_currentButton >= m_buttons.Count)
			{
				m_currentButton = 0;
			}
			else if (m_currentButton < 0)
			{
				m_currentButton = m_buttons.Count-1;
			}
			m_buttons.ElementAt(m_currentButton).setState(Button.State.Hover);
		}

		public void exitClick(Button a_b)
		{
			Music.getInstance().stop();
			Game.getInstance().Exit();
		}

		public void newGameClick(Button a_b)
		{
			Game.getInstance().setState(new HubMenu());
			Game.getInstance().setProgress("temp.prog", false);
		}

		public void loadGameClick(Button a_b)
		{
			Game.getInstance().setState(new LoadAndSaveMenu(false, this));
		}

		public void settingsClick(Button a_button)
		{
			Music.getInstance().stop();
			Game.getInstance().setState(new SettingsMenu());
		}
		
		public void levelSelectClick(Button a_button)
		{
			Music.getInstance().stop();
			Game.getInstance().setState(new LevelMenu());
		}

		private void creditsClick(Button a_button)
		{
			LinkedList<Text> t_list = GuiListFactory.createTextListFromArray(new string[] { "Joxe", "Gittan", "Zacko", "Lifegain", "Melburn", "Yuma", "Buddha the God", "Borre" }, "MotorwerkLarge", new Color(187, 194, 195));
			GuiListFactory.setTextListPosition(t_list, new Vector2(-(Game.getInstance().getResolution().X / 2) + 600, -(Game.getInstance().getResolution().Y / 2) + 10));
			GuiListFactory.setTextDistance(t_list, new Vector2(0, 60));
			foreach (Text t_text in t_list)
			{
				m_guiList.AddLast(t_text);
			}
		}
		#endregion
	}
}
