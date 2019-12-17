using System;
namespace Roguelike
{
    public class MenuItem
    {
        private string name;
        public string Name
        {
            get { return name; }
            protected set { name = value; }
        }
        public virtual void OnClick()
        {
            //do nothing
        }
    }

    class MenuItemNewGame : MenuItem
    {
        public MenuItemNewGame()
        {
            Name = "New Game";
        }
        public override void OnClick()
        {
            Program.GameEngine.Init();
            Program.GameEngine.PlayGame();
        }
    }
    class MenuItemSave : MenuItem
    {
        public MenuItemSave()
        {
            Name = "Save";
        }
        public override void OnClick()
        {
            Int16 number;
            Console.Write("Select savefile:");
            while (!Int16.TryParse(Console.ReadLine(), out number) || number < 1 || number > 5)
            {
                Console.WriteLine("Wrong input or savefile number > 6");
            }
            if (Program.GameEngine.GameStarted == true)
            {
                Program.GameEngine.Save(number);
                Program.GameEngine.PlayGame();
            }
        }
    }
    class MenuItemLoad : MenuItem
    {
        public MenuItemLoad()
        {
            Name = "Load";
        }
        public override void OnClick()
        {
            string path = Program.GameEngine.MakeCorrectPath();
            Int16 number;
            Console.Write("Select loadfile:");
            while (!Int16.TryParse(Console.ReadLine(), out number) || number < 1 || number > 5)
            {
                Console.WriteLine("Wrong input");
            }
            if (System.IO.File.Exists(path+$"{number}.txt"))
            {
                Program.GameEngine.InitFromSave(number);
                Program.GameEngine.PlayGame();
            }
        }
    }
    class MenuItemResume : MenuItem
    {
        public MenuItemResume()
        {
            Name = "Resume";
        }
        public override void OnClick()
        {
            if (Program.GameEngine.GameStarted == true)
                Program.GameEngine.PlayGame();
        }
    }

    class MenuItemSettings : MenuItem
    {
        public MenuItemSettings()
        {
            Name = "Settings";
        }
    }

    class MenuItemExit : MenuItem
    {
        public MenuItemExit()
        {
            Name = "Exit";
        }
        public override void OnClick()
        {
            Program.CleanUpAndExit();
        }
    }
}
