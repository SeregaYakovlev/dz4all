using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebScraping
{
    internal class Animal
    {
        private int energy = 100;
        internal virtual void Feed()
        {
            energy += 10;
        }
    }

    class Cat:Animal
    {
    }

    class Dog:Animal
    {
        private bool isOnAChain = false;
        override internal void Feed()
        {
            isOnAChain = true;
            base.Feed();
            isOnAChain = false;
        }
    }

    static class Prog
    {
        static async Task Main2()
        {
            var animals = new List<Animal>();
            animals.Add(new Cat());
            animals.Add(new Dog());

            foreach (var animal in animals)
            {
                animal.Feed();
            }

        }
    }
}
