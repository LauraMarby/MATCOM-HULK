public static class Expression_Type
{
    //Estas listas contienen los tokens de expresiones con resutados válidos y operaciones binarias
    //Su propósito es ayudar a reducir el código
    public static List<string> Values = new List<string> { Token.Number, Token.Text, Token.REFERENCE, Token.Boolean };
    public static List<string> Operations = new List<string> { "*", "/", "+", "-", "<", ">", "<=", ">=", "==", "!=", "&", "|", "^", "@", "%" };
    public static List<string> Boolean_Ops = new List<string> { "<", ">", "<=", ">=", "==", "!=" };

    //Método de parsear expresiones
    public static string Parser(List<string> input, List<string> tokens, List<string> Undefined_variables)
    {
        //Primero chequeamos que todas las variables de la función estén en Undefined_variables
        //Esto solo es útil para parsear los cuerpos de funciones
        if (Undefined_variables.Count != 0)
        {
            //Definiendo lista de variables definidas en la función
            List<string> Defined_variables = new List<string>();
            foreach (string x in Undefined_variables)
            {
                Defined_variables.Add(x);
            }
            //Analizando variables definidas
            for (int i = 0; i < input.Count; i++)
            {
                //Verificando que token en i sea una variable
                if (tokens[i] == Token.REFERENCE && i != input.Count - 1 && input[i + 1] != "(")    
                {
                    //Si la variable en cuestión es declarada en un let, agrégala a la lista de variables definidas para esta función
                    if (tokens[i + 1] == Token.EQUAL)
                    {
                        Defined_variables.Add(input[i]);
                    }
                    //Si la variable encontrada no está en la lista, devuelve un error
                    if (!Defined_variables.Contains(input[i]))
                    {
                        return "SYNTAX ERROR: Variable '" + input[i] + "' is not defined on function arguments or let-in expressions";
                    }
                }
                //Analizando el caso en que la variable encontrada sea el último elemento
                if (tokens[i] == Token.REFERENCE && i == input.Count - 1)
                {
                    if (!Defined_variables.Contains(input[i]))
                    {
                        return "SYNTAX ERROR: Variable '" + input[i] + "' is not defined on function arguments or let-in expressions";
                    }
                }
            }
        }

        //Ahora debemos empezar el parsing, en este caso consiste en analizar elemento a elemento de inicio a fin.
        string info = Recursive_Parser(input, tokens, 0);
        return info;
    }

    //Este es el método en el que hacemos el parsing recursivo descendente
    static string Recursive_Parser(List<string> input, List<string> tokens, int mark)
    {
        //Si los tokens a analizar solo contienen un elemento, analiza si es un valor válido y devuelvelo
        //La marca mark indica a partir de que punto se encuentran los tokens a analizar, de ahí para atrás
        //ya todo está analizado y solo queda subir recursivamente con los valores devueltos.
        if (tokens.Count - 1 == mark)
        {
            if (Values.Contains(tokens[mark]))
            {
                return tokens[mark];
            }
            else
            {
                return "SYNTAX ERROR: Unexpected token " + tokens[mark];
            }
        }

        //Si es una expresión que comienza por paréntesis, analizar lo de adentro del paréntesis y luego lo que sigue
        if (tokens[mark] == "(")
        {
            List<string> Parenthesis_Input = new List<string>();
            List<string> Parenthesis_tokens = new List<string>();
            int parenthesis_counter = 1;
            mark++;
            while (tokens[mark] != ")" || parenthesis_counter != 0)
            {
                if (tokens[mark] == "(")
                {
                    parenthesis_counter++;
                }
                if (tokens[mark] == ")")
                {
                    parenthesis_counter--;
                }
                if (parenthesis_counter != 0)
                {
                    Parenthesis_Input.Add(input[mark]);
                    Parenthesis_tokens.Add(tokens[mark]);
                    mark++;
                }
                else break;
            }

            //Obteniendo resultado del análisis
            string parenthesis_result = Recursive_Parser(Parenthesis_Input, Parenthesis_tokens, 0);
            if (!Values.Contains(parenthesis_result))
            {
                return parenthesis_result;
            }
            //Si todo está bien, continúa con el resultado
            tokens[mark] = parenthesis_result;
            return Recursive_Parser(input, tokens, mark);
        }

        //Si encontramos un llamado a una función con sus argumentos, asumimos que es válida (pues ya fué analizada sintácticamente)
        if (tokens[mark] == Token.REFERENCE && mark + 1 < tokens.Count - 1 && tokens[mark + 1] == "(")
        {
            string result = Token.REFERENCE;
            bool end = false;
            mark = mark + 2;
            while (!end)
            {
                int parenthesis_counter = 0;
                List<string> Arg_function = new List<string>();
                List<string> Arg_function_token = new List<string>();
                while (!(tokens[mark] == "," || tokens[mark] == ")") || parenthesis_counter != 0)
                {
                    if (tokens[mark] == "(")
                    {
                        parenthesis_counter++;
                    }
                    if (tokens[mark] == ")")
                    {
                        parenthesis_counter--;
                    }
                    Arg_function.Add(input[mark]);
                    Arg_function_token.Add(tokens[mark]);
                    mark++;
                }
                if (tokens[mark] == ")")
                {
                    end = true;
                }
                result = Recursive_Parser(Arg_function, Arg_function_token, 0);
                if (!Values.Contains(result))
                {
                    return result;
                }
                mark++;
            }
            //Ya se parseó la función, pues solo habia que revisar los argumentos. Ahora devuelve una referencia
            //y sigue analizando en dependencia de si queda algo por revisar de la entrada
            mark--;
            tokens[mark] = Token.REFERENCE;
            return Recursive_Parser(input, tokens, mark);
        }

        //Si encontramos un valor y una operación, verificamos el valor a la derecha y luego analizamos si es válida
        //la operación a través de Try_Solve_Binary
        if (Values.Contains(tokens[mark]) && mark < tokens.Count - 1)
        {
            if (Operations.Contains(tokens[mark + 1]))
            {
                if (tokens.Count - 1 == mark + 1)
                {
                    return "SYNTAX ERROR: Unexpected operation token '" + tokens[mark + 1] + "' at the end of the line";
                }
                return Try_Solve_Binary(tokens[mark], Recursive_Parser(input, tokens, mark + 2), tokens[mark + 1]);
            }
            else
            {
                return "SYNTAX ERROR: Unexpected operation '" + input[mark + 1] + "' after token '" + input[mark] + "'";
            }
        }

        //Si te encuentras con una expresión let-in, debes guardar todas las variables de let en Universal.Variables
        //y luego parsear lo de detrás de in como una expresión normal.
        if (tokens[mark] == Token.LET)
        {
            mark++;
            if (tokens[mark] == Token.IN)
            {
                return "SYNTAX ERROR: Empty variable declaration in let-in expression";
            }
            List<string> Let = new List<string>();
            List<string> Let_Tokens = new List<string>();
            while (tokens[mark] != "in")
            {
                Let.Add(input[mark]);
                Let_Tokens.Add(tokens[mark]);
                mark++;
            }
            List<List<string>> Let_List = new List<List<string>>();
            List<List<string>> Let_Tokens_List = new List<List<string>>();
            List<string> Current_List = new List<string>();
            List<string> Current_Tokens_List = new List<string>();
            for (int i = 0; i < Let.Count; i++)
            {
                if (Let[i] != "," && Let[i] != "in")
                {
                    Current_List.Add(Let[i]);
                    Current_Tokens_List.Add(Let_Tokens[i]);
                }
                else
                {
                    Let_List.Add(Current_List);
                    Let_Tokens_List.Add(Current_Tokens_List);
                    Current_List = new List<string>();
                    Current_Tokens_List = new List<string>();
                }
            }
            //Última variable declarada
            Let_List.Add(Current_List);
            Let_Tokens_List.Add(Current_Tokens_List);

            //Verificación de errores en la declaración de variables
            for (int i = 0; i < Let_List.Count; i++)
            {
                if (Let_Tokens_List[i].Count == 0)
                {
                    return "SYNTAX ERROR: Variable declaration expected";
                }
                if (Let_Tokens_List[i][0] != Token.REFERENCE)
                {
                    return "SYNTAX ERROR: Invalid token '" + Let_List[i][0] + "' as variable name";
                }
                if (Let_Tokens_List[i][1] != Token.EQUAL)
                {
                    return "SYNTAX ERROR: Invalid operation '" + Let_List[i][1] + "' in variable declaration";
                }
                if (Let_Tokens_List[i].Count < 2)
                {
                    return "SYNTAX ERROR: Value expected in variable declaration of variable '" + Let_List[i][0] + "'";
                }
                string Result = Recursive_Parser(Let_List[i], Let_Tokens_List[i], 2);
                if (Result != Token.REFERENCE && Result != Token.Number && Result != Token.Text && Result != Token.Boolean)
                {
                    return Result;
                }
                else
                {
                    if (Universal.Is_Variable_on_Dict(Let_List[i][0]))
                    {
                        Universal.Variables.Remove(Let_List[i][0]);
                    }
                    List<string> Value = new List<string>();
                    for (int j = 2; j < Let_List[i].Count; j++)
                    {
                        Value.Add(Let_List[i][j]);
                    }
                    Universal.Variables.Add(Let_List[i][0], Value);
                }
            }
            //Analizando que exista algo después de in que valga la pena analizar
            if (tokens.Count - 1 == mark)
            {
                return "SYNTAX ERROR: Null reference after 'in' declaration in 'let-in' expression";
            }
            return Recursive_Parser(input, tokens, mark + 1);
        }

        //Si es una condicional, hay que analizar su condición como bool y ambos cuerpos del if y else como expresiones
        //que devuelvan algún valor válido
        if (tokens[mark] == Token.IF)
        {
            if (tokens.Count - 1 == mark)
            {
                return "SYNTAX ERROR: Null reference after if declaration in conditional expression";
            }
            if (tokens[mark + 1] != "(")
            {
                return "SYNTAX ERROR: Missing argument reference after if declaration in conditional expression";
            }
            //En este punto, ya sabemos que if está bien, ahora vamos a analizar la condicional
            List<string> Cond = new List<string>();
            List<string> Cond_tokens = new List<string>();
            mark++; //Tenemos mark puesta sobre el "("
            mark++; //Ahora está sobre el primer elemento de la condicional
            int parenthesis_counter = 0;
            while (!(tokens[mark] == ")") || parenthesis_counter != 0)
            {
                if (tokens[mark] == "(")
                {
                    parenthesis_counter++;
                }
                if (tokens[mark] == ")")
                {
                    parenthesis_counter--;
                }
                Cond.Add(input[mark]);
                Cond_tokens.Add(tokens[mark]);
                mark++;
            }
            //Analizando la condicional. Si da mal hay que devolver error
            if (!Boolean_Parser(Cond, Cond_tokens))
            {
                return "SYNTAX ERROR: Non boolean argument detected in conditional expression arguments";
            }
            //Revisando que haya algo que analizar tras la condicional
            if (tokens.Count - 1 == mark || tokens[mark + 1] == Token.ELSE)
            {
                return "SYNTAX ERROR: Null reference after if declaration in conditional expression";
            }

            //Ahora revisamos la expresión de if y la expresión de else
            List<string> if_corpus_input = new List<string>();
            List<string> if_corpus_tokens = new List<string>();

            List<string> else_corpus_input = new List<string>();
            List<string> else_corpus_tokens = new List<string>();
            int i = mark + 1;
            int else_counter = 0;
            while (i < input.Count && (!(tokens[i] == Token.ELSE) || else_counter != 0))
            {
                if (tokens[i] == Token.IF)
                {
                    else_counter++;
                }
                if (tokens[i] == Token.ELSE)
                {
                    else_counter--;
                }
                if_corpus_input.Add(input[i]);
                if_corpus_tokens.Add(tokens[i]);
                i++;
            }
            if (i == input.Count)
            {
                return "SYNTAX ERROR: Not else declaration detected in conditional expression";
            }
            if (i == input.Count - 1)
            {
                return "SYNTAX ERROR: Null reference after else declaration in conditional expression";
            }
            i++;
            while (i < input.Count || else_counter > 0)
            {
                if (tokens[i] == Token.IF)
                {
                    else_counter++;
                }
                if (tokens[i] == Token.ELSE)
                {
                    else_counter--;
                }
                else_corpus_input.Add(input[i]);
                else_corpus_tokens.Add(tokens[i]);
                i++;
            }
            string if_result = Recursive_Parser(if_corpus_input, if_corpus_tokens, 0);
            string else_result = Recursive_Parser(else_corpus_input, else_corpus_tokens, 0);
            if (!Values.Contains(if_result))
            {
                return if_result;
            }
            return else_result;
        }

        //Ningún token válido encontrado
        return "Unknown Error happened :(";
    }

    //Este método evalúa los argumentos booleanos de las condicionales
    static bool Boolean_Parser(List<string> input, List<string> tokens)
    {
        //Separando los argumentos booleanos
        List<List<string>> Booleans = new List<List<string>>();
        List<List<string>> Input_Booleans = new List<List<string>>();
        int i = 0;
        while (i < input.Count)
        {
            List<string> Boolean = new List<string>();
            List<string> Input_Boolean = new List<string>();
            int parenthesis_counter = 0;
            while (i < input.Count && (!(tokens[i] == "&" || tokens[i] == "|") || parenthesis_counter != 0))
            {
                if (tokens[i] == "(")
                {
                    parenthesis_counter++;
                }
                if (tokens[i] == ")")
                {
                    parenthesis_counter--;
                }
                Boolean.Add(tokens[i]);
                Input_Boolean.Add(input[i]);
                i++;
            }
            Booleans.Add(Boolean);
            Input_Booleans.Add(Input_Boolean);
            i++;
        }
        //Para cada argumento booleano, se hace el análisis de su miembro izquierdo y derecho
        for (int count = 0; count < Booleans.Count; count++)
        {
            int j = 0;
            List<int> Bool_Op_position = new List<int>();
            while (j < Booleans[count].Count)
            {
                if (Boolean_Ops.Contains(Booleans[count][j]))
                {
                    Bool_Op_position.Add(j);
                }
                j++;
            }
            if (Bool_Op_position.Count != 1)
            {
                return false;
            }
            string Bool_Op = Booleans[count][Bool_Op_position[0]];

            List<string> Bool_Left = new List<string>();
            List<string> Input_Left = new List<string>();
            List<string> Bool_Right = new List<string>();
            List<string> Input_Right = new List<string>();

            for (int k = 0; k < Bool_Op_position[0]; k++)
            {
                Bool_Left.Add(Booleans[count][k]);
                Input_Left.Add(Input_Booleans[count][k]);
            }
            for (int k = Bool_Op_position[0] + 1; k < Booleans[count].Count; k++)
            {
                Bool_Right.Add(Booleans[count][k]);
                Input_Right.Add(Input_Booleans[count][k]);
            }

            //Resultados del análisis
            string Left_Result = Recursive_Parser(Input_Left, Bool_Left, 0);
            string Right_Result = Recursive_Parser(Input_Right, Bool_Right, 0);
            //Si son tipos de datos diferentes y ninguno es una variable, no se pueden comparar
            if (Left_Result != Right_Result && Left_Result != Token.REFERENCE && Right_Result != Token.REFERENCE)
            {
                return false;
            }
            //Si se usan operaciones de números entre datos que no son números, entonces está mal
            if (Bool_Op != "==" && Bool_Op != "!=" && Left_Result != Token.Number && Right_Result != Token.Number)
            {
                return false;
            }
        }
        return true;
    }

    //Este método recibe dos tipos de argumentos y una operación, y simula si se puede resolver o no una expresión binaria.
    //Devuelve el resultado de la operación, o el error de parseo correspondiente.
    static string Try_Solve_Binary(string Left, string Right, string Operation)
    {
        //Verificando que no haya errores en ambos miembros
        if (Left != Token.Number && Left != Token.Text && Left != Token.Boolean && Left != Token.REFERENCE)
        {
            return Left;
        }
        if (Right != Token.Number && Right != Token.Text && Right != Token.Boolean && Right != Token.REFERENCE)
        {
            return Right;
        }
        //Estas operaciones siempre tienen que devolver un número, y calculan con dos números
        if (Operation == "*" || Operation == "/" || Operation == "+" || Operation == "-" || Operation == "^" || Operation == "%")
        {
            if ((Left == Token.Number || Left == Token.REFERENCE) && (Right == Token.Number || Right == Token.REFERENCE))
            {
                return Token.Number;
            }
        }
        //Estas operaciones pueden recibir números o textos (en algunos casos bool también), los cuales comparan y devuelven un bool
        else if (Operation == "<" || Operation == ">" || Operation == "<=" || Operation == ">=" || Operation == "==" || Operation == "!=")
        {
            //Estas operaciones pueden admitir bool, y la condicion es que ambos valores sean del mismo token
            if ((Operation == "==" || Operation == "!=") && ((Left == Right) || Left == Token.REFERENCE || Right == Token.REFERENCE))
            {
                return Token.Boolean;
            }
            //Igual que el caso de arriba, pero no admiten bool ni texto
            if ((Operation == "<" || Operation == ">" || Operation == "<=" || Operation == ">=") && (Left != Token.Text && Right != Token.Text && Left != Token.Boolean && Left != Token.Boolean && Left == Right))
            {
                return Token.Boolean;
            }
        }
        //Estas operaciones solo pueden recibir dos bool y devuelven un bool
        else if (Operation == "&" || Operation == "|")
        {
            if ((Left == Token.Boolean || Left == Token.REFERENCE) && (Right == Token.Boolean || Right == Token.REFERENCE))
            {
                return Token.Boolean;
            }
        }
        //Estas operaciones reciben cualquier par de elementos, los concatenan y devuelven un texto
        else if (Operation == "@")
        {
            return Token.Text;
        }
        //Si no es ninguna de esas operaciones, hay algo mal
        return "SYNTAX ERROR: Invalid operation '" + Operation + "' between token '" + Left + "' and token '" + Right + "'";
    }
}