using System;
using System.Collections.Generic;
using System.Text;

namespace Mathutil
{
    public delegate T UnaryOperator<T>(T operand);

    public delegate T BinaryOperator<T>(T operand1, T operand2);

    public delegate bool LogicUnaryOperator<T>(T operand);

    public delegate bool LogicBinaryOperator<T>(T operand1, T operand2);

    public delegate T BitwiseShiftOperator<T>(T operand, int n);
}
