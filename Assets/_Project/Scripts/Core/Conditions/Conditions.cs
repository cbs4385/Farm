using System;
using System.Collections.Generic;
using System.Globalization;

namespace Farm.Core
{
    // What a condition can ask about the world. Implemented by the running game session; tests use a fake.
    public interface IWorldQuery
    {
        bool HasFlag(string flag);
        int GetVar(string name);
        GameDateTime Now { get; }
        string Weather { get; }
        string MapId { get; }
    }

    public sealed class ConditionException : Exception
    {
        public ConditionException(string message) : base(message) { }
    }

    // Small boolean expression language used by data: NPC schedules, dialogue variants, warps, events, shop stock...
    //
    //   expr   := or
    //   or     := and ('||' and)*
    //   and    := not ('&&' not)*
    //   not    := '!' not | '(' expr ')' | atom
    //   atom   := 'true' | 'false' | key ':' arg | 'hour' op N | 'day' op N | 'year' op N
    //
    // Built-in atoms:  flag:<id>   var:<id><op><n>   season:<spring|summer|fall|winter>   weather:<id>
    //                  moon:<phase>   map:<id>   hour>=20   day==15   year>=2
    // Operators: >= <= > < == !=   (hour is 0-25: 24 and 25 are the small hours after midnight)
    // Other systems add atoms with Conditions.Register("friendship", (arg, world) => ...).
    public static class Conditions
    {
        public delegate bool AtomEvaluator(string argument, IWorldQuery world);

        static readonly Dictionary<string, AtomEvaluator> Custom = new Dictionary<string, AtomEvaluator>();
        static readonly Dictionary<string, Node> Cache = new Dictionary<string, Node>();

        public static void Register(string key, AtomEvaluator evaluator)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key required", nameof(key));
            Custom[key] = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Cache.Clear();
        }

        public static void ClearCustomForTests()
        {
            Custom.Clear();
            Cache.Clear();
        }

        // Empty or null means "always true", so unconditioned data needs no special casing.
        public static bool Evaluate(string expression, IWorldQuery world)
        {
            if (string.IsNullOrWhiteSpace(expression)) return true;
            return Parse(expression).Eval(world);
        }

        public static bool TryEvaluate(string expression, IWorldQuery world, out bool result)
        {
            result = false;
            try { result = Evaluate(expression, world); return true; }
            catch (ConditionException) { return false; }
        }

