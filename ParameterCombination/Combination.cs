using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParameterCombination
{
    /// <summary>組み合わせ用の基。IDはなくてもOK</summary>
    public interface IHasID
    {
        int ID { get; }
    }

    /// <summary>任意のパラメータを持つ用</summary>
    /// <typeparam name="T"></typeparam>
    public interface IHasID<T> : IHasID
    {
        //IHasIDを継承するのはヒントとして入れてある。
        T Parameter { get; }
    }


    public static class Combination
    {
        /// <summary>
        /// 2つの配列の合成(!=zip)数え上げ。[a,b],[1,2]  => [(a,1),(a,2),(b,1),(b,2)]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values1"></param>
        /// <param name="values2"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Combine<T>(IEnumerable<T> values1, IEnumerable<T> values2)
        {
            return values1.SelectMany(v1 => values2.Select(v2 => new T[] { v1, v2 }));
        }

        /// <summary>
        /// 配列の加算みたいの　[(a1,a2),(b1,b2)],[1,2]  => [(a1,a2,1),(a1,a2,2),(b1,b2,1),(b1,b2,2)]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values1"></param>
        /// <param name="values2"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Combine<T>(IEnumerable<IEnumerable<T>> values1, IEnumerable<T> values2)
        {
            return values1.SelectMany(v1 => values2.Select(v2 => v1.Concat(new T[] { v2 })));
        }


        /// <summary>
        /// 全部同じ型ならこれでもよい
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<IHasID>> CreateValuesFromRaw<T>(params IEnumerable<T>[] values)
        {
            return CreateValuesCore(values.Select(v => HasID<T>.CreateParameters(v)).ToArray());
        }

        /// <summary>
        /// 色々な型のパラメータを入れるならIEnumerable<IHasID>型を作ってから入れること
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<IHasID>> CreateValues(params IEnumerable<IHasID>[] values)
        {
            return CreateValuesCore(values);
        }

        /// <summary>
        /// コア部分
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<IHasID>> CreateValuesCore(params IEnumerable<IHasID>[] values)
        {
            //上記2つを同じ名前で使えるように別名で作っておく。
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException();
            }
            if (values.Length == 1)
            {
                return values[0].Select(v => new IHasID[] { v });
            }
            else
            {
                var temp = Combine(values[0], values[1]);

                foreach (var item in values.Skip(2))
                    temp = Combine(temp, item);

                return temp;
            }
        }

    }

    public static class HasID
    {
        /// <summary>
        /// 型を間違るとnullそれ以外はちゃんとした値。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T staticGetParameterSafeFromClass<T>(this IHasID value)
            where T : class
        {
            return (value as IHasID<T>)?.Parameter;
        }


        /// <summary>
        /// 型を間違えると例外出ます。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetParameter<T>(this IHasID value)
        //where T : struct
        {
            return (value as IHasID<T>).Parameter;
        }

        /// <summary>
        /// 型を間違るとnullそれ以外はちゃんとした値。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T? GetParameterSafe<T>(this IHasID value)
           where T : struct
        {
            return (value as IHasID<T>)?.Parameter;
        }

        /// <summary>
        /// value.TryGetValue(out x)と使える。この場合<T>を書かなくてもxの型で類推される。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool TryGetValue<T>(this IHasID value, out T dest)
        {
            var temp = value as IHasID<T>;
            if (temp != null)
            {
                dest = temp.Parameter;
                return true;
            }
            else
            {
                dest = default(T);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<IHasID<T>> CreateParameters<T>(IEnumerable<T> values)
        {
            //こいつの存在で
            // CreateValues(HasID.GetValues(Enumerable.Range(1, 9)))
            //に<int>とかなしで類推が効くようになる。
            return HasID<T>.CreateParameters(values);
        }

        /// <summary>
        /// <para> enum版のfactory enum以外は例外発生</para>
        /// <para> こんな感じで使うHasID.GetEnums<T>() </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<IHasID<T>> CreateParametersFromEnum<T>()
        {
            return HasID<T>.CreateParametersFromEnum();
        }
    }

    public class HasID<T> : IHasID<T>
    {
        /// <summary>
        /// 要はfactory
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static IEnumerable<IHasID<T>> CreateParameters(IEnumerable<T> values)
        {
            return values.Select((v, i) => new HasID<T>(i, v));
        }

        /// <summary>
        /// <para> enum版のfactory enum以外は例外発生</para>
        /// <para> こんな感じで使うHasID<T>.GetEnums() </para>
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IHasID<T>> CreateParametersFromEnum()
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();

            return Enum.GetValues(typeof(T)).Cast<T>().Select((c, i) => new HasID<T>(i, c));
        }

        private HasID(int id, T param)
        {
            //常にCreateParameterを使って作ればいいのでprivate
            this.ID = id;
            this.Parameter = param;
        }

        /// <summary>ID</summary>
        public int ID { get; }

        /// <summary>Parameter</summary>
        public T Parameter { get; }


        public override string ToString() => $"{this.Parameter}({this.ID})";


    }


    public static class ParameterHelper
    {
        public static IEnumerable<Parameters> ToParameters(this IEnumerable<IEnumerable<IHasID>> values) => values.Select(v => new Parameters(v));
        public static IEnumerable<ParametersI> ToParametersI(this IEnumerable<IEnumerable<IHasID>> values) => values.Select(v => new ParametersI(v));

    }

    public class Parameters
    {
        public Parameters(IEnumerable<IHasID> values)
        {
            this.values = values.ToArray();
            this.Count = this.values.Count();
        }

        public int Count { get; }

        private IHasID[] values;

        public T GetValue<T>(int index)
        {
            if (-1 < index && index < this.Count)
                return HasID.GetParameter<T>(values[index]);
            else
                throw new ArgumentOutOfRangeException();
        }

        public override string ToString() => $"count={this.Count}:[{string.Join<IHasID>(",", this.values)}]";
    }

    public class ParametersI
    {
        public ParametersI(IEnumerable<IHasID> values, int count)
        {
            this.values = values;
            this.Count = count;
        }

        public ParametersI(IEnumerable<IHasID> values)
            :this(values,values.Count())
        {
        }


        public int Count { get; }

        private IEnumerable<IHasID> values;

        public T GetValue<T>(int index)
        {
            if (-1 < index && index < this.Count)
                return HasID.GetParameter<T>(values.ElementAt(index));
            else
                throw new ArgumentOutOfRangeException();
        }
        public override string ToString() => $"count={this.Count}:[{string.Join<IHasID>(",", this.values)}]";

    }
}
