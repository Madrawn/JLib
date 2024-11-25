using FluentAssertions;
using FluentAssertions.Primitives;
using JLib.Exceptions;
using JLib.Helper;
using Newtonsoft.Json.Linq;
using Xunit;

namespace JLib.ValueTypes.Tests;

// required methods:
//     Create, TryCreate, CreateNullable

// required test variants:
//     class, struct

// required parameters types:
//     Invalid, Null, Valid

// required overloads:
//     FullyGeneric, HalfGeneric, NonGeneric

// total number of required tests:
//     total: 52
//     per method: 16/18
//     per test variant: 7/9
//     per parameter type: 1/3
//     per overload: 0/1
// Note: since it is impossible to call Create with a null value as long as it is not non-generic, create/struct/null has no Fully- or HalfGeneric tests.
// therefore, the total number of tests is decreased by 2 from 54 to 52.
public class FactoryTests
{


    public record FiveCharacterString(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(5);
    }
    public record ThreeCharacterString(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(3);
    }

    public record PositiveInt(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> must)
            => must.BePositive();
    }
    public record NegativeInt(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> must)
            => must.BeNegative();
    }

    public class Create
    {
        public abstract class ValidBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected ValidBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var t1 = ValueType.Create<TVt1, TV>(_value1);
                var t2 = ValueType.Create<TVt2, TV>(_value2);
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var t1 = ValueType.Create<TV>(typeof(TVt1), _value1);
                var t2 = ValueType.Create<TV>(typeof(TVt2), _value2);
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var t1 = ValueType.Create(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var t2 = ValueType.Create(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }
        }

        public abstract class InvalidBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected InvalidBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var act1 = () => ValueType.Create<TVt1, TV>(_value1);
                var act2 = () => ValueType.Create<TVt2, TV>(_value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var act1 = () => ValueType.Create<TV>(typeof(TVt1), _value1);
                var act2 = () => ValueType.Create<TV>(typeof(TVt2), _value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var act1 = () => ValueType.Create(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var act2 = () => ValueType.Create(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }
        }
        public class Class
        {
            public class Valid : ValidBase<ThreeCharacterString, FiveCharacterString, string>
            {
                public Valid() : base("123", "12345")
                {
                }
            }
            public class Invalid : InvalidBase<ThreeCharacterString, FiveCharacterString, string>
            {
                public Invalid() : base("1234", "123456")
                {
                }
            }
            public class Null
            {
                [Fact]
                public void ClassFullyGeneric()
                {
                    var act1 = () => ValueType.Create<ThreeCharacterString, string>(null!);
                    var act2 = () => ValueType.Create<FiveCharacterString, string>(null!);
                    act1.Should().Throw<AggregateException>();
                    act2.Should().Throw<AggregateException>();
                }

                [Fact]
                public void ClassHalfGeneric()
                {
                    var act1 = () => ValueType.Create<string>(typeof(ThreeCharacterString), null!);
                    var act2 = () => ValueType.Create<string>(typeof(FiveCharacterString), null!);
                    act1.Should().Throw<AggregateException>();
                    act2.Should().Throw<AggregateException>();
                }

                [Fact]
                public void ClassNonGeneric()
                {
                    var act1 = () => ValueType.Create(typeof(ThreeCharacterString), ObjectCastExtensions.As<object>(null!));
                    var act2 = () => ValueType.Create(typeof(FiveCharacterString), ObjectCastExtensions.As<object>(null!));
                    act1.Should().Throw<AggregateException>();
                    act2.Should().Throw<AggregateException>();
                }
            }
        }

        public class Struct
        {
            public class Valid : ValidBase<NegativeInt, PositiveInt, int>
            {
                public Valid() : base(-1, 1)
                {
                }
            }
            public class Null
            {
                [Fact]
                public void ClassNonGeneric()
                {
                    var act1 = () => ValueType.Create(typeof(NegativeInt), ObjectCastExtensions.As<object>(null!));
                    var act2 = () => ValueType.Create(typeof(PositiveInt), ObjectCastExtensions.As<object>(null!));
                    act1.Should().Throw<AggregateException>();
                    act2.Should().Throw<AggregateException>();
                }
            }

            public class Invalid : InvalidBase<NegativeInt, PositiveInt, int>
            {
                public Invalid() : base(1, -1)
                {
                }
            }
        }
    }

    public class CreateNullable
    {
        public abstract class Base<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected Base(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var t1 = ValueType.CreateNullable<TVt1, TV>(_value1);
                var t2 = ValueType.CreateNullable<TVt2, TV>(_value2);
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var t1 = ValueType.CreateNullable<TV>(typeof(TVt1), _value1);
                var t2 = ValueType.CreateNullable<TV>(typeof(TVt2), _value2);
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var t1 = ValueType.CreateNullable(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var t2 = ValueType.CreateNullable(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }
        }

        public abstract class NullableStructBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
            where TV : struct
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected NullableStructBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var t1 = ValueType.CreateNullable<TVt1, TV>(_value1);
                var t2 = ValueType.CreateNullable<TVt2, TV>(_value2);
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var t1 = ValueType.CreateNullable(typeof(TVt1), _value1);
                var t2 = ValueType.CreateNullable(typeof(TVt2), _value2);
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var t1 = ValueType.CreateNullable(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var t2 = ValueType.CreateNullable(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }
        }
        public abstract class InvalidBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected InvalidBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var act1 = () => ValueType.CreateNullable<TVt1, TV>(_value1);
                var act2 = () => ValueType.CreateNullable<TVt2, TV>(_value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var act1 = () => ValueType.CreateNullable<TV>(typeof(TVt1), _value1);
                var act2 = () => ValueType.CreateNullable<TV>(typeof(TVt2), _value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var act1 = () => ValueType.CreateNullable(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var act2 = () => ValueType.CreateNullable(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }
        }

        public class Struct
        {
            public class Valid : Base<NegativeInt, PositiveInt, int>
            {
                public Valid() : base(-1, 1)
                {
                }
            }
            public class Invalid : InvalidBase<NegativeInt, PositiveInt, int>
            {
                public Invalid() : base(1, -1)
                {
                }
            }
            public class Null
            {
                [Fact]
                public void ClassFullyGeneric()
                {
                    var t1 = ValueType.CreateNullable<NegativeInt, int>(null!);
                    var t2 = ValueType.CreateNullable<PositiveInt, int>(null!);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }

                [Fact]
                public void ClassHalfGeneric()
                {
                    var t1 = ValueType.CreateNullable<int>(typeof(NegativeInt), null);
                    var t2 = ValueType.CreateNullable<int>(typeof(PositiveInt), null);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }

                [Fact]
                public void ClassNonGeneric()
                {
                    var t1 = ValueType.CreateNullable(typeof(NegativeInt), ObjectCastExtensions.As<object>(null!));
                    var t2 = ValueType.CreateNullable(typeof(PositiveInt), ObjectCastExtensions.As<object>(null!));
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }
            }
        }

        public class Class
        {
            public class Valid : Base<ThreeCharacterString, FiveCharacterString, string>
            {
                public Valid() : base("123", "12345")
                {
                }
            }
            public class Invalid : InvalidBase<ThreeCharacterString, FiveCharacterString, string>
            {
                public Invalid() : base("1234", "123456")
                {
                }
            }
            public class Null
            {
                [Fact]
                public void ClassFullyGeneric()
                {
                    var t1 = ValueType.CreateNullable<ThreeCharacterString, string>(null!);
                    var t2 = ValueType.CreateNullable<FiveCharacterString, string>(null!);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }

                [Fact]
                public void ClassHalfGeneric()
                {
                    var t1 = ValueType.CreateNullable<string>(typeof(ThreeCharacterString), null!);
                    var t2 = ValueType.CreateNullable<string>(typeof(FiveCharacterString), null!);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }

                [Fact]
                public void ClassNonGeneric()
                {
                    var t1 = ValueType.CreateNullable(typeof(ThreeCharacterString), ObjectCastExtensions.As<object>(null!));
                    var t2 = ValueType.CreateNullable(typeof(FiveCharacterString), ObjectCastExtensions.As<object>(null!));
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }
            }
        }
    }

    public class TryCreate
    {
        public abstract class NullableStructBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
            where TV : struct
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected NullableStructBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void FullyGeneric()
            {
                var t1 = ValueType.TryCreate<TVt1, TV>(_value1, out var e1);
                var t2 = ValueType.TryCreate<TVt2, TV>(_value2, out var e2);
                e1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                e2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void HalfGeneric()
            {
                var t1 = ValueType.TryCreate<TV>(typeof(TVt1), (TV?)_value1, out var e1);
                var t2 = ValueType.TryCreate<TV>(typeof(TVt2), (TV?)_value2, out var e2);
                e1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                e2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void NonGeneric()
            {
                var t1 = ValueType.TryCreate(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!), out var e1);
                var t2 = ValueType.TryCreate(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!), out var e2);
                e1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                e2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }
        }
        public abstract class InvalidNullableStructBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
            where TV : struct
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected InvalidNullableStructBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var act1 = () => ValueType.CreateNullable<TVt1, TV>(_value1);
                var act2 = () => ValueType.CreateNullable<TVt2, TV>(_value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var act1 = () => ValueType.CreateNullable<TV>(typeof(TVt1), _value1);
                var act2 = () => ValueType.CreateNullable<TV>(typeof(TVt2), _value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var act1 = () => ValueType.CreateNullable(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var act2 = () => ValueType.CreateNullable(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }
        }
        public abstract class Base<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected Base(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void ClassFullyGeneric()
            {
                var t1 = ValueType.TryCreate<TVt1, TV>(_value1, out var e1);
                var t2 = ValueType.TryCreate<TVt2, TV>(_value2, out var e2);
                e1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                e2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassHalfGeneric()
            {
                var t1 = ValueType.TryCreate<TV>(typeof(TVt1), _value1, out var e1);
                var t2 = ValueType.TryCreate<TV>(typeof(TVt2), _value2, out var e2);
                e1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                e2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }

            [Fact]
            public void ClassNonGeneric()
            {
                var t1 = ValueType.TryCreate(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!), out var e1);
                var t2 = ValueType.TryCreate(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!), out var e2);
                e1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                e2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                t1.Should().BeOfType<TVt1>();
                t2.Should().BeOfType<TVt2>();
            }
        }

        public abstract class InvalidBase<TVt1, TVt2, TV>
            where TVt1 : ValueType<TV>
            where TVt2 : ValueType<TV>
        {
            private readonly TV? _value1;
            private readonly TV? _value2;

            protected InvalidBase(TV? value1, TV? value2)
            {
                _value1 = value1;
                _value2 = value2;
            }
            [Fact]
            public void FullyGeneric()
            {
                var act1 = () => ValueType.CreateNullable<TVt1, TV>(_value1);
                var act2 = () => ValueType.CreateNullable<TVt2, TV>(_value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void HalfGeneric()
            {
                var act1 = () => ValueType.CreateNullable<TV>(typeof(TVt1), _value1);
                var act2 = () => ValueType.CreateNullable<TV>(typeof(TVt2), _value2);
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }

            [Fact]
            public void NonGeneric()
            {
                var act1 = () => ValueType.CreateNullable(typeof(TVt1), ObjectCastExtensions.As<object>(_value1!));
                var act2 = () => ValueType.CreateNullable(typeof(TVt2), ObjectCastExtensions.As<object>(_value2!));
                act1.Should().Throw<AggregateException>();
                act2.Should().Throw<AggregateException>();
            }
        }

        public class Class
        {
            public class Valid : Base<ThreeCharacterString, FiveCharacterString, string>
            {
                public Valid() : base("123", "12345")
                {
                }
            }
            public class Invalid : InvalidBase<ThreeCharacterString, FiveCharacterString, string>
            {
                public Invalid() : base("1234", "123456")
                {
                }
            }
            public class Null
            {
                [Fact]
                public void ClassFullyGeneric()
                {
                    var t1 = ValueType.CreateNullable<ThreeCharacterString, string>(null!);
                    var t2 = ValueType.CreateNullable<FiveCharacterString, string>(null!);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }

                [Fact]
                public void ClassHalfGeneric()
                {
                    var t1 = ValueType.CreateNullable<string>(typeof(ThreeCharacterString), null!);
                    var t2 = ValueType.CreateNullable<string>(typeof(FiveCharacterString), null!);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }

                [Fact]
                public void ClassNonGeneric()
                {
                    var t1 = ValueType.CreateNullable(typeof(ThreeCharacterString), ObjectCastExtensions.As<object>(null!));
                    var t2 = ValueType.CreateNullable(typeof(FiveCharacterString), ObjectCastExtensions.As<object>(null!));
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                }
            }
        }


        public class Struct
        {
            public class Valid : NullableStructBase<NegativeInt, PositiveInt, int>
            {
                public Valid() : base(-1, 1)
                {
                }
            }
            public class Invalid : InvalidNullableStructBase<NegativeInt, PositiveInt, int>
            {
                public Invalid() : base(1, -1)
                {
                }
            }
            public class Null
            {
                [Fact]
                public void ClassFullyGeneric()
                {
                    var t1 = ValueType.TryCreate<NegativeInt, int>(null!, out var res1);
                    var t2 = ValueType.TryCreate<PositiveInt, int>(null!, out var res2);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                    res1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                    res2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                }

                [Fact]
                public void ClassHalfGeneric()
                {
                    var t1 = ValueType.TryCreate<int>(typeof(NegativeInt), null, out var res1);
                    var t2 = ValueType.TryCreate<int>(typeof(PositiveInt), null, out var res2);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                    res1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                    res2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                }

                [Fact]
                public void ClassNonGeneric()
                {
                    var t1 = ValueType.TryCreate(typeof(NegativeInt), ObjectCastExtensions.As<object>(null!), out var res1);
                    var t2 = ValueType.TryCreate(typeof(PositiveInt), ObjectCastExtensions.As<object>(null!), out var res2);
                    t1.Should().BeNull();
                    t2.Should().BeNull();
                    res1.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                    res2.GetException()?.ToHumanOptimizedJsonObject().Should().BeNull();
                }
            }
        }
    }
}
