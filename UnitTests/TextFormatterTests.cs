﻿using NStack;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui.Views;
using Xunit;
using Xunit.Abstractions;

// Alias Console to MockConsole so we don't accidentally use Console
using Console = Terminal.Gui.FakeConsole;

namespace Terminal.Gui.Core {
	public class TextFormatterTests {
		readonly ITestOutputHelper output;

		public TextFormatterTests (ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void Basic_Usage ()
		{
			var testText = ustring.Make ("test");
			var expectedSize = new Size ();
			var testBounds = new Rect (0, 0, 100, 1);
			var tf = new TextFormatter ();

			tf.Text = testText;
			expectedSize = new Size (testText.Length, 1);
			Assert.Equal (testText, tf.Text);
			Assert.Equal (TextAlignment.Left, tf.Alignment);
			Assert.Equal (expectedSize, tf.Size);
			tf.Draw (testBounds, new Attribute (), new Attribute ());
			Assert.Equal (expectedSize, tf.Size);
			Assert.NotEmpty (tf.Lines);

			tf.Alignment = TextAlignment.Right;
			expectedSize = new Size (testText.Length, 1);
			Assert.Equal (testText, tf.Text);
			Assert.Equal (TextAlignment.Right, tf.Alignment);
			Assert.Equal (expectedSize, tf.Size);
			tf.Draw (testBounds, new Attribute (), new Attribute ());
			Assert.Equal (expectedSize, tf.Size);
			Assert.NotEmpty (tf.Lines);

			tf.Alignment = TextAlignment.Right;
			expectedSize = new Size (testText.Length * 2, 1);
			tf.Size = expectedSize;
			Assert.Equal (testText, tf.Text);
			Assert.Equal (TextAlignment.Right, tf.Alignment);
			Assert.Equal (expectedSize, tf.Size);
			tf.Draw (testBounds, new Attribute (), new Attribute ());
			Assert.Equal (expectedSize, tf.Size);
			Assert.NotEmpty (tf.Lines);

			tf.Alignment = TextAlignment.Centered;
			expectedSize = new Size (testText.Length * 2, 1);
			tf.Size = expectedSize;
			Assert.Equal (testText, tf.Text);
			Assert.Equal (TextAlignment.Centered, tf.Alignment);
			Assert.Equal (expectedSize, tf.Size);
			tf.Draw (testBounds, new Attribute (), new Attribute ());
			Assert.Equal (expectedSize, tf.Size);
			Assert.NotEmpty (tf.Lines);
		}

		[Fact]
		public void TestSize_TextChange ()
		{
			var tf = new TextFormatter () { Text = "你" };
			Assert.Equal (2, tf.Size.Width);
			tf.Text = "你你";
			Assert.Equal (4, tf.Size.Width);
		}

		[Fact]
		public void NeedsFormat_Sets ()
		{
			var testText = ustring.Make ("test");
			var testBounds = new Rect (0, 0, 100, 1);
			var tf = new TextFormatter ();

			tf.Text = "test";
			Assert.True (tf.NeedsFormat); // get_Lines causes a Format
			Assert.NotEmpty (tf.Lines);
			Assert.False (tf.NeedsFormat); // get_Lines causes a Format
			Assert.Equal (testText, tf.Text);
			tf.Draw (testBounds, new Attribute (), new Attribute ());
			Assert.False (tf.NeedsFormat);

			tf.Size = new Size (1, 1);
			Assert.True (tf.NeedsFormat);
			Assert.NotEmpty (tf.Lines);
			Assert.False (tf.NeedsFormat); // get_Lines causes a Format

			tf.Alignment = TextAlignment.Centered;
			Assert.True (tf.NeedsFormat);
			Assert.NotEmpty (tf.Lines);
			Assert.False (tf.NeedsFormat); // get_Lines causes a Format
		}

		[Fact]
		public void FindHotKey_Invalid_ReturnsFalse ()
		{
			var text = ustring.Empty;
			Rune hotKeySpecifier = '_';
			bool supportFirstUpperCase = false;
			int hotPos = 0;
			Key hotKey = Key.Unknown;
			bool result = false;

			text = null;
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "no hotkey";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "No hotkey, Upper Case";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Non-english: Сохранить";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);
		}

		[Fact]
		public void FindHotKey_AlphaUpperCase_Succeeds ()
		{
			var text = ustring.Empty;
			Rune hotKeySpecifier = '_';
			bool supportFirstUpperCase = false;
			int hotPos = 0;
			Key hotKey = Key.Unknown;
			bool result = false;

			text = "_K Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "a_K Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "Last _K";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "After K_";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Multiple _K and _R";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			// Cryllic K (К)
			text = "Non-english: _Кдать";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (13, hotPos);
			Assert.Equal ((Key)'К', hotKey);

			// Turn on FirstUpperCase and verify same results
			supportFirstUpperCase = true;
			text = "_K Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "a_K Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "Last _K";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "After K_";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Multiple _K and _R";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			// Cryllic K (К)
			text = "Non-english: _Кдать";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (13, hotPos);
			Assert.Equal ((Key)'К', hotKey);
		}
		[Fact]
		public void FindHotKey_AlphaLowerCase_Succeeds ()
		{
			var text = ustring.Empty;
			Rune hotKeySpecifier = '_';
			bool supportFirstUpperCase = false;
			int hotPos = 0;
			Key hotKey = Key.Unknown;
			bool result = false;

			// lower case should return uppercase Hotkey
			text = "_k Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "a_k Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "Last _k";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "After k_";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Multiple _k and _R";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			// Lower case Cryllic K (к)
			text = "Non-english: _кдать";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (13, hotPos);
			Assert.Equal ((Key)'К', hotKey);

			// Turn on FirstUpperCase and verify same results
			supportFirstUpperCase = true;
			text = "_k Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "a_k Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "Last _k";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "After k_";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Multiple _k and _R";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			// Lower case Cryllic K (к)
			text = "Non-english: _кдать";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (13, hotPos);
			Assert.Equal ((Key)'К', hotKey);
		}

		[Fact]
		public void FindHotKey_Numeric_Succeeds ()
		{
			var text = ustring.Empty;
			Rune hotKeySpecifier = '_';
			bool supportFirstUpperCase = false;
			int hotPos = 0;
			Key hotKey = Key.Unknown;
			bool result = false;
			// Digits 
			text = "_1 Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			text = "a_1 Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			text = "Last _1";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			text = "After 1_";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Multiple _1 and _2";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			// Turn on FirstUpperCase and verify same results
			supportFirstUpperCase = true;
			text = "_1 Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			text = "a_1 Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			text = "Last _1";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'1', hotKey);

			text = "After 1_";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "Multiple _1 and _2";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'1', hotKey);
		}

		[Fact]
		public void FindHotKey_Legacy_FirstUpperCase_Succeeds ()
		{
			bool supportFirstUpperCase = true;

			var text = ustring.Empty;
			Rune hotKeySpecifier = (Rune)0;
			int hotPos = 0;
			Key hotKey = Key.Unknown;
			bool result = false;

			text = "K Before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (0, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "aK Second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (1, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "last K";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (5, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			text = "multiple K and R";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (9, hotPos);
			Assert.Equal ((Key)'K', hotKey);

			// Cryllic K (К)
			text = "non-english: Кдать";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.True (result);
			Assert.Equal (13, hotPos);
			Assert.Equal ((Key)'К', hotKey);
		}

		[Fact]
		public void FindHotKey_Legacy_FirstUpperCase_NotFound_Returns_False ()
		{
			bool supportFirstUpperCase = true;

			var text = ustring.Empty;
			Rune hotKeySpecifier = (Rune)0;
			int hotPos = 0;
			Key hotKey = Key.Unknown;
			bool result = false;

			text = "k before";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "ak second";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "last k";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "multiple k and r";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			text = "12345";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			// punctuation
			text = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			// ~IsLetterOrDigit + Unicode
			text = " ~  s  gui.cs   master ↑10";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);

			// Lower case Cryllic K (к)
			text = "non-english: кдать";
			result = TextFormatter.FindHotKey (text, hotKeySpecifier, supportFirstUpperCase, out hotPos, out hotKey);
			Assert.False (result);
			Assert.Equal (-1, hotPos);
			Assert.Equal (Key.Unknown, hotKey);
		}

		static ustring testHotKeyAtStart = "_K Before";
		static ustring testHotKeyAtSecondPos = "a_K Second";
		static ustring testHotKeyAtLastPos = "Last _K";
		static ustring testHotKeyAfterLastChar = "After K_";
		static ustring testMultiHotKeys = "Multiple _K and _R";
		static ustring testNonEnglish = "Non-english: _Кдать";

		[Fact]
		public void RemoveHotKeySpecifier_InValid_ReturnsOriginal ()
		{
			Rune hotKeySpecifier = '_';

			Assert.Null (TextFormatter.RemoveHotKeySpecifier (null, 0, hotKeySpecifier));
			Assert.Equal ("", TextFormatter.RemoveHotKeySpecifier ("", 0, hotKeySpecifier));
			Assert.Equal ("", TextFormatter.RemoveHotKeySpecifier ("", -1, hotKeySpecifier));
			Assert.Equal ("", TextFormatter.RemoveHotKeySpecifier ("", 100, hotKeySpecifier));

			Assert.Equal ("a", TextFormatter.RemoveHotKeySpecifier ("a", -1, hotKeySpecifier));
			Assert.Equal ("a", TextFormatter.RemoveHotKeySpecifier ("a", 100, hotKeySpecifier));
		}

		[Fact]
		public void RemoveHotKeySpecifier_Valid_ReturnsStripped ()
		{
			Rune hotKeySpecifier = '_';

			Assert.Equal ("K Before", TextFormatter.RemoveHotKeySpecifier ("_K Before", 0, hotKeySpecifier));
			Assert.Equal ("aK Second", TextFormatter.RemoveHotKeySpecifier ("a_K Second", 1, hotKeySpecifier));
			Assert.Equal ("Last K", TextFormatter.RemoveHotKeySpecifier ("Last _K", 5, hotKeySpecifier));
			Assert.Equal ("After K", TextFormatter.RemoveHotKeySpecifier ("After K_", 7, hotKeySpecifier));
			Assert.Equal ("Multiple K and _R", TextFormatter.RemoveHotKeySpecifier ("Multiple _K and _R", 9, hotKeySpecifier));
			Assert.Equal ("Non-english: Кдать", TextFormatter.RemoveHotKeySpecifier ("Non-english: _Кдать", 13, hotKeySpecifier));
		}

		[Fact]
		public void RemoveHotKeySpecifier_Valid_Legacy_ReturnsOriginal ()
		{
			Rune hotKeySpecifier = '_';

			Assert.Equal ("all lower case", TextFormatter.RemoveHotKeySpecifier ("all lower case", 0, hotKeySpecifier));
			Assert.Equal ("K Before", TextFormatter.RemoveHotKeySpecifier ("K Before", 0, hotKeySpecifier));
			Assert.Equal ("aK Second", TextFormatter.RemoveHotKeySpecifier ("aK Second", 1, hotKeySpecifier));
			Assert.Equal ("Last K", TextFormatter.RemoveHotKeySpecifier ("Last K", 5, hotKeySpecifier));
			Assert.Equal ("After K", TextFormatter.RemoveHotKeySpecifier ("After K", 7, hotKeySpecifier));
			Assert.Equal ("Multiple K and R", TextFormatter.RemoveHotKeySpecifier ("Multiple K and R", 9, hotKeySpecifier));
			Assert.Equal ("Non-english: Кдать", TextFormatter.RemoveHotKeySpecifier ("Non-english: Кдать", 13, hotKeySpecifier));
		}

		[Fact]
		public void CalcRect_Invalid_Returns_Empty ()
		{
			Assert.Equal (Rect.Empty, TextFormatter.CalcRect (0, 0, null));
			Assert.Equal (Rect.Empty, TextFormatter.CalcRect (0, 0, ""));
			Assert.Equal (new Rect (new Point (1, 2), Size.Empty), TextFormatter.CalcRect (1, 2, ""));
			Assert.Equal (new Rect (new Point (-1, -2), Size.Empty), TextFormatter.CalcRect (-1, -2, ""));
		}

		[Fact]
		public void CalcRect_SingleLine_Returns_1High ()
		{
			var text = ustring.Empty;

			text = "test";
			Assert.Equal (new Rect (0, 0, text.RuneCount, 1), TextFormatter.CalcRect (0, 0, text));
			Assert.Equal (new Rect (0, 0, text.ConsoleWidth, 1), TextFormatter.CalcRect (0, 0, text));

			text = " ~  s  gui.cs   master ↑10";
			Assert.Equal (new Rect (0, 0, text.RuneCount, 1), TextFormatter.CalcRect (0, 0, text));
			Assert.Equal (new Rect (0, 0, text.ConsoleWidth, 1), TextFormatter.CalcRect (0, 0, text));
		}

		[Fact]
		public void CalcRect_MultiLine_Returns_nHigh ()
		{
			var text = ustring.Empty;
			var lines = 0;

			text = "line1\nline2";
			lines = 2;
			Assert.Equal (new Rect (0, 0, 5, lines), TextFormatter.CalcRect (0, 0, text));

			text = "\nline2";
			lines = 2;
			Assert.Equal (new Rect (0, 0, 5, lines), TextFormatter.CalcRect (0, 0, text));

			text = "\n\n";
			lines = 3;
			Assert.Equal (new Rect (0, 0, 0, lines), TextFormatter.CalcRect (0, 0, text));

			text = "\n\n\n";
			lines = 4;
			Assert.Equal (new Rect (0, 0, 0, lines), TextFormatter.CalcRect (0, 0, text));

			text = "line1\nline2\nline3long!";
			lines = 3;
			Assert.Equal (new Rect (0, 0, 10, lines), TextFormatter.CalcRect (0, 0, text));

			text = "line1\nline2\n\n";
			lines = 4;
			Assert.Equal (new Rect (0, 0, 5, lines), TextFormatter.CalcRect (0, 0, text));

			text = "line1\r\nline2";
			lines = 2;
			Assert.Equal (new Rect (0, 0, 5, lines), TextFormatter.CalcRect (0, 0, text));

			text = " ~  s  gui.cs   master ↑10\n";
			lines = 2;
			Assert.Equal (new Rect (0, 0, text.RuneCount - 1, lines), TextFormatter.CalcRect (0, 0, text));
			Assert.Equal (new Rect (0, 0, text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 0)), lines), TextFormatter.CalcRect (0, 0, text));

