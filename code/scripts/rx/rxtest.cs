using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace SimpleSequence
{
    class Program
    {
        static void Main(string[] args)
        {
            IObservable<int> source = Observable.Range(1, 10);
            IObserver<int> obsvr = Observer.Create<int>(
                x => Console.WriteLine("OnNext: {0}", x),
                ex => Console.WriteLine("OnError: {0}", ex.Message),
                () => Console.WriteLine("OnCompleted"));
            IDisposable subscription = source.Subscribe(obsvr);
            Console.WriteLine("Press ENTER to unsubscribe...");
            Console.ReadLine();
            subscription.Dispose();
       }
    }
}
