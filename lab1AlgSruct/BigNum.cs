using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
public class BigNum
{

    private List<int> _digits = new();
    public bool SIGN; //true if number is less then 0

    
    public BigNum() { }

    public BigNum(string num_in_str)
    {
        num_in_str = Strings.StrReverse(num_in_str);
        if (string.IsNullOrEmpty(num_in_str))
            return;
        if (num_in_str[0] == '-')
            SIGN = true;
        foreach (var digit in num_in_str)
        {
            if (digit == '-')
                continue;
            _digits.Add(digit - '0');
        }
    }
    public BigNum(int num_in_int)
    {
        SIGN = num_in_int < 0;
        num_in_int = Math.Abs(num_in_int);
        do
        {
            _digits.Add(num_in_int % 10);
            num_in_int /= 10;
        } while (num_in_int > 0);
    }
   

    #region ComparisonOfNumbers

    public static bool operator ==(BigNum a, BigNum b) => a?.SIGN == b?.SIGN && a._digits.SequenceEqual(b._digits);

    public static bool operator !=(BigNum a, BigNum b) => !(a == b);

    public static bool operator <(BigNum a, BigNum b)
    {
        if (a.SIGN != b.SIGN)
            return a.SIGN;

        if (a._digits.Count != b._digits.Count)
            return a.SIGN ? a._digits.Count > b._digits.Count : a._digits.Count < b._digits.Count;

        for (int i = a._digits.Count; i >= 0; i--)
            if (a[i] != b[i])
                return a.SIGN ? a[i] > b[i] : a[i] < b[i];

        return false;
    }

    public static bool operator <=(BigNum a, BigNum b) => a == b || a < b;

    public static bool operator >(BigNum a, BigNum b) => !(a <= b);

    public static bool operator >=(BigNum a, BigNum b) => !(a < b);

    #endregion

    #region PrivateMethods
    private int this[int i]
    {
        get => i < _digits.Count ? _digits[i] : 0;
        set => _digits[i] = value;
    }

    private void ClearZeros()
    {
        int c = _digits.Count - 1;
        while (c > 0 && _digits[c] == 0)
            _digits.RemoveAt(c--);
    }
    public static BigNum Abs(BigNum a)
    {
        return new BigNum() { _digits = a._digits, SIGN = false };
    }
    public static BigNum operator -(BigNum a) => new() { _digits = a._digits, SIGN = !a.SIGN };

    public static implicit operator BigNum(int n) => new(n);

    public static implicit operator int(BigNum n) => Int32.Parse(n.ToString());
    public override string ToString()
    {
        string result = SIGN ? "-" : "";
        for (int i = _digits.Count - 1; i >= 0; i--)
            result += _digits[i];
        return result;
    }

    public static BigNum Gcd(BigNum a, BigNum b)
    {
        a = Abs(a);
        b = Abs(b);
        while (a != 0 && b != 0)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }

        return a | b;
    }

    #endregion

    #region BaseOperations

    public static BigNum operator +(BigNum a, BigNum b)
    {
        var res = new BigNum();

        if (a.SIGN == b.SIGN)
        {
            int remainder = 0;
            res.SIGN = a.SIGN;

            for (int i = 0; i < Math.Max(a._digits.Count, b._digits.Count); i++)
            {
                var sum = a[i] + b[i] + remainder;
                res._digits.Add(sum % 10);
                remainder = sum / 10;
            }

            if (remainder == 1) res._digits.Add(1);
        }
        else
        {
            if (a.SIGN)
                res = b - (-a);
            else
                res = a - (-b);
        }

        return res;
    }

    public static BigNum operator -(BigNum a, BigNum b)
    {
        if (a == b) return new BigNum(0);

        var res = new BigNum();

        if (a.SIGN == b.SIGN)
        {
            int remainder = 0;
            int dif;

            if (a < b)
            {
                for (int i = 0; i < Math.Max(a._digits.Count, b._digits.Count); i++)
                {
                    dif = a.SIGN ? a[i] - b[i] - remainder : b[i] - a[i] - remainder;

                    if (dif < 0)
                    {
                        dif += 10;
                        remainder = 1;
                    }
                    else
                        remainder = 0;

                    res._digits.Add(dif);
                }
                res.SIGN = true;
            }
            else
            {
                res = -(b - a);
            }
        }
        else
            res = a.SIGN ? -(-a + b) : a + (-b);
        res.ClearZeros();
        return res;
    }

    public static BigNum operator *(BigNum a, BigNum b)
    {
        if (a == new BigNum(0) || b == new BigNum(0))
            return new BigNum(0);
        if (a < b)
        {
            (a, b) = (b, a);
        }
        var parts = new List<BigNum>();

        for (int i = 0; i < b._digits.Count; i++)
        {
            var reminder = 0;
            var part = new BigNum();
            for (int j = 0; j < i; j++)
                part._digits.Add(0);
            for (int j = 0; j < a._digits.Count; j++)
            {
                var mult = b[i] * a[j] + reminder;
                part._digits.Add(mult % 10);
                reminder = mult / 10;
            }
            if (reminder != 0)
                part._digits.Add(reminder);
            parts.Add(part);
        }

        var res = new BigNum() { SIGN = a.SIGN != b.SIGN };
        int carry = 0;

        for (int i = 0; i < parts[^1]._digits.Count; i++)
        {
            int sum = carry;

            for (int j = 0; j < parts.Count; j++)
                sum += parts[j][i];

            res._digits.Add(sum % 10);
            carry = sum / 10;
        }

        if (carry > 0)
        {
            while (carry > 0)
            {
                res._digits.Add(carry % 10);
                carry /= 10;
            }
        }

        return res;
    }

    public static BigNum operator /(BigNum a, BigNum b)
    {
        if (b == 0)
            return null;
        if (a == 0)
            return new BigNum(0);
        if (b == 1)
            return a;
        if (b == -1)
            return -a;

        var sub = new BigNum { SIGN = false };
        var res = new BigNum { SIGN = a.SIGN != b.SIGN };
        var absB = Abs(b);
        var i = a._digits.Count - 1;
        var firstStep = true;

        while (i >= 0)
        {
            int added = 0;

            do
            {
                sub._digits.Insert(0, a[i]);
                i--;
                added++;

                sub.ClearZeros();

                if (added > 1 && !firstStep)
                    res._digits.Insert(0, 0);
            } while (sub < absB && i >= 0);

            if (firstStep)
                firstStep = false;

            var quot = NaiveDiv(sub, absB);
            res._digits.Insert(0, quot);
            sub -= absB * quot;

            if (sub == new BigNum(0))
                sub._digits.Remove(0);
        }

        var modIs0 = sub != 0 && sub._digits.Count != 0;

        if (a.SIGN && b.SIGN && modIs0)
            res = res + 1;
        if (a.SIGN && !b.SIGN && modIs0)
            res = res - 1;
        if (res[0] == 0)
            res.SIGN = false;

        return res;
    }

    public static BigNum NaiveDiv(BigNum a, BigNum b)
    {
        var res = new BigNum(0);

        while (a >= b)
        {
            a -= b;
            res++;
        }
        return res;
    }

    public static BigNum operator %(BigNum a, BigNum b)
    {
        if (b == new BigNum(0))
            return null;

        var r = a - b * (a / b);
        return r == b ? new BigNum(0) : r;
    }

    public static BigNum Pow(BigNum a, BigNum b)
    {
        if (b == 1 || a == 0)
            return a;
        var res = new BigNum(1);

        while (b > 0)
        {
            if (b % 2 == 1)
                res *= a;
            a *= a;
            b /= 2;
        }

        return res;
    }
    public static BigNum Sqrt(BigNum a)
    {
        if (a.SIGN)
            return null;
        if (a < 4)
            return a == 0 ? 0 : 1;

        var k = 2 * Sqrt((a - a % 4) / 4);
        return a < Pow((k + 1), 2) ? k : k + 1;
    }

    #endregion

    #region OperationsByMod

    public static BigNum AddMod(BigNum a, BigNum b, BigNum m) => (a + b) % m;

    public static BigNum SubMod(BigNum a, BigNum b, BigNum m) => (a - b) % m;

    public static BigNum MulMod(BigNum a, BigNum b, BigNum m) => (a * b) % m;

    public static BigNum DivMod(BigNum a, BigNum b, BigNum m) => (a / b is null) ? null : (a / b) % m;

    public static BigNum ModMod(BigNum a, BigNum b, BigNum m) => (a % b is null) ? null : (a % b) % m;

    public static BigNum PowMod(BigNum a, BigNum b, BigNum m)
    {
        if (b < 0)
            return null;
        if (b == 0)
            return 1;
        if (b == 1 || a == 0)
            return a % m;

        var res = new BigNum(1);

        while (b > 0)
        {
            if (b % 2 == 1)
                res = (res * a) % m;
            a = (a * a) % m;
            b /= 2;
        }

        return res;
    }

    #endregion

    #region SystemOfComparisons

    public static (BigNum x, BigNum y) SystemOfComparisons(BigNum a, BigNum b, BigNum m)
    {
        if (m == 0)
        {
            return (null, null);
        }
        if (b == 0)
        {
            if (a == 0)
                return (0, 1);
            return (0, m);
        }
        if (a < 0 || a >= m)
            a %= m;
        if (b < 0 || b >= m)
            b %= m;

        var d = Gcd(a, m);
        if (b % d != 0)
            return (null, null);

        if (a == 0)
            return (0, 1);
        if (m == 0)
            return (1, 0);

        var q = m / a;
        var r = m - q * a;

        if (r == 0)
            return (1, 1 - q);

        m = a;
        a = r;
        var u = new BigNum(1);
        var u1 = new BigNum(1);
        var u2 = new BigNum(0);
        var v = -q;
        var v1 = -q;
        var v2 = new BigNum(1);

        while (m % a != 0)
        {
            q = m / a;
            r = m - q * a;
            m = a;
            a = r;
            u = -q * u1 + u2;
            v = -q * v1 + v2;
            u2 = u1;
            u1 = u;
            v2 = v1;
            v1 = v;
        }

        if (u.SIGN && u == 0)
            u.SIGN = false;
        if (v.SIGN && v == 0)
            v.SIGN = false;

        var f = b / d;
        return (v * f, u * f);
    }

    #endregion

    public static BigNum Rand(BigNum a, BigNum b)
    {
        var rnd = new Random();
        var res = new BigNum();
        var len = rnd.Next(a._digits.Count, b._digits.Count + 1);

        if (len == 1)
            res._digits.Add(rnd.Next(0, 10));
        else
            res._digits.Add(rnd.Next(1, 10));

        for (int i = 1; i < a._digits.Count; i++)
        {
            res._digits.Add(rnd.Next(a[i], 10));
        }

        var eq = len == b._digits.Count;
        for (int i = 0; i < len - a._digits.Count; i++)
        {
            var d = eq ? rnd.Next(0, b[a._digits.Count + i]) : rnd.Next(0, 10);
            res._digits.Add(d);
        }

        return res;
    }

    public static (BigNum K1, BigNum K2) GcdLinearRepresentation(BigNum a, BigNum b)
    {
        if (a == 0)
            return (0, 1);
        if (b == 0)
            return (1, 0);

        var q = a / b;
        var r = a - q * b;

        if (r == 0)
            return (1, 1 - q);

        a = b;
        b = r;
        var u = new BigNum(1);
        var u1 = new BigNum(1);
        var u2 = new BigNum(0);
        var v = -q;
        var v1 = -q;
        var v2 = new BigNum(1);

        while (a % b != 0)
        {
            q = a / b;
            r = a - q * b;
            a = b;
            b = r;
            u = -q * u1 + u2;
            v = -q * v1 + v2;
            u2 = u1;
            u1 = u;
            v2 = v1;
            v1 = v;
        }

        if (u.SIGN && u == 0)
            u.SIGN = false;
        if (v.SIGN && v == 0)
            v.SIGN = false;

        return (u, v);
    }
    public static BigNum MulInverse(BigNum a, BigNum m) => GcdLinearRepresentation(a, m).K1;
}
	
}