			text = "\n ~  s  gui.cs   master ↑10";
			lines = 2;
			Assert.Equal (new Rect (0, 0, text.RuneCount - 1, lines), TextFormatter.CalcRect (0, 0, text));
			Assert.Equal (new Rect (0, 0, text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 0)), lines), TextFormatter.CalcRect (0, 0, text));

			text = " ~  s  gui.cs   master\n↑10";
			lines = 2;
			Assert.Equal (new Rect (0, 0, ustring.Make (" ~  s  gui.cs   master\n").RuneCount - 1, lines), TextFormatter.CalcRect (0, 0, text));
			Assert.Equal (new Rect (0, 0, ustring.Make (" ~  s  gui.cs   master\n").ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 0)), lines), TextFormatter.CalcRect (0, 0, text));
		}

		[Fact]
		public void ClipAndJustify_Invalid_Returns_Original ()
		{
			var text = ustring.Empty;

			Assert.Equal (text, TextFormatter.ClipAndJustify (text, 0, TextAlignment.Left));

			text = null;
			Assert.Equal (text, TextFormatter.ClipAndJustify (text, 0, TextAlignment.Left));

			text = "test";
			Assert.Throws<ArgumentOutOfRangeException> (() => TextFormatter.ClipAndJustify (text, -1, TextAlignment.Left));
		}

		[Fact]
		public void ClipAndJustify_Valid_Left ()
		{
			var align = TextAlignment.Left;

			var text = ustring.Empty;
			var justifiedText = ustring.Empty;
			int maxWidth = 0;
			int expectedClippedWidth = 0;

			text = "test";
			maxWidth = 0;
			Assert.Equal (ustring.Empty, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = 2;
			Assert.Equal (text.ToRunes () [0..maxWidth], justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = int.MaxValue;
			Assert.Equal (text, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));
			Assert.True (justifiedText.RuneCount <= maxWidth);
			Assert.True (justifiedText.ConsoleWidth <= maxWidth);

			text = "A sentence has words.";
			// should fit
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);


			text = "A\tsentence\thas\twords.";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "A\tsentence\thas\twords.";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Unicode
			text = " ~  s  gui.cs   master ↑10";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// should fit
			text = "Ð ÑÐ";
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);
		}

		[Fact]
		public void ClipAndJustify_Valid_Right ()
		{
			var align = TextAlignment.Right;

			var text = ustring.Empty;
			var justifiedText = ustring.Empty;
			int maxWidth = 0;
			int expectedClippedWidth = 0;

			text = "test";
			maxWidth = 0;
			Assert.Equal (ustring.Empty, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = 2;
			Assert.Equal (text.ToRunes () [0..maxWidth], justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = int.MaxValue;
			Assert.Equal (text, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));
			Assert.True (justifiedText.RuneCount <= maxWidth);
			Assert.True (justifiedText.ConsoleWidth <= maxWidth);

			text = "A sentence has words.";
			// should fit
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);


			text = "A\tsentence\thas\twords.";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "A\tsentence\thas\twords.";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Unicode
			text = " ~  s  gui.cs   master ↑10";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// should fit
			text = "Ð ÑÐ";
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);
		}

		[Fact]
		public void ClipAndJustify_Valid_Centered ()
		{
			var align = TextAlignment.Centered;

			var text = ustring.Empty;
			var justifiedText = ustring.Empty;
			int maxWidth = 0;
			int expectedClippedWidth = 0;

			text = "test";
			maxWidth = 0;
			Assert.Equal (ustring.Empty, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = 2;
			Assert.Equal (text.ToRunes () [0..maxWidth], justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = int.MaxValue;
			Assert.Equal (text, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));
			Assert.True (justifiedText.RuneCount <= maxWidth);
			Assert.True (justifiedText.ConsoleWidth <= maxWidth);

			text = "A sentence has words.";
			// should fit
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "A\tsentence\thas\twords.";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "A\tsentence\thas\twords.";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Unicode
			text = " ~  s  gui.cs   master ↑10";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// should fit
			text = "Ð ÑÐ";
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);
		}


		[Fact]
		public void ClipAndJustify_Valid_Justified ()
		{
			var align = TextAlignment.Justified;

			var text = ustring.Empty;
			var justifiedText = ustring.Empty;
			int maxWidth = 0;
			int expectedClippedWidth = 0;

			text = "test";
			maxWidth = 0;
			Assert.Equal (ustring.Empty, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = 2;
			Assert.Equal (text.ToRunes () [0..maxWidth], justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));

			text = "test";
			maxWidth = int.MaxValue;
			Assert.Equal (text, justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align));
			Assert.True (justifiedText.RuneCount <= maxWidth);

			text = "A sentence has words.";
			// should fit
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = 500;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			//Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);


			text = "A\tsentence\thas\twords.";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "A\tsentence\thas\twords.";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = int.MaxValue;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			text = "line1\nline2\nline3long!";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)));
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Unicode
			text = " ~  s  gui.cs   master ↑10";
			maxWidth = 10;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// should fit
			text = "Ð ÑÐ";
			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			//Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.True (expectedClippedWidth <= maxWidth);
			//Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should fit.
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// Should not fit
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			justifiedText = TextFormatter.ClipAndJustify (text, maxWidth, align);
			Assert.Equal (expectedClippedWidth, justifiedText.RuneCount);
			Assert.Equal (expectedClippedWidth, justifiedText.ConsoleWidth);
			Assert.True (expectedClippedWidth <= maxWidth);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), justifiedText);

			// see Justify_ tests below

		}

		[Fact]
		public void Justify_Invalid ()
		{
			var text = ustring.Empty;
			Assert.Equal (text, TextFormatter.Justify (text, 0));

			text = null;
			Assert.Equal (text, TextFormatter.Justify (text, 0));

			text = "test";
			Assert.Throws<ArgumentOutOfRangeException> (() => TextFormatter.Justify (text, -1));
		}

		[Fact]
		public void Justify_SingleWord ()
		{
			var text = ustring.Empty;
			var justifiedText = ustring.Empty;
			int width = 0;
			char fillChar = '+';

			// Even # of chars
			text = "word";
			justifiedText = text;

			width = text.RuneCount;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 10;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 11;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());

			// Odd # of chars
			text = "word.";
			justifiedText = text;

			width = text.RuneCount;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 10;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 11;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());


			// Unicode (even #)
			text = "Ð¿ÑÐ¸Ð²ÐµÑ";
			justifiedText = text;

			width = text.RuneCount;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 10;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 11;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());

			// Unicode (odd # of chars)
			text = "Ð¿ÑÐ¸Ð²ÐµÑ.";
			justifiedText = text;

			width = text.RuneCount;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 10;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
			width = text.RuneCount + 11;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, width, fillChar).ToString ());
		}


		[Fact]
		public void Justify_Sentence ()
		{
			var text = ustring.Empty;
			var justifiedText = ustring.Empty;
			int forceToWidth = 0;
			char fillChar = '+';

			// Even # of spaces
			//      0123456789
			text = "012 456 89";

			forceToWidth = text.RuneCount;
			justifiedText = text.Replace (" ", "+");
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+");
			forceToWidth = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 3;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++");
			forceToWidth = text.RuneCount + 4;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++");
			forceToWidth = text.RuneCount + 5;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++++");
			forceToWidth = text.RuneCount + 6;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++++++++++");
			forceToWidth = text.RuneCount + 20;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++++++++++++");
			forceToWidth = text.RuneCount + 23;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			// Odd # of spaces
			//      0123456789
			text = "012 456 89 end";

			forceToWidth = text.RuneCount;
			justifiedText = text.Replace (" ", "+");
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+");
			forceToWidth = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+");
			forceToWidth = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 3;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 4;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 5;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++");
			forceToWidth = text.RuneCount + 6;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++++++");
			forceToWidth = text.RuneCount + 20;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++++++++");
			forceToWidth = text.RuneCount + 23;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			// Unicode
			// Even # of chars
			//      0123456789
			text = "Ð¿ÑÐ Ð²Ð Ñ";

			forceToWidth = text.RuneCount;
			justifiedText = text.Replace (" ", "+");
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+");
			forceToWidth = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 3;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++");
			forceToWidth = text.RuneCount + 4;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++");
			forceToWidth = text.RuneCount + 5;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++++");
			forceToWidth = text.RuneCount + 6;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++++++++++");
			forceToWidth = text.RuneCount + 20;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++++++++++++");
			forceToWidth = text.RuneCount + 23;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			// Unicode
			// Odd # of chars
			//      0123456789
			text = "Ð ÑÐ Ð²Ð Ñ";

			forceToWidth = text.RuneCount;
			justifiedText = text.Replace (" ", "+");
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+");
			forceToWidth = text.RuneCount + 1;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+");
			forceToWidth = text.RuneCount + 2;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 3;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 4;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++");
			forceToWidth = text.RuneCount + 5;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++");
			forceToWidth = text.RuneCount + 6;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "+++++++");
			forceToWidth = text.RuneCount + 20;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));

			justifiedText = text.Replace (" ", "++++++++");
			forceToWidth = text.RuneCount + 23;
			Assert.Equal (justifiedText.ToString (), TextFormatter.Justify (text, forceToWidth, fillChar).ToString ());
			Assert.True (Math.Abs (forceToWidth - justifiedText.RuneCount) < text.Count (" "));
			Assert.True (Math.Abs (forceToWidth - justifiedText.ConsoleWidth) < text.Count (" "));
		}

		[Fact]
		public void WordWrap_Invalid ()
		{
			var text = ustring.Empty;
			int width = 0;

			Assert.Empty (TextFormatter.WordWrap (null, width));
			Assert.Empty (TextFormatter.WordWrap (text, width));
			Assert.Throws<ArgumentOutOfRangeException> (() => TextFormatter.WordWrap (text, -1));
		}

		[Fact]
		public void WordWrap_SingleWordLine ()
		{
			var text = ustring.Empty;
			int width = 0;
			List<ustring> wrappedLines;

			text = "Constantinople";
			width = text.RuneCount;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.True (wrappedLines.Count == 1);

			width = text.RuneCount - 1;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal (text [0, text.RuneCount - 1].ToString (), wrappedLines [0].ToString ());
			Assert.Equal ("e", wrappedLines [1].ToString ());

			width = text.RuneCount - 2;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal (text [0, text.RuneCount - 2].ToString (), wrappedLines [0].ToString ());

			width = text.RuneCount - 5;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);

			width = (int)Math.Ceiling ((double)(text.RuneCount / 2F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal ("Constan", wrappedLines [0].ToString ());
			Assert.Equal ("tinople", wrappedLines [1].ToString ());

			width = (int)Math.Ceiling ((double)(text.RuneCount / 3F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (3, wrappedLines.Count);
			Assert.Equal ("Const", wrappedLines [0].ToString ());
			Assert.Equal ("antin", wrappedLines [1].ToString ());
			Assert.Equal ("ople", wrappedLines [2].ToString ());

			width = (int)Math.Ceiling ((double)(text.RuneCount / 4F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (4, wrappedLines.Count);

			width = (int)Math.Ceiling ((double)text.RuneCount / text.RuneCount);
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (text.RuneCount, wrappedLines.Count);
			Assert.Equal (text.ConsoleWidth, wrappedLines.Count);
			Assert.Equal ("C", wrappedLines [0].ToString ());
			Assert.Equal ("o", wrappedLines [1].ToString ());
			Assert.Equal ("n", wrappedLines [2].ToString ());
			Assert.Equal ("s", wrappedLines [3].ToString ());
			Assert.Equal ("t", wrappedLines [4].ToString ());
			Assert.Equal ("a", wrappedLines [5].ToString ());
			Assert.Equal ("n", wrappedLines [6].ToString ());
			Assert.Equal ("t", wrappedLines [7].ToString ());
			Assert.Equal ("i", wrappedLines [8].ToString ());
			Assert.Equal ("n", wrappedLines [9].ToString ());
			Assert.Equal ("o", wrappedLines [10].ToString ());
			Assert.Equal ("p", wrappedLines [11].ToString ());
			Assert.Equal ("l", wrappedLines [12].ToString ());
			Assert.Equal ("e", wrappedLines [13].ToString ());
		}

		[Fact]
		public void WordWrap_Unicode_SingleWordLine ()
		{
			var text = ustring.Empty;
			int width = 0;
			List<ustring> wrappedLines;

			text = "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำ";
			width = text.RuneCount;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.True (wrappedLines.Count == 1);

			width = text.RuneCount - 1;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.RuneCount - 1)]).ToString (), wrappedLines [0].ToString ());
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)) - 1)]).ToString (), wrappedLines [0].ToString ());
			Assert.Equal ("ำ", wrappedLines [1].ToString ());

			width = text.RuneCount - 2;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.RuneCount - 2)]).ToString (), wrappedLines [0].ToString ());
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)) - 2)]).ToString (), wrappedLines [0].ToString ());

			width = text.RuneCount - 5;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);

			width = (int)Math.Ceiling ((double)(text.RuneCount / 2F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal ("กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบ", wrappedLines [0].ToString ());
			Assert.Equal ("ปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำ", wrappedLines [1].ToString ());

			width = (int)Math.Ceiling ((double)(text.RuneCount / 3F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (3, wrappedLines.Count);
			Assert.Equal ("กขฃคฅฆงจฉชซฌญฎฏฐฑ", wrappedLines [0].ToString ());
			Assert.Equal ("ฒณดตถทธนบปผฝพฟภมย", wrappedLines [1].ToString ());
			Assert.Equal ("รฤลฦวศษสหฬอฮฯะัาำ", wrappedLines [2].ToString ());

			width = (int)Math.Ceiling ((double)(text.RuneCount / 4F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (4, wrappedLines.Count);

			width = (int)Math.Ceiling ((double)text.RuneCount / text.RuneCount);
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (text.RuneCount, wrappedLines.Count);
			Assert.Equal (text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)), wrappedLines.Count);
			Assert.Equal ("ก", wrappedLines [0].ToString ());
			Assert.Equal ("ข", wrappedLines [1].ToString ());
			Assert.Equal ("ฃ", wrappedLines [2].ToString ());
			Assert.Equal ("ำ", wrappedLines [^1].ToString ());
		}

		[Fact]
		public void WordWrap_Unicode_LineWithNonBreakingSpace ()
		{
			var text = ustring.Empty;
			int width = 0;
			List<ustring> wrappedLines;

			text = "This\u00A0is\u00A0a\u00A0sentence.";
			width = text.RuneCount;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.True (wrappedLines.Count == 1);

			width = text.RuneCount - 1;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.RuneCount - 1)]).ToString (), wrappedLines [0].ToString ());
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)) - 1)]).ToString (), wrappedLines [0].ToString ());
			Assert.Equal (".", wrappedLines [1].ToString ());

			width = text.RuneCount - 2;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.RuneCount - 2)]).ToString (), wrappedLines [0].ToString ());
			Assert.Equal (ustring.Make (text.ToRunes () [0..(text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)) - 2)]).ToString (), wrappedLines [0].ToString ());

			width = text.RuneCount - 5;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);

			width = (int)Math.Ceiling ((double)(text.RuneCount / 2F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (2, wrappedLines.Count);
			Assert.Equal ("This\u00A0is\u00A0a\u00A0", wrappedLines [0].ToString ());
			Assert.Equal ("sentence.", wrappedLines [1].ToString ());

			width = (int)Math.Ceiling ((double)(text.RuneCount / 3F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (3, wrappedLines.Count);
			Assert.Equal ("This\u00A0is", wrappedLines [0].ToString ());
			Assert.Equal ("\u00a0a\u00a0sent", wrappedLines [1].ToString ());
			Assert.Equal ("ence.", wrappedLines [2].ToString ());

			width = (int)Math.Ceiling ((double)(text.RuneCount / 4F));
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (4, wrappedLines.Count);

			width = (int)Math.Ceiling ((double)text.RuneCount / text.RuneCount);
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (text.RuneCount, wrappedLines.Count);
			Assert.Equal (text.ToRuneList ().Sum (r => Math.Max (Rune.ColumnWidth (r), 1)), wrappedLines.Count);
			Assert.Equal ("T", wrappedLines [0].ToString ());
			Assert.Equal ("h", wrappedLines [1].ToString ());
			Assert.Equal ("i", wrappedLines [2].ToString ());
			Assert.Equal (".", wrappedLines [^1].ToString ());
		}

		[Fact]
		public void WordWrap_Unicode_2LinesWithNonBreakingSpace ()
		{
			var text = ustring.Empty;
			int width = 0;
			List<ustring> wrappedLines;

			text = "This\u00A0is\n\u00A0a\u00A0sentence.";
			width = text.RuneCount;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.True (wrappedLines.Count == 1);

			width = text.RuneCount - 1;
			wrappedLines = TextFormatter.WordWrap (text, width);
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
			Assert.Equal (1, wrappedLines.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
			Assert.Equal ("This\u00A0is\u00A0a\u00A0sentence.", wrappedLines [0].ToString ());

			text = "\u00A0\u00A0\u00A0\u00A0\u00A0test\u00A0sentence.";
			width = text.RuneCount;
			wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.True (wrappedLines.Count == 1);
		}

		[Fact]
		public void WordWrap_NoNewLines ()
		{
			var text = ustring.Empty;
			int maxWidth = 0;
			int expectedClippedWidth = 0;

			List<ustring> wrappedLines;

			text = "A sentence has words.";
			maxWidth = text.RuneCount;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.True (wrappedLines.Count == 1);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has words.", wrappedLines [0].ToString ());

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has", wrappedLines [0].ToString ());
			Assert.Equal ("words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has", wrappedLines [0].ToString ());
			Assert.Equal ("words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - " words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has", wrappedLines [0].ToString ());
			Assert.Equal ("words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "s words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence", wrappedLines [0].ToString ());
			Assert.Equal ("has words.", wrappedLines [1].ToString ());

			// Unicode 
			text = "A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.";
			maxWidth = text.RuneCount;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.True (wrappedLines.Count == 1);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.", wrappedLines [0].ToString ());

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has", wrappedLines [0].ToString ());
			Assert.Equal ("words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has", wrappedLines [0].ToString ());
			Assert.Equal ("words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - " words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has", wrappedLines [0].ToString ());
			Assert.Equal ("words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "s words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ)", wrappedLines [0].ToString ());
			Assert.Equal ("has words.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "Ð²ÐµÑ) has words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence", wrappedLines [0].ToString ());
			Assert.Equal ("(Ð¿ÑÐ¸Ð²ÐµÑ) has words.", wrappedLines [1].ToString ());
		}

		/// <summary>
		/// WordWrap strips CRLF
		/// </summary>
		[Fact]
		public void WordWrap_WithNewLines ()
		{
			var text = ustring.Empty;
			int maxWidth = 0;
			int expectedClippedWidth = 0;

			List<ustring> wrappedLines;

			text = "A sentence has words.\nA paragraph has lines.";
			maxWidth = text.RuneCount;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
			Assert.Equal (1, wrappedLines.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.Equal ("A sentence has words.A paragraph has lines.", wrappedLines [0].ToString ());

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
			Assert.Equal (1, wrappedLines.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has words.A paragraph has lines.", wrappedLines [0].ToString ());

			maxWidth = text.RuneCount - "words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has words.A paragraph has", wrappedLines [0].ToString ());
			Assert.Equal ("lines.", wrappedLines [1].ToString ());

			// Unicode 
			text = "A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.A Unicode Пункт has Линии.";
			maxWidth = text.RuneCount;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.True (wrappedLines.Count == 1);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.A Unicode Пункт has Линии.", wrappedLines [0].ToString ());

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.A Unicode Пункт has", wrappedLines [0].ToString ());
			Assert.Equal ("Линии.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.A Unicode Пункт has", wrappedLines [0].ToString ());
			Assert.Equal ("Линии.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "s words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.A Unicode Пункт", wrappedLines [0].ToString ());
			Assert.Equal ("has Линии.", wrappedLines [1].ToString ());

			maxWidth = text.RuneCount - "Ð²ÐµÑ) has words.".Length;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.Equal (2, wrappedLines.Count);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A Unicode sentence (Ð¿ÑÐ¸Ð²ÐµÑ) has words.A Unicode", wrappedLines [0].ToString ());
			Assert.Equal ("Пункт has Линии.", wrappedLines [1].ToString ());
		}

		[Fact]
		public void WordWrap_Narrow ()
		{
			var text = ustring.Empty;
			int maxWidth = 1;
			int expectedClippedWidth = 1;

			List<ustring> wrappedLines;

			text = "A sentence has words.";
			maxWidth = 3;
			expectedClippedWidth = 3;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A", wrappedLines [0].ToString ());
			Assert.Equal ("sen", wrappedLines [1].ToString ());
			Assert.Equal ("ten", wrappedLines [2].ToString ());
			Assert.Equal ("ce", wrappedLines [3].ToString ());
			Assert.Equal ("has", wrappedLines [4].ToString ());
			Assert.Equal ("wor", wrappedLines [5].ToString ());
			Assert.Equal ("ds.", wrappedLines [6].ToString ());

			maxWidth = 2;
			expectedClippedWidth = 2;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A", wrappedLines [0].ToString ());
			Assert.Equal ("se", wrappedLines [1].ToString ());
			Assert.Equal ("nt", wrappedLines [2].ToString ());
			Assert.Equal ("en", wrappedLines [3].ToString ());
			Assert.Equal ("s.", wrappedLines [^1].ToString ());

			maxWidth = 1;
			expectedClippedWidth = 1;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A", wrappedLines [0].ToString ());
			Assert.Equal ("s", wrappedLines [1].ToString ());
			Assert.Equal ("e", wrappedLines [2].ToString ());
			Assert.Equal ("n", wrappedLines [3].ToString ());
			Assert.Equal (".", wrappedLines [^1].ToString ());
		}

		[Fact]
		public void WordWrap_preserveTrailingSpaces ()
		{
			var text = ustring.Empty;
			int maxWidth = 1;
			int expectedClippedWidth = 1;

			List<ustring> wrappedLines;

			text = "A sentence has words.";
			maxWidth = 14;
			expectedClippedWidth = 14;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A sentence has", wrappedLines [0].ToString ());
			Assert.Equal (" words.", wrappedLines [1].ToString ());
			Assert.True (wrappedLines.Count == 2);

			maxWidth = 3;
			expectedClippedWidth = 3;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A ", wrappedLines [0].ToString ());
			Assert.Equal ("sen", wrappedLines [1].ToString ());
			Assert.Equal ("ten", wrappedLines [2].ToString ());
			Assert.Equal ("ce ", wrappedLines [3].ToString ());
			Assert.Equal ("has", wrappedLines [4].ToString ());
			Assert.Equal (" ", wrappedLines [5].ToString ());
			Assert.Equal ("wor", wrappedLines [6].ToString ());
			Assert.Equal ("ds.", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == 8);

			maxWidth = 2;
			expectedClippedWidth = 2;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A ", wrappedLines [0].ToString ());
			Assert.Equal ("se", wrappedLines [1].ToString ());
			Assert.Equal ("nt", wrappedLines [2].ToString ());
			Assert.Equal ("en", wrappedLines [3].ToString ());
			Assert.Equal ("ce", wrappedLines [4].ToString ());
			Assert.Equal (" ", wrappedLines [5].ToString ());
			Assert.Equal ("ha", wrappedLines [6].ToString ());
			Assert.Equal ("s ", wrappedLines [7].ToString ());
			Assert.Equal ("wo", wrappedLines [8].ToString ());
			Assert.Equal ("rd", wrappedLines [9].ToString ());
			Assert.Equal ("s.", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == 11);

			maxWidth = 1;
			expectedClippedWidth = 1;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("A", wrappedLines [0].ToString ());
			Assert.Equal (" ", wrappedLines [1].ToString ());
			Assert.Equal ("s", wrappedLines [2].ToString ());
			Assert.Equal ("e", wrappedLines [3].ToString ());
			Assert.Equal ("n", wrappedLines [4].ToString ());
			Assert.Equal ("t", wrappedLines [5].ToString ());
			Assert.Equal ("e", wrappedLines [6].ToString ());
			Assert.Equal ("n", wrappedLines [7].ToString ());
			Assert.Equal ("c", wrappedLines [8].ToString ());
			Assert.Equal ("e", wrappedLines [9].ToString ());
			Assert.Equal (" ", wrappedLines [10].ToString ());
			Assert.Equal (".", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == text.Length);
		}

		[Fact]
		public void WordWrap_preserveTrailingSpaces_Wide_Runes ()
		{
			var text = ustring.Empty;
			int maxWidth = 1;
			int expectedClippedWidth = 1;

			List<ustring> wrappedLines;

			text = "文に は言葉 があり ます。";
			maxWidth = 14;
			expectedClippedWidth = 14;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("文に は言葉 ", wrappedLines [0].ToString ());
			Assert.Equal ("があり ます。", wrappedLines [1].ToString ());
			Assert.True (wrappedLines.Count == 2);

			maxWidth = 3;
			expectedClippedWidth = 3;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("文", wrappedLines [0].ToString ());
			Assert.Equal ("に ", wrappedLines [1].ToString ());
			Assert.Equal ("は", wrappedLines [2].ToString ());
			Assert.Equal ("言", wrappedLines [3].ToString ());
			Assert.Equal ("葉 ", wrappedLines [4].ToString ());
			Assert.Equal ("が", wrappedLines [5].ToString ());
			Assert.Equal ("あ", wrappedLines [6].ToString ());
			Assert.Equal ("り ", wrappedLines [7].ToString ());
			Assert.Equal ("ま", wrappedLines [8].ToString ());
			Assert.Equal ("す", wrappedLines [9].ToString ());
			Assert.Equal ("。", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == 11);

			maxWidth = 2;
			expectedClippedWidth = 2;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.ConsoleWidth));
			Assert.Equal ("文", wrappedLines [0].ToString ());
			Assert.Equal ("に", wrappedLines [1].ToString ());
			Assert.Equal (" ", wrappedLines [2].ToString ());
			Assert.Equal ("は", wrappedLines [3].ToString ());
			Assert.Equal ("言", wrappedLines [4].ToString ());
			Assert.Equal ("葉", wrappedLines [5].ToString ());
			Assert.Equal (" ", wrappedLines [6].ToString ());
			Assert.Equal ("が", wrappedLines [7].ToString ());
			Assert.Equal ("あ", wrappedLines [8].ToString ());
			Assert.Equal ("り", wrappedLines [9].ToString ());
			Assert.Equal (" ", wrappedLines [10].ToString ());
			Assert.Equal ("ま", wrappedLines [11].ToString ());
			Assert.Equal ("す", wrappedLines [12].ToString ());
			Assert.Equal ("。", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == 14);

			maxWidth = 1;
			expectedClippedWidth = 0;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true);
			Assert.Empty (wrappedLines);
			Assert.False (wrappedLines.Count == text.Length);
			Assert.False (wrappedLines.Count == text.RuneCount);
			Assert.False (wrappedLines.Count == text.ConsoleWidth);
			Assert.Equal (25, text.ConsoleWidth);
			Assert.Equal (25, TextFormatter.GetTextWidth (text));
		}

		[Fact, AutoInitShutdown]
		public void WordWrap_preserveTrailingSpaces_Horizontal_With_Simple_Runes ()
		{
			var text = "A sentence has words.";
			var width = 3;
			var height = 8;
			var wrappedLines = TextFormatter.WordWrap (text, width, true);
			var breakLines = "";
			foreach (var line in wrappedLines) {
				breakLines += $"{line}{Environment.NewLine}";
			}
			var label = new Label (breakLines) { Width = Dim.Fill (), Height = Dim.Fill () };
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (label);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (width + 2, height + 2);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, width, height + 1), label.Frame);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), frame.Frame);

			var expected = @"
┌───┐
│A  │
│sen│
│ten│
│ce │
│has│
│   │
│wor│
│ds.│
└───┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), pos);
		}

		[Fact, AutoInitShutdown]
		public void WordWrap_preserveTrailingSpaces_Vertical_With_Simple_Runes ()
		{
			var text = "A sentence has words.";
			var width = 8;
			var height = 3;
			var wrappedLines = TextFormatter.WordWrap (text, height, true);
			var breakLines = "";
			for (int i = 0; i < wrappedLines.Count; i++) {
				breakLines += $"{wrappedLines [i]}{(i < wrappedLines.Count - 1 ? Environment.NewLine : string.Empty)}";
			}
			var label = new Label (breakLines) {
				TextDirection = TextDirection.TopBottom_LeftRight,
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (label);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (width + 2, height + 2);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, width, height), label.Frame);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), frame.Frame);

			var expected = @"
