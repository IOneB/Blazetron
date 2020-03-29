using System;
using System.Collections.Generic;
using System.Text;
using LanguageExt;
using LanguageExt.ClassInstances;
using static LanguageExt.Prelude;


namespace LanguageExt
{
    class BaseExt
    {
        public BaseExt()
        {
            CreateFun();
            MapTuple();
            CreateSeq();

            TupleThings();
            Opt();
        }

        private void Opt()
        {
            var o = Some(123);
            o = o
                .Some(x => (Option<int>)None)
                .None(20);
            var x = o.Match(
                Some: x => 0,
                None: () => 10);

            o.IfNone(() => Console.WriteLine("None"));
            o.IfSome(x => Console.WriteLine(x));

        }

        private void TupleThings()
        {
            var a = (1, 2, 3).Add('4');
            (1,2,10).Sum<TInt, int>();
            (12, 12).Product<TInt, int>();
            // Есть еще Contains, Concat
            Console.WriteLine((12, 13).Concat<TInt, int>());
            Console.WriteLine((List(1, 2), List(4, 10)).Concat<TLst<int>, Lst<int>>());
        }

        private static void CreateSeq()
        {
            Seq<int> seq = Seq.generate(100, x => x);
            var x = Seq.map(seq, x => (x, x * x));

            Console.WriteLine(x);
        }

        private static void CreateFun()
        {
            var add = fun((int x, int y) => x + y);
            add(5, 10);
        }

        private static void MapTuple()
        {
            var name = Tuple("Joe", "Dim");
            var res = map(name, (first, second) => $"{first} {second}");
        }
    }
}
