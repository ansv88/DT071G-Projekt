using LiteDB;

namespace ProjectApp
{
    //Klass som representerar en meny med en lista av maträtter
    public class Menu
    {
        [BsonId]
        public int Id { get; set; } //Unikt ID för varje meny för identifiering i databasen

        [BsonRef("dishes")]
        public List<Dish> Dishes { get; set; } = new List<Dish>(); //Lista med maträtter som ingår i menyn. Initialiseras med en tom lista för att undvika null-värden.
    }
}