┌────────┐
│Astch wd│
│ eeea os│
│ nn s r.│
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), pos);
		}

		[Fact, AutoInitShutdown]
		public void WordWrap_preserveTrailingSpaces_Horizontal_With_Wide_Runes ()
		{
			var text = "文に は言葉 があり ます。";
			var width = 6;
			var height = 8;
			var wrappedLines = TextFormatter.WordWrap (text, width, true);
			var breakLines = "";
			foreach (var line in wrappedLines) {
				breakLines += $"{line}{Environment.NewLine}";
			}
			var label = new Label (breakLines) { Width = Dim.Fill (), Height = Dim.Fill () };
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (label);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (width + 2, height + 2);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, width, height), label.Frame);
			Assert.Equal (new Size (width, height), label.TextFormatter.Size);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), frame.Frame);

			var expected = @"
┌──────┐
│文に  │
│は言葉│
│ があ │
│り ま │
│す。  │
│      │
│      │
│      │
└──────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), pos);
		}

		[Fact, AutoInitShutdown]
		public void WordWrap_preserveTrailingSpaces_Vertical_With_Wide_Runes ()
		{
			var text = "文に は言葉 があり ます。";
			var width = 8;
			var height = 4;
			var wrappedLines = TextFormatter.WordWrap (text, width, true);
			var breakLines = "";
			for (int i = 0; i < wrappedLines.Count; i++) {
				breakLines += $"{wrappedLines [i]}{(i < wrappedLines.Count - 1 ? Environment.NewLine : string.Empty)}";
			}
			var label = new Label (breakLines) {
				TextDirection = TextDirection.TopBottom_LeftRight,
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (label);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (width + 2, height + 2);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, width, height), label.Frame);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), frame.Frame);

			var expected = @"
