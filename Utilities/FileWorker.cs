using System.IO;
using System.Text.Json;

namespace VectorChat.Utilities
{
	public static class FileWorker
	{
		public static void SaveToFile<TItem>(string path, TItem item, JsonSerializerOptions options = null)
		{
			File.WriteAllText(path, JsonSerializer.Serialize(item, item.GetType(), options));
		}

		public static TItem LoadFromFile<TItem>(string path, JsonSerializerOptions options = null)
		{
			return JsonSerializer.Deserialize<TItem>(File.ReadAllText(path), options);
		}
	}
}
