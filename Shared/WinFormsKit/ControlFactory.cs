using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsKit
{
    /// <summary>
    /// Фабрика элементов управления с единым стилем оформления форм
    /// </summary>
    public static class ControlFactory
    {
        // ============================================================
        // ПАЛИТРА
        // ============================================================

        public static readonly Color Primary = Color.FromArgb(0, 123, 255);
        public static readonly Color Danger = Color.FromArgb(220, 53, 69);
        public static readonly Color Secondary = Color.FromArgb(108, 117, 125);
        public static readonly Color Success = Color.FromArgb(40, 167, 69);
        public static readonly Color Accent = Color.FromArgb(23, 162, 184);
        public static readonly Color Surface = Color.FromArgb(240, 240, 240);
        public static readonly Color FieldBackground = Color.FromArgb(248, 248, 248);

        public static readonly Font FieldFont = new Font("Arial", 10);
        public static readonly Font FieldLabelFont = new Font("Arial", 10, FontStyle.Bold);

        // ============================================================
        // КНОПКИ
        // ============================================================

        /// <summary>
        /// Плоская кнопка с обработчиком нажатия
        /// </summary>
        public static Button CreateFlatButton(
            string text,
            Point location,
            Size size,
            Color backColor,
            EventHandler? onClick = null,
            Font? font = null)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;

            if (font != null)
                button.Font = font;

            if (onClick != null)
                button.Click += onClick;

            return button;
        }

        /// <summary>
        /// Плоская кнопка диалога, закрывающая форму с заданным результатом
        /// </summary>
        public static Button CreateDialogButton(
            string text,
            Point location,
            Size size,
            Color backColor,
            DialogResult dialogResult,
            EventHandler? onClick = null)
        {
            var button = CreateFlatButton(text, location, size, backColor, onClick, FieldLabelFont);
            button.DialogResult = dialogResult;
            return button;
        }

        // ============================================================
        // НАДПИСИ И ПОЛЯ ВВОДА
        // ============================================================

        /// <summary>
        /// Надпись рядом с полем ввода
        /// </summary>
        public static Label CreateFieldLabel(string text, Point location, Color? foreColor = null)
        {
            var label = new Label
            {
                Text = text,
                Location = location,
                Size = new Size(100, 25),
                Font = FieldLabelFont
            };

            if (foreColor != null)
                label.ForeColor = foreColor.Value;

            return label;
        }

        /// <summary>
        /// Надпись произвольного размера без специального шрифта
        /// </summary>
        public static Label CreateLabel(string text, Point location, Size size, Color foreColor)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = size,
                ForeColor = foreColor
            };
        }

        public static TextBox CreateFieldTextBox(
            Point location,
            Size size,
            bool multiline = false,
            Color? backColor = null,
            Color? foreColor = null,
            BorderStyle? borderStyle = null,
            string? text = null)
        {
            var textBox = new TextBox
            {
                Location = location,
                Size = size,
                Font = FieldFont,
                BackColor = backColor ?? FieldBackground,
                Multiline = multiline,
                ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None
            };

            if (foreColor != null)
                textBox.ForeColor = foreColor.Value;

            if (borderStyle != null)
                textBox.BorderStyle = borderStyle.Value;

            if (text != null)
                textBox.Text = text;

            return textBox;
        }

        // ============================================================
        // ТАБЛИЦА И МЕНЮ
        // ============================================================

        public static DataGridViewTextBoxColumn CreateTextColumn(string name, string headerText, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true
            };
        }

        public static ToolStripMenuItem CreateMenuItem(
            string text,
            EventHandler onClick,
            Keys shortcutKeys = Keys.None,
            string? shortcutDisplayString = null)
        {
            var item = new ToolStripMenuItem(text, null, onClick);

            if (shortcutKeys != Keys.None)
                item.ShortcutKeys = shortcutKeys;

            if (shortcutDisplayString != null)
                item.ShortcutKeyDisplayString = shortcutDisplayString;

            return item;
        }
    }
}
