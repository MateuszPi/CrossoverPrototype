using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Resources;

namespace CrossoverGenetic
{
    class City
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public string CityName { get; set; }

        public City(decimal x, decimal y, string cityName)
        {
            X = x;
            Y = y;
            CityName = cityName;
        }
    }

    class Calculations
    {
        public decimal CalculateDistances(City A, City B)
        {
            decimal distanceX = (decimal) Math.Pow((double) A.X - (double) B.X, 2);
            decimal distanceY = (decimal) Math.Pow((double) A.Y - (double) B.Y, 2);
            decimal result = (decimal) Math.Sqrt((double) distanceX + (double) distanceY);
            return result;
        }

        public void CalculateTotalDistance(Track track, List<Distance> listOfDistances)
        {
            for (int i = 0; i < track.CitiesList.Count - 1; i++)
            {
                string a = track.CitiesList[i].CityName;
                string b = track.CitiesList[i + 1].CityName;
                if (a == b)
                {
                    track.TotalDistance = 9999.0m;
                    return;
                }
                var result = (from n in listOfDistances
                    where (n.CityA == a && n.CityB == b) || (n.CityB == a && n.CityA == b)
                    select n.DistanceBetweenCities).Single();
                track.TotalDistance += result;
            }
        }

        public void CalculatePopulationDistances(Population population, List<Distance> listOfDistances)
        {
            var popSize = population.PopulationList.Count();
            for (int i = 0; i < popSize; i++)
            {
                CalculateTotalDistance(population.PopulationList[i], listOfDistances);
            }
        }


        public void OrderPopulation(Population population)
        {
            population.PopulationList.Sort(delegate(Track trackA, Track trackB)
            {
                return trackA.TotalDistance.CompareTo(trackB.TotalDistance);
            });
        }
    }

    class Distance
    {
        public string CityA { get; set; }
        public string CityB { get; set; }
        public decimal DistanceBetweenCities { get; set; }

        public Distance(string cityA, string cityB, decimal distanceBetweenCities)
        {
            CityA = cityA;
            CityB = cityB;
            DistanceBetweenCities = distanceBetweenCities;
        }
    }


    class Track
    {
        public List<City> CitiesList { get; set; }
        public decimal TotalDistance { get; set; }
    }

    class Population
    {
        public List<Track> PopulationList { get; set; }
    }

    public struct NeighborList
    {
        public string CityName;
        public int Neighbors;

        public NeighborList(string cityName, int neighbors)
        {
            CityName = cityName;
            Neighbors = neighbors;
        }
    }

    class Mutation
    {
        private readonly Random _rand = new Random();

        private Calculations _calc = new Calculations();

        public int Iterations { get; set; }

        public void GenerateRandomPopulation(Population population, List<City> citiesList, int populationSize)
        {
            Track[] randomPopulation = new Track[populationSize];

            List<Track> randomPopulationList = new List<Track>();

            for (int i = 0; i < populationSize; i++)
            {
                randomPopulationList.Add(GenerateRandomPopulationMember(citiesList));
            }
            population.PopulationList = randomPopulationList;
        }

        private Track GenerateRandomPopulationMember(List<City> citiesList)
        {
            int[] citiesIndexTable = new int[citiesList.Count];

            for (int i = 0; i < citiesList.Count; i++)
            {
                citiesIndexTable[i] = i;
            }

            int[] randomIndexTable = new int[citiesList.Count];


            Track track = new Track();

            int size = citiesIndexTable.Length;
            List<City> newCityList = new List<City>();

            for (int i = 0; i < citiesList.Count; i++)
            {
                int randomIndex = _rand.Next(0, size);
                randomIndexTable[i] = citiesIndexTable[randomIndex];
                newCityList.Add(citiesList[randomIndexTable[i]]);
                for (int j = randomIndex; j < size - 1; j++)
                {
                    citiesIndexTable[j] = citiesIndexTable[j + 1];
                }
                size--;
            }

            track.CitiesList = newCityList;
            return track;
        }

        //order crossover operator (order 1)

        public void MutateOX1(Population population, List<Distance> listOfDistances)
        {
            _calc.OrderPopulation(population);
            int sizeOfPopulation = population.PopulationList.Count();

            Track bestTrack = population.PopulationList[0];

            int licznikPoprawy = 0;

            bool poprawiono = false;

            do
            {
                decimal toKill1 = Convert.ToDecimal(sizeOfPopulation) * 0.2m;
                decimal toKill2 = Convert.ToDecimal(sizeOfPopulation) - toKill1;

                population.PopulationList.RemoveRange(Convert.ToInt16(toKill1), Convert.ToInt16(toKill2));

                int z = 0;
                for (int i = population.PopulationList.Count; i < sizeOfPopulation; i++)
                {
                    int newSizeOfPopulation = population.PopulationList.Count();
                    int parent1Index = _rand.Next(0, newSizeOfPopulation);
                    int parent2Index = _rand.Next(0, newSizeOfPopulation);
                    Track parent1 = population.PopulationList[parent1Index];
                    Track parent2 = population.PopulationList[parent2Index];
                    int sizeOfTrack = population.PopulationList[0].CitiesList.Count();
                    Track child = new Track();
                    City nullCity = new City(0, 0, "N");
                    List<City> nullCityList = new List<City>();
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        nullCityList.Add(nullCity);
                    }
                    child.CitiesList = nullCityList;
                    int cross1 = _rand.Next(0, sizeOfTrack - 1);
//                    Thread.Sleep(1);
                    int cross2 = _rand.Next(cross1, sizeOfTrack);
                    int c2c1 = cross2 - cross1;
                    for (int j = cross1; j <= cross2; j++)
                    {
                        child.CitiesList[j] = parent1.CitiesList[j];
                    }

                    int childIndex = 0;
                    int parentIndex = 0;

                    do
                    {
                        if (child.CitiesList[childIndex].CityName != "N")
                        {
                            childIndex++;
                            continue;
                        }

                        if (!child.CitiesList.Contains(parent2.CitiesList[parentIndex]))
                        {
                            child.CitiesList[childIndex] = parent2.CitiesList[parentIndex];
                            childIndex++;
                            parentIndex++;
                        }
                        else
                        {
                            parentIndex++;
                        }
                    } while (childIndex < sizeOfTrack);


                    _calc.CalculateTotalDistance(child, listOfDistances);
                    population.PopulationList.Add(child);
                }
                _calc.OrderPopulation(population);

                decimal oldBest = bestTrack.TotalDistance;
                decimal pretender = population.PopulationList[0].TotalDistance;

                if (population.PopulationList[0].TotalDistance < bestTrack.TotalDistance)
                {
                    bestTrack = population.PopulationList[0];
                    licznikPoprawy = 0;
                }
                else
                {
                    licznikPoprawy++;
                }
                Iterations++;
                Console.WriteLine($"Iteracja {Iterations}, current best: {bestTrack.TotalDistance}");
                z++;
            } while (licznikPoprawy <= 20);
        }

        //edge recombination operation
        public void MutateERO(Population population, List<Distance> listOfDistances, List<City>citiesList)
        {
            _calc.OrderPopulation(population);
            int sizeOfPopulation = population.PopulationList.Count();

            Track bestTrack = population.PopulationList[0];
            int sizeOfTrack = population.PopulationList[0].CitiesList.Count();

            int licznikPoprawy = 0;

            List<NeighborList> NLList = new List<NeighborList>();

            for (int i = 0; i < sizeOfTrack; i++)
            {
                NLList.Add(new NeighborList(citiesList[i].CityName, 0));
            }

            bool poprawiono = false;

            do
            {
                decimal toKill1 = Convert.ToDecimal(sizeOfPopulation) * 0.2m;
                decimal toKill2 = Convert.ToDecimal(sizeOfPopulation) - toKill1;
                population.PopulationList.RemoveRange(Convert.ToInt16(toKill1), Convert.ToInt16(toKill2));
                int z = 0;
                for (int i = population.PopulationList.Count; i < sizeOfPopulation; i++)
                {
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        NeighborList jList = new NeighborList();
                        jList.CityName = citiesList[j].CityName;
                        NLList[j] = jList;
                    }
                    int newSizeOfPopulation = population.PopulationList.Count();
                    int parent1Index = _rand.Next(0, newSizeOfPopulation);
                    int parent2Index = _rand.Next(0, newSizeOfPopulation);
                    Track parent1 = population.PopulationList[parent1Index];
                    Track parent2 = population.PopulationList[parent2Index];


                    Track child = new Track();
                    City nullCity = new City(0, 0, "N");
                    List<City> nullCityList = new List<City>();
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        nullCityList.Add(nullCity);
                    }
                    child.CitiesList = nullCityList;

                    string[] parent1Table = new string[sizeOfTrack];
                    string[] parent2Table = new string[sizeOfTrack];
                    string[] childTable = new string[sizeOfTrack];


                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        parent1Table[j] = parent1.CitiesList[j].CityName;
                        parent2Table[j] = parent2.CitiesList[j].CityName;
                    }


