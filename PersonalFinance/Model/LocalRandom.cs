using System;

namespace Lomont.PersonalFinance.Model
{
    /// <summary>
    /// Simple random class, good stats, not cryptographically secure
    /// </summary>
    [Serializable]
    public class LocalRandom
    {
        /// <summary>
        /// Create a random generator. 
        /// </summary>
        /// <param type="largePeriod">Use 2^128 bit (slower) or 2^64 bit period (faster) rng</param>
        /// <param type="lowSeed">low 64 bits of seed value</param>
        /// <param type="highSeed">high 64 bits of seed value</param>
        public LocalRandom(bool largePeriod = true, ulong? lowSeed = null, ulong? highSeed = null)
        {
            if (largePeriod)
                internalNext64 = Next128To64;
            else
                internalNext64 = Next64To64;
            if (lowSeed != null && highSeed != null)
                SetSeed(lowSeed.Value, highSeed.Value);
            else if (lowSeed != null)
                SetSeed(lowSeed.Value, 0);
            else
                SetSeed(42, 54); // seeds from paper for testing
        }


        public void SetSeed(ulong loSeed, ulong? hiSeed = null)
        {
            state = new UInt128(0,0);
            Next64();
            var high = hiSeed ?? 54;
            state += new UInt128(high, loSeed);
            Next64();
        }
        
        /// <summary>
        /// Get the seed as the low 64 bits, then the high 64 bits
        /// </summary>
        /// <returns></returns>
        public Tuple<ulong,ulong> GetSeed()
        {
            return new Tuple<ulong, ulong>(state.lo, state.hi);
        }

        /// <summary>
        /// Get next 32-bit random unsigned integer, uniformly distributed
        /// </summary>
        /// <returns></returns>
        public uint Next32()
        {
            return (uint)(Next64() >> 20);
        }

        /// <summary>
        /// Get next 64-bit random unsigned integer, uniformly distributed
        /// </summary>
        /// <returns></returns>
        public ulong Next64()
        {
            return internalNext64();
        }

        /// <summary>
        /// Uniform random integer in [0,max)
        /// </summary>
        /// <param type="max"></param>
        /// <returns></returns>
        public int Next(int max)
        {
            if (max <= 0) return 0;
            var threshold = (uint)((1UL << 32) - (ulong)max)%max;

            while (true)
            {
                var r = Next32();
                if (r >= threshold)
                    return (int)(r%max);
            }
        }

        /// <summary>
        /// Uniform random integer in [min,max)
        /// </summary>
        /// <param type="min"></param>
        /// <param type="max"></param>
        /// <returns></returns>
        public int Next(int min, int max)
        {
            if (max <= min) return min;
            return Next(max - min) + min;
        }


        /// <summary>
        /// Random in [0,1]
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            // from my post http://clomont.com/generating-uniform-random-numbers-on-01/
            // get exponent and first part of mantissa
            var e = -1 + 64;     // want e = -1, -64 happens always below
            ulong m = 0;         // this must be at least 64 bits
            do
            {
                m = Next64();    // get 64 random bits
                e = e - 64;      // we used 64 bits
                if (e < -1074)
                    return 0.0;  // will round to 0.0
            } while (m == 0);    // if all bits are so far 0, continue till we get a 1

            // pos will be the index of the leading 1 in the mantissa 
            var pos = 63;        // start at highest bit
            while (((m >> pos) & 1) == 0)
                pos = pos - 1;

            pos = 63 - pos;      // invert for ease of use. pos is now the # of leading 0s 

            if (pos != 0)
            {
                // we need more bits: shift m, and adjust exponent
                m = (m << pos) | (Next64() >> (64 - pos));
                e = e - pos;
            }

            // at this point, e is the exponent value 
            // m has a 1 in the highest bit, then 63 more bits, 
            // enough to pick the proper 52 for the mantissa and round correctly
            if (e > -1023)
            {
                // normal numbers
                m = m >> 10; // shift unneeded bits out, 54 left
                m = (m >> 1) + (m & 1); // shift and round
                if ((m >> 53) == 1)
                    // rounding caused overflow past implicit leading 1, fix exponent, m fixed below
                    e = e + 1;
            }

            else
            {
                // subnormal numbers, want top 52 bits of m
                int d = -1023 - e; // amount e is past the smallest e = -1023

                m = m >> (11 + d); // want 52 bits left, minus the excess, plus 1 for carry
                m = (m >> 1) + (m & 1); // shift and round
                e = -1023; // where we are
                if ((m >> 52) == 1)
                    // rounding caused overflow into implicit leading 0, fix exponent, m fixed below
                    e = e + 1;
            }

            ulong mask = 1;              // a single one bit
            mask = (mask << 52) - 1;      // done this way to avoid language specific shift issues
            m = m & mask;                 // mask bits 52-63 out. m now ready

            // now e is the final exponent and m is the final 52 bit mantissa, with no leading 1

            // create final 64 bit representation of the final value
            long fi;
            fi = e + 1023;                // exponent
            fi = fi << 52;                // room for m
            fi = fi | (long)m;                  // final value

