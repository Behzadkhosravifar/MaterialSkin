using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace MaterialSkin
{
    public enum Parameters
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
    }

    public class MathParser
    {
        private readonly List<String> _operationOrder = new List<string>();
        public Dictionary<Parameters, decimal> Parameters { get; set; } = new Dictionary<Parameters, decimal>();

        public MathParser()
        {
            _operationOrder.Add("/");
            _operationOrder.Add("*");
            _operationOrder.Add("-");
            _operationOrder.Add("+");
        }
        public decimal Calculate(string formula)
        {
            try
            {
                var arr = formula.Split("/+-*()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var de in Parameters)
                {
                    foreach (var s in arr)
                    {
                        if (s != de.Key.ToString() && s.EndsWith(de.Key.ToString()))
                        {
                            formula = formula.Replace(s, (Convert.ToDecimal(s.Replace(de.Key.ToString(), "")) * de.Value).ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    formula = formula.Replace(de.Key.ToString(), de.Value.ToString(CultureInfo.InvariantCulture));
                }
                while (formula.LastIndexOf("(", StringComparison.Ordinal) > -1)
                {
                    var lastOpenPhrantesisIndex = formula.LastIndexOf("(", StringComparison.Ordinal);
                    var firstClosePhrantesisIndexAfterLastOpened = formula.IndexOf(")", lastOpenPhrantesisIndex, StringComparison.Ordinal);
                    var result = ProcessOperation(formula.Substring(lastOpenPhrantesisIndex + 1, firstClosePhrantesisIndexAfterLastOpened - lastOpenPhrantesisIndex - 1));
                    var appendAsterix = false;
                    if (lastOpenPhrantesisIndex > 0)
                    {
                        if (formula.Substring(lastOpenPhrantesisIndex - 1, 1) != "(" && !_operationOrder.Contains(formula.Substring(lastOpenPhrantesisIndex - 1, 1)))
                        {
                            appendAsterix = true;
                        }
                    }

                    formula = formula.Substring(0, lastOpenPhrantesisIndex) + (appendAsterix ? "*" : "") + result + formula.Substring(firstClosePhrantesisIndexAfterLastOpened + 1);

                }
                return ProcessOperation(formula);
            }
            catch (Exception ex)
            {
                throw new Exception("Error Occured While Calculating. Check Syntax", ex);
            }
        }

        private decimal ProcessOperation(string operation)
        {
            var arr = new ArrayList();
            var s = "";
            for (var i = 0; i < operation.Length; i++)
            {
                var currentCharacter = operation.Substring(i, 1);
                if (_operationOrder.IndexOf(currentCharacter) > -1)
                {
                    if (s != "")
                    {
                        arr.Add(s);
                    }
                    arr.Add(currentCharacter);
                    s = "";
                }
                else
                {
                    s += currentCharacter;
                }
            }
            arr.Add(s);
            foreach (var op in _operationOrder)
            {
                while (arr.IndexOf(op) > -1)
                {
                    var operatorIndex = arr.IndexOf(op);
                    var digitBeforeOperator = Convert.ToDecimal(arr[operatorIndex - 1]);
                    decimal digitAfterOperator;
                    if (arr[operatorIndex + 1].ToString() == "-")
                    {
                        arr.RemoveAt(operatorIndex + 1);
                        digitAfterOperator = Convert.ToDecimal(arr[operatorIndex + 1]) * -1;
                    }
                    else
                    {
                        digitAfterOperator = Convert.ToDecimal(arr[operatorIndex + 1]);
                    }
                    arr[operatorIndex] = CalculateByOperator(digitBeforeOperator, digitAfterOperator, op);
                    arr.RemoveAt(operatorIndex - 1);
                    arr.RemoveAt(operatorIndex);
                }
            }
            return Convert.ToDecimal(arr[0]);
        }
        private decimal CalculateByOperator(decimal number1, decimal number2, string op)
        {
            if (op == "/")
            {
                return number1 / number2;
            }
            else if (op == "*")
            {
                return number1 * number2;
            }
            else if (op == "-")
            {
                return number1 - number2;
            }
            else if (op == "+")
            {
                return number1 + number2;
            }
            else
            {
                return 0;
            }
        }
    }
}
