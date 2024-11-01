using LiteDB;

namespace ProjectApp
{
    internal class MenuPrinter
    {
        //Metod för att hantera utskrift av en meny eller alla menyer
        public void PrintToDocument(LiteDatabase db)
        {
            Program.DisplayAllMenus(); //Visar alla sparade menyer
            Console.WriteLine("\nVälj [1] en veckomeny att skriva ut eller \n[2] skriv ut alla veckomenyer");
            int.TryParse(Console.ReadLine(), out int userChoice);

            if (userChoice == 1)
            {
                PrintSpecificMenu(db);
            }
            else if (userChoice == 2)
            {
                PrintAllMenus(db);
            }
            else
            {
                Console.WriteLine("Ogiltigt val. Försök igen.");
                Console.ReadLine();
                return;
            }
        }

        //Metod för att skriva ut en veckomeny till en textfil
        private static void PrintSpecificMenu(LiteDatabase db)
        {
            Console.WriteLine("\nAnge Meny-ID för den veckomeny som ska skrivas ut.");
            if (int.TryParse(Console.ReadLine(), out int menuID))
            {
                try
                {
                    var specificMenu = GetMenu(db, menuID); //Hämta menyn från databasen baserat på ID

                    //Kontroll om menyn existerar
                    if (specificMenu == null)
                    {
                        Console.WriteLine("Vald meny existerar inte.");
                        Console.ReadLine();
                        return;
                    }

                    string filePath = "Veckomeny.txt"; //Filen som menyn skrivs ut till

                    //Lista med veckodagar för att kunna tilldela rätt dag till rätt maträtt
                    string[] weekDays = { "Måndag", "Tisdag", "Onsdag", "Torsdag", "Fredag", "Lördag", "Söndag" };

                    //Skriv menyn till textfilen
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine($"Veckomeny ID: {menuID}\n");

                        //Iterera över maträtterna och veckodagarna och skriv ut dem
                        for (int i = 0; i < specificMenu.Dishes.Count; i++)
                        {
                            string day = weekDays[i % weekDays.Length]; //Bestämmer veckodag för maträtten; veckodagarna upprepas om det finns fler än sju rätter.
                            var dish = specificMenu.Dishes[i];  //Hämta maträtten

                            writer.WriteLine($"{day}: {dish.Name} ({dish.Category})"); //Skriv ut veckodagen och maträtten
                        }

                        Console.WriteLine($"\nVeckomeny med ID {menuID} har skrivits ut till {filePath}.");
                    }
                }
                catch (IOException ioEx)  //Hanterar IO-fel (t.ex. filåtkomstfel)
                {
                    Console.WriteLine($"Fel vid filåtkomst: {ioEx.Message}");
                }
                catch (Exception ex)  //Fångar alla andra fel
                {
                    Console.WriteLine($"Ett fel inträffade: {ex.Message}"); 
                }
                finally
                {

                    Console.WriteLine("\n\nTryck på valfri knapp för att fortsätta...");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("\nDu måste ange en giltig siffra.");
            }
        }

        //Metod för att skriva ut alla veckomenyer till en textfil
        private void PrintAllMenus(LiteDatabase db)
        {
            try
            {
                var allMenus = GetAllMenus(db); //Hämta alla menyer från databasen

                //Kontrollera om det finns några menyer att skriva ut
                if (allMenus == null || !allMenus.Any())
                {
                    Console.WriteLine("\nDet finns inga veckomenyer att skriva ut.");
                    Console.ReadLine();
                    return;
                }

                string filePathAll = "Alla_veckomenyer.txt"; //Filen som menyerna skrivs ut till

                //Lista med veckodagar för att kunna tilldela rätt dag till rätt maträtt
                string[] weekDays = { "Måndag", "Tisdag", "Onsdag", "Torsdag", "Fredag", "Lördag", "Söndag" };

                //Skriv menyn till textfilen
                using (StreamWriter writer = new StreamWriter(filePathAll))
                {
                    foreach (var menu in allMenus)
                    {
                        writer.WriteLine($"\nMeny ID: {menu.Id}\n");

                        //Iterera genom maträtterna i menyn
                        for (int i = 0; i < menu.Dishes.Count; i++)
                        {
                            string day = weekDays[i % weekDays.Length]; //Bestämmer vilken veckodag som ska användas
                            var dish = menu.Dishes[i]; //Hämta maträtten

                            writer.WriteLine($"{day}: {dish.Name} ({dish.Category})"); //Skriv ut veckodagen och maträtten med kategori
                        }

                        writer.WriteLine(new string('-', 30));
                    }
                }

                Console.WriteLine($"\nAlla veckomenyer har skrivits ut till {filePathAll}.");
            }
            catch (IOException ioEx)  //Hanterar IO-fel (t.ex. filåtkomstfel)
            {
                Console.WriteLine($"Fel vid filåtkomst: {ioEx.Message}");
            }
            catch (Exception ex)  //Fångar alla andra fel
            {
                Console.WriteLine($"Ett fel inträffade: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\n\nTryck på valfri knapp för att fortsätta...");
                Console.ReadLine();
            }
        }

        //Metod för att hämta en specifik meny från databasen baserat på ID
        private static Menu? GetMenu(LiteDatabase db, int menuID)
        {
            try
            {
                var menusCollection = db.GetCollection<Menu>("menus"); //Hämta menykollektionen från databasen
                return menusCollection.Include(m => m.Dishes).FindById(menuID); //Hämta den valda menyn från databasen baserat på ID
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning av meny: {ex.Message}"); //Hanterar eventuella fel vid databasåtkomst
                return null;
            }
        }

        //Metod för att hämta alla menyer från databasen
        private static List<Menu> GetAllMenus(LiteDatabase db)
        {
            try
            {
                var menusCollection = db.GetCollection<Menu>("menus"); //Hämta menykollektionen från databasen
                return menusCollection.Include(m => m.Dishes).FindAll().ToList(); //Returnera alla menyer från databasen
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning av menyer: {ex.Message}"); //Hanterar eventuella fel vid databasåtkomst
                return new List<Menu>(); //Returnerar en tom lista om ett fel inträffar för att undvika potentiella null-värden
            }
        }
    }
}