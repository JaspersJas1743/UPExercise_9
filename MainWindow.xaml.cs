using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;

namespace Задание__9
{
    public partial class MainWindow : Window
    {
        private const int ROWS = 3, COLUMNS = 5, MAX = ROWS * COLUMNS;
        private const int TOPOFFSET = 130, TOPOFFSETFORBUTTON = 30;
        private const int LEFTOFFSET = 180, LEFTOFFSETFORBUTTON = 45;
        private const int OFFSETFROMADDTOREMOVE = 50;
        private const string DEFAULTFILENAME = "Введите имя файла (без .docx)";
        private const string DEFAULTTABLENAME = "Введите название таблицы";

        public MainWindow()
        {
            InitializeComponent();
            DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(Button.IsPressedProperty, typeof(Button));
            desc.AddValueChanged(AddBlockButton, ButtonPressed);
            desc.AddValueChanged(RemoveBlockButton, ButtonPressed);
        }

        private void Drag(object sender, RoutedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                Application.Current.MainWindow.DragMove();
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Text = String.Empty;
            textBox.Foreground = Brushes.White;
            textBox.GotFocus -= TextBoxGotFocus;
        }

        private void TableNameBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(TableNameBox.Text))
                return;
            TableNameBox.Text = DEFAULTTABLENAME;
            TableNameBox.Foreground = Brushes.LightGray;
            TableNameBox.GotFocus += TextBoxGotFocus;
        }

        private void FileNameBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(FileNameBox.Text))
                return;
            FileNameBox.Text = DEFAULTFILENAME;
            FileNameBox.Foreground = Brushes.LightGray;
            FileNameBox.GotFocus += TextBoxGotFocus;
        }

        private void ExitClick(object sender, RoutedEventArgs e)
            => Exit();

        private void Exit()
        {
            if (UserAgree("Вы уверены, что хотите закрыть окно?"))
            {
                DocumentCreator.Exit();
                Application.Current.Shutdown();
            }
        }
        private void DeactivateClick(object sender, RoutedEventArgs e)
            => Application.Current.MainWindow.WindowState = WindowState.Minimized;

        private void ButtonPressed(object? sender, EventArgs e)
        {
            Button? button = sender as Button;
            if (button is null)
                return;
            Thickness margin = RecalculateValues(GridForStackPanel.Children.Count - 1);
            double resize = 5, offset = 0;
            if (button.IsPressed)
            {
                resize *= -1;
                offset = 5;
            }
            button.Height = button.Width = button.Width + resize * 2;
            button.FontSize = button.FontSize + resize;
            bool isAdd = button.Content.ToString() == "➕";
            button.Margin = margin with {
                Left = margin.Left + (isAdd ? 0 : OFFSETFROMADDTOREMOVE) + LEFTOFFSETFORBUTTON + offset,
                Top = margin.Top + TOPOFFSET + TOPOFFSETFORBUTTON + offset + (isAdd ? RemoveBlockButton.Height : AddBlockButton.Height)
            };
        }   

        private void FileNameBoxKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: CreateAfterAgree(); break;
                case Key.Escape: Keyboard.ClearFocus(); break;
            }
        }

        private void TableNameBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Keyboard.ClearFocus();
        }
        
        private void FileNameChanged(object sender, TextCompositionEventArgs e)
            => e.Handled = "\\/:*?\"<>|".Contains(e.Text);

        private void AddBlockClick(object sender, RoutedEventArgs e)
        {
            if (GridForStackPanel.Children.Count > MAX - 1)
                return;
            Thickness margin = RecalculateValues(GridForStackPanel.Children.Count);
            if (GridForStackPanel.Children.Count.Equals(MAX - 1))
            {
                ChangeButton(margin, "MyDeactivateButton", "MyActivateButton");
                AddBlockButton.ToolTip = CreatorUIElements.CreateToolTip($"Максимальное количество строк - {MAX}");
            }
            else ChangeButton(margin);
            GridForStackPanel.Children.Add(CreatorUIElements.CreateGroupBox(GridForStackPanel.Children.Count + 1));
        }

        private void RemoveBlockClick(object sender, RoutedEventArgs e)
        {
            if (GridForStackPanel.Children.Count.Equals(0)) 
                return;
            Thickness margin = RecalculateValues(GridForStackPanel.Children.Count - 2);
            if (GridForStackPanel.Children.Count.Equals(1))
            {
                ChangeButton(margin, "MyActivateButton", "MyDeactivateButton");
                RemoveBlockButton.ToolTip = CreatorUIElements.CreateToolTip("Нет строк для удаления");
            }
            else ChangeButton(margin);
            GridForStackPanel.Children.RemoveAt(GridForStackPanel.Children.Count - 1);
        }

        private void CreateTableClick(object sender, RoutedEventArgs e)
            => CreateAfterAgree();

        private void CreateAfterAgree()
        {
            try
            {
                if (!UserAgree("Создать таблицу?"))
                    return;
                List<string[]> uiElements = new();
                foreach (GroupBox groupbox in GridForStackPanel.Children)
                {
                    var collection = ((StackPanel)groupbox.Content).Children;
                    uiElements.Add(new[]{ ((TextBox)collection[1]).Text, ((TextBox)collection[3]).Text });
                }
                FileInfo file = DocumentCreator.CreateFileWithTable(
                    FileNameBox.Text, String.IsNullOrWhiteSpace(TableNameBox.Text) || TableNameBox.Text == DEFAULTTABLENAME ? String.Empty : $"{TableNameBox.Text}\n", uiElements
                );
                MessageBox.Show($"Файл сохранен как {file.Name}", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static Thickness RecalculateValues(int count)
            => new(count / ROWS * LEFTOFFSET + 20, count % ROWS * TOPOFFSET, 0, 0);

        public static Style GetStyleByName(string name)
            => (Style)Application.Current.FindResource(name);

        private void ChangeButton(Thickness margin, string AddStyle = "MyActivateButton", string RemoveStyle = "MyActivateButton")
        {
            AddBlockButton.Style = MainWindow.GetStyleByName(AddStyle);
            RemoveBlockButton.Style = MainWindow.GetStyleByName(RemoveStyle);
            AddBlockButton.ToolTip = CreatorUIElements.CreateToolTip("Добавить строку таблицы");
            RemoveBlockButton.ToolTip = CreatorUIElements.CreateToolTip("Удалить строку таблицы");
            AddBlockButton.Margin = margin with { Left = margin.Left + LEFTOFFSETFORBUTTON, Top = margin.Top + TOPOFFSET + RemoveBlockButton.Height + TOPOFFSETFORBUTTON };
            RemoveBlockButton.Margin = AddBlockButton.Margin with { Left = AddBlockButton.Margin.Left + OFFSETFROMADDTOREMOVE };
        }

        private static bool UserAgree(string question) =>
            MessageBox.Show(question, "Подтверждение действия", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}