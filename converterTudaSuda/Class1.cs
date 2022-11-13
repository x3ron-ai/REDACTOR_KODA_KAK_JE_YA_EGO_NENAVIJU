using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace converterTudaSuda
{
    public class Rifle
    {
        public Rifle()
        {

        }
        public string model;
        public string caliber;
        public string color;
        public Rifle(string Model, string Caliber, string Color)
        {
            model = Model;
            caliber = Caliber;
            color = Color;
        }
    }
    public class Converters
    {
        internal static string ToText(string path)
        {
            string text = File.ReadAllText(path);

            List<Rifle> result;
            string ext = path.Split(".")[^1];
            if (ext == "xml") {
                XmlSerializer xml = new XmlSerializer(typeof(List<Rifle>));
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    result = (List<Rifle>)xml.Deserialize(fs);
                }
            }
            else if (ext == "json")
            {
                result = JsonConvert.DeserializeObject<List<Rifle>>(File.ReadAllText(path));
            }
            else
            {
                result = ToList(text);
            }
            string response = "";
            foreach (Rifle rifle in result)
            {
                response += $"{rifle.model}\n{rifle.caliber}\n{rifle.color}\n";
            }
            return response;
        }
        internal static List<Rifle> ToList(string text)
        {
            List<string> lines = text.Split("\n").ToList();
            List<Rifle> rifles = new List<Rifle>();
            lines.RemoveAll(x => x == "");
            for (int i = 0; i < lines.Count(); i += 3)
            {
                try {
                    string model = lines[i];
                    string caliber = lines[i + 1];
                    string color = lines[i + 2];
                    Rifle rifle = new Rifle(model, caliber, color);
                    rifles.Add(rifle); 
                }
                catch
                {
                    break;
                }
            }
            return rifles;
        }
        internal static string ToJson(string text)
        {
            List<Rifle> rifles = ToList(text);
            return JsonConvert.SerializeObject(rifles, Formatting.Indented);
        }
        internal static string ToXml(string text)
        {
            List<Rifle> rifles = ToList(text);
            XmlSerializer xml = new XmlSerializer(typeof(List<Rifle>));
            using (FileStream fs = new FileStream("cache.xml", FileMode.OpenOrCreate))
            {
                xml.Serialize(fs, rifles);
            }
            string response = File.ReadAllText("cache.xml");
            File.Delete("cache.xml");
            return response;
        }
        public static void Convert(string path)
        {
            string converted = Converters.ToText(path);
            string ext = path.Split(".")[^1];
            Console.WriteLine("Введите путь до файла, в который надо сохранить сериализованный текст");
            string exp = Console.ReadLine();
            Console.Clear();
            if (ext == "xml")
            {
                converted = Converters.ToXml(converted);
                File.WriteAllText(exp, converted);
            }
            else if (ext == "json")
            {
                converted = Converters.ToJson(converted);
                File.WriteAllText(exp, converted);
            }
            else
            {
                File.WriteAllText(exp, ToText(converted));
            }
            Console.Clear();
            Console.WriteLine("Готово!");
        }
        
    }
    public class Cursor
    {
        public int position = 1;
        public int offset = 1;
        public int max = 1;
    }
    public class OpenedFile
    {
        public string path;
        public string text;
    }
    public class Editor
    {
        public static string Edit(string path)
        {
            string readFile = Converters.ToText(path);
            string file = EditFile(readFile.Split("\n").ToList(), path);
            File.WriteAllText(path, file);
            return file;
        }
        private static string EditFile(List<string> text, string path)
        {
            Console.Clear();
            static string ArrayToString(List<char> line)
            {
                string response = "";
                foreach (char c in line)
                {
                    response += c;
                }
                return response;
            }
            static List<char> RemoveChar(List<char> line, int position)
            {
                List<char> new_line = new List<char>();
                int y = 0;
                for (int i = 0; i < line.Count(); i++)
                {
                    if (i != position)
                    {
                        new_line.Add(line[i]);
                        y++;
                    }
                }
                return new_line;
            }
            int pos_x = 0;
            int max_pos_x = 0;
            int pos_y = 0;
            int max_pos_y = 0;
            ConsoleKeyInfo aa;
            bool exit = false;
            while (!exit)
            {
                if (pos_x < 0)
                    pos_x = 0;
                if (pos_y < 0)
                    pos_y = 0;
                if (pos_x > max_pos_x)
                    pos_x = max_pos_x;
                Console.Clear();
                max_pos_y = 0;
                Console.SetCursorPosition(0, 0);
                foreach (string line in text)
                {
                    max_pos_y++;
                    Console.WriteLine(line);
                }
                List<char> currentLine = new List<char>();
                currentLine.AddRange(text[pos_y].ToArray());

                max_pos_x = currentLine.Count();
                Console.SetCursorPosition(pos_x, pos_y);
                aa = Console.ReadKey();
                switch (aa.Key)
                {
                    case ConsoleKey.RightArrow:
                        if (pos_x != max_pos_x)
                            pos_x++;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (pos_x != 0)
                            pos_x--;
                        break;
                    case ConsoleKey.UpArrow:
                        if (pos_y != 0)
                            pos_y--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (pos_y != max_pos_y - 1)
                            pos_y++;
                        break;
                    case ConsoleKey.Backspace:
                        if (pos_x != 0)
                        {
                            pos_x -= 1;
                            currentLine = RemoveChar(currentLine, pos_x);
                            text[pos_y] = ArrayToString(currentLine);
                        }
                        else
                        {
                            if (pos_y != 0)
                            {
                                text[pos_y - 1] += ArrayToString(currentLine);
                                text.RemoveAt(pos_y);
                                pos_y -= 1;
                                pos_x = 0;
                            }
                        }
                        break;
                    case ConsoleKey.Escape:
                        exit = true;
                        break;
                    case ConsoleKey.Enter:
                        string partOfLine = ArrayToString(currentLine.ToArray()[pos_x..^0].ToList());
                        text.Insert(pos_y+1, partOfLine);
                        currentLine = currentLine.ToArray()[0..pos_x].ToList();
                        text[pos_y] = ArrayToString(currentLine);
                        pos_x = 0;
                        pos_y++;
                        break;
                    case ConsoleKey.Delete:
                        break;
                    default:
                        {
                            currentLine.Insert(pos_x, aa.KeyChar);
                            text[pos_y] = ArrayToString(currentLine);
                            pos_x += 1;
                            max_pos_x += 1;
                            break;
                        }
                }
            }
            string response = "";
            foreach(string line in text)
            {
                response += (line+"\n");
            }
            if (path.Contains(".xml"))
            {
                response = Converters.ToXml(response).Replace("\n", "");
            }
            else if (path.Contains(".json"))
            {
                response = Converters.ToJson(response);
            }
            return response;
        }
    }
}