┌────────┐
│文言あす│
│に葉り。│
│        │
│はがま  │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, width + 2, height + 2), pos);
		}

		[Fact]
		public void WordWrap_preserveTrailingSpaces_With_Tab ()
		{
			var text = ustring.Empty;
			int maxWidth = 1;
			int expectedClippedWidth = 1;

			List<ustring> wrappedLines;

			text = "A sentence\t\t\t has words.";
			var tabWidth = 4;
			maxWidth = 14;
			expectedClippedWidth = 11;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true, tabWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.Equal ("A sentence\t", wrappedLines [0].ToString ());
			Assert.Equal ("\t\t has ", wrappedLines [1].ToString ());
			Assert.Equal ("words.", wrappedLines [2].ToString ());
			Assert.True (wrappedLines.Count == 3);

			maxWidth = 3;
			expectedClippedWidth = 3;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true, tabWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.Equal ("A ", wrappedLines [0].ToString ());
			Assert.Equal ("sen", wrappedLines [1].ToString ());
			Assert.Equal ("ten", wrappedLines [2].ToString ());
			Assert.Equal ("ce", wrappedLines [3].ToString ());
			Assert.Equal ("\t", wrappedLines [4].ToString ());
			Assert.Equal ("\t", wrappedLines [5].ToString ());
			Assert.Equal ("\t", wrappedLines [6].ToString ());
			Assert.Equal (" ", wrappedLines [7].ToString ());
			Assert.Equal ("has", wrappedLines [8].ToString ());
			Assert.Equal (" ", wrappedLines [9].ToString ());
			Assert.Equal ("wor", wrappedLines [10].ToString ());
			Assert.Equal ("ds.", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == 12);

			maxWidth = 2;
			expectedClippedWidth = 2;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true, tabWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.Equal ("A ", wrappedLines [0].ToString ());
			Assert.Equal ("se", wrappedLines [1].ToString ());
			Assert.Equal ("nt", wrappedLines [2].ToString ());
			Assert.Equal ("en", wrappedLines [3].ToString ());
			Assert.Equal ("ce", wrappedLines [4].ToString ());
			Assert.Equal ("\t", wrappedLines [5].ToString ());
			Assert.Equal ("\t", wrappedLines [6].ToString ());
			Assert.Equal ("\t", wrappedLines [7].ToString ());
			Assert.Equal (" ", wrappedLines [8].ToString ());
			Assert.Equal ("ha", wrappedLines [9].ToString ());
			Assert.Equal ("s ", wrappedLines [10].ToString ());
			Assert.Equal ("wo", wrappedLines [11].ToString ());
			Assert.Equal ("rd", wrappedLines [12].ToString ());
			Assert.Equal ("s.", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == 14);

			maxWidth = 1;
			expectedClippedWidth = 1;
			wrappedLines = TextFormatter.WordWrap (text, maxWidth, true, tabWidth);
			Assert.True (expectedClippedWidth >= wrappedLines.Max (l => l.RuneCount));
			Assert.Equal ("A", wrappedLines [0].ToString ());
			Assert.Equal (" ", wrappedLines [1].ToString ());
			Assert.Equal ("s", wrappedLines [2].ToString ());
			Assert.Equal ("e", wrappedLines [3].ToString ());
			Assert.Equal ("n", wrappedLines [4].ToString ());
			Assert.Equal ("t", wrappedLines [5].ToString ());
			Assert.Equal ("e", wrappedLines [6].ToString ());
			Assert.Equal ("n", wrappedLines [7].ToString ());
			Assert.Equal ("c", wrappedLines [8].ToString ());
			Assert.Equal ("e", wrappedLines [9].ToString ());
			Assert.Equal ("\t", wrappedLines [10].ToString ());
			Assert.Equal ("\t", wrappedLines [11].ToString ());
			Assert.Equal ("\t", wrappedLines [12].ToString ());
			Assert.Equal (" ", wrappedLines [13].ToString ());
			Assert.Equal (".", wrappedLines [^1].ToString ());
			Assert.True (wrappedLines.Count == text.Length);
		}

		[Fact]
		public void WordWrap_Unicode_Wide_Runes ()
		{
			ustring text = "これが最初の行です。 こんにちは世界。 これが2行目です。";
			var width = text.RuneCount;
			var wrappedLines = TextFormatter.WordWrap (text, width);
			Assert.Equal (3, wrappedLines.Count);
			Assert.Equal ("これが最初の行です。", wrappedLines [0].ToString ());
			Assert.Equal ("こんにちは世界。", wrappedLines [1].ToString ());
			Assert.Equal ("これが2行目です。", wrappedLines [^1].ToString ());
		}

		[Fact]
		public void ReplaceHotKeyWithTag ()
		{
			var tf = new TextFormatter ();
			ustring text = "test";
			int hotPos = 0;
			uint tag = tf.HotKeyTagMask | 't';

			Assert.Equal (ustring.Make (new Rune [] { tag, 'e', 's', 't' }), tf.ReplaceHotKeyWithTag (text, hotPos));

			tag = tf.HotKeyTagMask | 'e';
			hotPos = 1;
			Assert.Equal (ustring.Make (new Rune [] { 't', tag, 's', 't' }), tf.ReplaceHotKeyWithTag (text, hotPos));

			var result = tf.ReplaceHotKeyWithTag (text, hotPos);
			Assert.Equal ('e', (uint)(result.ToRunes () [1] & ~tf.HotKeyTagMask));

			text = "Ok";
			tag = 0x100000 | 'O';
			hotPos = 0;
			Assert.Equal (ustring.Make (new Rune [] { tag, 'k' }), result = tf.ReplaceHotKeyWithTag (text, hotPos));
			Assert.Equal ('O', (uint)(result.ToRunes () [0] & ~tf.HotKeyTagMask));

			text = "[◦ Ok ◦]";
			text = ustring.Make (new Rune [] { '[', '◦', ' ', 'O', 'k', ' ', '◦', ']' });
			var runes = text.ToRuneList ();
			Assert.Equal (text.RuneCount, runes.Count);
			Assert.Equal (text, ustring.Make (runes));
			tag = tf.HotKeyTagMask | 'O';
			hotPos = 3;
			Assert.Equal (ustring.Make (new Rune [] { '[', '◦', ' ', tag, 'k', ' ', '◦', ']' }), result = tf.ReplaceHotKeyWithTag (text, hotPos));
			Assert.Equal ('O', (uint)(result.ToRunes () [3] & ~tf.HotKeyTagMask));

			text = "^k";
			tag = '^';
			hotPos = 0;
			Assert.Equal (ustring.Make (new Rune [] { tag, 'k' }), result = tf.ReplaceHotKeyWithTag (text, hotPos));
			Assert.Equal ('^', (uint)(result.ToRunes () [0] & ~tf.HotKeyTagMask));
		}

		[Fact]
		public void Reformat_Invalid ()
		{
			var text = ustring.Empty;
			var list = new List<ustring> ();

			Assert.Throws<ArgumentOutOfRangeException> (() => TextFormatter.Format (text, -1, TextAlignment.Left, false));

			list = TextFormatter.Format (text, 0, TextAlignment.Left, false);
			Assert.NotEmpty (list);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);

			text = null;
			list = TextFormatter.Format (text, 0, TextAlignment.Left, false);
			Assert.NotEmpty (list);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);

			list = TextFormatter.Format (text, 0, TextAlignment.Left, true);
			Assert.NotEmpty (list);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);
		}

		[Fact]
		public void Reformat_NoWordrap_SingleLine ()
		{
			var text = ustring.Empty;
			var list = new List<ustring> ();
			var maxWidth = 0;
			var expectedClippedWidth = 0;
			var wrap = false;

			maxWidth = 0;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);

			maxWidth = 1;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);

			text = "A sentence has words.";
			maxWidth = 0;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);

			maxWidth = 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = 5;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);
		}

		[Fact]
		public void Reformat_NoWordrap_NewLines ()
		{
			var text = ustring.Empty;
			var list = new List<ustring> ();
			var maxWidth = 0;
			var expectedClippedWidth = 0;
			var wrap = false;

			text = "A sentence has words.\nLine 2.";
			maxWidth = 0;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);

			maxWidth = 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = 5;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]).Replace ("\n", " "), list [0]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]).Replace ("\n", " "), list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]).Replace ("\n", " "), list [0]);

			text = "A sentence has words.\r\nLine 2.";
			maxWidth = 0;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);

			maxWidth = 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = 5;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth) + 1;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]).Replace ("\r\n", " ").ToString (), list [0].ToString ());

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]).Replace ("\r\n", " "), list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]).Replace ("\r\n", " "), list [0]);
		}

		[Fact]
		public void Reformat_Wrap_Spaces_No_NewLines ()
		{
			var text = ustring.Empty;
			var list = new List<ustring> ();
			var maxWidth = 0;
			var expectedClippedWidth = 0;
			var wrap = true;
			var preserveTrailingSpaces = true;

			// Even # of spaces
			//      0123456789
			text = "012 456 89";

			// See WordWrap BUGBUGs above.
			maxWidth = 0;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal (ustring.Empty, list [0]);

			maxWidth = 1;
			// remove 3 whitespace chars
			expectedClippedWidth = text.RuneCount - text.Sum (r => r == ' ' ? 1 : 0);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.Equal (expectedClippedWidth, list.Count);
			Assert.Equal ("01245689", ustring.Join ("", list.ToArray ()));

			maxWidth = 1;
			// keep 3 whitespace chars
			expectedClippedWidth = text.RuneCount;
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap, preserveTrailingSpaces);
			Assert.Equal (expectedClippedWidth, list.Count);
			Assert.Equal (text, ustring.Join ("", list.ToArray ()));

			maxWidth = 5;
			// remove 3 whitespace chars
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth - text.Sum (r => r == ' ' ? 1 : 0));
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.Equal (expectedClippedWidth, list.Count);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);
			Assert.Equal ("01245689", ustring.Join ("", list.ToArray ()));

			maxWidth = 5;
			// keep 3 whitespace chars
			expectedClippedWidth = Math.Min (text.RuneCount, (int)Math.Ceiling ((double)(text.RuneCount / 3F)));
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap, preserveTrailingSpaces);
			Assert.Equal (expectedClippedWidth - (maxWidth - expectedClippedWidth), list.Count);
			Assert.Equal (ustring.Make (text.ToRunes () [0..expectedClippedWidth]), list [0]);
			Assert.Equal ("012 456 89", ustring.Join ("", list.ToArray ()));

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 2);
			Assert.Equal ("012 456", list [0]);
			Assert.Equal ("89", list [1]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("012 456 89", list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("012 456 89", list [0]);

			// Odd # of spaces
			//      0123456789
			text = "012 456 89 end";
			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 2);
			Assert.Equal ("012 456 89", list [0]);
			Assert.Equal ("end", list [1]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("012 456 89 end", list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("012 456 89 end", list [0]);
		}

		[Fact]
		public void Reformat_Unicode_Wrap_Spaces_No_NewLines ()
		{
			var text = ustring.Empty;
			var list = new List<ustring> ();
			var maxWidth = 0;
			var expectedClippedWidth = 0;
			var wrap = true;

			// Unicode
			// Even # of chars
			//      0123456789
			text = "\u2660Ð¿ÑÐ Ð²Ð Ñ";

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 2);
			Assert.Equal ("\u2660Ð¿ÑÐ Ð²Ð", list [0]);
			Assert.Equal ("Ñ", list [1]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("\u2660Ð¿ÑÐ Ð²Ð Ñ", list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("\u2660Ð¿ÑÐ Ð²Ð Ñ", list [0]);

			// Unicode
			// Odd # of chars
			//      0123456789
			text = "\u2660 ÑÐ Ð²Ð Ñ";

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 2);
			Assert.Equal ("\u2660 ÑÐ Ð²Ð", list [0]);
			Assert.Equal ("Ñ", list [1]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("\u2660 ÑÐ Ð²Ð Ñ", list [0]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.True (list.Count == 1);
			Assert.Equal ("\u2660 ÑÐ Ð²Ð Ñ", list [0]);
		}

		[Fact]
		public void Reformat_Unicode_Wrap_Spaces_NewLines ()
		{
			var text = ustring.Empty;
			var list = new List<ustring> ();
			var maxWidth = 0;
			var expectedClippedWidth = 0;
			var wrap = true;

			// Unicode
			text = "\u2460\u2461\u2462\n\u2460\u2461\u2462\u2463\u2464";

			maxWidth = text.RuneCount - 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.Equal (2, list.Count);
			Assert.Equal ("\u2460\u2461\u2462", list [0]);
			Assert.Equal ("\u2460\u2461\u2462\u2463\u2464", list [1]);

			// no clip
			maxWidth = text.RuneCount + 0;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.Equal (2, list.Count);
			Assert.Equal ("\u2460\u2461\u2462", list [0]);
			Assert.Equal ("\u2460\u2461\u2462\u2463\u2464", list [1]);

			maxWidth = text.RuneCount + 1;
			expectedClippedWidth = Math.Min (text.RuneCount, maxWidth);
			list = TextFormatter.Format (text, maxWidth, TextAlignment.Left, wrap);
			Assert.Equal (2, list.Count);
			Assert.Equal ("\u2460\u2461\u2462", list [0]);
			Assert.Equal ("\u2460\u2461\u2462\u2463\u2464", list [1]);
		}

		[Fact]
		public void System_Rune_ColumnWidth ()
		{
			var c = new System.Rune ('a');
			Assert.Equal (1, Rune.ColumnWidth (c));
			Assert.Equal (1, ustring.Make (c).ConsoleWidth);
			Assert.Equal (1, ustring.Make (c).Length);

			c = new System.Rune ('b');
			Assert.Equal (1, Rune.ColumnWidth (c));
			Assert.Equal (1, ustring.Make (c).ConsoleWidth);
			Assert.Equal (1, ustring.Make (c).Length);

			c = new System.Rune (123);
			Assert.Equal (1, Rune.ColumnWidth (c));
			Assert.Equal (1, ustring.Make (c).ConsoleWidth);
			Assert.Equal (1, ustring.Make (c).Length);

			c = new System.Rune ('\u1150');
			Assert.Equal (2, Rune.ColumnWidth (c));      // 0x1150	ᅐ	Unicode Technical Report #11
			Assert.Equal (2, ustring.Make (c).ConsoleWidth);
			Assert.Equal (3, ustring.Make (c).Length);

			c = new System.Rune ('\u1161');
			Assert.Equal (0, Rune.ColumnWidth (c));      // 0x1161	ᅡ	column width of 0
			Assert.Equal (0, ustring.Make (c).ConsoleWidth);
			Assert.Equal (3, ustring.Make (c).Length);

			c = new System.Rune (31);
			Assert.Equal (-1, Rune.ColumnWidth (c));        // non printable character
			Assert.Equal (-1, ustring.Make (c).ConsoleWidth);
			Assert.Equal (1, ustring.Make (c).Length);

			c = new System.Rune (127);
			Assert.Equal (-1, Rune.ColumnWidth (c));       // non printable character
			Assert.Equal (-1, ustring.Make (c).ConsoleWidth);
			Assert.Equal (1, ustring.Make (c).Length);
		}

		[Fact]
		public void System_Text_Rune ()
		{
			var c = new System.Text.Rune ('a');
			Assert.Equal (1, c.Utf8SequenceLength);

			c = new System.Text.Rune ('b');
			Assert.Equal (1, c.Utf8SequenceLength);

			c = new System.Text.Rune (123);
			Assert.Equal (1, c.Utf8SequenceLength);

			c = new System.Text.Rune ('\u1150');
			Assert.Equal (3, c.Utf8SequenceLength);         // 0x1150	ᅐ	Unicode Technical Report #11

			c = new System.Text.Rune ('\u1161');
			Assert.Equal (3, c.Utf8SequenceLength);         // 0x1161	ᅡ	column width of 0

			c = new System.Text.Rune (31);
			Assert.Equal (1, c.Utf8SequenceLength);         // non printable character

			c = new System.Text.Rune (127);
			Assert.Equal (1, c.Utf8SequenceLength);         // non printable character
		}

		[Fact]
		public void Format_WordWrap_preserveTrailingSpaces ()
		{
			ustring text = " A sentence has words. \n This is the second Line - 2. ";

			// With preserveTrailingSpaces = false by default.
			var list1 = TextFormatter.Format (text, 4, TextAlignment.Left, true);
			ustring wrappedText1 = ustring.Empty;
			Assert.Equal (" A", list1 [0].ToString ());
			Assert.Equal ("sent", list1 [1].ToString ());
			Assert.Equal ("ence", list1 [2].ToString ());
			Assert.Equal ("has", list1 [3].ToString ());
			Assert.Equal ("word", list1 [4].ToString ());
			Assert.Equal ("s. ", list1 [5].ToString ());
			Assert.Equal (" Thi", list1 [6].ToString ());
			Assert.Equal ("s is", list1 [7].ToString ());
			Assert.Equal ("the", list1 [8].ToString ());
			Assert.Equal ("seco", list1 [9].ToString ());
			Assert.Equal ("nd", list1 [10].ToString ());
			Assert.Equal ("Line", list1 [11].ToString ());
			Assert.Equal ("- 2.", list1 [^1].ToString ());
			foreach (var txt in list1) {
				wrappedText1 += txt;
			}
			Assert.Equal (" Asentencehaswords.  This isthesecondLine- 2.", wrappedText1);

			// With preserveTrailingSpaces = true.
			var list2 = TextFormatter.Format (text, 4, TextAlignment.Left, true, true);
			ustring wrappedText2 = ustring.Empty;
			Assert.Equal (" A ", list2 [0].ToString ());
			Assert.Equal ("sent", list2 [1].ToString ());
			Assert.Equal ("ence", list2 [2].ToString ());
			Assert.Equal (" has", list2 [3].ToString ());
			Assert.Equal (" ", list2 [4].ToString ());
			Assert.Equal ("word", list2 [5].ToString ());
			Assert.Equal ("s. ", list2 [6].ToString ());
			Assert.Equal (" ", list2 [7].ToString ());
			Assert.Equal ("This", list2 [8].ToString ());
			Assert.Equal (" is ", list2 [9].ToString ());
			Assert.Equal ("the ", list2 [10].ToString ());
			Assert.Equal ("seco", list2 [11].ToString ());
			Assert.Equal ("nd ", list2 [12].ToString ());
			Assert.Equal ("Line", list2 [13].ToString ());
			Assert.Equal (" - ", list2 [14].ToString ());
			Assert.Equal ("2. ", list2 [^1].ToString ());
			foreach (var txt in list2) {
				wrappedText2 += txt;
			}
			Assert.Equal (" A sentence has words.  This is the second Line - 2. ", wrappedText2);
		}

		[Fact]
		public void Format_Dont_Throw_ArgumentException_With_WordWrap_As_False_And_Keep_End_Spaces_As_True ()
		{
			var exception = Record.Exception (() => TextFormatter.Format ("Some text", 4, TextAlignment.Left, false, true));
			Assert.Null (exception);
		}

		[Fact]
		public void Draw_Horizontal_Throws_IndexOutOfRangeException_With_Negative_Bounds ()
		{
			Application.Init (new FakeDriver (), new FakeMainLoop (() => FakeConsole.ReadKey (true)));

			var top = Application.Top;

			var view = new View ("view") { X = -2 };
			top.Add (view);

			Application.Iteration += () => {
				Assert.Equal (-2, view.X);

				Application.RequestStop ();
			};

			try {
				Application.Run ();
			} catch (IndexOutOfRangeException ex) {
				// After the fix this exception will not be caught.
				Assert.IsType<IndexOutOfRangeException> (ex);
			}

			// Shutdown must be called to safely clean up Application if Init has been called
			Application.Shutdown ();
		}

		[Fact]
		public void TestClipOrPad_ShortWord ()
		{
			// word is short but we want it to fill 6 so it should be padded
			Assert.Equal ("fff   ", TextFormatter.ClipOrPad ("fff", 6));
		}

		[Fact]
		public void TestClipOrPad_LongWord ()
		{
			// word is long but we want it to fill 3 space only
			Assert.Equal ("123", TextFormatter.ClipOrPad ("123456789", 3));
		}
		[Fact]
		public void Draw_Vertical_Throws_IndexOutOfRangeException_With_Negative_Bounds ()
		{
			Application.Init (new FakeDriver (), new FakeMainLoop (() => FakeConsole.ReadKey (true)));

			var top = Application.Top;

			var view = new View ("view") {
				Y = -2,
				Height = 10,
				TextDirection = TextDirection.TopBottom_LeftRight
			};
			top.Add (view);

			Application.Iteration += () => {
				Assert.Equal (-2, view.Y);

				Application.RequestStop ();
			};

			try {
				Application.Run ();
			} catch (IndexOutOfRangeException ex) {
				// After the fix this exception will not be caught.
				Assert.IsType<IndexOutOfRangeException> (ex);
			}

			// Shutdown must be called to safely clean up Application if Init has been called
			Application.Shutdown ();
		}

		[Fact]
		public void Internal_Tests ()
		{
			var tf = new TextFormatter ();
			Assert.Equal (Key.Null, tf.HotKey);
			tf.HotKey = Key.CtrlMask | Key.Q;
			Assert.Equal (Key.CtrlMask | Key.Q, tf.HotKey);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Horizontal_Simple_Runes ()
		{
			var label = new Label ("Demo Simple Rune");
			Application.Top.Add (label);
			Application.Begin (Application.Top);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 16, 1), label.Frame);

			var expected = @"
Demo Simple Rune
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 16, 1), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Vertical_Simple_Runes ()
		{
			var label = new Label ("Demo Simple Rune") {
				TextDirection = TextDirection.TopBottom_LeftRight
			};
			Application.Top.Add (label);
			Application.Begin (Application.Top);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 1, 16), label.Frame);
			Assert.NotNull (label.Width);
			Assert.NotNull (label.Height);

			var expected = @"
D
e
m
o
 
S
i
m
p
l
e
 
R
u
n
e
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 1, 16), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Horizontal_Wide_Runes ()
		{
			var label = new Label ("デモエムポンズ");
			Application.Top.Add (label);
			Application.Begin (Application.Top);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 14, 1), label.Frame);

			var expected = @"
