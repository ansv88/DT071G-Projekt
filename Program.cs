/* Konsolapp för generering av slumpade veckomatsedlar, av Annelie Svensson */

using LiteDB;
using System.Text.Json.Serialization;
using STJson = System.Text.Json;

namespace ProjectApp
{
    internal class Program
    {
        //Listor för att hålla maträtter och menyer
        private static List<Dish> dishes = new List<Dish>();
        private static List<Menu> menus = new List<Menu>();

        //Array med veckodagar som används för att koppla rätter till veckodagar
        private static readonly string[] weekDays = { "Måndag", "Tisdag", "Onsdag", "Torsdag", "Fredag", "Lördag", "Söndag" };

        static void Main(string[] args)
        {
            //Skapa databasens sökväg, kontrollera och skapa Data-mappen om den inte existerar
            var dataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }

            var dbPath = Path.Combine(dataFolderPath, "database.db");

            try
            {
                //Initiera LiteDB-databasen
                using (var db = new LiteDatabase(dbPath))
                {
                    //Ladda maträtter och menyer från databasen
                    LoadDishes(db);
                    LoadMenus(db);

                    bool exitProgram = false;

                    //Huvudloopen för programmet
                    while (!exitProgram)
                    {
                        PrintMenu(); //Visa huvudmenyn
                        Console.Write("\nVal: ");
                        var userInput = Console.ReadLine() ?? ""; //Tilldela tom sträng om null

                        //Hantera användarval
                        switch (userInput.ToLower())
                        {
                            case "1":
                                GenerateMenu(db); //Generera en ny veckomeny
                                break;

                            case "2":
                                EditMenu(db); //Ändra/slumpa om en maträtt i en befintlig veckomeny
                                break;

                            case "3":
                                AddDish(db); //Lägg till en ny maträtt i databasen
                                break;

                            case "4":
                                DisplayAllMenus(); //Visa alla sparade veckomenyer
                                break;

                            case "5":
                                DeleteMenu(db); //Ta bort en sparad veckomeny
                                break;

                            case "6":
                                var menuPrinter = new MenuPrinter();
                                menuPrinter.PrintToDocument(db); //Skriv ut menyer till en textfil
                                break;

                            case "x":
                                exitProgram = true; //Avsluta programmet
                                break;

                            default:
                                Console.WriteLine("\nVälj ett giltigt menyval.\n\nTryck på valfri knapp för att fortsätta...");
                                Console.ReadLine();
                                break;
                        }
                    }
                }
            }
            catch (LiteException ex) //Felhantering om det uppstår fel vid initiering av databasen
            {
                Console.WriteLine($"Fel vid initiering av databasen: {ex.Message}");
                Console.ReadLine();
            }
        }

        //Metod för att ladda maträtter från databasen
        private static void LoadDishes(LiteDatabase db)
        {
            var dishesCollection = db.GetCollection<Dish>("dishes"); //Hämta maträttskollektionen från databasen
            dishes = dishesCollection.FindAll().ToList();

            //Databasen är tom, ladda data från JSON-filen
            if (dishes.Count == 0)
            {
                LoadDishesFromJson(dishesCollection);
                dishes = dishesCollection.FindAll().ToList();
            }
        }

        //Metod för att ladda alla sparade menyer från databasen
        private static void LoadMenus(LiteDatabase db)
        {
            var menusCollection = db.GetCollection<Menu>("menus"); //Hämta menykollektionen från databasen
            menus = menusCollection.Include(m => m.Dishes).FindAll().ToList();
        }

        //Metod för att skriva ut huvudmenyn
        private static void PrintMenu()
        {
            Console.Clear();

            //Skriv ut huvudmenyn
            Console.WriteLine("\nGenerera veckans matsedel");
            Console.WriteLine("\n[ MENY ]");
            Console.WriteLine("\n1. Slumpa veckomeny");
            Console.WriteLine("\n2. Slumpa om en maträtt i veckomenyn");
            Console.WriteLine("\n3. Lägg till maträtt i databasen");
            Console.WriteLine("\n4. Se alla skapade veckomenyer");
            Console.WriteLine("\n5. Ta bort en veckomeny");
            Console.WriteLine("\n6. Skriv ut veckomenyer till textfil");
            Console.WriteLine("\n\nX. Avsluta");
            Console.WriteLine(new string('-', 30));
        }

