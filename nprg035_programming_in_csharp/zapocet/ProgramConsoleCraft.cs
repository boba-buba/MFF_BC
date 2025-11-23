using System;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data;

namespace zapocetConsoleCraft
{   
    public class ProgramInputOutputState : IDisposable
    {
        public const string ArgumentErrorMessage = "Argument Error";
        public const string FileErrorMessage = "Could not find file \'";

        public StreamReader? Reader { get; private set; }

        public bool InitializeFromCommandLineArgs(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Write(ArgumentErrorMessage + "\n");
                return false;
            }

            try
            {
                Reader = new StreamReader(args[0]);
            }
            catch (IOException)
            {
                Console.Write(FileErrorMessage + args[0] + "\'\n");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.Write(FileErrorMessage + args[0] + "\'\n");
                return false;
            }
            catch (ArgumentException)
            {
                Console.Write(FileErrorMessage + args[0] + "\'\n");
                return false;
            }
            return true;
        }
        public void Dispose()
        {
            Reader?.Dispose();
        }
    }
    abstract record Recipe
    {

        public string Name;
        public Dictionary<string, int> _ingredients;
        public bool Locked;
        public bool Known;
    }
    record RecipeItem : Recipe 
    {

        public RecipeItem(string name, Dictionary<string, int> ingredients, bool locked, int instances, bool known)
        {
            Name = name;
            _ingredients = ingredients;
            Locked = locked;
            NumberOfInstances = instances;
            Known = known;
        }
        public int NumberOfInstances { get; private set; }
    }
    record RecipeEureka : Recipe
    {
        public RecipeEureka(string name, Dictionary<string, int> ingredients, bool locked)
        {
            Name = name;
            _ingredients = ingredients;
            Locked = locked;
            Known = false;
        }
        private List<string> _itemsToUnlock = new List<string>();
        public void AddItemsToUnlock(string name) => _itemsToUnlock.Add(name);
        public List<string> GetItemsToUnlock() => _itemsToUnlock;
        public string GetItemsWithLock { get
            {
                string s = "";
                foreach (string item in _itemsToUnlock) { s += ' ' + item; }
                return s;
            }
        } 
    }
    class GameData
    {
        private Dictionary<string, int> _myObjects = new Dictionary<string, int> { }; // -1 is infinity
        private Dictionary<string, Recipe> _recipes = new Dictionary<string, Recipe>();
        private Dictionary<string, int> ParseIngredients(string ingr)
        {
            var resultIngr = new Dictionary<string, int>();
            string[] ingredients = ingr.Split(',');
            foreach(string item in  ingredients)
            {
                string[] pair = item.Split(' ');
                resultIngr[pair[1]] = int.Parse(pair[0]);
            }
            return resultIngr;
        }
        private void ReadItemRecipe(StreamReader reader)
        {
            string name = reader.ReadLine();
            var ingredients = ParseIngredients(reader.ReadLine());
            bool locked = reader.ReadLine() == "Locked" ? true : false; 
            int instances = int.Parse(reader.ReadLine());
            bool known =  reader.ReadLine() == "Known" ? true : false;
            //reader.ReadLine();

            RecipeItem item = new RecipeItem(name, ingredients, locked, instances, known);
            _recipes[name] = item;
            
        }
        private void ReadEurekaRecipe(StreamReader reader)
        {
            string name = reader.ReadLine();
            var ingredients = ParseIngredients(reader.ReadLine());
            bool locked = reader.ReadLine() == "Locked" ? true : false;
            
            RecipeEureka eureka = new RecipeEureka(name, ingredients, locked);
            string[] itemsToUnlock = reader.ReadLine().Split(',');
            foreach(string item in itemsToUnlock)
            {
                eureka.AddItemsToUnlock(item);
            }
            _recipes[name] = eureka;
            //reader.ReadLine();
        }
        private void ReadInitObjects(StreamReader reader)
        {
            while (reader.Peek() > -1)
            {
                string[] items = reader.ReadLine().Split(',');
                foreach(string item in items)
                {
                    string[] pair = item.Split(' ');
                    if (pair[0] == "*") { _myObjects[pair[1]] = -1; }
                    else { _myObjects[pair[1]] = int.Parse(pair[0]); }
                }
            }
            
        }
        public void ReadInitData(StreamReader reader)
        {
            while (reader.Peek() > -1)
            {
                var line = reader.ReadLine();
                switch (line)
                {
                    case "item":
                        ReadItemRecipe(reader); 
                        break;
                    case "eureka":
                        ReadEurekaRecipe(reader);
                        break;
                    case "===":
                        ReadInitObjects(reader);
                        break;
                    case "---":
                        break;
                    default: break;
                }
            }
            reader.Close();
        }
        public Recipe? GetRecipe(string name)
        {
            Recipe rec = _recipes[name];
            if (rec == null) return null;
            if (rec.Locked) return null;
            if (rec.GetType() == typeof(RecipeItem) && ((RecipeItem)rec).Known)
            {
                return rec;
            }
            else if (rec.GetType() == typeof(RecipeEureka))
            {
                return rec;
            }
            return null;
        }
        public int GetNumberOfInstancesOfObject(string name) 
        {
            if (_myObjects.ContainsKey(name))
            {
                return _myObjects[name];
            }
            return 0;
        }
        public void AddObjectInstances(string name, int number)
        {
            if (_myObjects.ContainsKey(name))
            {
                if (_myObjects[name] == -1) return;
                else _myObjects[name] += number;
            }
            else _myObjects[name] = number;
        }
        public void DeleteObjectInstances(string name, int number)
        {
            if (_myObjects[name] == -1) return;
            else _myObjects[name] -= number;
        }
        public void UnlockItems(List<string> list)
        {
            foreach (string item in list)
            {
                if (_recipes.ContainsKey(item))
                {
                    _recipes[item].Locked = false;
                    if (_recipes[item].GetType() == typeof(RecipeItem)) {_recipes[item].Known = true; }
                }
                else
                {
                    _recipes[item] = null;
                }
            }
        }
        public void PrintRecipes()
        {
            foreach (var recipe in _recipes)
            {
                if ( recipe.Value == null || !recipe.Value.Known) continue;
                Console.WriteLine(recipe.Key);
                Console.WriteLine("Ingedients: ");
                foreach(var ing in recipe.Value._ingredients)
                {
                    Console.WriteLine(ing.Key + " " + ing.Value.ToString());
                }
                Console.WriteLine("# of instances " + ((RecipeItem)recipe.Value).NumberOfInstances);
                Console.WriteLine("-o0o-");
            }
            Console.WriteLine();
        }
        public void PrintInventory()
        {
            foreach (var obj in _myObjects)
            {
                Console.WriteLine(obj.Key + " " + obj.Value.ToString());
            }
            Console.WriteLine();
        }
        public string? CheckForAlreadyExisting(Dictionary<string, int> ingredients, out bool IngrOK, out bool IngNumOK, out bool IsItem)
        {
            
            string? result = null;
            IngrOK = true; IngNumOK = true; IsItem = false;
             
            foreach (var recipe in _recipes) 
            {
                
                if (recipe.Value == null) continue;
                if (recipe.Value._ingredients.Count != ingredients.Count) continue;
                IngrOK = true;
                IngNumOK = true;
                IsItem = false;
                foreach(var ing in ingredients)
                {
                    if (!recipe.Value._ingredients.ContainsKey(ing.Key)) 
                    {
                        IngrOK = false;
                        IngNumOK = false;
                        IsItem = false;
                        break;
                    }
                    else
                    {
                        if (recipe.Value._ingredients[ing.Key] != ing.Value)
                        {
                            IngrOK = true;
                            IngNumOK &= false;
                        }
                        else 
                            IngNumOK &= true;
                    }
                }
                if (IngrOK)
                {
                    result = !recipe.Value.Locked ? recipe.Key : null;
                    if (recipe.Value.GetType() == typeof(RecipeItem)) 
                        IsItem = true;
                    break;
                }
                
            }

            return result;
        }
        public void DeleteRecipe(string name)
        {
           _recipes.Remove(name);
        }
    }

    class Game
    {
        private GameData _data = new GameData();
        private StreamReader _reader;
        public Game(StreamReader reader)
        {
            _reader = reader;
        }
        private bool EnoughResources(Recipe recipe)
        {
            foreach (var ingredient in recipe._ingredients)
            {
                int count = _data.GetNumberOfInstancesOfObject(ingredient.Key);
                if (ingredient.Value > count && count != -1)
                {
                    return false;
                }
            }
            return true;
        } 
        private void CookRecipe(Recipe item)
        {
            if(!EnoughResources(item)) { Console.WriteLine("Not enough Resources"); return; }
            foreach (var ingredient in item._ingredients)
            {
                _data.DeleteObjectInstances(ingredient.Key, ingredient.Value);
            }

            if (item.GetType() == typeof(RecipeItem))
            {
                var RecItem = (RecipeItem)item;
                _data.AddObjectInstances(RecItem.Name, RecItem.NumberOfInstances);
                Console.WriteLine("Created " + RecItem.NumberOfInstances + " " + RecItem.Name );
            }
            else
            {
                var RecEureka = (RecipeEureka)item;
                _data.UnlockItems(RecEureka.GetItemsToUnlock());
                Console.WriteLine("Unlocked Recipes:" + RecEureka.GetItemsWithLock);

            }
            
        }
        private void CraftRequest(string[] tokens)
        {
            if (tokens.Length == 1) { return; }
            string name = tokens[1];
            var recipe = _data.GetRecipe(name);
            if (recipe == null) return;

            CookRecipe(recipe);
        }
        private void RecipesRequest(string[] tokens)
        {
            _data.PrintRecipes();
        }
        private void InventoryRequest()
        {
            _data.PrintInventory();
        }
        private void CombineRequest(string line)
        {
            string[] tokens = line.Split(',');
            var first = tokens[0].Split(' ');
            var ingredients = new Dictionary<string, int> { { first[2], int.Parse(first[1]) } };

            for (int i = 1; i < tokens.Length; i++)
            {
                var pair = tokens[i].Split(' ');
                ingredients[pair[1]] = int.Parse(pair[0]);
            }
            bool IngrOK;
            bool IngrNum;
            bool IsItem;
            string? existingRecipe = _data.CheckForAlreadyExisting(ingredients, out IngrOK, out IngrNum, out IsItem);
            if (existingRecipe != null)
            {
                if (IngrOK && IsItem && IngrNum) 
                {
                    if (!EnoughResources(_data.GetRecipe(existingRecipe))) { Console.WriteLine(":("); return; }
                    CraftRequest(new string[] { "craft", existingRecipe });
                }
                else if (IngrOK && IsItem) { Console.WriteLine("Discovered " + existingRecipe); }
                else if (IngrOK && !IngrNum && !IsItem) { Console.WriteLine("Almost..."); }
                else if (IngrOK && IngrNum && !IsItem)
                {
                    Console.WriteLine("EUREKA! " + existingRecipe);
                    if (!EnoughResources(_data.GetRecipe(existingRecipe))) { Console.WriteLine(":("); return; }
                    CraftRequest(new string[] { "craft", existingRecipe });
                    _data.DeleteRecipe(existingRecipe);
                }
            }
            else Console.WriteLine(":(");
        }
        public void StartTheGame()
        {
            _data.ReadInitData(_reader);
            Console.WriteLine("===Start of The Game===");
            while (true)
            {
                string req = Console.ReadLine();
                string[] line = req.Split(' ');
                switch(line[0])
                {
                    case "exit":
                        return;
                    case "craft":
                        CraftRequest(line);
                        break;
                    case "combine":
                        CombineRequest(req);
                        break;
                    case "recipes":
                        RecipesRequest(line);
                        break;
                    case "inventory":
                        InventoryRequest();
                        break;
                    default:
                        Console.WriteLine("Unknown request");
                        break;
                }
            }
        }
    }
    internal class Program2
    {
        static void Main2(string[] args)
        {
            ProgramInputOutputState state = new ProgramInputOutputState();
            if (!state.InitializeFromCommandLineArgs(args))
            {
                return;
            }
            Game game = new Game(state.Reader);
            game.StartTheGame();
        }
    }
}
