using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KorshunovDirectory
{
    class Program
    {
        private static DirectoryInfo _rootDirectory;
        private static string[] _specDirectories = new string[] { "Изображения", "Документы", "Прочее" };
        private static int _imagesCount = 0, _documentsCount = 0, _othersCount = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Введите путь к диску:");
            string directoryPath = Console.ReadLine();
            var driveInfo = new DriveInfo(directoryPath);
            Console.WriteLine($"Информация о диске: {driveInfo.VolumeLabel},всего {driveInfo.TotalSize / 1024 / 1024} МБ, " +
            $"свободно {driveInfo.AvailableFreeSpace / 1024 / 1024} МБ. Нажмите Enter для продолжения");
            Console.ReadLine();

            _rootDirectory = driveInfo.RootDirectory;
            SearchDirectories(_rootDirectory);

            foreach (var directory in _rootDirectory.GetDirectories())
            {
                if (!_specDirectories.Contains(directory.Name))
                    directory.Delete(true);
            }

            var resultText = $"Всего обработано {_imagesCount + _documentsCount + _othersCount} Файлов." +
            $"Из них {_imagesCount} изображение, {_documentsCount} документов, {_othersCount} прочих файлов.";
            Console.WriteLine(resultText);
            File.WriteAllText(_rootDirectory + "\\Инфо.txt", resultText);

            Console.ReadLine();

        }
        private static void SearchDirectories(DirectoryInfo currentDirectory)
        {
            if (!_specDirectories.Contains(currentDirectory.Name))
            {
                FilterFiles(currentDirectory);
                foreach (var childDirectory in currentDirectory.GetDirectories())
                {
                    SearchDirectories(childDirectory);
                }

            }
        }
        private static void FilterFiles(DirectoryInfo currentDirectory)
        {
            var currentFiles = currentDirectory.GetFiles();
            foreach (var fileInfo in currentFiles)
            {
                if (new string[] { ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".svg" }
                .Contains(fileInfo.Extension.ToLower()))
                {
                    var photoDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[0]}\\");
                    if (!photoDirectory.Exists)
                        photoDirectory.Create();

                    var yearDirectory = new DirectoryInfo(photoDirectory + $"{fileInfo.LastWriteTime.Date.Year}\\");
                    if (!yearDirectory.Exists)
                        yearDirectory.Create();

                    MoveFile(fileInfo, yearDirectory);
                    _imagesCount = _imagesCount + 1;
                }
                else if (new string[] { ".doc", ".docx", ".pdf", ".xls", ".ppt", ".pptx" }
                .Contains(fileInfo.Extension.ToLower()))
                {
                    var documentsDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[1]}\\");
                    if (!documentsDirectory.Exists)
                        documentsDirectory.Create();

                    DirectoryInfo lengthDirectory = null;
                    if (fileInfo.Length / 1024 / 1024 < 1)
                        lengthDirectory = new DirectoryInfo(documentsDirectory + "Менее 1 МБ\\");
                    else if (fileInfo.Length / 1024 / 1024 > 10)
                        lengthDirectory = new DirectoryInfo(documentsDirectory + "Более 10 МБ\\");
                    else
                        lengthDirectory = new DirectoryInfo(documentsDirectory + "От 1 до 10 МБ\\");

                    if (!lengthDirectory.Exists)
                        lengthDirectory.Create();

                    MoveFile(fileInfo, lengthDirectory);
                    _documentsCount++;
                }
                else
                {
                    var othersDirectory = new DirectoryInfo(_rootDirectory + $"{_specDirectories[2]}\\");
                    if (!othersDirectory.Exists)
                        othersDirectory.Create();

                    MoveFile(fileInfo, othersDirectory);
                    _othersCount++;
                }
            }
        }
        private static void MoveFile(FileInfo fileInfo, DirectoryInfo dirInfo)
        {
            var newFileInfo = new FileInfo($"{dirInfo}\\{fileInfo.Name}");
            while (newFileInfo.Exists)
            {
                newFileInfo = new FileInfo($"{dirInfo}\\{Path.GetFileNameWithoutExtension(newFileInfo.FullName)}_{fileInfo.Extension}");
            }

            fileInfo.MoveTo(newFileInfo.FullName);
        }
    }

}
