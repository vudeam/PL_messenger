using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace VectorChat.Utilities
{
	public static class FileWorker
	{
		public static void SaveToFile(string path, object item, Formatting options = Formatting.Indented)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(item, options));
		}

		public static TItem LoadFromFile<TItem>(string path, JsonSerializerSettings settings = null)
		{
			return JsonConvert.DeserializeObject<TItem>(File.ReadAllText(path), settings);
		}

		public static TItem LoadFromFileIfExists<TItem>(string path, JsonSerializerSettings settings = null)
		{
			return File.Exists(path) ? FileWorker.LoadFromFile<TItem>(path, settings) : default(TItem);
		}

		public static async void SaveToFileAsync(string path, object item, Formatting options = Formatting.Indented)
		{
			await Task.Run(() => File.WriteAllTextAsync(path, JsonConvert.SerializeObject(item, options)));
		}

		public static void SaveToBinary(string path, object item, FileMode mode = FileMode.Append)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream fstream = new FileStream(path, mode))
			{
				formatter.Serialize(fstream, item);
			}
		}

		public static TItem LoadFromBinary<TItem>(string path, FileMode mode = FileMode.Open)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream fstream = new FileStream(path, mode))
			{
				return (TItem)formatter.Deserialize(fstream);
			}
		}
	}
}