        // For the data validator: is this expression well formed (including known atom keys)?
        public static bool Validate(string expression, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(expression)) return true;
            try { Parse(expression); return true; }
            catch (ConditionException e) { error = e.Message; return false; }
        }

        static Node Parse(string expression)
        {
            if (Cache.TryGetValue(expression, out var cached)) return cached;
            var parser = new Parser(expression);
            var node = parser.ParseAll();
            Cache[expression] = node;
            return node;
        }

        // ---- AST --------------------------------------------------------------------------------------------

        abstract class Node { public abstract bool Eval(IWorldQuery w); }

        sealed class Const : Node
        {
            readonly bool _value;
            public Const(bool value) { _value = value; }
            public override bool Eval(IWorldQuery w) => _value;
        }

        sealed class Not : Node
        {
            readonly Node _inner;
            public Not(Node inner) { _inner = inner; }
            public override bool Eval(IWorldQuery w) => !_inner.Eval(w);
        }

        sealed class And : Node
        {
            readonly Node _a, _b;
            public And(Node a, Node b) { _a = a; _b = b; }
            public override bool Eval(IWorldQuery w) => _a.Eval(w) && _b.Eval(w);
        }

        sealed class Or : Node
        {
            readonly Node _a, _b;
            public Or(Node a, Node b) { _a = a; _b = b; }
            public override bool Eval(IWorldQuery w) => _a.Eval(w) || _b.Eval(w);
        }

        sealed class Atom : Node
        {
            readonly Func<IWorldQuery, bool> _eval;
            public Atom(Func<IWorldQuery, bool> eval) { _eval = eval; }
            public override bool Eval(IWorldQuery w) => _eval(w);
        }

        // ---- parser -----------------------------------------------------------------------------------------

        sealed class Parser
        {
            readonly string _s;
            int _i;

            public Parser(string s) { _s = s; }

            public Node ParseAll()
            {
                var node = ParseOr();
                SkipSpace();
                if (_i < _s.Length) throw Error($"unexpected '{_s[_i]}'");
                return node;
            }

            ConditionException Error(string message) => new ConditionException($"Condition '{_s}': {message} at position {_i}");

            void SkipSpace()
            {
                while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++;
            }

            bool Peek(string token)
            {
                SkipSpace();
                return string.CompareOrdinal(_s, _i, token, 0, token.Length) == 0;
            }

            Node ParseOr()
            {
                var left = ParseAnd();
                while (Peek("||")) { _i += 2; left = new Or(left, ParseAnd()); }
                return left;
            }

            Node ParseAnd()
            {
                var left = ParseNot();
                while (Peek("&&")) { _i += 2; left = new And(left, ParseNot()); }
                return left;
            }

            Node ParseNot()
            {
                SkipSpace();
                if (Peek("!") && !Peek("!=")) { _i++; return new Not(ParseNot()); }
                if (Peek("("))
                {
                    _i++;
                    var inner = ParseOr();
                    if (!Peek(")")) throw Error("missing ')'");
                    _i++;
                    return inner;
                }
                return ParseAtom();
            }

            Node ParseAtom()
            {
                SkipSpace();
                var start = _i;
                // An atom runs until whitespace, '&', '|' or ')'.
                while (_i < _s.Length && !char.IsWhiteSpace(_s[_i]) && _s[_i] != '&' && _s[_i] != '|' && _s[_i] != ')') _i++;
                var text = _s.Substring(start, _i - start);
                if (text.Length == 0) throw Error("expected a condition");
                return MakeAtom(text);
            }

            Node MakeAtom(string text)
            {
                if (text == "true") return new Const(true);
                if (text == "false") return new Const(false);

                // hour>=20, day==15, year>=2
                foreach (var name in new[] { "hour", "day", "year" })
                {
                    if (text.StartsWith(name, StringComparison.Ordinal) && SplitComparison(text.Substring(name.Length), out var op, out var n))
                    {
                        var which = name;
                        return new Atom(w => Compare(which == "hour" ? w.Now.Hour : which == "day" ? w.Now.Day : w.Now.Year, op, n));
                    }
                }

                var colon = text.IndexOf(':');
                if (colon <= 0 || colon == text.Length - 1) throw Error($"cannot understand '{text}'");
                var key = text.Substring(0, colon);
                var arg = text.Substring(colon + 1);

                switch (key)
                {
                    case "flag": return new Atom(w => w.HasFlag(arg));
                    case "weather": return new Atom(w => w.Weather == arg);
                    case "map": return new Atom(w => w.MapId == arg);
                    case "season":
                        if (!Enum.TryParse<Season>(arg, true, out var season)) throw Error($"unknown season '{arg}'");
                        return new Atom(w => w.Now.Season == season);
                    case "moon":
                        if (!Enum.TryParse<MoonPhase>(arg, true, out var phase)) throw Error($"unknown moon phase '{arg}'");
                        return new Atom(w => w.Now.MoonPhase == phase);
                    case "var":
                    {
                        var idx = arg.IndexOfAny(new[] { '>', '<', '=', '!' });
                        if (idx <= 0 || !SplitComparison(arg.Substring(idx), out var op, out var n))
                            throw Error($"var needs a comparison like var:dread>=3, got '{arg}'");
                        var name = arg.Substring(0, idx);
                        return new Atom(w => Compare(w.GetVar(name), op, n));
                    }
                }

                if (Custom.TryGetValue(key, out var custom)) return new Atom(w => custom(arg, w));
                throw Error($"unknown condition type '{key}'");
            }

            bool SplitComparison(string text, out string op, out int number)
            {
                op = null;
                number = 0;
                foreach (var candidate in new[] { ">=", "<=", "==", "!=", ">", "<" })
                {
                    if (text.StartsWith(candidate, StringComparison.Ordinal))
                    {
                        op = candidate;
                        return int.TryParse(text.Substring(candidate.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
                    }
                }
                return false;
            }
        }

        static bool Compare(int left, string op, int right)
        {
            switch (op)
            {
                case ">=": return left >= right;
                case "<=": return left <= right;
                case ">": return left > right;
                case "<": return left < right;
                case "==": return left == right;
                default: return left != right;
            }
        }
    }
}