#if DEBUG
                    string[] P1 = new string[sizeOfTrack];
                    string[] P2 = new string[sizeOfTrack];

                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        P1[j] = parent1Table[j];
                        P2[j] = parent2Table[j];
                    }
#endif

                    NLList = generateNeighborLists(NLList, parent2Table, parent1Table);

                    childTable[0] = parent1Table[0];
                    parent1Table[0] = "X";

                    for (int j = 0; j < parent2Table.Length; j++)
                    {
                        if (parent2Table[j] == childTable[0])
                        {
                            parent2Table[j] = "X";
                        }
                    }

                    int idx = NLList.Select((value, index) => new {value, index})
                        .Where(x => x.value.CityName == childTable[0]).Select(x => x.index).FirstOrDefault();

                    NeighborList zaz = new NeighborList();
                    zaz.CityName = "X";
                    NLList[idx] = zaz;


                    for (int j = 1; j < sizeOfTrack; j++)
                    {
                        int r = 0;
                        List<string> minimalCities = new List<string>();
                        int minumum = 4;
                        for (int k = 0; k < sizeOfTrack; k++)
                        {
                            if (NLList[k].Neighbors < minumum && NLList[k].CityName != "X")
                            {
                                minumum = NLList[k].Neighbors;
                            }
                        }
                        for (int k = 0; k < sizeOfTrack; k++)
                        {
                            if (NLList[k].Neighbors == minumum)
                            {
                                minimalCities.Add(NLList[k].CityName);
                            }
                        }

                        if (minimalCities.Count == 0)
                        {
                            Console.WriteLine("AAA");
                        }

                        try
                        {
                            r = _rand.Next(0, minimalCities.Count());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR: Pusta lista miast z minimalną liczbą sąsiadów!!!");
                            throw;
                        }

                        childTable[j] = minimalCities[r];
                        for (int k = 0; k < parent2Table.Length; k++)
                        {
                            if (parent2Table[k] == minimalCities[r])
                            {
                                parent2Table[k] = "X";
                            }
                        }
                        for (int k = 0; k < parent1Table.Length; k++)
                        {
                            if (parent1Table[k] == minimalCities[r])
                            {
                                parent1Table[k] = "X";
                            }
                        }
                        int indeks = NLList.Select((value, index) => new {value, index})
                            .Where(x => x.value.CityName == childTable[j]).Select(x => x.index).FirstOrDefault();

                        NeighborList zaa = new NeighborList();
                        zaa.CityName = "X";
                        r = indeks;
                        NLList[indeks] = zaa;
                    }

                    City[] citiesTable = new City[sizeOfTrack];
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        City c = new City(0, 0, "X");
                        var cc = citiesList.Where(x => x.CityName == childTable[j]).GroupBy(x => x.CityName)
                            .Select(x => x.FirstOrDefault());
                        foreach (var VARIABLE in cc)
                        {
                            c = VARIABLE;
                        }
                        citiesTable[j] = c;
                    }

                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        child.CitiesList[j] = citiesTable[j];
                    }
                    _calc.CalculateTotalDistance(child, listOfDistances);
                    population.PopulationList.Add(child);
                }

                _calc.OrderPopulation(population);

                decimal oldBest = bestTrack.TotalDistance;
                decimal pretender = population.PopulationList[0].TotalDistance;

                if (population.PopulationList[0].TotalDistance < bestTrack.TotalDistance)
                {
                    bestTrack = population.PopulationList[0];
                    licznikPoprawy = 0;
                }
                else
                {
                    licznikPoprawy++;
                }
                Iterations++;
                Console.WriteLine($"Iteracja {Iterations}, current best: {bestTrack.TotalDistance}");
                z++;
            } while (licznikPoprawy <= 20);
        }


        public void RandomMutation(Population population, List<Distance> listOfDistances)
        {
            _calc.OrderPopulation(population);
            int sizeOfPopulation = population.PopulationList.Count();
            int sizeOfTrack = population.PopulationList[0].CitiesList.Count();
            Track bestTrack = population.PopulationList[0];

            int licznikPoprawy = 0;

            bool poprawiono = false;

            do
            {
                decimal toKill1 = Convert.ToDecimal(sizeOfPopulation) * 0.2m;
                decimal toKill2 = Convert.ToDecimal(sizeOfPopulation) - toKill1;

                population.PopulationList.RemoveRange(Convert.ToInt16(toKill1), Convert.ToInt16(toKill2));

                int z = 0;
                for (int i = population.PopulationList.Count; i < sizeOfPopulation; i++)
                {
                    int randomParentIndex = _rand.Next(0, population.PopulationList.Count);
                    int random1Index = _rand.Next(0, sizeOfTrack);
                    int random2Index = _rand.Next(0, sizeOfTrack);
                    Track parent = population.PopulationList[randomParentIndex];
                    Track child = new Track();
                    City nullCity = new City(0, 0, "N");
                    List<City> nullCityList = new List<City>();
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        nullCityList.Add(nullCity);
                    }
                    child.CitiesList = nullCityList;
                    int[] indexes = new int[sizeOfTrack];
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        if (j == random1Index)
                        {
                            indexes[j] = random2Index;
                            continue;
                        }
                        if (j == random2Index)
                        {
                            indexes[j] = random1Index;
                            continue;
                        }
                        indexes[j] = j;
                    }
                    for (int j = 0; j < sizeOfTrack; j++)
                    {
                        child.CitiesList[j] = parent.CitiesList[indexes[j]];
                    }
                    _calc.CalculateTotalDistance(child, listOfDistances);
                    population.PopulationList.Add(child);
                }
                _calc.OrderPopulation(population);

                decimal oldBest = bestTrack.TotalDistance;
                decimal pretender = population.PopulationList[0].TotalDistance;

                if (population.PopulationList[0].TotalDistance < bestTrack.TotalDistance)
                {
                    bestTrack = population.PopulationList[0];
                    licznikPoprawy = 0;
                }
                else
                {
                    licznikPoprawy++;
                }
                Iterations++;
                Console.WriteLine($"Iteracja {Iterations}, current best: {bestTrack.TotalDistance}");
                z++;
            } while (licznikPoprawy <= 20);
        }

        private List<NeighborList> generateNeighborLists(List<NeighborList> NLL, string[] parent1, string[] parent2)
        {
            for (int i = 0; i < NLL.Count(); i++)
            {
                NeighborList n1 = NLL[i];
                string city = n1.CityName;
                int lvl = GetNeighborLevel(parent1, city);
                lvl += GetNeighborLevel(parent2, city);
                n1.Neighbors = lvl;
                NLL[i] = n1;
            }
            return NLL;
        }

        private int GetNeighborLevel(string[] parent, string cityName)
        {
            int lvl = 0;
            for (int i = 0; i < parent.Length; i++)
            {
                if (parent[i] == cityName)
                {
                    if (i == 0)
                    {
                        if (parent[i + 1] != "X")
                        {
                            lvl++;
                        }
                    }
                    else if (i == parent.Length - 1)
                    {
                        if (parent[i - 1] != "X")
                        {
                            lvl++;
                        }
                    }
                    else
                    {
                        if (parent[i + 1] != "X")
                        {
                            lvl++;
                        }
                        if (parent[i - 1] != "X")
                        {
                            lvl++;
                        }
                    }
                }
            }

            return lvl;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var rand = new Random();
            List<City> listaMiast = new List<City>();


//            Console.WriteLine("Podaj liczbę miast:");
//            string liczbaMiast = Console.ReadLine();
//            int liczbaMiastInt = Convert.ToInt16(liczbaMiast);
//
//            for (int i = 0; i < liczbaMiastInt; i++)
//            {
//                listaMiast.Add(new City(rand.Next(-100, 100), rand.Next(-100, 100), Convert.ToString(i)));
//            }

//            string miasta =
//                $" listaMiast.Add(new City({rand.Next(-100, 100)}, {rand.Next(-100, 100)}, {Convert.ToString(i)}));";

//            for (int i = 0; i < 25; i++)
//            {
//                Console.WriteLine($" listaMiast.Add(new City({rand.Next(-100, 100)}, {rand.Next(-100, 100)}, \"{Convert.ToString(i)}\"));");
//            }

            listaMiast.Add(new City(37, 79, "0"));
            listaMiast.Add(new City(-90, -44, "1"));
            listaMiast.Add(new City(-89, 13, "2"));
            listaMiast.Add(new City(38, -66, "3"));
            listaMiast.Add(new City(36, -100, "4"));
            listaMiast.Add(new City(38, 41, "5"));
            listaMiast.Add(new City(-31, 74, "6"));
            listaMiast.Add(new City(61, 20, "7"));
            listaMiast.Add(new City(-29, 39, "8"));
            listaMiast.Add(new City(71, -85, "9"));
            listaMiast.Add(new City(12, -2, "10"));
            listaMiast.Add(new City(-41, 17, "11"));
            listaMiast.Add(new City(17, -36, "12"));
            listaMiast.Add(new City(91, -66, "13"));
            listaMiast.Add(new City(71, -70, "14"));
            listaMiast.Add(new City(74, -21, "15"));
            listaMiast.Add(new City(16, -74, "16"));
            listaMiast.Add(new City(-56, -67, "17"));
            listaMiast.Add(new City(63, 15, "18"));
            listaMiast.Add(new City(-55, 26, "19"));
            listaMiast.Add(new City(-38, 26, "20"));
            listaMiast.Add(new City(8, 59, "21"));
            listaMiast.Add(new City(91, -86, "22"));
            listaMiast.Add(new City(-99, -12, "23"));
            listaMiast.Add(new City(-96, -58, "24"));


            foreach (var item in listaMiast)
            {
                Console.WriteLine($"{item.X} \t{item.Y} \t{item.CityName}");
            }

            Calculations calc = new Calculations();


            List<Distance> listOfDistances = new List<Distance>();

            for (int i = 0; i < listaMiast.Count; i++)
            {
                for (int j = i + 1; j < listaMiast.Count; j++)
                {
                    decimal distanceBetweenCities = calc.CalculateDistances(listaMiast[i], listaMiast[j]);
                    listOfDistances.Add(new Distance(listaMiast[i].CityName, listaMiast[j].CityName,
                        distanceBetweenCities));
                }
            }

            foreach (var item in listOfDistances)
            {
                Console.WriteLine($"Dystans z {item.CityA} do {item.CityB} to {item.DistanceBetweenCities}");
            }

            var pop = new Population();

            var mut = new Mutation();

            Console.WriteLine("Podaj liczebność populacji:");
            string liczebnosc = Console.ReadLine();
            int liczebnoscInt = Convert.ToInt32(liczebnosc);


            Console.WriteLine("Wybierz metodę:\r\n" +
                              "[0] - Order crossover operator(OX1)\r\n" +
                              "[1] - Edge recombination operation (ERO)\r\n" +
                              "[2] - Random Mutation\r\n");
            string metoda = Console.ReadLine();
            int metodaInt = Convert.ToInt32(metoda);

            mut.GenerateRandomPopulation(pop, listaMiast, liczebnoscInt);
            calc.CalculatePopulationDistances(pop, listOfDistances);
            calc.OrderPopulation(pop);
            Stopwatch sw = new Stopwatch();
            sw.Start();
//            mut.RandomMutation(pop, listOfDistances);

            switch (metodaInt)
            {
                case 0:
                    mut.MutateOX1(pop, listOfDistances);
                    break;
                case 1:
                    mut.MutateERO(pop, listOfDistances, listaMiast);
                    break;
                case 2:
                    mut.RandomMutation(pop, listOfDistances);
                    break;
                default:
                    Console.WriteLine("Zła metoda!!!");
                    break;
            }
            sw.Stop();


            Console.WriteLine(pop.PopulationList[0].TotalDistance);
            Console.WriteLine($"Liczba iteracji: {mut.Iterations} w czasie {sw.Elapsed}");

            string wynik = "";
            var lista = pop.PopulationList[0].CitiesList;
            foreach (var VARIABLE in lista)
            {
                wynik += $"{VARIABLE.CityName}|";
            }

            Console.WriteLine(wynik);
            Console.Read();
        }
    }
}