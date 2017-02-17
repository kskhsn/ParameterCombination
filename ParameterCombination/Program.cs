using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterCombination
{
    class Program
    {

        enum AType { A1,A2,A3}
        enum BType { B1, B2, B3 }
        enum CType { C1, C2, C3, C4, C5 }

        static void Sample1(AType type1,BType type2,CType type3)
        {
            Console.WriteLine($"{type1} , {type2} , {type3} ");
        }

        static void Sample2(int val1,int val2)
        {
            Console.WriteLine($"{val1} * {val2} = {val1 * val2} ");
        }
        static void Sample3(int val1, int val2, AType type1, BType type2)
        {

            Console.WriteLine($"{val1} * {val2} = {val1 * val2} : ({type1},{type2})");
        }

        static void Main(string[] args)
        {
            //case enum
            var pa1 = HasID.CreateParametersFromEnum<AType>();
            var pa2 = HasID.CreateParametersFromEnum<BType>();
            var pa3 = HasID.CreateParametersFromEnum<CType>();

            var paramsA = Combination.CreateValues(pa1, pa2, pa3);

            foreach(var p in paramsA)
            {
                var ps = new Parameters(p);
                Sample1(ps.GetValue<AType>(0), ps.GetValue<BType>(1), ps.GetValue<CType>(2));
                //var temp = p.ToArray();
                //Sample1(temp[0].GetParameter<AType>(), temp[1].GetParameter<BType>(), temp[2].GetParameter<CType>());
            }
            Console.WriteLine($"paramsA count = {paramsA.Count()}");// 3 * 3 * 5  = 45

            //case int[] part1
            var pb1 = HasID.CreateParameters(Enumerable.Range(5, 10).ToArray());
            var pb2 = HasID.CreateParameters(new int[] { 99, 999, 9999 });

            var paramsB = Combination.CreateValues(pb1, pb2);
          
            foreach (var p in paramsB)
            {
                var ps = new Parameters(p);
                Sample2(ps.GetValue<int>(0), ps.GetValue<int>(1));
                //var a = p.ToArray();
                //Sample2(a[0].GetParameter<int>(), a[1].GetParameter<int>());
            }
            Console.WriteLine($"paramsB count = {paramsB.Count()}");

            //case int[] part2
            var pc1 = Enumerable.Range(5, 10);
            var pc2 = new int[] { 99, 999, 9999 };

            var paramsC = Combination.CreateValuesFromRaw(pc1, pc2);

            foreach (var p in paramsC)
            {
                var ps = new Parameters(p);
                Sample2(ps.GetValue<int>(0), ps.GetValue<int>(1));
                //var temp = p.ToArray();
                //Sample2(temp[0].GetParameter<int>(), temp[1].GetParameter<int>());
            }
            Console.WriteLine($"paramsC count = {paramsC.Count()}");

            //mix
            var paramsD = Combination.CreateValues(pb1, pb2,pa1,pa2);

            foreach (var p in paramsD)
            {
                var ps = new Parameters(p);
                Sample3(ps.GetValue<int>(0), ps.GetValue<int>(1), ps.GetValue<AType>(2), ps.GetValue<BType>(3));                
            }
            Console.WriteLine($"paramsD count = {paramsD.Count()}");


            //
            var paramsE = Combination.CreateValues(pb1, pb2, pa1, pa2);
            var l1 = paramsE.Select(v => new ParametersI(v, 4)).Where(v => v.GetValue<int>(0) * v.GetValue<int>(1) > 1000).ToArray();

            var l2 = paramsE.ToParameters();
            var l3 = paramsE.ToParameters().Where(v => v.GetValue<int>(0) * v.GetValue<int>(1) > 1000).ToArray();

           foreach(var p in l3)
            {
                Console.WriteLine(p);
            }

        }
    }
}
