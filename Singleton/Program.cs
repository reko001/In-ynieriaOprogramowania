using System.Text;
using System.Text.RegularExpressions;

namespace Program
{

    class Program
    {
        public static void Main()
        {
            CityStatistic.BiggestCityInCountry("Egypt");
        }
    }

    //Rekord miasta zawierający dane z tabeli
    public record City(string city, string city_ascii, double lat, double lng, string country, string iso2, string iso3,
    string admin_name, string capital, decimal? population, long id);

    //klasa typu Singleton
    public class cityDAO
    {
        //konstruktor inicjalizujący listę rekordów tabeli i sortujący ją malejąco po liczbie ludności
        private cityDAO()
        {
            cities = new List<City>();
            string dirPath = Directory.GetCurrentDirectory();
            string[] records = File.ReadAllLines(Path.Combine(dirPath, "worldcities.csv"));
            //dodanie rekordów do listy z pominięciem pierwszego - nazw kolumn
            foreach(string r in records.Skip(1)) 
            {
                string[] c = handleCommas(r).Split(',');
                cities.Add(new City(c[0], c[1], Convert.ToDouble(c[2]), Convert.ToDouble(c[3]), c[4], c[5], c[6], c[7], c[8], 
                c[9] == "" ? null : Convert.ToDecimal(c[9]), Convert.ToInt64(c[10])));
            }
            //posortowanie listy
            cities = cities.OrderByDescending(x => x.population).ToList();
        }


        private static cityDAO? _instance;
        private List<City> cities;

        public static cityDAO Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new cityDAO();
                }
                return _instance;
            }
        }

        //metoda zwracająca listę z wszystkimi miastami
        public IEnumerable<City> GetAllCities()
        {
            return cities;
        }

        //metoda zwracająca rekord csv bez pól z dodatkowymi przecinkami typu "*,*" -> "* *"
        private string handleCommas(string record)
        {
            StringBuilder s = new StringBuilder();
            bool deleteComma = false;
            for(int i = 0; i < record.Length; i++)
            {
                if(record[i] == '\"')
                {
                    deleteComma = !deleteComma;
                    continue;
                }
                if(deleteComma == true)
                {
                    if(record[i] != ',')
                    {
                        s.Append(record[i]);
                    }
                }
                else 
                {
                    s.Append(record[i]);
                }
            }
            return s.ToString();
        }
    }

    public static class CityPresenter
    {
        //metoda wypisująca rekord miasta
        public static void PrintCity(string city)
        {
            //regex znajdujący miasto zaczynające się na city
            Regex rgx = new Regex(@$"{city}\w*", RegexOptions.IgnoreCase);
            List<City> matched = cityDAO.Instance.GetAllCities().Where(x => rgx.IsMatch(x.city) || rgx.IsMatch(x.city_ascii)).ToList();
            if(matched.Count == 0)
            {
                System.Console.WriteLine("No city was found");
                return;
            }
            City c = matched.First();
            System.Console.WriteLine(c);
        }
    }  

    public static class CityStatistic
    {
        //metoda wypisująca 10 najbardziej zaludnionych miast
        public static void Top10Cities()
        {
            foreach(City city in cityDAO.Instance.GetAllCities().Take(10))
            {
                System.Console.WriteLine(city.city);
            }
        }

        //metoda wypisująca średnią liczbę ludzi w mistach na świecie
        public static void AveragePopulationWorld()
        {
            int count = 0;
            decimal? population = 0M;
            foreach(City city in cityDAO.Instance.GetAllCities().Where(x => x.population != null))
            {
                population += city.population;
                count++;
            }
            if(count == 0)
            {
                System.Console.WriteLine("There are no cities");
                return;
            }
            System.Console.WriteLine($"Average population in world cities is {population / count:0.##}");
        }

        //metoda wypisująca największe miasto w kraju
        public static void BiggestCityInCountry(string country)
        {
            City? biggest = cityDAO.Instance.GetAllCities().Where(x => x.country.ToLower() == country.ToLower()).FirstOrDefault();
            if(biggest != null)
            {
                System.Console.WriteLine($"The biggest city in {country} is {biggest.city}");
            }
            else
            {
                System.Console.WriteLine("No data about cities in this country");
            }
        }

    } 
}

