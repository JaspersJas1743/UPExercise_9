using System;
using System.Windows;
using System.Windows.Controls;

namespace Задание__9
{
    public static class CreatorUIElements
    {
        private const int CHARWIDTH = 8;

        public static TextBlock CreateTextBlock(string content)
            => new TextBlock() { Text = content, Style = MainWindow.GetStyleByName("MyTextBlock") };

        public static TextBox CreateTextBox(string content)
            => new TextBox() 
            {
                Style = MainWindow.GetStyleByName("MyTextBox"),
                ToolTip = CreatorUIElements.CreateToolTip(content)
            };
        
        public static ToolTip CreateToolTip(string content)
            => new ToolTip()
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Content = content,
                Style = MainWindow.GetStyleByName("MyToolTip"),
                Width = content.Replace(" ", String.Empty).Length * CHARWIDTH
            };

        public static GroupBox CreateGroupBox(int row)
        {
            StackPanel panel = new StackPanel() { Style = MainWindow.GetStyleByName("MyStackPanel") };
            foreach (UIElement element in new UIElement[] {
                    CreatorUIElements.CreateTextBlock("Имя параметра: "), CreatorUIElements.CreateTextBox($"Левый столбец {row} строки таблицы"),
                    CreatorUIElements.CreateTextBlock("Значение: "), CreatorUIElements.CreateTextBox($"Правый столбец {row} строки таблицы")
                    })
                panel.Children.Add(element);
            return new GroupBox()
            {
                Margin = MainWindow.RecalculateValues(row - 1),
                Header = $"{row} строка",
                ToolTip = CreatorUIElements.CreateToolTip($"Информация, заносящаяся в {row} строку таблицы"),
                Style = MainWindow.GetStyleByName("MyGroupBox"),
                Content = panel
            };
        }
    }
}