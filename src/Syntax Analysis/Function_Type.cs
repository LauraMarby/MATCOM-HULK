//Aquí se parsea una declaración de función
public class Function_type
{
    public string name; //Nombre de la función
    public string error; //Error que puede contener en su estructura, si está vacío no contiene errores
    public Function_Arguments arguments; //Argumentos que recibe
    public List<string> corpus; //Cuerpo de la función
    public List<string> corpus_tk; //Tokens del cuerpo de la function


    //Esto parsea la función, y termina creando un tipo de dato Function_type
    public Function_type(List<string> input, List<string> tokens)
    {
        name = input[1];
        error = "";
        arguments = new Function_Arguments();
        corpus = new List<string>();
        corpus_tk = new List<string>();
        bool Del_func = false;

        //Empieza el parseo de la función, el cual se divide en 3 partes:
        //1-Revisar la declaración y el nombre
        //2-Parsear los argumentos de la función
        //3-Parsear el cuerpo de la función

        //Revisando nombre y declaración de función
        if (tokens[1] == Token.REFERENCE)
        {
            if (Universal.Is_Function_on_List(name))
            {
                //Si la función ya está implementada, sustituirla por la nueva
                Del_func = true;
            }

            //Si no hubo error durante el chequeo inicial del nombre, parsea los argumentos
            arguments = new Function_Arguments(input, tokens);
            error = arguments.error;

            //Si no hubo error durante el parseo de argumentos, entonces parsea el cuerpo
            if (error == "")
            {
                //Buscamos en qué punto empieza el cuerpo de la función
                int init = 0;
                while (input[init] != Token.FUNCTION_CORPUS)
                {
                    if (init < input.Count - 1)
                    {
                        init++;
                    }
                    else
                    {
                        error = "SYNTAX ERROR: Missing '" + Token.FUNCTION_CORPUS + "' after function declaration";
                        break;
                    }
                }

                //Ahora hacemos una lista solo con el cuerpo de la función (sin el ';' del final)
                List<string> Corpus = new List<string>();
                List<string> Token_Corpus = new List<string>();
                for (int i = init + 1; i < input.Count - 1; i++)
                {
                    Corpus.Add(input[i]);
                    Token_Corpus.Add(tokens[i]);
                }

                //Aquí revisamos si el cuerpo de la función está vacío
                if (Corpus.Count == 0 || Token_Corpus.Count == 0)
                {
                    error = "SYNTAX ERROR: Empty implementation detected in function '" + name + "'";
                }
                else
                {
                    //Ahora que el cuerpo está listo, hacemos el parseo como si fuera una expressión normal
                    //pero pasándole las variables de argumentos como entradas

                    //Guardando lista de tokens inalterada (esto se debe a que la lista de tokens puede sufrir cambios durante el análisis)
                    List<string> TK = new List<string>();
                    foreach(string tk in Token_Corpus)
                    {
                        TK.Add(tk);
                    }

                    //Se analiza el cuerpo de la función igual que si fuera una expresión, pero las variables de su argumento
                    //se tratan como variables indefinidas, por lo cual se asume que contienen un dato válido para cualquier expresión
                    error = Expression_Type.Parser(Corpus, Token_Corpus, arguments.Arguments);
                    if (error == Token.Boolean || error == Token.Number || error == Token.Text || error == Token.REFERENCE)
                    {
                        corpus = Corpus;
                        corpus_tk = TK;
                        error = "";
                        //Si la función está implementada, borra la anterior, pues esta está sin errores y será
                        //guardada en la lista de funciones
                        if(Del_func==true)
                        {
                            Universal.Remove_Function(input[1]);
                        }
                    }
                }
            }
        }
        else
        {
            //Error en caso de que el nombre de la función sea un token inválido
            error = "SYNTAX ERROR: Invalid token '" + input[1] + "': It must be a '" + Token.REFERENCE + "' token";
        }
    }
}

//Aquí se parsean los argumentos de una función
public class Function_Arguments
{
    public List<string> Arguments;
    public string error;

    //Parser de argumentos de funciones
    public Function_Arguments(List<string> input, List<string> tokens)
    {
        //Primero declaramos las variables
        int end = 2;
        Arguments = new List<string>();
        error = "";
        //Ahora chequeamos si luego del nombre viene '('
        if ((2 < input.Count && input[2] != "(") || 2 > input.Count - 1)
        {
            error = "SYNTAX ERROR: Invalid token '" + input[2] + "', expected token '(' in arguments declaration";
        }
        else
        {
            //Buscando el final de la declaración de argumentos
            while (input[end] != ")")
            {
                end++;
            }
            //Revisando los argumentos

            for (int i = 3; i < end; i++) //Este for se va a ejecutar desde 3 hasta end a menos que haya un error
            {
                if (error != "")
                {
                    break;
                }
                if (tokens[i] == Token.REFERENCE)
                {
                    if (tokens[i + 1] != "," && tokens[i + 1] != ")")
                    {
                        error = "SYNTAX ERROR: Unexpected token '" + input[i + 1] + "' after REFERENCE";
                        break;
                    }
                    //Si la referencia está bien, entonces se guarda como argumento
                    else
                    {
                        if (Arguments.Contains(input[i]))
                        {
                            error = "SYNTAX ERROR: Argument '" + input[i] + "' already defined'";
                            break;
                        }
                        else
                        {
                            Arguments.Add(input[i]);
                        }
                    }
                }
                else if (tokens[i] == ",")
                {
                    //Verificando que las comas estén entre argumentos
                    if (tokens[i + 1] != Token.REFERENCE)
                    {
                        error = "SYNTAX ERROR: Invalid token '" + input[i + 1] + "' as function argument";
                    }
                    if (tokens[i - 1] != Token.REFERENCE)
                    {
                        error = "SYNTAX ERROR: Invalid token '" + input[i - 1] + "' as function argument";
                    }
                }
                //Si no es una coma o una referencia, entonces está mal
                else
                {
                    error = "SYNTAX ERROR: Unexpected token '" + input[i] + "' after '" + input[i - 1] + "' in argument declaration";
                }
            }
        }
    }

    //Esto es para generar un argumento de función vacío en caso de que se encuentre un error en el parsing
    public Function_Arguments()
    {
        Arguments = new List<string>();
        error = "";
    }
}