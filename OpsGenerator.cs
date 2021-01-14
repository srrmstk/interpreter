using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class OpsGenerator
    {

        GeneratorTask currentTask;
        TextAnalyzer.Lexeme currentLexeme;
        AutomatState currentState;
        Table currentTable;
        string currentVariableName;
        Stack<MagazineItem> automatMagazine = new Stack<MagazineItem>();
        Stack<GeneratorTask> automatGenerator = new Stack<GeneratorTask>();
        Stack<int> labels = new Stack<int>();
        InterpreterData data = new InterpreterData();

        List<TextAnalyzer.Lexeme> inputData;
        private int countValuesByKey(Dictionary<string, List<int>> d, string keyValue)
        {
            int count = 0;
            foreach (string c in d.Keys)
            {
                if (c == keyValue) count++;
            }
            return count;
        }
        public enum OpsType
        {
            VARIABLE,
            NUMBER,
            OPERATION,
            ERROR
        }
        public enum OpsOperation
        {
			READ,
			WRITE,
			PLUS,
			MINUS,
			MULTIPLY,
			DIVIDE,
			LESS,
			ASSIGN,
			MORE,
			EQUAL,
			LESS_OR_EQUAL,
			MORE_OR_EQUAL,
			NOT_EQUAL,
			J, // just jump
			JF, // jump if false
			I, // index
			ERROR
		}

		public class OpsItem
		{
			public OpsType type = OpsType.ERROR;
			public OpsOperation operation = OpsOperation.ERROR;
			public string variableName;
			public int index = 0;
			public int number = 0;
			public int line = -1;
		    public int position = -1;

			public OpsItem(string name, TextAnalyzer.Lexeme lexeme)
			{
				type = OpsType.VARIABLE;
				variableName = name;
				line = lexeme.line;
				position = lexeme.position;
			}

			public OpsItem(OpsOperation op, TextAnalyzer.Lexeme lexeme)
			{
				type = OpsType.OPERATION;
				operation = op;
				line = lexeme.line;
				position = lexeme.position;
			}

			public OpsItem(int num,  TextAnalyzer.Lexeme lexeme)
			{
				type = OpsType.NUMBER;
				number = num;
				line = lexeme.line;
				position = lexeme.position;
			}

			public OpsItem(int num, int l, int p)
			{ 
				type = OpsType.NUMBER;
				number = num;
				line = l;
				position = p;
			}
		}

		public class InterpreterData
		{
			public List<OpsItem> Ops = new List<OpsItem>();
            public Dictionary<string, int> variables = new Dictionary<string, int>();
            public Dictionary<string, List<int>> arrays = new Dictionary<string, List<int>>();
		}


		void RunGeneratorTask()
		{
			switch (currentTask)
			{
				case GeneratorTask.EMPTY:
					break;
				case GeneratorTask.VARIABLE:
                    data.Ops.Add(new OpsItem(currentLexeme.value, currentLexeme));
                    break;
				case GeneratorTask.NUM:
				{
                    int num = int.Parse(currentLexeme.value);
					data.Ops.Add(new OpsItem(num, currentLexeme));
					break;
				}
				case GeneratorTask.READ:
					data.Ops.Add(new OpsItem(OpsOperation.READ, currentLexeme));
					break;
				case GeneratorTask.WRITE:
					data.Ops.Add(new OpsItem(OpsOperation.WRITE, currentLexeme));
					break;
				case GeneratorTask.PLUS:
					data.Ops.Add(new OpsItem(OpsOperation.PLUS, currentLexeme));
					break;
				case GeneratorTask.MINUS:
					data.Ops.Add(new OpsItem(OpsOperation.MINUS, currentLexeme));
					break;
				case GeneratorTask.MULTIPLY:
					data.Ops.Add(new OpsItem(OpsOperation.MULTIPLY, currentLexeme));
					break;
				case GeneratorTask.DIVIDE:
					data.Ops.Add(new OpsItem(OpsOperation.DIVIDE, currentLexeme));
					break;
				case GeneratorTask.LESS:
					data.Ops.Add(new OpsItem(OpsOperation.LESS, currentLexeme));
					break;
				case GeneratorTask.ASSIGN:
					data.Ops.Add(new OpsItem(OpsOperation.ASSIGN, currentLexeme));
					break;
				case GeneratorTask.MORE:
					data.Ops.Add(new OpsItem(OpsOperation.MORE, currentLexeme));
					break;
				case GeneratorTask.EQUAL:
					data.Ops.Add(new OpsItem(OpsOperation.EQUAL, currentLexeme));
					break;
				case GeneratorTask.LESS_OR_EQUAL:
					data.Ops.Add(new OpsItem(OpsOperation.LESS_OR_EQUAL, currentLexeme));
					break;
				case GeneratorTask.MORE_OR_EQUAL:
					data.Ops.Add(new OpsItem(OpsOperation.MORE_OR_EQUAL, currentLexeme));
					break;
				case GeneratorTask.NOT_EQUAL:
					data.Ops.Add(new OpsItem(OpsOperation.NOT_EQUAL, currentLexeme));
					break;
				case GeneratorTask.I:
					data.Ops.Add(new OpsItem(OpsOperation.I, currentLexeme));
					break;
				case GeneratorTask.TASK1:
					{
						labels.Push(data.Ops.Count);
						data.Ops.Add(new OpsItem(0, currentLexeme));
						data.Ops.Add(new OpsItem(OpsOperation.JF, currentLexeme));
						break;
					}
				case GeneratorTask.TASK2:
					{
						int place = labels.Peek();
						labels.Pop();
						labels.Push(data.Ops.Count);
						data.Ops.Add(new OpsItem(0, currentLexeme));
						data.Ops.Add(new OpsItem(OpsOperation.J, currentLexeme));
						data.Ops[place].number = data.Ops.Count;
						break;
					}
				case GeneratorTask.TASK3:
					{
						int place = labels.Peek();
						labels.Pop();
						data.Ops[place].number = data.Ops.Count;
						break;
					}
				case GeneratorTask.TASK4:
					{
						labels.Push(data.Ops.Count);
						break;
					}
				case GeneratorTask.TASK5:
					{
						int place = labels.Peek();
						labels.Pop();
						data.Ops.Add(new OpsItem(labels.Peek(), currentLexeme));
						labels.Pop();
						data.Ops.Add(new OpsItem(OpsOperation.J, currentLexeme));
						data.Ops[place].number = data.Ops.Count;
						break;
					}
				case GeneratorTask.TASK6:
					{
						currentTable = Table.VARIABLE;
						break;
					}
				case GeneratorTask.TASK7:
					{
						currentTable = Table.ARRAY;
						break;
					}
				case GeneratorTask.TASK8:
					{
						string name = currentLexeme.value;

						if (data.arrays.ContainsKey(name) || (data.variables.ContainsKey(name)))
                        {
							string message = "Redefine a variable; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
							throw new Exception(message);
						}

						if (currentTable == Table.VARIABLE)
						{
							data.variables.Add( name, 0 );
						}
						else
						{
							currentVariableName = name;
						}
						break;
					}
				case GeneratorTask.TASK9:
					{
                        var size = currentLexeme.value.Select(x => Convert.ToInt32(x.ToString())).ToList();
                        data.arrays.Add(currentVariableName, size);
						break;
					}
				default:
					{
						string message = "Generator error; line = " + currentLexeme.line .ToString()+ ", position = " + currentLexeme.position.ToString();
						throw new Exception(message);
					}
			}
		}

		public void Run()
		{
			automatMagazine.Push(new MagazineItem(AutomatState.S));
			automatGenerator.Push(GeneratorTask.EMPTY);
			currentTable = Table.VARIABLE;
			int currentLexemeId = 0;
			currentLexeme = inputData[currentLexemeId];
            

			for (; !(automatGenerator.Count == 0) && !(automatMagazine.Count == 0);)
			{
				MagazineItem currentMagazineItem = automatMagazine.Peek();
				automatMagazine.Pop();
				currentState = currentMagazineItem.state;
				currentTask = automatGenerator.Peek();
				automatGenerator.Pop();

				// run generator task
				RunGeneratorTask();

				// next state or read lexeme
				if (currentMagazineItem.isTerminal)
				{
					if (currentLexeme.lexemeType == TextAnalyzer.LexemeType.FINISH)
					{
						string message = "Generator error; All lexemes are read, BUT magazine is not empty; line = "
							+ currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
						throw new Exception(message);
					}

					// read lexeme
					if (currentMagazineItem.lexeme == currentLexeme.lexemeType)
					{
						currentLexemeId++;
						currentLexeme = inputData[currentLexemeId];
					}
					else
					{
						string message = "Generator error; Unexpected lexeme; line = "
							+ currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
						throw new Exception(message);
					}
				}
				else
				{
					NextState();
				}
			}
			if (currentLexeme.lexemeType != TextAnalyzer.LexemeType.FINISH)
			{
				string message = "There are unrecognized lexemes; line = "
					+ currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
				throw new Exception(message);
			}
		}

		public InterpreterData GetData()
		{
			return data;
		}
		public OpsGenerator(List<TextAnalyzer.Lexeme> lexemes)
		{
			inputData = lexemes;
		}

		void NextState()
		{
            //Console.WriteLine(currentState);
            switch (currentState)
            {
                case AutomatState.S:
                    {
                        //Console.WriteLine(currentLexeme.lexemeType);
                        //Console.WriteLine(currentLexeme.value);
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.INT:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.S));
                                    automatMagazine.Push(new MagazineItem(AutomatState.I));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.INT));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK6);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.ARRAY:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.S));
                                    automatMagazine.Push(new MagazineItem(AutomatState.P));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.ARRAY));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK7);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.ASSIGN));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.ASSIGN);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.READ:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.READ));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.READ);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.WRITE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.WRITE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.WRITE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.IF:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.K));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.C));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.IF));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK3);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK1);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.WHILE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.C));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.WHILE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK5);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK1);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK4);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.I:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.M));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK8);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.M:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.COMMA:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.M));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.COMMA));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK8);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.SEMICOLON:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.P:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.N));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_SQUARE_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NUMBER));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_SQUARE_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK9);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK8);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.N:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.COMMA:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.N));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_SQUARE_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NUMBER));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_SQUARE_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.COMMA));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK9);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK8);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.SEMICOLON:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.Q:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.ASSIGN));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.ASSIGN);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.READ:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.READ));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.READ);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.WRITE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.WRITE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.WRITE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.IF:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.K));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.C));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.IF));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK3);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK1);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.WHILE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.C));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.WHILE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK5);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK1);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK4);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case AutomatState.A:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.ASSIGN));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.ASSIGN);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.READ:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.READ));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.READ);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.WRITE:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.SEMICOLON));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.WRITE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.WRITE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.IF:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.K));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.C));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.IF));

                                    automatGenerator.Push(GeneratorTask.TASK3);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK1);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.WHILE:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.C));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.WHILE));

                                    automatGenerator.Push(GeneratorTask.TASK5);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK1);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK4);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.H:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.LEFT_SQUARE_BRACKET:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_SQUARE_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_SQUARE_BRACKET));

                                    automatGenerator.Push(GeneratorTask.I);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case AutomatState.C:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.L));
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.L));
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.NUMBER:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.L));
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NUMBER));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.NUM);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.L:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.LESS:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LESS));

                                    automatGenerator.Push(GeneratorTask.LESS);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.MORE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.MORE));

                                    automatGenerator.Push(GeneratorTask.MORE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.EQUAL:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.EQUAL));

                                    automatGenerator.Push(GeneratorTask.EQUAL);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.LESS_OR_EQUAL:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LESS_OR_EQUAL));

                                    automatGenerator.Push(GeneratorTask.LESS_OR_EQUAL);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.MORE_OR_EQUAL:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.MORE_OR_EQUAL));

                                    automatGenerator.Push(GeneratorTask.MORE_OR_EQUAL);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.NOT_EQUAL:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.Z));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NOT_EQUAL));

                                    automatGenerator.Push(GeneratorTask.NOT_EQUAL);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.K:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.ELSE:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_BRACE));
                                    automatMagazine.Push(new MagazineItem(AutomatState.Q));
                                    automatMagazine.Push(new MagazineItem(AutomatState.A));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_BRACE));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.ELSE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.TASK2);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case AutomatState.E:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.NUMBER:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NUMBER));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.NUM);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.U:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.PLUS:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.T));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.PLUS));

                                    automatGenerator.Push(GeneratorTask.PLUS);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.MINUS:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.U));
                                    automatMagazine.Push(new MagazineItem(AutomatState.T));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.MINUS));

                                    automatGenerator.Push(GeneratorTask.MINUS);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case AutomatState.T:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.NUMBER:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NUMBER));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.NUM);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.V:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.MULTIPLY:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(AutomatState.F));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.MULTIPLY));

                                    automatGenerator.Push(GeneratorTask.MULTIPLY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.DIVIDE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.V));
                                    automatMagazine.Push(new MagazineItem(AutomatState.F));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.DIVIDE));

                                    automatGenerator.Push(GeneratorTask.DIVIDE);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case AutomatState.F:
                    {
                        switch (currentLexeme.lexemeType)
                        {
                            case TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.RIGHT_ROUND_BRACKET));
                                    automatMagazine.Push(new MagazineItem(AutomatState.E));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.LEFT_ROUND_BRACKET));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.VARIABLE:
                                {
                                    automatMagazine.Push(new MagazineItem(AutomatState.H));
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.VARIABLE));

                                    automatGenerator.Push(GeneratorTask.EMPTY);
                                    automatGenerator.Push(GeneratorTask.VARIABLE);
                                    break;
                                }
                            case TextAnalyzer.LexemeType.NUMBER:
                                {
                                    automatMagazine.Push(new MagazineItem(TextAnalyzer.LexemeType.NUMBER));

                                    automatGenerator.Push(GeneratorTask.NUM);
                                    break;
                                }
                            default:
                                {
                                    string msg = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString(); //?
                                    throw new Exception(msg);
                                }
                        }
                        break;
                    }
                case AutomatState.Z:
                    {
                        break;
                    }
                case AutomatState.ERROR:
                default:
                    string message = "Generator error; line = " + currentLexeme.line.ToString() + ", position = " + currentLexeme.position.ToString();
                    throw new Exception(message);
            }
        }

		enum Table
		{
			ARRAY,
			VARIABLE
		}

		enum AutomatState
		{
			S, // → intIS | arrayPS | aH = E; Q | read(aH); Q | write(E); Q | if (C) { SQ }KZQ | while (C) { SQ }Q
			Q, // → aH = E; Q | read(aH); Q | write(E); Q | if (C) { SQ }KZQ | while (C) { SQ }Q | λ
			A, // → aH = E; | read(aH); | write(E); | if (C) { SQ }KZ | while (C) { SQ }
			I, // → aM
			M, // → ,aM | ;
			P, // → a[k]N
			N, // → ,a[k]N | ;
			H, // → [E] | λ
			C, // → (E)VUL | aHVUL | kVUL
			L, // → <EZ | >EZ | == EZ | ≤EZ | ≥EZ | !=EZ
			K, // → else { SQ } | λ
			E, // → (E)VU | aHVU | kVU
			U, // → + TU | -TU | λ
			T, // → (E)V | aHV | kV
			V, // → *FV | /FV | λ
			F, // → (E) | aH | k
			Z, // → λ
			ERROR // error state
		}

		enum GeneratorTask
		{
			EMPTY,
			VARIABLE,
			NUM,
			READ,
			WRITE,
			PLUS,
			MINUS,
			MULTIPLY,
			DIVIDE,
			LESS,
			ASSIGN,
			MORE,
			EQUAL,
			LESS_OR_EQUAL,
			MORE_OR_EQUAL,
			NOT_EQUAL,
			I,
			TASK1, // use with IF and WHILE
			TASK2, // use with ELSE
			TASK3, // use with IF
			TASK4, // use with WHILE
			TASK5, // use with WHILE
			TASK6, // use with INT
			TASK7, // use with ARRAY
			TASK8, // use with VARS
			TASK9, // use with VARS
		}

		struct MagazineItem
		{
			public bool isTerminal;
			public TextAnalyzer.LexemeType lexeme;
			public AutomatState state;

            public MagazineItem(TextAnalyzer.LexemeType lex) {
                isTerminal = true;
                lexeme = lex;
                state = AutomatState.ERROR;
            }
            public MagazineItem(AutomatState st) {
                isTerminal = false;
                lexeme = TextAnalyzer.LexemeType.ERROR;
                state = st;
            }
		}

	}
}
