using LanguageExt.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LanguageExt.Attributes;
using System.Diagnostics.Contracts;
using LanguageExt.Types;
using LanguageExt.ClassInstances;
using LanguageExt.Monades.ReadMonade;
using static LanguageExt.Prelude;

namespace LanguageExt.AbstractUnion
{
    // размеченное объединение с помощью абстрактного класса
    [Union]
    public abstract partial class Shape<A>
    {
        public abstract Shape<A> Rectangle(A width, A length);
        public abstract Shape<A> Circle(A radius);
        public abstract Shape<A> Prism(A width, A height);
    }
}

namespace LanguageExt.Types
{
    //Простая запись
    [Record]
    public partial struct Person
    {
        public readonly string Forename;
        public readonly string Surname;
    }

    // размеченное объединение
    [Union]
    public interface Shape<A>
    {
        Shape<A> Rectangle(A width, A length);
        Shape<A> Circle(A radius);
        Shape<A> Prism(A width, A height);
    }

    class HowToTypes
    {
        HowToTypes()
        {
            Person.New("a", "joe");

            Either<int, Error> x = Shape.Circle(15) switch {
                Circle<int>(var v) => v + 10,
                _ => Error.New("")
            };


        }
    }
}

namespace LanguageExt.Monades
{
    namespace Maybe
    {
        [Free]
        public interface Maybe<A>
        {
            [Pure] A Just(A value);
            [Pure] A Nothing();

            public static Maybe<B> Map<B>(Maybe<A> ma, Func<A, B> f) => ma switch //Необязательно переопределять
            {
                Just<A>(var x) => Maybe.Just(f(x)),
                _ => Maybe.Nothing<B>()
            };
        }

        class HowToMaybe
        {
            public HowToMaybe()
            {
                var ma = Maybe.Just(10);
                var mb = Maybe.Just(20);
                var mn = Maybe.Nothing<int>();

                var r1 = from a in ma
                         from b in mb
                         select a + b;  // Just(30)

                var r2 = from a in ma
                         from b in mb
                         from _ in mn
                         select a + b;  // Nothing
            }
        }
    }

    /// <summary>
    /// Конекретный пример монады
    /// </summary>
    namespace Monade
    {
        [Free]
        public interface FreeIO<A>
        {
            [Pure] A Pure(A value);
            [Pure] A Fail(Error error);
            string ReadAllText(string path);
            Unit WriteAllText(string path, string text);
        }

        public class Logic
        {
            public static async Task<A> InterpretAsync<A>(FreeIO<A> ma) => ma switch
            {
                Pure<A>(var value) => value,
                Fail<A>(var error) => await Task.FromException<A>(error),
                ReadAllText<A>(var path, var next) => await InterpretAsync(next(await Read(path))),
                WriteAllText<A>(var path, var text, var next) => await InterpretAsync(next(await Write(path, text))),
            };

            static Task<string> Read(string path) =>
                File.ReadAllTextAsync(path);

            static Task<Unit> Write(string path, string text)
                => File.WriteAllTextAsync(path, text).ToUnit();


            public async Task HowToMonade()
            {
                var dslPipeLine = new ReadAllText<Unit>("test.txt",
                    txt => new WriteAllText<Unit>("test2.txt", txt,
                      unit => new Pure<Unit>(unit)));

                var dsl = from t in FreeIO.ReadAllText("test.txt")
                          from unit in FreeIO.WriteAllText("test2.txt", t)
                          select unit;

                Console.WriteLine(await InterpretAsync(dsl));
            }
        }
    }
}

namespace LanguageExt.ReadWriteStateMonade
{
    [RWS(WriterMonoid: typeof(MSeq<string>),
         Env:          typeof(IO),
         State:        typeof(Person),
         Constructor:  "Pure",
         Fail:         "Error" )]
    public partial struct SubSys<T>
    {

    }

    class HowToRWS
    {
        public HowToRWS()
        {
            // IDK
        }
    }
}

namespace LanguageExt.With
{
    [With]
    public partial class AWith
    {
        public readonly int X;
        public readonly bool Y;

        public AWith(int x, bool y)
        {
            X = x;
            Y = y;
        }
    }

    class HowTo
    {
        public HowTo()
        {
            AWith v = new AWith(12, false);

            var newV = v.With(Y: true);
        }
    }
}

namespace LanguageExt.WithLens
{
    [WithLens]
    public partial class Person : Record<Person>
    {
        public readonly string Name;
        public readonly string Surname;
        public readonly Map<int, Appt> Appts;

        public Person(string name, string surname, Map<int, Appt> appts)
        {
            Name = name;
            Surname = surname;
            Appts = appts;
        }
    }

    [WithLens]
    public partial class Appt : Record<Appt>
    {
        public readonly int Id;
        public readonly DateTime StartDate;
        public readonly ApptState State;

        public Appt(int id, DateTime startDate, ApptState state)
        {
            Id = id;
            StartDate = startDate;
            State = state;
        }
    }

    public enum ApptState
    {
        NotArrived,
        Arrived,
        DNA,
        Cancelled
    }

    class HowTo
    {
        public HowTo()
        {
            var person = new Person("Paul", "Louth", Map(
                (1, new Appt(1, DateTime.Parse("1/1/2010"), ApptState.NotArrived)),
                (2, new Appt(2, DateTime.Parse("2/1/2010"), ApptState.NotArrived)),
                (3, new Appt(3, DateTime.Parse("3/1/2010"), ApptState.NotArrived))));

            // Local function for composing a new lens from 3 other lenses
            Lens<Person, ApptState> setState(int id) =>
                lens(Person.appts, Map<int, Appt>.item(id), Appt.state);

            // Transform
            var person2 = setState(2).Set(ApptState.Arrived, person);
        }
    }
} 

namespace LanguageExt.Monades.ReadMonade
{
    public interface IO
    {
        Seq<string> ReadAllLines(string fileName);
        Unit WriteAllLines(string file, Seq<string> lines);
        Person ReadFromDb();
        int Zero { get; }
    }

    internal class IOImplementation : IO
    {
        public int Zero => 0;

        public Seq<string> ReadAllLines(string fileName)
        {
            return File.ReadAllLines(fileName).ToSeq();
        }

        public Person ReadFromDb()
        {
            return Person.New("", "");
        }

        public Unit WriteAllLines(string file, Seq<string> lines)
        {
            File.WriteAllLines(file, lines);
            return Unit.Default;
        }
    }

    [Reader(typeof(IO))]
    public partial struct Subsystem<A>
    {
        public Subsystem<B> Map<B>(System.Func<A, B> f) => Bind(a => Subsystem<B>.Return(f(a)));
    }

    class HowToReader
    {
        public HowToReader()
        {
            var comp = from ze in Subsystem.Zero
                       from ls in Subsystem.ReadAllLines("test.txt")
                       from _ in Subsystem.WriteAllLines("test2.txt", ls)
                       select ls.Count;

            var res = comp.Run(new IOImplementation()).IfFail(0);
            Console.WriteLine(res);
        }
    }
}