        //Metod för att generera en ny veckomeny
        private static void GenerateMenu(LiteDatabase db)
        {
            var menusCollection = db.GetCollection<Menu>("menus"); //Hämta menykollektionen från databasen

            Console.WriteLine("\n\nHur många rätter ska vara på menyn? (1 rätt/dag)");
            int.TryParse(Console.ReadLine(), out int numberOfDishes); //Omvandla inmatning till heltal

            //Kontrollera att användaren har angett ett giltigt antal rätter
            if (numberOfDishes < 1 || numberOfDishes > 31)
            {
                Console.WriteLine($"Antalet rätter måste vara mellan 1 och 31. Försök igen.");
                Console.ReadLine();
                return;
            }
            else
            {
                Console.WriteLine("\nVill du att rätter från alla kategorier slumpas [1] eller vill du välja fördelning själv [2]?");
                var chosenCategories = Console.ReadLine();

                Menu newMenu = new Menu();

                if (chosenCategories == "1")
                {
                    //Slumpa maträtter från alla kategorier
                    newMenu.Dishes = GenerateRandomMenu(numberOfDishes);

                }
                else if (chosenCategories == "2")
                {
                    //Låter användaren välja antal rätter från varje kategori
                    int meatCount = GetDishCountFromUser(DishCategory.Kött);
                    int fishCount = GetDishCountFromUser(DishCategory.Fisk);
                    int chickenCount = GetDishCountFromUser(DishCategory.Kyckling);
                    int vegCount = GetDishCountFromUser(DishCategory.Vegetarisk);
                    int soupCount = GetDishCountFromUser(DishCategory.Soppa);

                    //Kontrollera om totala antalet valda rätter stämmer
                    int totalSelectedDishes = meatCount + fishCount + chickenCount + vegCount + soupCount;
                    if (totalSelectedDishes > numberOfDishes)
                    {
                        Console.WriteLine($"\nDet totala antalet rätter i valda kategorier ({totalSelectedDishes}) överstiger det angivna antalet ({numberOfDishes}). Försök igen.");
                        Console.ReadLine();
                        return;
                    }
                    else if (totalSelectedDishes < numberOfDishes)
                    {
                        Console.WriteLine($"\nDet totala antalet rätter i valda kategorier ({totalSelectedDishes}) understiger det angivna antalet ({numberOfDishes}). Försök igen.");
                        Console.ReadLine();
                        return;
                    }
                    //Generera meny baserat på användarens val
                    newMenu.Dishes = GenerateRandomMenuWithChoices(meatCount, fishCount, chickenCount, vegCount, soupCount); //Generera meny baserat på användarens val
                }
                else
                {
                    Console.WriteLine("Ogiltigt val, försök igen.");
                    Console.ReadLine();
                    return;
                }

                menusCollection.Insert(newMenu); //Spara den nya menyn i databasen
                menus.Add(newMenu); //Uppdatera den lokala listan av menyer
                DisplayMenu(newMenu); //Visa den genererade menyn

            }
        }

        //Metod för att generera en slumpad meny med maträtter från alla kategorier
        private static List<Dish> GenerateRandomMenu(int numberOfDishes)
        {
            if (dishes.Count == 0)
            {
                Console.WriteLine("Det finns inga maträtter att slumpa från.");
                return new List<Dish>();
            }

            return dishes.OrderBy(d => Guid.NewGuid()).Take(numberOfDishes).ToList(); //Slumpa maträtter från listan
        }

        //Metod för att fråga användaren om antal maträtter för en kategori
        private static int GetDishCountFromUser(DishCategory category)
        {
            Console.WriteLine($"\nHur många av rätterna ska vara {category.ToString().ToLower()}?");

            if (int.TryParse(Console.ReadLine(), out int count) && count >= 0) //Omvandla inmatning till heltal
                return count; //Returnera antal om det är giltigt

            Console.WriteLine("Ogiltigt antal, försök igen.");
            Console.ReadLine();
            return GetDishCountFromUser(category); //Fråga igen vid ogiltig inmatning
        }

