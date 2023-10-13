public static class Semantic
{
    //Esta lista va a contener a todas las variables que encontremos en nuestro procesamiento
    //El primer elemento de la lista es el nombre de la variable, el segundo su valor y el
    //tercer elemento (int) representa el let-in en que se encuentra. Las variables con int=0 
    //son aquellas que nunca se van a borrar de la lista (como E y PI)                         
    public static List<(string, string, int)> Variables = new List<(string, string, int)> { ("PI", Math.PI.ToString(), 0), ("E", Math.E.ToString(), 0) };

    //Este int va a marcar el let-in actual en que se deben guardar las variables que se encuentren.
    //Su estado inicial es 1.
    static int Scope_Counter = 1;

    //Análisis semántico
    public static string Analyze(List<(string, string)> splitted_input)
    {
        //Si es una declaración de una función, ya se hizo la revisión en Function_Type
        if (splitted_input[0].Item1 == "function")
        {
            return "Function Declaration Completed";
        }
        //Si es una expresión, hay que analizarla
        List<string> Expression = new List<string>();
        List<string> Expr_Tokens = new List<string>();
        //Eliminando el ';' al final
        for (int i = 0; i < splitted_input.Count - 1; i++)
        {
            Expression.Add(splitted_input[i].Item1);
            Expr_Tokens.Add(splitted_input[i].Item2);
        }
        //Retornando error o solución de la expresión
        return Recursive_Analyzer(Expression, Expr_Tokens);
    }


    //Analizador semántico recursivo
    static string Recursive_Analyzer(List<string> Expression, List<string> Expr_Tokens)
    {
        //Si la expresión usa declaraciones de variables
        if (Expression[0] == "let")
        {
            //Aqui solo hay que guardar variables con el valor establecido en la expresión
            //y luego analizar la expresión de in
            int mark = 1;
            List<string> Let = new List<string>();
            List<string> Let_Tokens = new List<string>();
            while (Expression[mark] != "in")
            {
                Let.Add(Expression[mark]);
                Let_Tokens.Add(Expr_Tokens[mark]);
                mark++;
            }
            mark++;
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

            //Analizando cada variable declarada
            for (int i = 0; i < Let_List.Count; i++)
            {
                //Guardando el nombre y dejando en la lista solo el valor asignado a la variable
                string var_name = Let_List[i][0];
                Let_List[i].Remove(Let_List[i][0]);
                Let_List[i].Remove(Let_List[i][0]);
                Let_Tokens_List[i].Remove(Let_Tokens_List[i][0]);
                Let_Tokens_List[i].Remove(Let_Tokens_List[i][0]);

                //Análisis del valor de la variable
                string var_Value = Recursive_Analyzer(Let_List[i], Let_Tokens_List[i]);

                //Análisis de errores en la variable
                if (Var_Eval(var_Value) != "text" && Var_Eval(var_Value) != "boolean" && Var_Eval(var_Value) != "number")
                {
                    return var_Value;
                }
                else
                {
                    //Si la variable ya fue declarada en el mismo scope, sustituir su valor, sino agregarla normal 
                    if (Variables_Contains_on_Scope(var_name))
                    {
                        Variables_Remove(var_name);
                    }
                    Variables.Add((var_name, var_Value, Scope_Counter));
                }
            }
            //Al terminar con let, subimos un nivel del scope
            Scope_Counter++;
            //Analizando la expresión del in
            List<string> In_Expression = new List<string>();
            List<string> In_Expr_Tokens = new List<string>();
            while (mark < Expression.Count)
            {
                In_Expression.Add(Expression[mark]);
                In_Expr_Tokens.Add(Expr_Tokens[mark]);
                mark++;
            }
            string Result = Recursive_Analyzer(In_Expression, In_Expr_Tokens);
            //Limpiando scope
            Scope_Counter--;
            Scope_Clean();
            //Retornando resultado de evaluar la expresión
            return Result;
        }

        //Si la expresión es una condicional
        if (Expr_Tokens[0] == Token.IF)
        {
            //Aquí el procedimiento es evaluar el booleano que está entre los paréntesis
            //y en dependencia de si da true o false, se analiza el cuerpo de if o el de
            //else y se devuelve el resultado

            //Analizando argumentos de la condicional (debe retornar booleano)
            int mark = 2;
            int par_counter = 0;
            List<string> boolean_exp = new List<string>();
            List<string> token_boolean_exp = new List<string>();
            while (Expression[mark] != ")" || par_counter != 0)
            {
                boolean_exp.Add(Expression[mark]);
                token_boolean_exp.Add(Expr_Tokens[mark]);
                if (Expression[mark] == "(")
                {
                    par_counter++;
                }
                if (Expression[mark] == ")")
                {
                    par_counter--;
                }
                mark++;
            }
            //Resultado del argumento de la condicional
            string boolean = Recursive_Analyzer(boolean_exp, token_boolean_exp);

            //Si es true, analizar cuerpo del IF
            if (boolean == "True")
            {
                List<string> If_event = new List<string>();
                List<string> If_event_tokens = new List<string>();
                mark++;
                while (Expression[mark] != "else")
                {
                    If_event.Add(Expression[mark]);
                    If_event_tokens.Add(Expr_Tokens[mark]);
                    mark++;
                }
                return Recursive_Analyzer(If_event, If_event_tokens);
            }

            //Si es False, analizar cuerpo del ELSE
            else if (boolean == "False")
            {
                List<string> Else_event = new List<string>();
                List<string> Else_event_tokens = new List<string>();
                while (Expression[mark] != "else")
                {
                    mark++;
                }
                mark++;
                while (mark < Expression.Count)
                {
                    Else_event.Add(Expression[mark]);
                    Else_event_tokens.Add(Expr_Tokens[mark]);
                    mark++;
                }
                return Recursive_Analyzer(Else_event, Else_event_tokens);
            }

            //Si no es ninguno de esos casos, es un error, entonces devolver el error
            else
            {
                return boolean;
            }
        }

        //Analizando el caso en que la expresión es un valor o una combinación de valores a través de operaciones binarias y/o funciones.
        else
        {
            //Principalmente buscar operaciones binarias en orden de prioridad
            int parentheses_count = 0;

            //Este for da importancia a los & |
            for (int i = 0; i < Expression.Count; i++)
            {
                if (Expression[i] == "(")
                {
                    parentheses_count++;
                }
                if (Expression[i] == ")")
                {
                    parentheses_count--;
                }
                if ((Expression[i] == Token.AND && parentheses_count == 0) || (Expression[i] == Token.OR && parentheses_count == 0))
                {
                    return Solve_Binary(Expression, Expr_Tokens, Expression[i], i);
                }
            }
            //Este for le da importancia a los == != < <= > >=
            for (int i = 0; i < Expression.Count; i++)
            {
                if (Expression[i] == "(")
                {
                    parentheses_count++;
                }
                if (Expression[i] == ")")
                {
                    parentheses_count--;
                }
                if ((Expression[i] == Token.EQUIVALENT && parentheses_count == 0) || (Expression[i] == Token.DIFFERENT && parentheses_count == 0) || (Expression[i] == Token.MINOR_E && parentheses_count == 0) || (Expression[i] == Token.MINOR && parentheses_count == 0) || (Expression[i] == Token.MAJOR && parentheses_count == 0) || (Expression[i] == Token.MAJOR_E && parentheses_count == 0))
                {
                    return Solve_Binary(Expression, Expr_Tokens, Expression[i], i);
                }
            }

            //Este for le da importancia a suma y resta
            for (int i = 0; i < Expression.Count; i++)
            {
                if (Expression[i] == "(")
                {
                    parentheses_count++;
                }
                if (Expression[i] == ")")
                {
                    parentheses_count--;
                }
                if ((Expression[i] == Token.ADD && parentheses_count == 0) || (Expression[i] == Token.SUBSTRACT && parentheses_count == 0))
                {
                    return Solve_Binary(Expression, Expr_Tokens, Expression[i], i);
                }
            }

            //Este for le da importancia a * / %
            for (int i = 0; i < Expression.Count; i++)
            {
                if (Expression[i] == "(")
                {
                    parentheses_count++;
                }
                if (Expression[i] == ")")
                {
                    parentheses_count--;
                }
                if ((Expression[i] == Token.MULTIPLY && parentheses_count == 0) || (Expression[i] == Token.SPLIT && parentheses_count == 0) || (Expression[i] == Token.REST && parentheses_count == 0))
                {
                    return Solve_Binary(Expression, Expr_Tokens, Expression[i], i);
                }
            }

            //Este for le da importancia a ^
            for (int i = 0; i < Expression.Count; i++)
            {
                if (Expression[i] == "(")
                {
                    parentheses_count++;
                }
                if (Expression[i] == ")")
                {
                    parentheses_count--;
                }
                if (Expression[i] == Token.EXPONENTIAL && parentheses_count == 0) //^
                {
                    return Solve_Binary(Expression, Expr_Tokens, Expression[i], i);
                }
            }

            //Este for le da importancia a @
            for (int i = 0; i < Expression.Count; i++)
            {
                if (Expression[i] == "(")
                {
                    parentheses_count++;
                }
                if (Expression[i] == ")")
                {
                    parentheses_count--;
                }
                if (Expression[i] == Token.CONCAT && parentheses_count == 0) //@
                {
                    return Solve_Binary(Expression, Expr_Tokens, Expression[i], i);
                }
            }

            //Cuando no encontré ninguna operación binaria y debo resolver los paréntesiss
            if (Expression[0] == "(" && Expression[Expression.Count - 1] == ")")
            {
                List<string> new_expression = Frag(Expression, 1, Expression.Count - 1);
                List<string> new_exp_tokens = Frag(Expr_Tokens, 1, Expression.Count - 1);
                return Recursive_Analyzer(new_expression, new_exp_tokens);
            }

            //Cuando la entrada es un llamado a una función
            if (Expr_Tokens[0] == Token.REFERENCE && Expr_Tokens.Count > 1 && Expr_Tokens[1] == Token.OPEN_1 && Expr_Tokens[Expr_Tokens.Count - 1] == Token.CLOSED_1 && Is_All_Par(Frag(Expression, 1, Expression.Count - 1)))
            {
                //Analizando argumentos de la función
                List<string> Arguments = Frag(Expression, 2, Expression.Count - 1);
                List<string> Args_Tokens = Frag(Expr_Tokens, 2, Expr_Tokens.Count - 1);
                double a = 0;

                //Procesando argumentos para obtener su resultado
                Arguments = Process_Args(Arguments, Args_Tokens);
                foreach (string x in Arguments)
                {
                    if (Var_Eval(x) != "text" && Var_Eval(x) != "number" && Var_Eval(x) != "boolean")
                    {
                        return x;
                    }
                }

                //Analizando si se trata de alguna función previamente conocida
                if (Expression[0] == "sin")
                {
                    if (Arguments.Count != 1)
                    {
                        return "SEMANTIC ERROR: Function 'sin' receives 1 argument, but '" + Arguments.Count + "' were given";
                    }
                    if (!double.TryParse(Arguments[0], out a))
                    {
                        return "SEMANTIC ERROR: Function 'sin' receives a token 'number' argument, but a token '" + Var_Eval(Arguments[0]) + "' argument was given";
                    }
                    return Math.Sin(a).ToString();
                }
                if (Expression[0] == "cos")
                {
                    if (Arguments.Count != 1)
                    {
                        return "SEMANTIC ERROR: Function 'cos' receives 1 argument, but '" + Arguments.Count + "' were given";
                    }
                    if (!double.TryParse(Arguments[0], out a))
                    {
                        return "SEMANTIC ERROR: Function 'cos' receives a token 'number' argument, but a token '" + Var_Eval(Arguments[0]) + "' argument was given";
                    }
                    return Math.Cos(a).ToString();
                }
                if (Expression[0] == "log")
                {
                    if (Arguments.Count != 2)
                    {
                        return "SEMANTIC ERROR: Function 'log' receives 2 argument, but '" + Arguments.Count + "' were given";
                    }
                    if (!double.TryParse(Arguments[0], out a))
                    {
                        return "SEMANTIC ERROR: Function 'cos' receives a token 'number' argument, but a token '" + Var_Eval(Arguments[0]) + "' argument was given";
                    }
                    if (!double.TryParse(Arguments[1], out a))
                    {
                        return "SEMANTIC ERROR: Function 'cos' receives a token 'number' argument, but a token '" + Var_Eval(Arguments[0]) + "' argument was given";
                    }
                    return Math.Log(double.Parse(Arguments[0]), double.Parse(Arguments[1])).ToString();
                }
                if (Expression[0] == "print")
                {
                    if (Arguments.Count != 1)
                    {
                        return "SEMANTIC ERROR: Function 'print' receives 1 argument, but '" + Arguments.Count + "' were given";
                    }
                    return (Arguments[0]);
                }

                //Analizando función
                if (!Universal.Is_Function_on_List(Expression[0]))
                {
                    return "SEMANTIC ERROR: Function '" + Expression[0] + "' is not defined on this context";
                }
                Function_type function = Universal.Get_Func(Expression[0]);
                //Verificando que tenga la misma cantidad de argumentos que recibe
                if (function.arguments.Arguments.Count != Arguments.Count)
                {
                    return "SEMANTIC ERROR: Function '" + function.name + "' receives '" + function.arguments.Arguments.Count + "' arguments, but '" + Arguments.Count + "' were given";
                }
                //Creando cuerpo de función a evaluar
                List<string> Corpus = new List<string>();
                List<string> Token_Corpus = new List<string>();
                for (int i = 0; i < function.corpus.Count; i++)
                {
                    Corpus.Add(function.corpus[i]);
                    Token_Corpus.Add(function.corpus_tk[i]);
                }
                //Guardando los argumentos como variables con valores en el cuerpo de la función
                for (int i = 0; i < Arguments.Count; i++)
                {
                    if (Variables_Contains_on_Scope(function.arguments.Arguments[i]))
                    {
                        Variables_Remove(function.arguments.Arguments[i]);
                    }
                    Variables.Add((function.arguments.Arguments[i], Arguments[i], Scope_Counter));
                }
                //Saltando de scope tras guardar las variables
                Scope_Counter++;

                //Retornando valor
                string Result = Recursive_Analyzer(Corpus, Token_Corpus);
                //Limpiando scope
                Scope_Counter--;
                Scope_Clean();
                return Result;
            }

            //Cuando la entrada es una variable
            if (Expression.Count == 1 && Expr_Tokens[0] == Token.REFERENCE)
            {
                if (Variables_Contains(Expression[0]))
                {
                    return Get_Value(Expression[0]);
                }
                else
                {
                    return "SEMANTIC ERROR: Variable '" + Expression[0] + "' is not defined on this context";
                }
            }

            //Cuando la entrada es un valor
            if (Expression.Count == 1 && (Var_Eval(Expression[0]) == "number" || Var_Eval(Expression[0]) == "text" || Var_Eval(Expression[0]) == "boolean"))
            {
                return Expression[0];
            }

            //Cuando no entra en ningún caso
            return "Unknown semantic error !o_o!";
        }
    }

    //Verifica si la lista de variables contiene a una variable específica en cualquier scope
    static bool Variables_Contains(string name)
    {
        foreach ((string, string, int) var in Variables)
        {
            if (name == var.Item1)
            {
                return true;
            }
        }
        return false;
    }

    //Verifica si la lista de variables contiene a una variable específica en el scope actual
    static bool Variables_Contains_on_Scope(string name)
    {
        foreach ((string, string, int) var in Variables)
        {
            if (name == var.Item1 && Scope_Counter == var.Item3)
            {
                return true;
            }
        }
        return false;
    }

    //Obtiene el valor de una variable de la lista
    static string Get_Value(string name)
    {
        string R = "";
        int actual_scope = 0;
        foreach ((string, string, int) var in Variables)
        {
            if (var.Item1 == name && var.Item3 >= actual_scope)
            {
                R = var.Item2;
                actual_scope = var.Item3;
            }
        }
        return R;
    }

    //Elimina variables repetidas en el mismo scope para volver a insertarlas con un nuevo valor
    static void Variables_Remove(string name)
    {
        List<(string, string, int)> Remove = new List<(string, string, int)>();
        foreach ((string, string, int) var in Variables)
        {
            if (name == var.Item1 && var.Item3 == Scope_Counter)
            {
                Remove.Add(var);
            }
        }
        foreach ((string, string, int) rem in Remove)
        {
            Variables.Remove(rem);
        }
    }

    //Esto limpia el último scope creado después de ejecutar la expresión in
    static void Scope_Clean()
    {
        List<(string, string, int)> Remove = new List<(string, string, int)>();
        foreach ((string, string, int) var in Variables)
        {
            if (var.Item3 == Scope_Counter)
            {
                Remove.Add(var);
            }
        }
        foreach ((string, string, int) rem in Remove)
        {
            Variables.Remove(rem);
        }
    }

    //Evaluar el tipo de dato que guarda una variable
    static string Var_Eval(string value)
    {
        if (value[0] == '"')
        {
            return "text";
        }
        if (value == "True" || value == "False")
        {
            return "boolean";
        }
        double a;
        if (double.TryParse(value, out a) || value == Math.PI.ToString() || value == Math.E.ToString())
        {
            return "number";
        }
        else
        {
            return value;
        }
    }

    //Procesar una operación binaria
    static string Solve_Binary(List<string> Expression, List<string> Expr_Tokens, string Operation, int Op_position)
    {
        //Declarando variables de MI y MD
        string left;
        string right;

        //Analizando miembro izquierdo
        List<string> expressions = Frag(Expression, 0, Op_position);
        List<string> token_exp = Frag(Expr_Tokens, 0, Op_position);
        left = Recursive_Analyzer(expressions, token_exp);

        //Analizando miembro derecho
        expressions = Frag(Expression, Op_position + 1, Expression.Count);
        token_exp = Frag(Expr_Tokens, Op_position + 1, Expression.Count);
        right = Recursive_Analyzer(expressions, token_exp);

        //Resolviendo para cada operación
        if (Operation == Token.AND || Operation == Token.OR)
        {
            //Evaluando correctitud del miembro izquierdo
            if (Var_Eval(left) != "boolean")
            {
                if (Var_Eval(left) == "text" || Var_Eval(left) == "number")
                {
                    return "SEMANTIC ERROR: Invalid data type detected on left argument in '" + Operation + "' operation, 'boolean' data expected...";
                }
                else
                {
                    return left;
                }
            }
            //Evaluando correctitud del miembro derecho
            if (Var_Eval(right) != "boolean")
            {
                if (Var_Eval(right) == "text" || Var_Eval(right) == "number")
                {
                    return "SEMANTIC ERROR: Invalid data type detected on right argument in '" + Operation + "' operation, 'boolean' data expected...";
                }
                else
                {
                    return right;
                }
            }
            //Retornar valores según la operación actual
            if (Operation == Token.AND)
            {
                return (left == "True" && right == "True").ToString();
            }
            if (Operation == Token.OR)
            {
                return (left == "True" || right == "True").ToString();
            }
        }

        if (Operation == Token.EQUIVALENT || Operation == Token.DIFFERENT)
        {
            //Retornar error si los valores hallados en la izquierda o derecha de la expresion son errores
            if (Var_Eval(left) != "text" && Var_Eval(left) != "number" && Var_Eval(left) != "boolean")
            {
                return left;
            }
            if (Var_Eval(right) != "text" && Var_Eval(right) != "number" && Var_Eval(right) != "boolean")
            {
                return right;
            }
            if (Var_Eval(left) != Var_Eval(right))
            {
                return "SEMANTIC ERROR: Invalid operation '" + Operation + "' between token '" + Var_Eval(left) + "' and token '" + Var_Eval(right) + "'";
            }
            //Retornar valores según la operación actual
            if (Operation == Token.EQUIVALENT)
            {
                return (left == right).ToString();
            }
            if (Operation == Token.DIFFERENT)
            {
                return (left != right).ToString();
            }

        }

        if (Operation == Token.MINOR || Operation == Token.MINOR_E || Operation == Token.MAJOR || Operation == Token.MAJOR_E)
        {
            //Retornar error si los valores hallados en la izquierda o derecha de la expresion son errores o no son tipo número
            if (Var_Eval(left) != "number" || Var_Eval(right) != "number")
            {
                if (Var_Eval(left) != "number" && Var_Eval(left) != "text" && Var_Eval(left) != "boolean")
                {
                    return left;
                }
                if (Var_Eval(right) != "number" && Var_Eval(right) != "text" && Var_Eval(right) != "boolean")
                {
                    return right;
                }
                return "SEMANTIC ERROR: The '" + Operation + "' comparation must be between two numbers, but token '" + Var_Eval(left) + "' and token '" + Var_Eval(right) + "' were given";
            }
            //Retornar valores según la operación actual
            if (Operation == Token.MINOR_E)
            {
                return (double.Parse(left) <= double.Parse(right)).ToString();
            }
            if (Operation == Token.MINOR)
            {
                return (double.Parse(left) < double.Parse(right)).ToString();
            }
            if (Operation == Token.MAJOR)
            {
                return (double.Parse(left) > double.Parse(right)).ToString();
            }
            if (Operation == Token.MAJOR_E)
            {
                return (double.Parse(left) >= double.Parse(right)).ToString();
            }
        }

        if (Operation == Token.ADD || Operation == Token.SUBSTRACT || Operation == Token.MULTIPLY || Operation == Token.SPLIT || Operation == Token.REST || Operation == Token.EXPONENTIAL)
        {
            //Retornar error si los valores hallados en la izquierda o derecha de la expresion son errores o no son tipo número
            if (Var_Eval(left) != "number" || Var_Eval(right) != "number")
            {
                if (Var_Eval(left) != "number" && Var_Eval(left) != "text" && Var_Eval(left) != "boolean")
                {
                    return left;
                }
                if (Var_Eval(right) != "number" && Var_Eval(right) != "text" && Var_Eval(right) != "boolean")
                {
                    return right;
                }
                return "SEMANTIC ERROR: The '" + Operation + "' operation must be between two numbers, but token '" + Var_Eval(left) + "' and token '" + Var_Eval(right) + "' were given";
            }
            //Retornar valores según la operación actual
            if (Operation == Token.ADD)
            {
                return (double.Parse(left) + double.Parse(right)).ToString();
            }
            if (Operation == Token.SUBSTRACT)
            {
                return (double.Parse(left) - double.Parse(right)).ToString();
            }
            if (Operation == Token.MULTIPLY)
            {
                return (double.Parse(left) * double.Parse(right)).ToString();
            }
            if (Operation == Token.SPLIT)
            {
                return (double.Parse(left) / double.Parse(right)).ToString();
            }
            if (Operation == Token.REST)
            {
                return (double.Parse(left) % double.Parse(right)).ToString();
            }
            if (Operation == Token.EXPONENTIAL)
            {
                return (Math.Pow(double.Parse(left), double.Parse(right)).ToString());
            }
        }

        if (Operation == Token.CONCAT)
        {
            //Retornar el error que puedan presentar alguno de los dos miembros
            if (Var_Eval(left) != "text" && Var_Eval(left) != "number" && Var_Eval(left) != "boolean")
            {
                return left;
            }
            if (Var_Eval(right) != "text" && Var_Eval(right) != "number" && Var_Eval(right) != "boolean")
            {
                return right;
            }
            //Retornar concatenación esperada
            return left + right;
        }

        return "Non binary";
    }

    //Fragmentar una entrada (devuelve una sublista de elementos consecutivos)
    static List<string> Frag(List<string> Input, int init, int end)
    {
        List<string> R = new List<string>();
        while (init != end)
        {
            R.Add(Input[init]);
            init++;
        }
        return R;
    }

    //Chequear que toda la expresión esté contenida en un mismo paréntesis 
    static bool Is_All_Par(List<string> Input)
    {
        int count = 1;
        for (int i = 1; i < Input.Count - 1; i++)
        {
            if (count == 0)
            {
                return false;
            }
            if (Input[i] == "(")
            {
                count++;
            }
            if (Input[i] == ")")
            {
                count--;
            }
        }
        return true;
    }

    //Procesar los argumentos de entrada de una función para hallar su valor real
    static List<string> Process_Args(List<string> Input, List<string> Tokens)
    {
        List<string> Values = new List<string>();
        List<string> Exp = new List<string>();
        List<string> Exp_Tk = new List<string>();
        int mark = 0;
        int par_counter = 0;
        while (mark < Input.Count)
        {
            if (Input[mark] == "(")
            {
                par_counter++;
            }
            if (Input[mark] == ")")
            {
                par_counter--;
            }
            if (Input[mark] == "," && par_counter == 0)
            {
                Values.Add(Recursive_Analyzer(Exp, Exp_Tk));
                Exp = new List<string>();
                Exp_Tk = new List<string>();
            }
            else
            {
                Exp.Add(Input[mark]);
                Exp_Tk.Add(Tokens[mark]);
            }
            mark++;
        }
        Values.Add(Recursive_Analyzer(Exp, Exp_Tk));
        return Values;
    }
}