            // language specific conversion trick
            // C/C++ :  memcpy(&f,&fi,8)  // other methods often use undefined behavior
            // C#    :  f = BitConverter.Int64BitsToDouble(fi)
            // Java  :  f = Double.longBitsToDouble(fi)
            return BitConverter.Int64BitsToDouble(fi);
        }

        /// <summary>
        /// Random in [a,b]
        /// </summary>
        /// <returns></returns>
        public double NextDouble(double min, double max)
        {
            throw new NotImplementedException("NextDouble not finished");
            if (min > max) return (min + max) / 2;
            var v = (double)Next32();
            v /= (1 << 16);
            v /= (1 << 16);
            return (max - min) * v + min; // todo - not sure this is as mathematically solid as possible
        }


        static void TestRand01(Action<string> outputAction)
        {
            outputAction("Testing rand01 idea");

            // need about 1800 bits, 29 ulongs will work 
            var randSeq = new ulong[29];
            var randIndex = 0;

            Func<ulong> rand64 = () => randSeq[randIndex++];
            Func<double> rand01 = () =>
            {
                // from my post http://clomont.com/generating-uniform-random-numbers-on-01/
                // get exponent and first part of mantissa
                var e = -1 + 64;    // want e = -1, -64 happens always below
                ulong m = 0;       // this must be at least 64 bits
                do
                {
                    m = rand64(); // get 64 random bits
                    e = e - 64; // we used 64 bits
                    if (e < -1074)
                        return 0.0; // will round to 0.0
                } while (m == 0);    // if all bits are so far 0, continue till we get a 1

                // pos will be the index of the leading 1 in the mantissa 
                var pos = 63;       // start at highest bit
                while (((m >> pos) & 1) == 0)
                    pos = pos - 1;

                pos = 63 - pos;     // invert for ease of use. pos is now the # of leading 0s 

                if (pos != 0)
                {
                    // we need more bits: shift m, and adjust exponent
                    m = (m << pos) | (rand64() >> (64 - pos));
                    e = e - pos;
                }

                // at this point, e is the exponent value 
                // m has a 1 in the highest bit, then 63 more bits, 
                // enough to pick the proper 52 for the mantissa and round correctly
                if (e > -1023)
                {
                    // normal numbers
                    m = m >> 10; // shift unneeded bits out, 54 left
                    m = (m >> 1) + (m & 1); // shift and round
                    if ((m >> 53) == 1)
                        // rounding caused overflow past implicit leading 1, fix exponent, m fixed below
                        e = e + 1;
                }

                else
                {
                    // subnormal numbers, want top 52 bits of m
                    int d = -1023 - e; // amount e is past the smallest e = -1023

                    m = m >> (11 + d); // want 52 bits left, minus the excess, plus 1 for carry
                    m = (m >> 1) + (m & 1); // shift and round
                    e = -1023; // where we are
                    if ((m >> 52) == 1)
                        // rounding caused overflow into implicit leading 0, fix exponent, m fixed below
                        e = e + 1;
                }

                ulong mask = 1;              // a single one bit
                mask = (mask << 52) - 1;      // done this way to avoid language specific shift issues
                m = m & mask;                 // mask bits 52-63 out. m now ready

                // now e is the final exponent and m is the final 52 bit mantissa, with no leading 1

                // create final 64 bit representation of the final value
                long fi;
                fi = e + 1023;                // exponent
                fi = fi << 52;                // room for m
                fi = fi | (long)m;                  // final value

                // language specific conversion trick
                // C/C++ :  memcpy(&f,&fi,8)  // other methods often use undefined behavior
                // C#    :  f = BitConverter.Int64BitsToDouble(fi)
                // Java  :  f = Double.longBitsToDouble(fi)
                return BitConverter.Int64BitsToDouble(fi);
            };

            Action div2 = () =>
            {
                for (var i = randSeq.Length-1; i >= 0; i--)
                {
                    randSeq[i] >>= 1;
                    var c = i > 0 ? randSeq[i - 1] : 0;
                    randSeq[i] |= c << 63;
                }
            };

            //load with value 1.0 = enough 1 bits to fill it
            randSeq[0] = randSeq[1] = UInt64.MaxValue;

            double randVal, startVal = 2.0;

            var count = 0;
            var success = true;
            while (startVal > 0)
            {
                startVal /= 2;
                randIndex = 0;
                randVal = rand01();
                success &= startVal == randVal;
                outputAction($"{count}: {startVal == randVal}, {startVal} = {randVal}");
                ++count;

                div2();
            }

            outputAction($"Test rand01 success: {success}");

        }