デモエムポンズ
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 14, 1), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Vertical_Wide_Runes ()
		{
			var label = new Label ("デモエムポンズ") {
				TextDirection = TextDirection.TopBottom_LeftRight
			};
			Application.Top.Add (label);
			Application.Begin (Application.Top);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 2, 7), label.Frame);

			var expected = @"
デ
モ
エ
ム
ポ
ン
ズ
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 2, 7), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Vertical_Wide_Runes_With_ForceValidatePosDim ()
		{
			var label = new Label ("デモエムポンズ") {
				Width = Dim.Fill (),
				Height = Dim.Percent (50f),
				TextDirection = TextDirection.TopBottom_LeftRight,
				ForceValidatePosDim = true
			};
			Application.Top.Add (label);
			Application.Begin (Application.Top);

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 80, 12), label.Frame);

			var expected = @"
デ
モ
エ
ム
ポ
ン
ズ
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 2, 7), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Horizontal_Simple_TextAlignments ()
		{
			var text = "Hello World";
			var width = 20;
			var lblLeft = new Label (text) { Width = width };
			var lblCenter = new Label (text) { Y = 1, Width = width, TextAlignment = TextAlignment.Centered };
			var lblRight = new Label (text) { Y = 2, Width = width, TextAlignment = TextAlignment.Right };
			var lblJust = new Label (text) { Y = 3, Width = width, TextAlignment = TextAlignment.Justified };
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (lblLeft, lblCenter, lblRight, lblJust);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (width + 2, 6);

			Assert.True (lblLeft.AutoSize);
			Assert.True (lblCenter.AutoSize);
			Assert.True (lblRight.AutoSize);
			Assert.True (lblJust.AutoSize);
			Assert.Equal (new Rect (0, 0, width, 1), lblLeft.Frame);
			Assert.Equal (new Rect (0, 1, width, 1), lblCenter.Frame);
			Assert.Equal (new Rect (0, 2, width, 1), lblRight.Frame);
			Assert.Equal (new Rect (0, 3, width, 1), lblJust.Frame);
			Assert.Equal (new Rect (0, 0, width + 2, 6), frame.Frame);

			var expected = @"
┌────────────────────┐
│Hello World         │
│    Hello World     │
│         Hello World│
│Hello          World│
└────────────────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, width + 2, 6), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Vertical_Simple_TextAlignments ()
		{
			var text = "Hello World";
			var height = 20;
			var lblLeft = new Label (text, direction: TextDirection.TopBottom_LeftRight) { Height = height };
			var lblCenter = new Label (text, direction: TextDirection.TopBottom_LeftRight) { X = 2, Height = height, VerticalTextAlignment = VerticalTextAlignment.Middle };
			var lblRight = new Label (text, direction: TextDirection.TopBottom_LeftRight) { X = 4, Height = height, VerticalTextAlignment = VerticalTextAlignment.Bottom };
			var lblJust = new Label (text, direction: TextDirection.TopBottom_LeftRight) { X = 6, Height = height, VerticalTextAlignment = VerticalTextAlignment.Justified };
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (lblLeft, lblCenter, lblRight, lblJust);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (9, height + 2);

			Assert.True (lblLeft.AutoSize);
			Assert.True (lblCenter.AutoSize);
			Assert.True (lblRight.AutoSize);
			Assert.True (lblJust.AutoSize);
			Assert.Equal (new Rect (0, 0, 1, height), lblLeft.Frame);
			Assert.Equal (new Rect (2, 0, 1, height), lblCenter.Frame);
			Assert.Equal (new Rect (4, 0, 1, height), lblRight.Frame);
			Assert.Equal (new Rect (6, 0, 1, height), lblJust.Frame);
			Assert.Equal (new Rect (0, 0, 9, height + 2), frame.Frame);

			var expected = @"
┌───────┐
│H     H│
│e     e│
│l     l│
│l     l│
│o H   o│
│  e    │
│W l    │
│o l    │
│r o    │
│l   H  │
│d W e  │
│  o l  │
│  r l  │
│  l o  │
│  d    │
│    W W│
│    o o│
│    r r│
│    l l│
│    d d│
└───────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 9, height + 2), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Horizontal_Wide_TextAlignments ()
		{
			var text = "こんにちは 世界";
			var width = 25;
			var lblLeft = new Label (text) { Width = width };
			var lblCenter = new Label (text) { Y = 1, Width = width, TextAlignment = TextAlignment.Centered };
			var lblRight = new Label (text) { Y = 2, Width = width, TextAlignment = TextAlignment.Right };
			var lblJust = new Label (text) { Y = 3, Width = width, TextAlignment = TextAlignment.Justified };
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (lblLeft, lblCenter, lblRight, lblJust);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (width + 2, 6);

			Assert.True (lblLeft.AutoSize);
			Assert.True (lblCenter.AutoSize);
			Assert.True (lblRight.AutoSize);
			Assert.True (lblJust.AutoSize);
			Assert.Equal (new Rect (0, 0, width, 1), lblLeft.Frame);
			Assert.Equal (new Rect (0, 1, width, 1), lblCenter.Frame);
			Assert.Equal (new Rect (0, 2, width, 1), lblRight.Frame);
			Assert.Equal (new Rect (0, 3, width, 1), lblJust.Frame);
			Assert.Equal (new Rect (0, 0, width + 2, 6), frame.Frame);

			var expected = @"
┌─────────────────────────┐
│こんにちは 世界          │
│     こんにちは 世界     │
│          こんにちは 世界│
│こんにちは           世界│
└─────────────────────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, width + 2, 6), pos);
		}

		[Fact, AutoInitShutdown]
		public void Draw_Vertical_Wide_TextAlignments ()
		{
			var text = "こんにちは 世界";
			var height = 23;
			var lblLeft = new Label (text) { Width = 2, Height = height, TextDirection = TextDirection.TopBottom_LeftRight };
			var lblCenter = new Label (text) { X = 3, Width = 2, Height = height, TextDirection = TextDirection.TopBottom_LeftRight, VerticalTextAlignment = VerticalTextAlignment.Middle };
			var lblRight = new Label (text) { X = 6, Width = 2, Height = height, TextDirection = TextDirection.TopBottom_LeftRight, VerticalTextAlignment = VerticalTextAlignment.Bottom };
			var lblJust = new Label (text) { X = 9, Width = 2, Height = height, TextDirection = TextDirection.TopBottom_LeftRight, VerticalTextAlignment = VerticalTextAlignment.Justified };
			var frame = new FrameView () { Width = Dim.Fill (), Height = Dim.Fill () };

			frame.Add (lblLeft, lblCenter, lblRight, lblJust);
			Application.Top.Add (frame);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (13, height + 2);

			// All AutoSize are false because the Frame.Height != TextFormatter.Size.Height
			Assert.True (lblLeft.AutoSize);
			Assert.True (lblCenter.AutoSize);
			Assert.True (lblRight.AutoSize);
			Assert.True (lblJust.AutoSize);
			Assert.Equal (new Rect (0, 0, 2, height), lblLeft.Frame);
			Assert.Equal (new Rect (3, 0, 2, height), lblCenter.Frame);
			Assert.Equal (new Rect (6, 0, 2, height), lblRight.Frame);
			Assert.Equal (new Rect (9, 0, 2, height), lblJust.Frame);
			Assert.Equal (new Rect (0, 0, 13, height + 2), frame.Frame);

			var expected = @"
┌───────────┐
│こ       こ│
│ん       ん│
│に       に│
│ち       ち│
│は       は│
│           │
│世         │
│界 こ      │
│   ん      │
│   に      │
│   ち      │
│   は      │
│           │
│   世      │
│   界      │
│      こ   │
│      ん   │
│      に   │
│      ち   │
│      は   │
│           │
│      世 世│
│      界 界│
└───────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 13, height + 2), pos);
		}

		[Fact]
		public void GetTextWidth_Simple_And_Wide_Runes ()
		{
			ustring text = "Hello World";
			Assert.Equal (11, TextFormatter.GetTextWidth (text));
			text = "こんにちは世界";
			Assert.Equal (14, TextFormatter.GetTextWidth (text));
		}

		[Fact]
		public void GetSumMaxCharWidth_Simple_And_Wide_Runes ()
		{
			ustring text = "Hello World";
			Assert.Equal (11, TextFormatter.GetSumMaxCharWidth (text));
			Assert.Equal (1, TextFormatter.GetSumMaxCharWidth (text, 6, 1));
			text = "こんにちは 世界";
			Assert.Equal (15, TextFormatter.GetSumMaxCharWidth (text));
			Assert.Equal (2, TextFormatter.GetSumMaxCharWidth (text, 6, 1));
		}

		[Fact]
		public void GetSumMaxCharWidth_List_Simple_And_Wide_Runes ()
		{
			List<ustring> text = new List<ustring> () { "Hello", "World" };
			Assert.Equal (2, TextFormatter.GetSumMaxCharWidth (text));
			Assert.Equal (1, TextFormatter.GetSumMaxCharWidth (text, 1, 1));
			text = new List<ustring> () { "こんにちは", "世界" };
			Assert.Equal (4, TextFormatter.GetSumMaxCharWidth (text));
			Assert.Equal (2, TextFormatter.GetSumMaxCharWidth (text, 1, 1));
		}

		[Fact]
		public void GetMaxLengthForWidth_Simple_And_Wide_Runes ()
		{
			ustring text = "Hello World";
			Assert.Equal (6, TextFormatter.GetMaxLengthForWidth (text, 6));
			text = "こんにちは 世界";
			Assert.Equal (3, TextFormatter.GetMaxLengthForWidth (text, 6));
		}

		[Fact]
		public void GetMaxLengthForWidth_List_Simple_And_Wide_Runes ()
		{
			var runes = ustring.Make ("Hello World").ToRuneList ();
			Assert.Equal (6, TextFormatter.GetMaxLengthForWidth (runes, 6));
			runes = ustring.Make ("こんにちは 世界").ToRuneList ();
			Assert.Equal (3, TextFormatter.GetMaxLengthForWidth (runes, 6));
			runes = ustring.Make ("[ Say Hello 你 ]").ToRuneList ();
			Assert.Equal (15, TextFormatter.GetMaxLengthForWidth (runes, 16));
		}

		[Fact]
		public void Format_Truncate_Simple_And_Wide_Runes ()
		{
			var text = "Truncate";
			var list = TextFormatter.Format (text, 3, false, false);
			Assert.Equal ("Tru", list [^1].ToString ());

			text = "デモエムポンズ";
			list = TextFormatter.Format (text, 3, false, false);
			Assert.Equal ("デ", list [^1].ToString ());
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_False_View_IsEmpty_False_Return_Null_Lines ()
		{
			var text = "Views";
			var view = new View () {
				Width = Dim.Fill () - text.Length,
				Height = 1,
				Text = text
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (view);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (10, 4);

			Assert.Equal (5, text.Length);
			Assert.False (view.AutoSize);
			Assert.Equal (new Rect (0, 0, 3, 1), view.Frame);
			Assert.Equal (new Size (3, 1), view.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { "Vie" }, view.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 10, 4), win.Frame);
			Assert.Equal (new Rect (0, 0, 10, 4), Application.Top.Frame);
			var expected = @"
┌ Wind ──┐
│Vie     │
│        │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			view.Width = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 0, 1), view.Frame);
			Assert.Equal (new Size (0, 1), view.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { ustring.Empty }, view.TextFormatter.Lines);
			expected = @"
