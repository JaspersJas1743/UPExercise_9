using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetOffice.WordApi.Enums;
using Word = NetOffice.WordApi;

namespace Задание__9
{
    public static class DocumentCreator
    {
        private readonly static Word.Application App;
        private const string DEFAULTFILENAME = "Введите имя файла (без .docx)";
        private const string DEFAULTTABLENAME = "Введите название таблицы";

        static DocumentCreator() => App = new Word.Application();

        public static FileInfo CreateFileWithTable(string fileName, string tableName, ICollection<string[]> collection)
        {
            if (collection.Count.Equals(0))
                throw new Exception("Нельзя создать таблицу без строк!");
            Word.Document doc = App.Documents.Add();
            App.Selection.TypeText(tableName);
            Word.Table table = doc.Tables.Add(App.Selection.Range, collection.Count, 2);
            table.Borders.Enable = 1;
            for (int row = 0; row < collection.Count; ++row)
            {
                for (int column = 0; column < 2; ++column)
                {
                    table.Cell(row + 1, column + 1).Select();
                    App.Selection.TypeText(collection.ElementAt(row)[column].ToString());
                }
            }
            App.Selection.GoToNext(WdGoToItem.wdGoToLine);
            App.Selection.TypeText("Таблица сгенерирована средствами работы .NET с MS Word");
            FileInfo file = CreateNameForFile(
                CreateDirectoryIfEmpty(Environment.CurrentDirectory + @"\Assets\Documents"), fileName
            );
            doc.SaveAs(file.FullName);
            doc.Close();
            return file;
        }

        private static FileInfo CreateNameForFile(DirectoryInfo directory, string fileName)
        {
            fileName = String.IsNullOrWhiteSpace(fileName) || fileName == DEFAULTFILENAME ? "Безымянный" : fileName;
            FileInfo[] files = directory.GetFiles($"{fileName}*");
            if (files.Length == 1) fileName += "(1)";
            else if (files.Length > 1)
                fileName = files.OrderBy(x => x.Name).Last().Name.Replace($"({files.Length - 1})", $"({files.Length})").Replace(".docx", String.Empty);
            return new FileInfo(directory.FullName + $"\\{fileName}.docx");
        }

        private static DirectoryInfo CreateDirectoryIfEmpty(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
                directory.Create();
            return directory;
        }

        public static void Exit()
            => App.Quit(WdSaveOptions.wdDoNotSaveChanges);
    }
}