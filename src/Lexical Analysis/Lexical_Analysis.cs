public static class Lexical_Analysis
{
    //Método para separar en una lista de string los posibles tokens del input (con su token)
    public static List<(string, string)> Token_Separator(string input)
    {
        //Declarando listas y valores de apoyo iniciales para realizar el análisis
        List<(string, string)> tokens = new List<(string, string)>();
        List<string> states = new List<string>();
        bool IsToken;
        string aux = "";
        char next_to = ' ';

        //Analizando la entrada
        for (int i = 0; i < input.Length; i++)
        {
            //Recibimos una entrada vacía
            if (input[i] == ' ' && i < input.Length - 1)
            {
                //Si estamos en un texto, importa guardar el espacio, si no, no
                if (aux != "" && aux[0] == '"')
                {
                    aux = aux + ' ';
                }
                else
                {
                    aux = "";
                    i++;
                }
                //Como ya agregamos (o no) el espacio vacío, saltamos al sgte caracter    
            }
            //En caso de que no sea una entrada vacía, se agrega
            else
            {
                aux = aux + input[i];
            }
            //Haciendo varias podas para el método
            if (aux == "" && input[i] != ' ')
            {
                aux = input[i].ToString();
            }
            if (i != input.Length - 1)
            {
                next_to = input[i + 1];
            }
            //En este caso, es el fin de la entrada y lo marco con el símbolo $ ya que no
            //pertenece a ningún token (representa el final de la cadena)
            else
            {
                next_to = '$';
            }

            //Analizando el estado actual de la cadena
            if (input[i] != ' ')
            {
                (states, IsToken) = States_determinator(aux, states, next_to);

                //Si la entrada acabó y aún no se encuentra ningún token válido, devolver error
                if ((i == input.Length - 1 && IsToken == false) || states.Count() == 0)
                {
                    Console.WriteLine("LEXICAL ERROR: '" + aux + "' is not a valid token");
                    tokens.Add(("error", "error"));
                    break;
                }

                //Si el token es una operación comparativa o una asignación, saltar al siguiente token
                if (states[0] == Token.OR || states[0] == Token.EQUIVALENT || states[0] == Token.MINOR_E || states[0] == Token.MAJOR_E || states[0] == Token.DIFFERENT || states[0] == Token.FUNCTION_CORPUS || states[0] == Token.AND)
                {
                    i++;
                    aux = aux + next_to;
                }

                //Verificando si hay algún error en el tipo de token registrado actualmente
                if (states.Contains("lexical error, " + aux + " is not a valid token"))
                {
                    Console.WriteLine("LEXICAL ERROR: '" + (aux + next_to) + "' is not a valid token");
                    tokens.Add(("error", "error"));
                    break;
                }

                //Finalmente si no hay errores de ningún tipo y es un token válido, agregar a la lista
                if (IsToken == true)
                {
                    tokens.Add((aux, states[0]));
                    aux = "";
                    next_to = ' ';
                    states = new List<string>();
                }
            }
        }
        return tokens;
    }

    //Método para determinar en qué estado se encuentra el análisis del token actual y devuelve
    //qué posibles tokens puede ser
    static (List<string>, bool) States_determinator(string aux, List<string> states, char next_to)
    {
        //verifico si en el primer char hay un ", de ser asi, estamos en presencia de un text
        //y por tanto debe terminar en "
        if (aux[0] == Token.OP_EN_TEXT)
        {
            if (!states.Contains(Token.Text) && !states.Contains("not closed"))
            {
                states.Add(Token.Text);
                states.Add("not closed");
            }
            else if (states.Contains(Token.Text) && states.Contains("not closed") && aux[aux.Length - 1] == Token.OP_EN_TEXT)
            {
                states.Remove("not closed");
                return (states, true);
            }
            return (states, false);
        }

        //verifico si todos los char son números
        if (aux.All(Char.IsNumber))
        {
            if (!states.Contains(Token.Number))
            {
                states.Add(Token.Number);
            }
            if (!Char.IsNumber(next_to))
            {
                if (Char.IsLetter(next_to))
                {
                    aux = aux + next_to;
                    states.Add("lexical error, " + aux + " is not a valid token");
                    return (states, false);
                }
                else
                {
                    return (states, true);
                }
            }
            return (states, false);
        }

        //verifico si son referencia o palabra clave
        if (aux.All(Char.IsLetter))
        {
            if (!states.Contains(Token.REFERENCE))
            {
                states.Add(Token.REFERENCE);
            }

            if (aux == Token.IF && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.IF);
                return (states, true);
            }

            if (aux == Token.ELSE && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.ELSE);
                return (states, true);
            }

            if (aux == Token.FUNCTION_DECLARATOR && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.FUNCTION_DECLARATOR);
                return (states, true);
            }

            if (aux == Token.LET && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.LET);
                return (states, true);
            }

            if (aux == Token.IN && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.IN);
                return (states, true);
            }

            if ((aux == "false" || aux == "true") && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.Boolean);
                return (states, true);
            }

            if (aux == Token.PI && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.PI);
                return (states, true);
            }

            if (aux == Token.E && !Char.IsLetterOrDigit(next_to))
            {
                states.Remove(Token.REFERENCE);
                states.Add(Token.E);
                return (states, true);
            }

            if (states.Contains(Token.REFERENCE) && !Char.IsLetterOrDigit(next_to))
            {
                return (states, true);
            }
            return (states, false);
        }

        //Verifico qué símbolos son (no son números ni referencias)
        if (!aux.All(Char.IsLetterOrDigit))
        {
            if (aux == Token.ADD)
            {
                states.Add(Token.ADD);
            }

            if (aux == Token.SUBSTRACT)
            {
                states.Add(Token.SUBSTRACT);
            }

            if (aux == Token.MULTIPLY)
            {
                states.Add(Token.MULTIPLY);
            }

            if (aux == Token.SPLIT)
            {
                states.Add(Token.SPLIT);
            }

            if (aux == Token.EXPONENTIAL)
            {
                states.Add(Token.EXPONENTIAL);
            }

            if (aux == Token.REST)
            {
                states.Add(Token.REST);
            }

            if (aux == Token.CONCAT)
            {
                states.Add(Token.CONCAT);
            }

            if (aux == Token.OPEN_1)
            {
                states.Add(Token.OPEN_1);
            }

            if (aux == Token.CLOSED_1)
            {
                states.Add(Token.CLOSED_1);
            }

            if (aux == Token.FOLLOW)
            {
                states.Add(Token.FOLLOW);
            }

            if (aux == Token.CLOSE_LINE)
            {
                states.Add(Token.CLOSE_LINE);
            }

            if (aux == "|")
            {
                states.Add(Token.OR);
            }

            if (aux == "&")
            {
                states.Add(Token.AND);
            }

            if (aux == Token.NOT)
            {
                if (next_to == '=')
                {
                    states.Add(Token.DIFFERENT);
                }
                else
                {
                    states.Add(Token.NOT);
                }
            }

            if (aux == Token.MINOR)
            {
                if (next_to == '=')
                {
                    states.Add(Token.MINOR_E);
                }
                else
                {
                    states.Add(Token.MINOR);
                }
            }

            if (aux == Token.MAJOR)
            {
                if (next_to == '=')
                {
                    states.Add(Token.MAJOR_E);
                }
                else
                {
                    states.Add(Token.MAJOR);
                }
            }

            if (aux == Token.EQUAL)
            {
                if (next_to == '=')
                {
                    states.Add(Token.EQUIVALENT);
                }
                if (next_to == '>')
                {
                    states.Add(Token.FUNCTION_CORPUS);
                }
                else
                {
                    states.Add(Token.EQUAL);
                }
            }
            return (states, true);
        }

        //Retornando estado actual del análisis
        return (states, true);
    }
}