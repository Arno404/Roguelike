using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace Roguelike
{   [DataContract]
    class InfoBorder : Rectangle
    {   [DataMember]
        private string[] Info;
        [DataMember]
        private int Index = 0;

        public InfoBorder() { }

        public InfoBorder(int width, int height, Point location) : base(width, height, location)
        {
            Info = new string[height - 2];
        }

        public InfoBorder(int width, int height, int x, int y) : base(width, height, x, y)
        {
            Info = new string[height - 2];
        }

        public void Clear()
        {
            if (Info == null)
                Info = new string[Height - 2];
            Index = 0;
            for (int i = 0; i < Info.Length; i++)
                Info[i] = null;
            for (int i = 0; i < Height - 2; i++)
            {
                ClearLine(i);
            }
        }

        public void ClearAndWrite(string line)
        {
            Clear();
            WriteLine(line);
            Info[Index] = line;
            Index++;
        }

        public void WriteNextLine(string line)
        {
            if (Info == null)
                Info = new string[Height - 2];
            if (Index < Height - 2)
            {
                Info[Index] = line;
                ClearLineAndWrite(line, Index);
                Index++;
            }
            else
            {
                for (int i = 0; i < Info.Length - 1; i++)
                {
                    Info[i] = Info[i + 1];
                    ClearLineAndWrite(Info[i], i);
                }
                ClearLineAndWrite(line, Index - 1);
                Info[Index - 1] = line;
            }
        }

        public void ClearLine(int number = 0)
        {
            Console.SetCursorPosition(Offset.X, Offset.Y + number);
            Console.Write(new string(' ', Width - 2));
        }

        public void WriteLine(string line, int number = 0)
        {
            Console.SetCursorPosition(Offset.X, Offset.Y + number);
            Console.Write(line);
        }

        public void ClearLineAndWrite(string line, int number = 0)
        {
            ClearLine(number);
            WriteLine(line, number);
        }
    }
}