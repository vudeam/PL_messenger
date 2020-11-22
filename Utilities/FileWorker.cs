using System.IO;
using System.Text.Json;

namespace VectorChat.Utilities
{
	public static class FileWorker
	{
		public static void SaveToFile<TItem>(string path, TItem item)
		{
			File.WriteAllText(path, JsonSerializer.Serialize(item));
		}

		public static TItem LoadFromFile<TItem>(string path)
		{
			return JsonSerializer.Deserialize<TItem>(File.ReadAllText(path));
		}
	}
}
