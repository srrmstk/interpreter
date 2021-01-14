using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Interpreter
{
	partial class TextAnalyzer
	{
		string programText;
		int currentIndex;
		List<Lexeme> data = new List<Lexeme>();
		// составляем enum типов лексем
		public enum LexemeType
		{
			VARIABLE,
			NUMBER,
			INT,
			ARRAY,
			IF,
			ELSE,
			WHILE,
			READ,
			WRITE,
			LEFT_BRACE,
			RIGHT_BRACE,
			LEFT_SQUARE_BRACKET,
			RIGHT_SQUARE_BRACKET,
			LEFT_ROUND_BRACKET,
			RIGHT_ROUND_BRACKET,
			PLUS,
			MINUS,
			MULTIPLY,
			DIVIDE,
			SEMICOLON,
			COMMA,
			LESS,
			ASSIGN,
			MORE,
			EQUAL,
			LESS_OR_EQUAL,
			MORE_OR_EQUAL,
			NOT_EQUAL,
			FINISH,
			ERROR
		}

		// создаем структуру Лексемы. Она содержит тип лексемы, значение и ее позицию в строке
		public struct Lexeme
		{
			public LexemeType lexemeType;
			public string value;
			public int line;
			public int position;
		}

		// конструктор анализатора текста
		public TextAnalyzer(string prText)
		{
			programText = prText;
			currentIndex = 0;
		}

		public List<Lexeme> GetData()
		{
			return data;
		}

		public void Run() {
			Lexeme currentLexeme = new Lexeme();
			int currentPosition = 1;
			int currentLine = 1;

			while (currentIndex < programText.Length)
			{
				while (char.IsWhiteSpace(programText[currentIndex])){
					switch (programText[currentIndex++])
					{
						case ' ':
							++currentPosition;
							break;
						case '\f':
						case '\n':
						case '\v':
							++currentLine;
							currentPosition = 1;
							break;
						case '\t':
							currentPosition += 4;
							break;
					}
				}
                //Console.Write(currentLexeme.value);
                //Console.Write("\n" + currentLexeme.line + " " + currentLexeme.position);
				
				currentLexeme = NextLexeme();
				currentLexeme.line = currentLine;
				currentLexeme.position = currentPosition;

				if (currentLexeme.lexemeType == LexemeType.ERROR)
				{
					string message = "Analyzer error; line = " + currentLine.ToString() + " pos = " + currentPosition.ToString();
					throw new Exception(message);
				}

				currentPosition += currentLexeme.value.Length;
				data.Add(currentLexeme);
			}

			currentLexeme.lexemeType = LexemeType.FINISH;
			currentLexeme.line = currentLine;
			currentLexeme.position = currentPosition;
			data.Add(currentLexeme);
		}
		
		bool IsChar(char ch) {
			return ('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z');
		}

		Lexeme NextLexeme() {
			char currentChar = programText[currentIndex];
			currentIndex++;

			Lexeme result = new Lexeme();
			result.value = currentChar.ToString();

			if (IsChar(currentChar))
			{
				result.lexemeType = LexemeType.VARIABLE;
				currentChar = programText[currentIndex];

				while (currentIndex < programText.Length && IsChar(currentChar) || char.IsDigit(currentChar))
				{
					result.value += currentChar;
					currentIndex++;
					currentChar = programText[currentIndex];
				}

				if (result.value == "int")
				{
					result.lexemeType = LexemeType.INT;
				}
				if (result.value == "array")
				{
					result.lexemeType = LexemeType.ARRAY;
				}
				if (result.value == "if")
				{
					result.lexemeType = LexemeType.IF;
				}
				if (result.value == "else")
				{
					result.lexemeType = LexemeType.ELSE;
				}
				if (result.value == "while")
				{
					result.lexemeType = LexemeType.WHILE;
				}
				if (result.value == "read")
				{
					result.lexemeType = LexemeType.READ;
				}
				if (result.value == "write")
				{
					result.lexemeType = LexemeType.WRITE;
				}
			}
			else if (char.IsDigit(currentChar))
			{
				result.lexemeType = LexemeType.NUMBER;
				currentChar = programText[currentIndex];
				while (currentIndex < programText.Length && char.IsDigit(currentChar))
				{
					result.value += currentChar;
					currentIndex++;
					currentChar = programText[currentIndex];
				}

				if (currentIndex < programText.Length && IsChar(currentChar))
				{
					while (currentIndex < programText.Length && IsChar(currentChar))
					{
						result.value += currentChar;
						++currentIndex;
						currentChar = programText[currentIndex];
					}
					result.lexemeType = LexemeType.ERROR;
				}
			}
			else if (currentChar == '{')
			{
				result.lexemeType = LexemeType.LEFT_BRACE;
			}
			else if (currentChar == '}')
			{
				result.lexemeType = LexemeType.RIGHT_BRACE;
			}
			else if (currentChar == '[')
			{
				result.lexemeType = LexemeType.LEFT_SQUARE_BRACKET;
			}
			else if (currentChar == ']')
			{
				result.lexemeType = LexemeType.RIGHT_SQUARE_BRACKET;
			}
			else if (currentChar == '(')
			{
				result.lexemeType = LexemeType.LEFT_ROUND_BRACKET;
			}
			else if (currentChar == ')')
			{
				result.lexemeType = LexemeType.RIGHT_ROUND_BRACKET;
			}
			else if (currentChar == '+')
			{
				result.lexemeType = LexemeType.PLUS;
			}
			else if (currentChar == '-')
			{
				result.lexemeType = LexemeType.MINUS;
			}
			else if (currentChar == '*')
			{
				result.lexemeType = LexemeType.MULTIPLY;
			}
			else if (currentChar == '/')
			{
				result.lexemeType = LexemeType.DIVIDE;
			}
			else if (currentChar == ';')
			{
				result.lexemeType = LexemeType.SEMICOLON;
			}
			else if (currentChar == ',')
			{
				result.lexemeType = LexemeType.COMMA;
			}
			else if (currentChar == '<')
			{
				result.lexemeType = LexemeType.LESS;
				if (currentIndex < programText.Length && programText[currentIndex] == '=')
				{
					currentIndex++;
					result.lexemeType = LexemeType.LESS_OR_EQUAL;
					result.value = "<=";
				}
			}
			else if (currentChar == '>')
			{
				result.lexemeType = LexemeType.MORE;
				if (currentIndex < programText.Length && programText[currentIndex] == '=')
				{
					currentIndex++;
					result.lexemeType = LexemeType.MORE_OR_EQUAL;
					result.value = ">=";
				}
			}
			else if (currentChar == '=')
			{
				result.lexemeType = LexemeType.ASSIGN;
				if (currentIndex < programText.Length && programText[currentIndex] == '=')
				{
					currentIndex++;
					result.lexemeType = LexemeType.EQUAL;
					result.value = "==";
				}
			}
			else if (currentChar == '!' && currentIndex + 1 < programText.Length && programText[currentIndex + 1] == '=')
			{
				currentIndex++;
				result.lexemeType = LexemeType.NOT_EQUAL;
				result.value = "!=";
			}

			return result;
		}
    }
}
