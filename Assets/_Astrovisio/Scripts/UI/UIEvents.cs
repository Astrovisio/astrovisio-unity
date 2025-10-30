/*
 * Astrovisio - Astrophysical Data Visualization Tool
 * Copyright (C) 2024-2025 Alkemy, Metaverso
 *
 * This file is part of the Astrovisio project.
 *
 * Astrovisio is free software: you can redistribute it and/or modify it under the terms 
 * of the GNU Lesser General Public License (LGPL) as published by the Free Software 
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Astrovisio is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
 * PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with 
 * Astrovisio in the LICENSE file. If not, see <https://www.gnu.org/licenses/>.
 *
 */

using System;

namespace Astrovisio
{
    /// <summary>
    /// Public static delegates to manage UI changes (note these are "events" in the conceptual sense
    /// and not the strict C# sense).
    /// </summary>
    public static class UIEvents
    {

        // NOT USED

        #region Menu/screen events:

        // Close the screen and go back
        public static Action ScreenClosed;

        public static Action SplashScreenShown;

        // Show the Main Menu selection (Settings, Level Selection)
        public static Action MainMenuShown;

        // Show the user settings (sound volume)
        public static Action SettingsShown;

        // Show a level select screen to choose what quiz to play
        public static Action LevelSelectionShown;

        // Show the main gameplay screen (one formatted question)
        public static Action GameScreenShown;

        // Show a pause screen during gameplay to abort the game
        public static Action PauseScreenShown;

        // Show the results of the quiz
        public static Action EndScreenShown;

        #endregion

        #region Response panel events:

        // User has toggled off all responses (nothing selected)
        public static Action ResponsesDeselected;
        #endregion

        #region Progress bar events:

        // Draw the progress bar's percentage complete
        public static Action<float> ProgressBarUpdated;

        // Show numeric percentage of correct answers
        public static Action<float> AccuracyCalculated;

        #endregion

        #region End screen events:

        // Show the quiz name
        public static Action<string> QuizTitleShown;

        // Show ratio of correct to total answers (i.e. "4 out of 5")
        public static Action<string> ScoreMessageShown;

        // Show correct answer/incorrect answer/total questions
        public static Action<int, int, int> ScoresTotaled;

        // Show text description of the main menu button
        public static Action<string> MenuDescriptionShown;

        public static Action<string> UrlOpened;

        #endregion

    }
}