┌ Wind ──┐
│        │
│        │
└────────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_False_View_IsEmpty_True_Minimum_Height ()
		{
			var text = "Views";
			var view = new View () {
				Width = Dim.Fill () - text.Length,
				Text = text
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (view);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (10, 4);

			Assert.Equal (5, text.Length);
			Assert.False (view.AutoSize);
			Assert.Equal (new Rect (0, 0, 3, 1), view.Frame);
			Assert.Equal (new Size (3, 1), view.TextFormatter.Size);
			Assert.Single (view.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 10, 4), win.Frame);
			Assert.Equal (new Rect (0, 0, 10, 4), Application.Top.Frame);
			var expected = @"
┌ Wind ──┐
│Vie     │
│        │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			view.Width = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 0, 1), view.Frame);
			Assert.Equal (new Size (0, 1), view.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Equal (new List<ustring> () { ustring.Empty }, view.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌ Wind ──┐
│        │
│        │
└────────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_True_Label_IsEmpty_False_Never_Return_Null_Lines ()
		{
			var text = "Label";
			var label = new Label () {
				Width = Dim.Fill () - text.Length,
				Height = 1,
				Text = text
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (label);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (10, 4);

			Assert.Equal (5, text.Length);
			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 5, 1), label.Frame);
			Assert.Equal (new Size (5, 1), label.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { "Label" }, label.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 10, 4), win.Frame);
			Assert.Equal (new Rect (0, 0, 10, 4), Application.Top.Frame);
			var expected = @"
┌ Wind ──┐
│Label   │
│        │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			label.Width = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 5, 1), label.Frame);
			Assert.Equal (new Size (5, 1), label.TextFormatter.Size);
			Assert.Single (label.TextFormatter.Lines);
			expected = @"
