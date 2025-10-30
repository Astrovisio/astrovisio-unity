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
using UnityEngine;
using UnityEngine.UIElements;

namespace Astrovisio
{
    public class UIDebouncer
    {
        private readonly VisualElement _owner;
        private readonly int _delayMs;
        private IVisualElementScheduledItem _scheduled;
        private Action _pending;

        public UIDebouncer(VisualElement owner, int delayMs)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _delayMs = Mathf.Max(0, delayMs);
        }

        public void Run(Action action)
        {
            _pending = action;

            _scheduled?.Pause();

            _scheduled = _owner.schedule
                .Execute(() => { _pending?.Invoke(); })
                .StartingIn(_delayMs);
        }
    }
}
