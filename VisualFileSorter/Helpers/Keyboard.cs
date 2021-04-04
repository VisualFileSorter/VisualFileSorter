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

        // Get the currently held keys as a shortcut
        public static HashSet<Key> GetUserShortcut()
        {
            HashSet<Key> Shortcut = new HashSet<Key>();
            while (true)
            {
                if (Keys.Count != 0)
                {
                    if (Keys.Contains(Key.LeftCtrl) || Keys.Contains(Key.RightCtrl))
                    {
                        if (GetNonModifierKey(ref Shortcut, false, true))
                        {
                            Shortcut.Add(Key.LeftCtrl);
                            return Shortcut;
                        }
                    }
                    else if (Keys.Contains(Key.LeftAlt) || Keys.Contains(Key.RightAlt))
                    {
                        if (GetNonModifierKey(ref Shortcut, true, true))
                        {
                            Shortcut.Add(Key.LeftAlt);
                            return Shortcut;
                        }
                    }
                    else if (Keys.Contains(Key.LeftShift) || Keys.Contains(Key.RightShift))
                    {
                        if (GetNonModifierKey(ref Shortcut, true, true))
                        {
                            Shortcut.Add(Key.LeftShift);
                            return Shortcut;
                        }
                    }
                    else
                    {
                        if (GetNonModifierKey(ref Shortcut, true, false))
                        {
                            return Shortcut;
                        }
                    }
                }
            }
        }

        // Get the non-modifier key
        private static bool GetNonModifierKey(ref HashSet<Key> Shortcut, bool allowYZOS, bool allowTabCapsLock)
        {
            if      (Keys.Contains(Key.Back))             { Shortcut.Add(Key.Back); return true; }
            else if (Keys.Contains(Key.Tab))              { if (allowTabCapsLock) 
                                                          { Shortcut.Add(Key.Tab); return true; } else { return false; } }
            else if (Keys.Contains(Key.CapsLock))         { if (allowTabCapsLock) 
                                                          { Shortcut.Add(Key.CapsLock); return true; } else { return false; } }
            else if (Keys.Contains(Key.Left))             { Shortcut.Add(Key.Left); return true; }
            else if (Keys.Contains(Key.Up))               { Shortcut.Add(Key.Up); return true; }
            else if (Keys.Contains(Key.Right))            { Shortcut.Add(Key.Right); return true; }
            else if (Keys.Contains(Key.Down))             { Shortcut.Add(Key.Down); return true; }
            else if (Keys.Contains(Key.Delete))           { Shortcut.Add(Key.Delete); return true; }
            else if (Keys.Contains(Key.D0))               { Shortcut.Add(Key.D0); return true; }
            else if (Keys.Contains(Key.D1))               { Shortcut.Add(Key.D1); return true; }
            else if (Keys.Contains(Key.D2))               { Shortcut.Add(Key.D2); return true; }
            else if (Keys.Contains(Key.D3))               { Shortcut.Add(Key.D3); return true; }
            else if (Keys.Contains(Key.D4))               { Shortcut.Add(Key.D4); return true; }
            else if (Keys.Contains(Key.D5))               { Shortcut.Add(Key.D5); return true; }
            else if (Keys.Contains(Key.D6))               { Shortcut.Add(Key.D6); return true; }
            else if (Keys.Contains(Key.D7))               { Shortcut.Add(Key.D7); return true; }
            else if (Keys.Contains(Key.D8))               { Shortcut.Add(Key.D8); return true; }
            else if (Keys.Contains(Key.D9))               { Shortcut.Add(Key.D9); return true; }
            else if (Keys.Contains(Key.A))                { Shortcut.Add(Key.A); return true; }
            else if (Keys.Contains(Key.B))                { Shortcut.Add(Key.B); return true; }
            else if (Keys.Contains(Key.C))                { Shortcut.Add(Key.C); return true; }
            else if (Keys.Contains(Key.D))                { Shortcut.Add(Key.D); return true; }
            else if (Keys.Contains(Key.E))                { Shortcut.Add(Key.E); return true; }
            else if (Keys.Contains(Key.F))                { Shortcut.Add(Key.F); return true; }
            else if (Keys.Contains(Key.G))                { Shortcut.Add(Key.G); return true; }
            else if (Keys.Contains(Key.H))                { Shortcut.Add(Key.H); return true; }
            else if (Keys.Contains(Key.I))                { Shortcut.Add(Key.I); return true; }
            else if (Keys.Contains(Key.J))                { Shortcut.Add(Key.J); return true; }
            else if (Keys.Contains(Key.K))                { Shortcut.Add(Key.K); return true; }
            else if (Keys.Contains(Key.L))                { Shortcut.Add(Key.L); return true; }
            else if (Keys.Contains(Key.M))                { Shortcut.Add(Key.M); return true; }
            else if (Keys.Contains(Key.N))                { Shortcut.Add(Key.N); return true; }
            else if (Keys.Contains(Key.O))                { if (allowYZOS) 
                                                          { Shortcut.Add(Key.O); return true; } else { return false; } }
            else if (Keys.Contains(Key.P))                { Shortcut.Add(Key.P); return true; }
            else if (Keys.Contains(Key.Q))                { Shortcut.Add(Key.Q); return true; }
            else if (Keys.Contains(Key.R))                { Shortcut.Add(Key.R); return true; }
            else if (Keys.Contains(Key.S))                { if (allowYZOS) 
                                                          { Shortcut.Add(Key.S); return true; } else { return false; } }
            else if (Keys.Contains(Key.T))                { Shortcut.Add(Key.T); return true; }
            else if (Keys.Contains(Key.U))                { Shortcut.Add(Key.U); return true; }
            else if (Keys.Contains(Key.V))                { Shortcut.Add(Key.V); return true; }
            else if (Keys.Contains(Key.W))                { Shortcut.Add(Key.W); return true; }
            else if (Keys.Contains(Key.X))                { Shortcut.Add(Key.X); return true; }
            else if (Keys.Contains(Key.Y))                { if (allowYZOS) 
                                                          { Shortcut.Add(Key.Y); return true; } else { return false; } }
            else if (Keys.Contains(Key.Z))                { if (allowYZOS) 
                                                          { Shortcut.Add(Key.Z); return true; } else { return false; } }
            else if (Keys.Contains(Key.NumPad0))          { Shortcut.Add(Key.NumPad0); return true; }
            else if (Keys.Contains(Key.NumPad1))          { Shortcut.Add(Key.NumPad1); return true; }
            else if (Keys.Contains(Key.NumPad2))          { Shortcut.Add(Key.NumPad2); return true; }
            else if (Keys.Contains(Key.NumPad3))          { Shortcut.Add(Key.NumPad3); return true; }
            else if (Keys.Contains(Key.NumPad4))          { Shortcut.Add(Key.NumPad4); return true; }
            else if (Keys.Contains(Key.NumPad5))          { Shortcut.Add(Key.NumPad5); return true; }
            else if (Keys.Contains(Key.NumPad6))          { Shortcut.Add(Key.NumPad6); return true; }
            else if (Keys.Contains(Key.NumPad7))          { Shortcut.Add(Key.NumPad7); return true; }
            else if (Keys.Contains(Key.NumPad8))          { Shortcut.Add(Key.NumPad8); return true; }
            else if (Keys.Contains(Key.NumPad9))          { Shortcut.Add(Key.NumPad9); return true; }
            else if (Keys.Contains(Key.Multiply))         { Shortcut.Add(Key.Multiply); return true; }
            else if (Keys.Contains(Key.Add))              { Shortcut.Add(Key.Add); return true; }
            else if (Keys.Contains(Key.Separator))        { Shortcut.Add(Key.Separator); return true; }
            else if (Keys.Contains(Key.Subtract))         { Shortcut.Add(Key.Subtract); return true; }
            else if (Keys.Contains(Key.Decimal))          { Shortcut.Add(Key.Decimal); return true; }
            else if (Keys.Contains(Key.Divide))           { Shortcut.Add(Key.Divide); return true; }
            else if (Keys.Contains(Key.F1))               { Shortcut.Add(Key.F1); return true; }
            else if (Keys.Contains(Key.F2))               { Shortcut.Add(Key.F2); return true; }
            else if (Keys.Contains(Key.F3))               { Shortcut.Add(Key.F3); return true; }
            else if (Keys.Contains(Key.F4))               { Shortcut.Add(Key.F4); return true; }
            else if (Keys.Contains(Key.F5))               { Shortcut.Add(Key.F5); return true; }
            else if (Keys.Contains(Key.F6))               { Shortcut.Add(Key.F6); return true; }
            else if (Keys.Contains(Key.F7))               { Shortcut.Add(Key.F7); return true; }
            else if (Keys.Contains(Key.F8))               { Shortcut.Add(Key.F8); return true; }
            else if (Keys.Contains(Key.F9))               { Shortcut.Add(Key.F9); return true; }
            else if (Keys.Contains(Key.F10))              { Shortcut.Add(Key.F10); return true; }
            else if (Keys.Contains(Key.F11))              { Shortcut.Add(Key.F11); return true; }
            else if (Keys.Contains(Key.F12))              { Shortcut.Add(Key.F12); return true; }
            else if (Keys.Contains(Key.OemSemicolon))     { Shortcut.Add(Key.OemSemicolon); return true; }
            else if (Keys.Contains(Key.OemPlus))          { Shortcut.Add(Key.OemPlus); return true; }
            else if (Keys.Contains(Key.OemComma))         { Shortcut.Add(Key.OemComma); return true; }
            else if (Keys.Contains(Key.OemMinus))         { Shortcut.Add(Key.OemMinus); return true; }
            else if (Keys.Contains(Key.OemPeriod))        { Shortcut.Add(Key.OemPeriod); return true; }
            else if (Keys.Contains(Key.Oem2))             { Shortcut.Add(Key.Oem2); return true; }
            else if (Keys.Contains(Key.OemTilde))         { Shortcut.Add(Key.OemTilde); return true; }
            else if (Keys.Contains(Key.Oem4))             { Shortcut.Add(Key.Oem4); return true; }
            else if (Keys.Contains(Key.OemPipe))          { Shortcut.Add(Key.OemPipe); return true; }
            else if (Keys.Contains(Key.OemCloseBrackets)) { Shortcut.Add(Key.OemCloseBrackets); return true; }
            else if (Keys.Contains(Key.OemQuotes))        { Shortcut.Add(Key.OemQuotes); return true; }
            else if (Keys.Contains(Key.Oem8))             { Shortcut.Add(Key.Oem8); return true; }
            else if (Keys.Contains(Key.OemBackslash))     { Shortcut.Add(Key.OemBackslash); return true; }
            else
            {
                return false;
            }
        }
    }
}