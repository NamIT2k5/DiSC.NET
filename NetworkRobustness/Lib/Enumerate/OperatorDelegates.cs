using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Mathutil
{
    public class OperatorDelegates<T>
    {
        static OperatorDelegates()
        {
            Type[]
                ts1 = new Type[1] { typeof(T) },
                ts2 = new Type[2] { typeof(T), typeof(T) },
                tss = new Type[2] { typeof(T), typeof(int) };
            MethodInfo mi;
            mi = typeof(T).GetMethod("op_UnaryNegation", ts1);
            if (mi != null && mi.ReturnType == typeof(T))
                UnaryNegation = (UnaryOperator<T>)Delegate.CreateDelegate(typeof(UnaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_UnaryPlus", ts1);
            if (mi != null && mi.ReturnType == typeof(T))
                UnaryPlus = (UnaryOperator<T>)Delegate.CreateDelegate(typeof(UnaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Increment", ts1);
            if (mi != null && mi.ReturnType == typeof(T))
                Increment = (UnaryOperator<T>)Delegate.CreateDelegate(typeof(UnaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Decrement", ts1);
            if (mi != null && mi.ReturnType == typeof(T))
                Decrement = (UnaryOperator<T>)Delegate.CreateDelegate(typeof(UnaryOperator<T>), mi);

            mi = typeof(T).GetMethod("op_Addition", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                Addition = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Subtraction", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                Subtraction = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Multiply", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                Multiply = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Division", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                Division = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Modulus", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                Modulus = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);

            mi = typeof(T).GetMethod("op_GreaterThan", ts2);
            if (mi != null && mi.ReturnType == typeof(bool))
                GreaterThan = (LogicBinaryOperator<T>)Delegate.CreateDelegate(typeof(LogicBinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_GreaterThanOrEqual", ts2);
            if (mi != null && mi.ReturnType == typeof(bool))
                GreaterThanOrEqual = (LogicBinaryOperator<T>)Delegate.CreateDelegate(typeof(LogicBinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_LessThan", ts2);
            if (mi != null && mi.ReturnType == typeof(bool))
                LessThan = (LogicBinaryOperator<T>)Delegate.CreateDelegate(typeof(LogicBinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_LessThanOrEqual", ts2);
            if (mi != null && mi.ReturnType == typeof(bool))
                LessThanOrEqual = (LogicBinaryOperator<T>)Delegate.CreateDelegate(typeof(LogicBinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Equality", ts2);
            if (mi != null && mi.ReturnType == typeof(bool))
                Equality = (LogicBinaryOperator<T>)Delegate.CreateDelegate(typeof(LogicBinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_Inequality", ts2);
            if (mi != null && mi.ReturnType == typeof(bool))
                Inequality = (LogicBinaryOperator<T>)Delegate.CreateDelegate(typeof(LogicBinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_True", ts1);
            if (mi != null && mi.ReturnType == typeof(bool))
                True = (LogicUnaryOperator<T>)Delegate.CreateDelegate(typeof(LogicUnaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_False", ts1);
            if (mi != null && mi.ReturnType == typeof(bool))
                False = (LogicUnaryOperator<T>)Delegate.CreateDelegate(typeof(LogicUnaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_LogicalNot", ts1);
            if (mi != null && mi.ReturnType == typeof(bool))
                LogicalNot = (LogicUnaryOperator<T>)Delegate.CreateDelegate(typeof(LogicUnaryOperator<T>), mi);

            mi = typeof(T).GetMethod("op_BitwiseAnd", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                BitwiseAnd = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_BitwiseOr", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                BitwiseOr = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_ExclusiveOr", ts2);
            if (mi != null && mi.ReturnType == typeof(T))
                ExclusiveOr = (BinaryOperator<T>)Delegate.CreateDelegate(typeof(BinaryOperator<T>), mi);
            mi = typeof(T).GetMethod("op_OnesComplement", ts1);
            if (mi != null && mi.ReturnType == typeof(T))
                OnesComplement = (UnaryOperator<T>)Delegate.CreateDelegate(typeof(UnaryOperator<T>), mi);

            mi = typeof(T).GetMethod("op_LeftShift", tss);
            if (mi != null && mi.ReturnType == typeof(T))
                LeftShift = (BitwiseShiftOperator<T>)Delegate.CreateDelegate(typeof(BitwiseShiftOperator<T>), mi);
            mi = typeof(T).GetMethod("op_RightShift", tss);
            if (mi != null && mi.ReturnType == typeof(T))
                RightShift = (BitwiseShiftOperator<T>)Delegate.CreateDelegate(typeof(BitwiseShiftOperator<T>), mi);
        }

        //+,-
        public static readonly UnaryOperator<T> UnaryNegation;
        public static readonly UnaryOperator<T> UnaryPlus;
        //++, --
        public static readonly UnaryOperator<T> Increment;
        public static readonly UnaryOperator<T> Decrement;
        //+,-,*,/,%
        public static readonly BinaryOperator<T> Addition;
        public static readonly BinaryOperator<T> Subtraction;
        public static readonly BinaryOperator<T> Multiply;
        public static readonly BinaryOperator<T> Division;
        public static readonly BinaryOperator<T> Modulus;

        //>,>=,<,<=,==,!=,true,false,!
        public static readonly LogicBinaryOperator<T> GreaterThan;
        public static readonly LogicBinaryOperator<T> GreaterThanOrEqual;
        public static readonly LogicBinaryOperator<T> LessThan;
        public static readonly LogicBinaryOperator<T> LessThanOrEqual;
        public static readonly LogicBinaryOperator<T> Equality;
        public static readonly LogicBinaryOperator<T> Inequality;
        public static readonly LogicUnaryOperator<T> True;
        public static readonly LogicUnaryOperator<T> False;
        public static readonly LogicUnaryOperator<T> LogicalNot;

        //&,|,^,~
        public static readonly BinaryOperator<T> BitwiseAnd;
        public static readonly BinaryOperator<T> BitwiseOr;
        public static readonly BinaryOperator<T> ExclusiveOr;
        public static readonly UnaryOperator<T> OnesComplement;

        //<<,>>
        public static readonly BitwiseShiftOperator<T> LeftShift;
        public static readonly BitwiseShiftOperator<T> RightShift;
    }
}
