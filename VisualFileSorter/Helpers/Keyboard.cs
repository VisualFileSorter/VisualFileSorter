using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;

namespace VisualFileSorter.Helpers
{
    // Adapted from https://github.com/kekekeks/Avalonia.BattleCity/blob/master/Keyboard.cs
    static class Keyboard
    {
        public static readonly HashSet<Key> Keys = new HashSet<Key>();
        public static bool IsKeyDown(Key key) => Keys.Contains(key);

        // There is probably a better way of doing this
        // Do I know a better way of doing this?
        // No
        public static HashSet<Key> GetUserShortcut()
        {
            HashSet<Key> Shortcut = new HashSet<Key>();
            while (true)
            {
                if (Keys.Count != 0)
                {
                    if (Keys.Contains(Key.LeftCtrl) || Keys.Contains(Key.RightCtrl))
                    {
                        Shortcut.Add(Key.LeftCtrl);
                        if (Keys.Contains(Key.E))
                        {
                            Shortcut.Add(Key.E);
                            return Shortcut;
                        }

                    }
                    else
                    {
                        if (Keys.Contains(Key.E))
                        {
                            Shortcut.Add(Key.E);
                            return Shortcut;
                        }
                    }
                }
            }
        } 
    }
}