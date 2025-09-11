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

            // Cancella eventuale esecuzione pianificata
            _scheduled?.Pause();

            // Pianifica sul main thread tra _delayMs ms
            _scheduled = _owner.schedule
                .Execute(() => { _pending?.Invoke(); })
                .StartingIn(_delayMs);
        }
    }
}
