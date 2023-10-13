Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Havana University Language 4 Kompilers Console");
Console.ForegroundColor = ConsoleColor.DarkCyan;
Console.WriteLine("Laura Mártir Beltrán  C-111");
Console.ForegroundColor = ConsoleColor.White;
while (true)
{
    Console.Write('>');
    string input = Console.ReadLine();
    if (input != "")
    {
        //Haciendo el análisis léxico
        List<(string, string)> splitted_input = Lexical_Analysis.Token_Separator(input);
        //Si no hay errores, haz el análisis sintáctico
        if (!splitted_input.Contains(("error", "error")))
        {
            string error = Syntax_Analysis.Parsing(splitted_input);
            if (error != "")
            {
                Console.WriteLine(error);
            }
            //Si no hay errores, haz el análisis semántico
            else
            {
                //Esto va a imprimir un string, que puede ser un error semántico o puede ser
                //el valor esperado de la expresión.
                if (splitted_input.Count > 1)
                {
                    Console.WriteLine(Semantic.Analyze(splitted_input));
                }
                else
                {
                    Semantic.Analyze(splitted_input);
                }
                //Limpiando lista de variables del análisis sintáctico
                Universal.Clean_Var();
            }
        }
    }
}