┌ Wind ──┐
│Label   │
│        │
└────────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_False_Label_IsEmpty_True_Return_Null_Lines ()
		{
			var text = "Label";
			var label = new Label () {
				Width = Dim.Fill () - text.Length,
				Height = 1,
				Text = text,
				AutoSize = false
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (label);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (10, 4);

			Assert.Equal (5, text.Length);
			Assert.False (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 3, 1), label.Frame);
			Assert.Equal (new Size (3, 1), label.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { "Lab" }, label.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 10, 4), win.Frame);
			Assert.Equal (new Rect (0, 0, 10, 4), Application.Top.Frame);
			var expected = @"
┌ Wind ──┐
│Lab     │
│        │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			label.Width = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.False (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 0, 1), label.Frame);
			Assert.Equal (new Size (0, 1), label.TextFormatter.Size);
			Assert.Equal (new List<ustring> { ustring.Empty }, label.TextFormatter.Lines);
			expected = @"
┌ Wind ──┐
│        │
│        │
└────────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_True_Label_IsEmpty_False_Minimum_Height ()
		{
			var text = "Label";
			var label = new Label () {
				Width = Dim.Fill () - text.Length,
				Text = text
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (label);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (10, 4);

			Assert.Equal (5, text.Length);
			Assert.True (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 5, 1), label.Frame);
			Assert.Equal (new Size (5, 1), label.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { "Label" }, label.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 10, 4), win.Frame);
			Assert.Equal (new Rect (0, 0, 10, 4), Application.Top.Frame);
			var expected = @"
