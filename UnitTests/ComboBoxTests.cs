﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Terminal.Gui.Views {
	public class ComboBoxTests {
		ITestOutputHelper output;

		public ComboBoxTests (ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Constructors_Defaults ()
		{
			var cb = new ComboBox ();
			Assert.Equal (string.Empty, cb.Text);
			Assert.Null (cb.Source);
			Assert.False (cb.AutoSize);
			Assert.Equal (new Rect (0, 0, 0, 2), cb.Frame);

			cb = new ComboBox ("Test");
			Assert.Equal ("Test", cb.Text);
			Assert.Null (cb.Source);
			Assert.False (cb.AutoSize);
			Assert.Equal (new Rect (0, 0, 0, 2), cb.Frame);

			cb = new ComboBox (new Rect (1, 2, 10, 20), new List<string> () { "One", "Two", "Three" });
			Assert.Equal (string.Empty, cb.Text);
			Assert.NotNull (cb.Source);
			Assert.False (cb.AutoSize);
			Assert.Equal (new Rect (1, 2, 10, 20), cb.Frame);
		}

		[Fact]
		[AutoInitShutdown]
		public void EnsureKeyEventsDoNotCauseExceptions ()
		{
			var comboBox = new ComboBox ("0");

			var source = Enumerable.Range (0, 15).Select (x => x.ToString ()).ToArray ();
			comboBox.SetSource (source);

			Application.Top.Add (comboBox);

			foreach (var key in (Key [])Enum.GetValues (typeof (Key))) {
				Assert.Null (Record.Exception (() => comboBox.ProcessKey (new KeyEvent (key, new KeyModifiers ()))));
			}
		}


		[Fact]
		[AutoInitShutdown]
		public void KeyBindings_Command ()
		{
			List<string> source = new List<string> () { "One", "Two", "Three" };
			ComboBox cb = new ComboBox () { Width = 10 };
			cb.SetSource (source);
			Application.Top.Add (cb);
			Application.Top.FocusFirst ();
			Assert.Equal (-1, cb.SelectedItem);
			Assert.Equal (string.Empty, cb.Text);
			var opened = false;
			cb.OpenSelectedItem += (_) => opened = true;
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Enter, new KeyModifiers ())));
			Assert.False (opened);
			cb.Text = "Tw";
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Enter, new KeyModifiers ())));
			Assert.True (opened);
			Assert.Equal ("Two", cb.Text);
			cb.SetSource (null);
			Assert.False (cb.ProcessKey (new KeyEvent (Key.Enter, new KeyModifiers ())));
			Assert.True (cb.ProcessKey (new KeyEvent (Key.F4, new KeyModifiers ()))); // with no source also expand empty
			Assert.True (cb.IsShow);
			Assert.Equal (-1, cb.SelectedItem);
			cb.SetSource (source);
			cb.Text = "";
			Assert.True (cb.ProcessKey (new KeyEvent (Key.F4, new KeyModifiers ()))); // collapse
			Assert.False (cb.IsShow);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.F4, new KeyModifiers ()))); // expand
			Assert.True (cb.IsShow);
			cb.Collapse ();
			Assert.False (cb.IsShow);
			Assert.True (cb.HasFocus);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorDown, new KeyModifiers ()))); // losing focus
			Assert.False (cb.IsShow);
			Assert.False (cb.HasFocus);
			Application.Top.FocusFirst (); // Gets focus again
			Assert.False (cb.IsShow);
			Assert.True (cb.HasFocus);
			cb.Expand ();
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorDown, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (1, cb.SelectedItem);
			Assert.Equal ("Two", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorDown, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (2, cb.SelectedItem);
			Assert.Equal ("Three", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorDown, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (2, cb.SelectedItem);
			Assert.Equal ("Three", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorUp, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (1, cb.SelectedItem);
			Assert.Equal ("Two", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorUp, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorUp, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Application.Begin (Application.Top);
			GraphViewTests.AssertDriverContentsWithFrameAre (@"
One      ▼
One       
", output);

			Assert.True (cb.ProcessKey (new KeyEvent (Key.PageDown, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (1, cb.SelectedItem);
			Assert.Equal ("Two", cb.Text);
			Application.Begin (Application.Top);
			GraphViewTests.AssertDriverContentsWithFrameAre (@"
Two      ▼
Two       
", output);

			Assert.True (cb.ProcessKey (new KeyEvent (Key.PageDown, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (2, cb.SelectedItem);
			Assert.Equal ("Three", cb.Text);
			Application.Begin (Application.Top);
			GraphViewTests.AssertDriverContentsWithFrameAre (@"
Three    ▼
Three     
", output);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.PageUp, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (1, cb.SelectedItem);
			Assert.Equal ("Two", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.PageUp, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.F4, new KeyModifiers ())));
			Assert.False (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.End, new KeyModifiers ())));
			Assert.False (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Home, new KeyModifiers ())));
			Assert.False (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.F4, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.End, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (2, cb.SelectedItem);
			Assert.Equal ("Three", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Home, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Esc, new KeyModifiers ())));
			Assert.False (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.CursorDown, new KeyModifiers ()))); // losing focus
			Assert.False (cb.HasFocus);
			Assert.False (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Application.Top.FocusFirst (); // Gets focus again
			Assert.True (cb.HasFocus);
			Assert.False (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.U | Key.CtrlMask, new KeyModifiers ())));
			Assert.True (cb.HasFocus);
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("", cb.Text);
			Assert.Equal (3, cb.Source.Count);
		}

		[Fact]
		[AutoInitShutdown]
		public void Source_Equal_Null_Or_Count_Equal_Zero_Sets_SelectedItem_Equal_To_Minus_One ()
		{
			var cb = new ComboBox ();
			Application.Top.Add (cb);
			Application.Top.FocusFirst ();
			Assert.Null (cb.Source);
			Assert.Equal (-1, cb.SelectedItem);
			var source = new List<string> ();
			cb.SetSource (source);
			Assert.NotNull (cb.Source);
			Assert.Equal (0, cb.Source.Count);
			Assert.Equal (-1, cb.SelectedItem);
			source.Add ("One");
			Assert.Equal (1, cb.Source.Count);
			Assert.Equal (-1, cb.SelectedItem);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.F4, new KeyModifiers ())));
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			source.Add ("Two");
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("One", cb.Text);
			cb.Text = "T";
			Assert.True (cb.IsShow);
			Assert.Equal (0, cb.SelectedItem);
			Assert.Equal ("T", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Enter, new KeyModifiers ())));
			Assert.False (cb.IsShow);
			Assert.Equal (2, cb.Source.Count);
			Assert.Equal (1, cb.SelectedItem);
			Assert.Equal ("Two", cb.Text);
			Assert.True (cb.ProcessKey (new KeyEvent (Key.Esc, new KeyModifiers ())));
			Assert.False (cb.IsShow);
			Assert.Equal (1, cb.SelectedItem); // retains last accept selected item
			Assert.Equal ("", cb.Text); // clear text
			cb.SetSource (new List<string> ());
			Assert.Equal (0, cb.Source.Count);
			Assert.Equal (-1, cb.SelectedItem);
			Assert.Equal ("", cb.Text);
		}
	}
}