        //Metod för att generera en veckomeny baserat på användarens val av antal rätter per kategori
        private static List<Dish> GenerateRandomMenuWithChoices(int meatCount, int fishCount, int chickenCount, int vegCount, int soupCount)
        {
            List<Dish> selectedDishes = new List<Dish>();

            //Lägg till rätter från varje vald kategori
            selectedDishes.AddRange(dishes.Where(d => d.Category == DishCategory.Kött).OrderBy(d => Guid.NewGuid()).Take(meatCount));
            selectedDishes.AddRange(dishes.Where(d => d.Category == DishCategory.Fisk).OrderBy(d => Guid.NewGuid()).Take(fishCount));
            selectedDishes.AddRange(dishes.Where(d => d.Category == DishCategory.Kyckling).OrderBy(d => Guid.NewGuid()).Take(chickenCount));
            selectedDishes.AddRange(dishes.Where(d => d.Category == DishCategory.Vegetarisk).OrderBy(d => Guid.NewGuid()).Take(vegCount));
            selectedDishes.AddRange(dishes.Where(d => d.Category == DishCategory.Soppa).OrderBy(d => Guid.NewGuid()).Take(soupCount));

            return selectedDishes.OrderBy(d => Guid.NewGuid()).ToList(); //Slumpa ordningen på rätterna
        }

        //Metod för att visa en veckomeny
        private static void DisplayMenu(Menu menu)
        {
            Console.WriteLine($"\nHär är din veckomeny (Meny ID: {menu.Id}):\n");

            //Iterera över maträtterna och tilldela veckodagar
            for (int i = 0; i < menu.Dishes.Count; i++)
            {
                string day = weekDays[i % weekDays.Length]; //Bestämmer vilken veckodag som ska användas
                var dish = menu.Dishes[i];  //Hämta maträtten

                if (day == "Söndag")
                {
                    Console.ForegroundColor = ConsoleColor.Red; //Sätt textfärgen till röd för söndag
                }
               
                Console.WriteLine($"{day}: {dish.Name} ({dish.Category})"); //Skriv ut veckodagen och maträtten

                Console.ResetColor(); //Återställ färgen efter att menyn skrivits ut
            }

            Console.WriteLine("\n\nTryck på valfri knapp för att fortsätta...");
            Console.ReadLine();
        }

        //Metod för att redigera/slumpa om en maträtt i en veckomeny
        private static void EditMenu(LiteDatabase db)
        {
            var menusCollection = db.GetCollection<Menu>("menus"); //Hämta menykollektionen från databasen

            DisplayAllMenus(); //Visa alla tillgängliga menyer

            Console.WriteLine("\nAnge Id för den meny du vill redigera:");

            if (int.TryParse(Console.ReadLine(), out int menuID)) //Omvandla inmatning till heltal
            {
                var menu = menusCollection.Include(m => m.Dishes).FindById(menuID); //Hämta den valda menyn från databasen

                if (menu == null)
                {
                    Console.WriteLine("Meny med angivet ID hittades inte.");
                    Console.ReadLine();
                    return;
                }

                int currentIndex = 0;
                bool editing = true;

                while (editing)
                {
                    Console.Clear();
                    Console.WriteLine($"Redigera meny-ID: {menu.Id}\n");

                    //Visa veckomeny och markera vald maträtt
                    for (int i = 0; i < menu.Dishes.Count; i++)
                    {
                        string day = weekDays[i % weekDays.Length];
                        var dish = menu.Dishes[i];
                        if (i == currentIndex)
                        {
                            Console.WriteLine($"> {day}: {dish.Name} ({dish.Category})"); //Markera den valda dagen
                        }
                        else
                        {
                            Console.WriteLine($"  {day}: {dish.Name} ({dish.Category})");
                        }
                    }

                    Console.WriteLine("\nAnvänd upp/ner-piltangenterna för att navigera och tryck 'Enter' för att slumpa om maträtten. \nTryck 'Esc' för att avsluta.");

                    var key = Console.ReadKey(true); //Fånga tangenttryckningarna

                    //Hantera tangenttryckningarna
                    if (key.Key == ConsoleKey.UpArrow && currentIndex > 0)
                    {
                        currentIndex--; //Flytta uppåt om möjligt
                    }
                    else if (key.Key == ConsoleKey.DownArrow && currentIndex < menu.Dishes.Count - 1)
                    {
                        currentIndex++; //Flytta nedåt om möjligt
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        //Slumpa om maträtten på den valda dagen
                        var newDish = dishes
                             .OrderBy(d => Guid.NewGuid())
                             .FirstOrDefault(d => d.Id != menu.Dishes[currentIndex].Id); //Se till att det inte blir samma maträtt

                        if (newDish != null)
                        {
                            menu.Dishes[currentIndex] = newDish; //Byt ut maträtten
                            Console.WriteLine($"\nMaträtten för {weekDays[currentIndex % weekDays.Length]} har ändrats till: {newDish.Name} ({newDish.Category})");

                            menusCollection.Update(menu); //Uppdatera databasen
                            LoadMenus(db); //Ladda om alla menyer så att `menus`-listan är uppdaterad
                        }

                        Console.WriteLine("\nTryck på valfri knapp för att fortsätta...");
                        Console.ReadKey(true);
                    }
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        editing = false; //Avsluta redigeringsläget
                    }
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt meny-ID, försök igen.");
                Console.ReadLine();
            }
        }

