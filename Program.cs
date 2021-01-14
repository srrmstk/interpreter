using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter;

namespace Interpreter
{
    class Program
    {
        static void RunProgram(string programText)
        {
            try
            {
                TextAnalyzer textAnalyzer = new TextAnalyzer(programText);
                textAnalyzer.Run();

                OpsGenerator opsGenerator = new OpsGenerator(textAnalyzer.GetData());
                opsGenerator.Run();

                OpsInterpreter opsInterpreter = new OpsInterpreter(opsGenerator.GetData());
                opsInterpreter.Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
        
        static void RunTest()
        {
            Console.WriteLine("Run test");
            int a, b, c;
            a = 5;
            b = 3;
            c = 1;
            a = a + 5 / 2 - c * 3;
            b = a / (b + c - 2) * 6;
            c = a - (3 + c) * 2 - (a + c * b) - c;
            Console.WriteLine("correct output:");
            Console.WriteLine(a.ToString() + '\n' + b.ToString() + '\n' + c.ToString());

            string programText = null;
            programText += "int a, b, c;";
            programText += "a = 5;";
            programText += "b = 3;";
            programText += "c = 1;";
            programText += "a = a + 5 / 2 - c * 3;";
            programText += "b = a / (b + c - 2) * 6;";
            programText += "c = a - (3 + c) * 2 - (a + c * b) - c;";
            programText += "write(a);";
            programText += "write(b);";
            programText += "write(c);";

            Console.WriteLine("program output:");
            RunProgram(programText);
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            RunTest();

            
            string fileName = "program.txt";
            /*FileStream program = File.Open(fileName, FileMode.Open);
            // преобразуем строку в байты
            byte[] bytes = new byte[program.Length];
            // считываем
            program.Read(bytes, 0, bytes.Length);
            // декодируем в строку
            string programText = System.Text.Encoding.Default.GetString(bytes);
            */

            StreamReader program = new StreamReader(fileName);
            string programText = program.ReadToEnd();
            Console.WriteLine("run main program");
            RunProgram(programText);
            Console.WriteLine("finish main program");

        }
    }
}
