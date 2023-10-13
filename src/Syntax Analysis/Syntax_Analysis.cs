public static class Syntax_Analysis
{
    //Método principal de análisis sintáctico y parseo
    public static string Parsing(List<(string, string)> splitted_input)
    {
        //separando en dos listas, una de los tokens, y la otra de los strings incertados por el usuario en la consola
        List<string> tokens = Tokens(splitted_input);
        List<string> input = Input(splitted_input);

        //Antes de parsear, tenemos que revisar que todos los parentesis, condicionales, let-in, '=>' y ';' estén balanceados
        string error = "";
        error = Parenthesis_Balance(input);
        if (error != "")
        {
            return error;
        }
        error = Let_In_Balance(input);
        if (error != "")
        {
            return error;
        }
        error = EndLine_Balance(input);
        if (error != "")
        {
            return error;
        }
        error = Function_Balance(input);
        if (error != "")
        {
            return error;
        }
        error = Conditional_Balance(input);
        if (error != "")
        {
            return error;
        }

        //En este punto, ya revisamos los balances, asi que si la función llegó hasta aqui es pq todo está bien

        //Ahora tenemos que parsear 3 casos bases diferentes:
        //1-Cuando la entrada declara una función
        //2-Cuando la entrada declara una expresión (la cual puede o no tener variables definidas en un let-in)

        //Este es el parseo que se hace en caso que se esté creando una función
        if (input[0] == "function")    
        {
            //Parseando la función 
            Function_type function = new Function_type(input, tokens);  
            if (function.error != "")
            {
                error = function.error;
            }
            else
            {
                Universal.Functions.Add(function);
            }
        }

        //Parseo para cualquier otro caso, que siempre lleva devolver un valor
        else
        {
            //Eliminando el ';' del final
            input.Remove(input[input.Count - 1]);   
            tokens.Remove(tokens[tokens.Count - 1]);

            //Parseando la expresión
            error = Expression_Type.Parser(input, tokens, new List<string>());  
            if (error == Token.Boolean || error == Token.Number || error == Token.Text || error == Token.REFERENCE)
            {
                error = "";
            }
        }
        return error;
    }

    //Método para extraer la lista de tokens de la entrada (tokens)
    public static List<string> Tokens(List<(string, string)> splitted_input)
    {
        List<string> tokens = new List<string>();
        for (int i = 0; i < splitted_input.Count(); i++)
        {
            string trash;
            string use;
            (trash, use) = splitted_input[i];
            tokens.Add(use);
        }
        return tokens;
    }

    //Método para extraer la lista de entradas del usuario (input)
    public static List<string> Input(List<(string, string)> splitted_input)
    {
        List<string> input = new List<string>(splitted_input.Count());
        for (int i = 0; i < splitted_input.Count(); i++)
        {
            string trash;
            string use;
            (use, trash) = splitted_input[i];
            input.Add(use);
        }
        return input;
    }

    //Balance de los '=>'
    public static string Function_Balance(List<string> input)
    {
        int count = 0;
        for (int i = 0; i < input.Count; i++)
        {
            if (input[i] == Token.FUNCTION_CORPUS)
            {
                if (i == 0 || input[i - 1] != Token.CLOSED_1)
                {
                    return "SYNTAX ERROR: Unexpected token '=>', there is no function declarated before it";
                }
                if (i == input.Count - 2)
                {
                    return "SYNTAX ERROR: Missing function implementation after '" + input[i] + "'";
                }
                else
                {
                    count++;
                }
            }
        }
        if (count > 1)
        {
            return "SYNTAX ERROR: Unexpected token '=>' detected";
        }
        if (count == 1 && input[0] != "function")
        {
            return "SYNTAX ERROR: Unexpected token '=>' detected";
        }
        return "";
    }

    //Balance de los ';'
    public static string EndLine_Balance(List<string> input)                
    {
        for (int i = 0; i < input.Count; i++)
        {
            if (i != input.Count - 1 && input[i] == Token.CLOSE_LINE)
            {
                return "SYNTAX ERROR: '" + input[i] + "' must be at the end of the line";
            }
            if (i == input.Count - 1 && input[i] != Token.CLOSE_LINE)
            {
                return "SYNTAX ERROR: Endline token ';' missing after '" + input[i] + "' expression";
            }
        }
        return "";
    }

    //Balance de los let-in
    public static string Let_In_Balance(List<string> input)                 
    {
        string error = "";
        int counter = 0;
        for (int i = 0; i < input.Count; i++)
        {
            if (counter < 0)
            {
                error = "SYNTAX ERROR: Unexpected token 'in', you need to declarate a reference first";
                return error;
            }
            if (input[i] == Token.LET)
            {
                counter++;
            }
            if (input[i] == Token.IN)
            {
                counter--;
            }
        }
        if (counter > 0)
        {
            error = "SYNTAX ERROR: Missing " + counter + " expression 'in' in let-in expression";
        }
        return error;
    }

    //Balance de los paréntesis
    public static string Parenthesis_Balance(List<string> input)
    {
        string error = "";
        int counter = 0;
        for (int i = 0; i < input.Count; i++)
        {
            if (counter < 0)
            {
                error = "SYNTAX ERROR: Unexpected token ')', you can't close a sentence without opening one before";
                return error;
            }
            if (input[i] == Token.OPEN_1)
            {
                counter++;
            }
            if (input[i] == Token.CLOSED_1)
            {
                counter--;
            }
        }
        if (counter > 0)
        {
            error = "SYNTAX ERROR: Missing closing " + counter + " parenthesis on input";
        }

        return error;
    }

    //Balance de las condicionales if-else
    public static string Conditional_Balance(List<string> input)
    {
        int counter=0;
        foreach(string str in input)
        {
            if(counter<0)
            {
                return "SYNTAX ERROR: Invalid else declaration in expression, missing if declaration";
            }
            if(str=="if")
            {
                counter++;
            }
            if(str=="else")
            {
                counter--;
            }
        }
        if(counter>0)
        {
            return "SYNTAX ERROR: Missing '"+counter+"' else declaration in conditional expression";
        }
        return "";
    }
}