        //Metod för att lägga till en ny maträtt
        private static void AddDish(LiteDatabase db)
        {
            var dishesCollection = db.GetCollection<Dish>("dishes"); //Hämta maträttskollektionen från databasen

            Console.WriteLine("Ange namn på maträtt:");
            string name = Console.ReadLine() ?? ""; //Tilldela tom sträng om null

            //Kontrollera om namnet är tomt
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Namnet får inte vara tomt. Försök igen.");
                Console.ReadLine();
                return;
            }

            //Visa kategorierna
            Console.WriteLine("\nAnge kategori (välj en siffra):");
            Console.WriteLine("1. Kött");
            Console.WriteLine("2. Fisk");
            Console.WriteLine("3. Kyckling");
            Console.WriteLine("4. Vegetarisk");
            Console.WriteLine("5. Soppa");
            Console.WriteLine("\nVal:");

            if (int.TryParse(Console.ReadLine(), out int categoryChoice)) //Omvandla inmatning till heltal
            {
                DishCategory category;
                switch (categoryChoice)
                {
                    case 1:
                        category = DishCategory.Kött;
                        break;
                    case 2:
                        category = DishCategory.Fisk;
                        break;
                    case 3:
                        category = DishCategory.Kyckling;
                        break;
                    case 4:
                        category = DishCategory.Vegetarisk;
                        break;
                    case 5:
                        category = DishCategory.Soppa;
                        break;
                    default:
                        Console.WriteLine("Ogiltigt val, försök igen.");
                        Console.ReadLine();
                        return;
                }

                var newDish = new Dish
                {
                    Id = dishes.Count + 1, //Ger ett nytt unikt ID
                    Name = name,
                    Category = category
                };

                dishesCollection.Insert(newDish); //Lägg till den nya maträtten i databasen
                dishes.Add(newDish); //Uppdatera den lokala listan

                Console.WriteLine("\n\nMaträtt tillagd.");

                Console.WriteLine("\n\nTryck på valfri knapp för att fortsätta...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Ogiltigt val, försök igen.");
                Console.ReadLine();
            }
        }

        //Metod för att visa alla tidigare sparade menyer
        public static void DisplayAllMenus()
        {
            if (menus.Any())
            {
                foreach (var menu in menus)
                {
                    Console.WriteLine($"\nMeny ID: {menu.Id}");

                    //Iterera genom maträtterna i menyn
                    for (int i = 0; i < menu.Dishes.Count; i++)
                    {
                        string day = weekDays[i % weekDays.Length]; //Bestämmer vilken veckodag som ska användas
                        var dish = menu.Dishes[i]; //Hämta maträtten

                        if (day == "Söndag")
                        {
                            Console.ForegroundColor = ConsoleColor.Red; //Sätt textfärgen till röd för söndag
                        }

                        Console.WriteLine($"{day}: {dish.Name} ({dish.Category})"); //Skriv ut veckodagen och maträtten
                    }

                    Console.ResetColor(); //Återställ färgen efter att menyn skrivits ut

                    Console.WriteLine(new string('-', 30));
                }
            }
            else
            {
                Console.WriteLine("\n\n[ Det finns inga sparade menyer. ]");
                Console.ReadLine();
            }

            Console.WriteLine("\n\nTryck på valfri knapp för att fortsätta...");
            Console.ReadLine();
        }

        //Metod för att ta bort en meny
        private static void DeleteMenu(LiteDatabase db)
        {
            Console.WriteLine("\n\nTa bort en meny");

            DisplayAllMenus(); //Visa alla sparade menyer

            var menusCollection = db.GetCollection<Menu>("menus"); //Hämta menykollektionen från databasen

            Console.WriteLine("\nAnge meny-ID för att ta bort en meny, tryck [X] för att avbryta.");

            var input = Console.ReadLine(); //Läs in användarens inmatning

            //Kontrollera om användaren vill avbryta
            if (input?.ToLower() == "x")
            {
                Console.WriteLine("Åtgärden har avbrutits.");
                Console.ReadLine();
                return;
            }

            //Kontrollera om inmatning är ett giltigt meny-ID
            if (int.TryParse(input, out int menuId)) //Omvandla inmatning till heltal
            {
                var menuToDelete = menus.FirstOrDefault(m => m.Id == menuId); //Hitta menyn med det specifika ID:t

                if (menuToDelete != null)
                {
                    menus.Remove(menuToDelete); //Ta bort menyn från den lokala listan
                    menusCollection.Delete(menuId); //Ta bort menyn från databasen

                    Console.WriteLine($"\nMeny med ID {menuId} har tagits bort.");

                    Console.WriteLine("\n\nTryck på valfri knapp för att fortsätta...");
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Ingen meny med det angivna ID:t hittades. Försök igen.");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Ogiltigt meny-ID. Försök igen.");
                Console.ReadLine();
            }
        }

        //Metod för att ladda maträtter från JSON-fil
        private static void LoadDishesFromJson(ILiteCollection<Dish> dishesCollection)
        {
            var filePathDishes = Path.Combine("Data", "dishes.json"); //Sökväg till JSON-filen som ligger i "Data"-mappen

            if (File.Exists(filePathDishes))  //Kontrollerar om JSON-filen existerar innan inläsning startar
            {
                var json = File.ReadAllText(filePathDishes);  //Läser in hela JSON-filens innehåll till en sträng

                //Try-catch som hanterar eventuella fel vid inläsning och deserialisering av JSON-filen
                try
                {
                    var jsonOptions = new STJson.JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter() } //Konverterar enums till strängar
                    };

                    //Deserialiserar JSON-strängen till en lista av Dish-objekt, om det misslyckas returneras en tom lista för att förhindra null-referenser
                    var dishesFromJson = STJson.JsonSerializer.Deserialize<List<Dish>>(json, jsonOptions) ?? new List<Dish>();

                    if (dishesFromJson.Any()) //Om JSON-innehållet innehåller maträtter efter deserialisering, lägg till dessa i databasen
                    {
                        dishesCollection.InsertBulk(dishesFromJson); //Lägger till alla maträtter från JSON-filen till databasen i ett steg
                        Console.WriteLine("Maträtter importerade från dishes.json till databasen.");
                    }
                    else
                    {
                        Console.WriteLine("Inga maträtter hittades i dishes.json. Kontrollera filens innehåll.");
                    }
                }
                catch (IOException ioEx) //Fångar och hanterar filrelaterade fel (t.ex.om filen inte kan läsas)
                {
                    Console.WriteLine($"Filfel vid inläsning av dishes.json: {ioEx.Message}");
                    Console.ReadLine();
                }
                catch (Exception ex) //Fångar alla andra fel som kan uppstå vid deserialisering
                {
                    Console.WriteLine($"Fel vid deserialisering av dishes.json: {ex.Message}");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Filen dishes.json hittades inte. Kontrollera att filen finns i Data-mappen.");
                Console.ReadLine();
            }
        }
    }
}