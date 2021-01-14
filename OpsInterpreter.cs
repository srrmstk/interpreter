using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class OpsInterpreter
    {

        OpsGenerator.InterpreterData inputData;
        public void Run()
        {
            Stack<OpsGenerator.OpsItem> magazine = new Stack<OpsGenerator.OpsItem>();
            var ops = inputData.Ops;
            var variables = inputData.variables;
            var arrays = inputData.arrays;

            for (int i = 0; i < ops.Count; ++i)
            {
                switch (ops[i].type)
                {
                    case OpsGenerator.OpsType.VARIABLE:
                        {
                            if (!arrays.ContainsKey(ops[i].variableName) && !variables.ContainsKey(ops[i].variableName))
                            {
                                string message = "Interpreter error; Unknown variable; line = "
                                    + ops[i].line.ToString() + ", position = " + ops[i].position.ToString();
                                throw new Exception(message);
                            }
                            magazine.Push(ops[i]);
                            break;
                        }
                    case OpsGenerator.OpsType.NUMBER:
                        {
                            magazine.Push(ops[i]);
                            break;
                        }
                    case OpsGenerator.OpsType.OPERATION:
                        {
                            switch (ops[i].operation)
                            {
                                case OpsGenerator.OpsOperation.READ:
                                    {
                                        int number;
                                        number = int.Parse(Console.ReadLine());
                                        var a = magazine.Peek();
                                        magazine.Pop();
                                        SetNum(a, number);
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.WRITE:
                                    {
                                        var a = magazine.Peek(); magazine.Pop();
                                        Console.WriteLine(GetNum(a));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.PLUS:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        int result = GetNum(a) + GetNum(b);

                                        magazine.Push(new OpsGenerator.OpsItem(result, a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.MINUS:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        int result = GetNum(a) - GetNum(b);

                                        magazine.Push(new OpsGenerator.OpsItem(result, a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.MULTIPLY:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        int result = GetNum(a) * GetNum(b);

                                        magazine.Push(new OpsGenerator.OpsItem(result, a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.DIVIDE:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        int result = GetNum(a) / GetNum(b);

                                        magazine.Push(new OpsGenerator.OpsItem(result, a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.LESS:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        magazine.Push(new OpsGenerator.OpsItem(int.Parse((GetNum(a) < GetNum(b)).ToString()), a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.ASSIGN:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        SetNum(a, GetNum(b));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.MORE:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        magazine.Push(new OpsGenerator.OpsItem(int.Parse((GetNum(a) > GetNum(b)).ToString()), a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.EQUAL:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        magazine.Push(new OpsGenerator.OpsItem(int.Parse((GetNum(a) == GetNum(b)).ToString()), a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.LESS_OR_EQUAL:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        magazine.Push(new OpsGenerator.OpsItem(int.Parse((GetNum(a) <= GetNum(b)).ToString()), a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.MORE_OR_EQUAL:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        magazine.Push(new OpsGenerator.OpsItem(int.Parse((GetNum(a) >= GetNum(b)).ToString()), a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.NOT_EQUAL:
                                    {
                                        var b = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();
                                        
                                        magazine.Push(new OpsGenerator.OpsItem(int.Parse((GetNum(a) != GetNum(b)).ToString()), a.line, a.position));
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.J:
                                    {
                                        var metka = magazine.Peek(); magazine.Pop();

                                        i = GetNum(metka) - 1;
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.JF:
                                    {
                                        var metka = magazine.Peek(); magazine.Pop();
                                        var a = magazine.Peek(); magazine.Pop();

                                        if (GetNum(a) == 0)
                                        {
                                            i = GetNum(metka) - 1;
                                        }
                                        break;
                                    }
                                case OpsGenerator.OpsOperation.I:
                                    {
                                        var idx = magazine.Peek(); magazine.Pop();
                                        var arr = magazine.Peek(); magazine.Pop();

                                        arr.index = GetNum(idx);

                                        magazine.Push(arr);
                                        break;
                                    }
                                default:
                                    {
                                        string msg = "Interpreter error; Unknown operation; line = "
                                            + ops[i].line.ToString() + ", position = " + ops[i].position.ToString();
                                        throw new Exception(msg);
                                    }
                            }
                            break;
                        }
                    default:
                        {
                            string message = "Interpreter error; Unknown ops item; line = "
                                + ops[i].line.ToString() + ", position = " + ops[i].position.ToString();
                            throw new Exception(message);
                        }
                }
            }
        }

        public OpsInterpreter(OpsGenerator.InterpreterData data)
        {
            inputData = data;
        }

        int GetNum(OpsGenerator.OpsItem item)
        {
            if (item.type == OpsGenerator.OpsType.NUMBER)
            {
                return item.number;
            }
            else if (item.type == OpsGenerator.OpsType.VARIABLE)
            {
                if (inputData.variables.ContainsKey(item.variableName))
                {
                    return inputData.variables[item.variableName];
                }
                else
                {
                    return inputData.arrays[item.variableName][item.index];
                }
            }
            else
            {
                string message = "Interpreter error; Variable or number was expected; line = "
                    + item.line.ToString() + " position = " + item.position.ToString();
                throw new Exception(message);
            }
        }

        void SetNum(OpsGenerator.OpsItem item, int number)
        {
            if (inputData.variables.ContainsKey(item.variableName))
            {
                inputData.variables[item.variableName] = number;
            }
            else if (inputData.arrays.ContainsKey(item.variableName))
            {
                inputData.arrays[item.variableName][item.index] = number;
            }
            else
            {
                string message = "Interpreter error; Variable was expected; line = "
                    + item.line.ToString() + ", position = " + item.position.ToString();
                throw new Exception(message);
            }
        }

    }
}