        /// <summary>
        /// Run a test on the rand generators, return true on success
        /// </summary>
        /// <param type="outputAction"></param>
        /// <returns></returns>
        public static bool Test64(Action<string> outputAction)
        {
            TestRand01(outputAction);

            // local testing helper
            Func<Action<string>, bool, ulong[], bool> RunTest = (printf, use128Bit, vals) =>
                {
                    var rand = new LocalRandom(use128Bit, 0, 0);
                    rand.SetSeed(42, 0); // seed from paper

                    var success1 = true;
                    foreach (var correctValue in vals)
                    {
                        var randValue = rand.Next64();
                        printf($"0x{randValue:X16}=0x{correctValue:X16}, {randValue == correctValue} ");
                        success1 &= randValue == correctValue;
                    }
                    return success1;
                };
           
            var success = true;
            outputAction("Testing rand 64");
            success &= RunTest(outputAction, false, new ulong[]
            {
                0x27a53829edf003a9,
                0xdf28458e5c04c31c,
                0x2756dc550bc36037,
                0xa10325553eb09ee9,
                0x40a0fccb8d9df09f,
                0x5c2047cfefb5e9ca
            });
            outputAction("Testing rand 128");
            success &= RunTest(outputAction, true, new ulong[]
            {
                0x287472e87ff5705a,
                0xbbd190b04ed0b545,
                0xb6cee3580db14880,
                0xbf5f7d7e4c3d1864,
                0x734eedbe7e50bbc5,
                0xa5b6b5f867691c77
            });
            outputAction(success ? "Rand test succeeded." : "Rand test failed.");
            return success;
        }



        #region Implementation

        /// <summary>
        /// Represent a 128 bit unsigned integer
        /// </summary>
        [Serializable]
        struct UInt128 // TODO : IComparable<Int128>, IComparable, IEquatable<Int128>, IConvertible, IFormattable , IBinarySerialize
        {
            internal ulong hi, lo;

            internal UInt128(ulong high64Bits, ulong low64Bits)
            {
                hi = high64Bits;
                lo = low64Bits;
            }

            uint[] ToUint32Array()
            {
                return new[]
                {
                (uint) (lo),
                (uint) (lo >> 32),
                (uint) (hi),
                (uint) (hi >> 32),
            };
            }

            public static UInt128 operator *(UInt128 left, UInt128 right)
            {
                uint[] xInts = left.ToUint32Array();
                uint[] yInts = right.ToUint32Array();
                uint[] mulInts = new uint[8];

                for (var i = 0; i < xInts.Length; i++)
                {
                    var index = i;
                    ulong remainder = 0;
                    foreach (var yi in yInts)
                    {
                        remainder = remainder + (ulong)xInts[i] * yi + mulInts[index];
                        mulInts[index++] = (uint)remainder;
                        remainder = remainder >> 32;
                    }

                    while (remainder != 0)
                    {
                        remainder += mulInts[index];
                        mulInts[index++] = (uint)remainder;
                        remainder = remainder >> 32;
                    }
                }
                ulong newLo = mulInts[1];
                newLo = (newLo << 32) | mulInts[0];
                ulong newHi = mulInts[3];
                newHi = (newHi << 32) | mulInts[2];

                return new UInt128(newHi, newLo);
            }

            public static UInt128 operator +(UInt128 left, UInt128 right)
            {
                UInt128 add = left;
                add.hi += right.hi;
                add.lo += right.lo;
                if (add.lo < left.lo)
                    add.hi++;
                return add;
            }

        }


        // source (128 or 64 bit state depending on settings) of a 64 it random integer
        readonly Func<ulong> internalNext64 = null;

        static readonly UInt128 PcgDefaultMultiplier128 = new UInt128(2549297995355413924UL, 4865540595714422341UL);
        static readonly UInt128 PcgDefaultIncrement128  = new UInt128(6364136223846793005UL, 1442695040888963407UL);
        UInt128 state = new UInt128(54,42); // default state for testing from paper

        /// <summary>
        /// Random 64 bit unsigned integer, uniformly distributed, period 2^128
        /// PCG XSL RR 128/64 (LCG) 
        /// period 2^128, state 128 bits, speed 1.70 ns/rand (from paper setup)
        /// </summary>
        /// <returns>next random 64-bit unsigned intger</returns>
        ulong Next128To64()
        {
            state = state * PcgDefaultMultiplier128 + PcgDefaultIncrement128;
            var rot = (int)(state.hi >> (122 - 64));
            var value = state.hi ^ state.lo;
            return (value >> rot) | (value << ((-rot) & 63));
        }


        const ulong PcgDefaultMultiplier64 = 6364136223846793005UL;
        const ulong PcgDefaultIncrement64  = 1442695040888963407UL;

        /// <summary>
        /// Random 64 bit unsigned integer, uniformly distributed, period 2^64
        /// algorithm PCG RXS M XS 64 (LCG) 
        /// has period 2^64, state 64 bits, speed 1.01 ns/rand (from paper setup)
        /// </summary>
        /// <returns>next random 64-bit unsigned integer</returns>
        ulong Next64To64()
        {
            var temp = ((state.lo >> ((int)(state.lo >> 59) + 5)) ^ state.lo) * 12605985483714917081UL;
            state.lo = state.lo * PcgDefaultMultiplier64 + PcgDefaultIncrement64;
            return (temp >> 43) ^ temp;
        }
#endregion

    }
}


