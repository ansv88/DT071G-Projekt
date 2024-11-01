using LiteDB;

namespace ProjectApp
{
    //Enum som definierar olika kategorier för maträtter
    public enum DishCategory
    {
        Kött,
        Fisk,
        Kyckling,
        Vegetarisk,
        Soppa
    }

    //Klass som representerar en maträtt med ett ID, ett namn och en kategori
    public class Dish
    {
        [BsonId]
        public int Id { get; set; } //Unikt ID för varje maträtt, LiteDB ordnar inkrementering automatiskt
        public required string Name { get; set; }  //Namnet på maträtten, obligatoriskt
        public DishCategory Category { get; set; } //Kategori för maträtten baserat på enumen
    }
}