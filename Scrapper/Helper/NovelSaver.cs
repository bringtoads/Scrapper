namespace Scrapper.Helper
{
    internal class NovelSaver
    {
        public static string SaveChapter(string novelName, string chapterName, string content)
        {
            // Get the root directory of the project
            string projectDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo? parent = Directory.GetParent(projectDirectory);
            parent = parent?.Parent;
            parent = parent?.Parent;
            string baseFolder = Path.Combine(projectDirectory, "novels");
            string novelFolder = Path.Combine(baseFolder, novelName);
            //string baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "novels");
            //string novelFolder = Path.Combine(baseFolder, novelName);
            if (parent == null)
            {
                throw new InvalidOperationException("Unable to determine the project directory.");
            }

            string projectDirectoryPath = parent.FullName;
            if (!Directory.Exists(novelFolder))
            {
                Directory.CreateDirectory(novelFolder);
            }

            // Sanitize chapter name for file name safety
            string safeChapterName = string.Join("_", chapterName.Split(Path.GetInvalidFileNameChars()));
            string chapterFilePath = Path.Combine(novelFolder, $"{safeChapterName}.txt");

            // Write content to the file
            File.WriteAllText(chapterFilePath, content);
            return chapterFilePath;

        }
    }
}