┌ Wind ──┐
│Label   │
│        │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			label.Width = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 5, 1), label.Frame);
			Assert.Equal (new Size (5, 1), label.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Single (label.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌ Wind ──┐
│Label   │
│        │
└────────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_False_Label_Height_Zero_Returns_Minimum_Height ()
		{
			var text = "Label";
			var label = new Label () {
				Width = Dim.Fill () - text.Length,
				Text = text,
				AutoSize = false
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (label);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (10, 4);

			Assert.Equal (5, text.Length);
			Assert.False (label.AutoSize);
			Assert.Equal (new Rect (0, 0, 3, 1), label.Frame);
			Assert.Equal (new Size (3, 1), label.TextFormatter.Size);
			Assert.Single (label.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 10, 4), win.Frame);
			Assert.Equal (new Rect (0, 0, 10, 4), Application.Top.Frame);
			var expected = @"
┌ Wind ──┐
│Lab     │
│        │
└────────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			label.Width = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 0, 1), label.Frame);
			Assert.Equal (new Size (0, 1), label.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Equal (new List<ustring> () { ustring.Empty }, label.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌ Wind ──┐
│        │
│        │
└────────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 10, 4), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_True_View_IsEmpty_False_Minimum_Width ()
		{
			var text = "Views";
			var view = new View () {
				TextDirection = TextDirection.TopBottom_LeftRight,
				Height = Dim.Fill () - text.Length,
				Text = text,
				AutoSize = true
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (view);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (4, 10);

			Assert.Equal (5, text.Length);
			Assert.True (view.AutoSize);
			Assert.Equal (new Rect (0, 0, 1, 5), view.Frame);
			Assert.Equal (new Size (1, 5), view.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { "Views" }, view.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 4, 10), win.Frame);
			Assert.Equal (new Rect (0, 0, 4, 10), Application.Top.Frame);
			var expected = @"
┌──┐
│V │
│i │
│e │
│w │
│s │
│  │
│  │
│  │
└──┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			view.Height = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 1, 5), view.Frame);
			Assert.Equal (new Size (1, 5), view.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Single (view.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌──┐
│V │
│i │
│e │
│w │
│s │
│  │
│  │
│  │
└──┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_False_View_Width_Null_Returns_Host_Frame_Width ()
		{
			var text = "Views";
			var view = new View () {
				TextDirection = TextDirection.TopBottom_LeftRight,
				Height = Dim.Fill () - text.Length,
				Text = text
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (view);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (4, 10);

			Assert.Equal (5, text.Length);
			Assert.False (view.AutoSize);
			Assert.Equal (new Rect (0, 0, 1, 3), view.Frame);
			Assert.Equal (new Size (1, 3), view.TextFormatter.Size);
			Assert.Single (view.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 4, 10), win.Frame);
			Assert.Equal (new Rect (0, 0, 4, 10), Application.Top.Frame);
			var expected = @"
┌──┐
│V │
│i │
│e │
│  │
│  │
│  │
│  │
│  │
└──┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			view.Height = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 1, 0), view.Frame);
			Assert.Equal (new Size (1, 0), view.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Equal (new List<ustring> () { ustring.Empty }, view.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌──┐
│  │
│  │
│  │
│  │
│  │
│  │
│  │
│  │
└──┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_True_View_IsEmpty_False_Minimum_Width_Wide_Rune ()
		{
			var text = "界View";
			var view = new View () {
				TextDirection = TextDirection.TopBottom_LeftRight,
				Height = Dim.Fill () - text.Length,
				Text = text,
				AutoSize = true
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (view);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (4, 10);

			Assert.Equal (5, text.Length);
			Assert.True (view.AutoSize);
			Assert.Equal (new Rect (0, 0, 2, 5), view.Frame);
			Assert.Equal (new Size (2, 5), view.TextFormatter.Size);
			Assert.Equal (new List<ustring> () { "界View" }, view.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 4, 10), win.Frame);
			Assert.Equal (new Rect (0, 0, 4, 10), Application.Top.Frame);
			var expected = @"
┌──┐
│界│
│V │
│i │
│e │
│w │
│  │
│  │
│  │
└──┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			view.Height = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 2, 5), view.Frame);
			Assert.Equal (new Size (2, 5), view.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Equal (new List<ustring> () { "界View" }, view.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌──┐
│界│
│V │
│i │
│e │
│w │
│  │
│  │
│  │
└──┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);
		}

		[Fact, AutoInitShutdown]
		public void AutoSize_False_View_Width_Zero_Returns_Minimum_Width_With_Wide_Rune ()
		{
			var text = "界View";
			var view = new View () {
				TextDirection = TextDirection.TopBottom_LeftRight,
				Height = Dim.Fill () - text.Length,
				Text = text
			};
			var win = new Window ("Window") {
				Width = Dim.Fill (),
				Height = Dim.Fill ()
			};
			win.Add (view);
			Application.Top.Add (win);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (4, 10);

			Assert.Equal (5, text.Length);
			Assert.False (view.AutoSize);
			Assert.Equal (new Rect (0, 0, 2, 3), view.Frame);
			Assert.Equal (new Size (2, 3), view.TextFormatter.Size);
			Assert.Single (view.TextFormatter.Lines);
			Assert.Equal (new Rect (0, 0, 4, 10), win.Frame);
			Assert.Equal (new Rect (0, 0, 4, 10), Application.Top.Frame);
			var expected = @"
┌──┐
│界│
│V │
│i │
│  │
│  │
│  │
│  │
│  │
└──┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);

			text = "0123456789";
			Assert.Equal (10, text.Length);
			view.Height = Dim.Fill () - text.Length;
			Application.Refresh ();

			Assert.Equal (new Rect (0, 0, 2, 0), view.Frame);
			Assert.Equal (new Size (2, 0), view.TextFormatter.Size);
			var exception = Record.Exception (() => Assert.Equal (new List<ustring> () { ustring.Empty }, view.TextFormatter.Lines));
			Assert.Null (exception);
			expected = @"
┌──┐
│  │
│  │
│  │
│  │
│  │
│  │
│  │
│  │
└──┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 0, 4, 10), pos);
		}
	}
}